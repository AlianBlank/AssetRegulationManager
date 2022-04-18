// --------------------------------------------------------------
// Copyright 2021 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetRegulationManager.Editor.Core.Data;
using AssetRegulationManager.Editor.Core.Model.Adapters;
using AssetRegulationManager.Editor.Core.Model.AssetRegulations;
using AssetRegulationManager.Editor.Core.Model.AssetRegulationTests;

namespace AssetRegulationManager.Editor.Core.Model
{
    public sealed class AssetRegulationTestGenerateService
    {
        private readonly IAssetDatabaseAdapter _assetDatabaseAdapter;
        private readonly IAssetRegulationRepository _regulationRepository;
        private readonly IAssetRegulationTestStore _testStore;

        public AssetRegulationTestGenerateService(IAssetRegulationRepository regulationRepository,
            IAssetRegulationTestStore testStore, IAssetDatabaseAdapter assetDatabaseAdapter)
        {
            _regulationRepository = regulationRepository;
            _testStore = testStore;
            _assetDatabaseAdapter = assetDatabaseAdapter;
        }

        /// <summary>
        ///     Generate the asset regulation tests.
        /// </summary>
        /// <param name="assetFilter">Similar to what you enter in the search field of the project view.</param>
        /// <param name="regulationDescriptionFilters">
        ///     If not empty, only regulations whose description matches this regex will be
        ///     considered.
        /// </param>
        public void Run(string assetFilter, IReadOnlyList<string> regulationDescriptionFilters = null)
        {
            var assetPaths = string.IsNullOrWhiteSpace(assetFilter)
                ? Array.Empty<string>()
                : _assetDatabaseAdapter.FindAssetPaths(assetFilter);

            RunInternal(assetPaths, regulationDescriptionFilters);
        }

        /// <summary>
        ///     Generate the asset regulation tests.
        /// </summary>
        /// <param name="assetPathFilters">Only assets whose name matches this regex will be considered.</param>
        /// <param name="regulationDescriptionFilters">
        ///     If not empty, only regulations whose description matches this regex will be
        ///     considered.
        /// </param>
        public void Run(IReadOnlyList<string> assetPathFilters, IReadOnlyList<string> regulationDescriptionFilters = null)
        {
            if (assetPathFilters == null || assetPathFilters.Count == 0)
            {
                var assetPaths = _assetDatabaseAdapter.GetAllAssetPaths();
                RunInternal(assetPaths, regulationDescriptionFilters);
            }
            else
            {
                var assetPathFilterRegexes = assetPathFilters.Select(x => new Regex(x)).ToArray();

                // Grouping by 100 AssetPaths.
                var assetPaths = _assetDatabaseAdapter.GetAllAssetPaths();
                var assetPathGroups = assetPaths.Select((v, i) => new { v, i })
                    .GroupBy(x => x.i / 100)
                    .Select(g => g.Select(x => x.v).ToArray());

                // Process each group in different threads.
                var matchedAssetPathsTasks = assetPathGroups
                    .Select(assetPathGroup => GetMatchedAssetPathsAsync(assetPathGroup, assetPathFilterRegexes))
                    .ToList();

                var matchedAssetPaths = Task.WhenAll(matchedAssetPathsTasks).Result.SelectMany(x => x);

                RunInternal(matchedAssetPaths, regulationDescriptionFilters);
            }
        }

        private void RunInternal(IEnumerable<string> assetPaths, IReadOnlyList<string> regulationNameFilters = null)
        {
            _testStore.ClearTests();

            var regulations = _regulationRepository.GetAllRegulations().ToArray();

            // Filter regulations.
            if (regulationNameFilters != null && regulationNameFilters.Count >= 1)
            {
                var regulationNameFiltersRegexes = regulationNameFilters
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new Regex(x));
                
                regulations = regulations
                    .Where(x => regulationNameFiltersRegexes.Any(y => y.IsMatch(x.Name.Value)))
                    .ToArray();
            }

            // Setup all regulations.
            foreach (var regulation in regulations)
            {
                regulation.Setup();
            }

            // Grouping by 100 AssetPaths.
            var assetPathGroups = assetPaths.Select((v, i) => new { v, i })
                .GroupBy(x => x.i / 100)
                .Select(g => g.Select(x => x.v).ToArray());

            // Process each group in different threads.
            var createTestsTasks = assetPathGroups
                .Select(assetPathGroup =>
                    CreateTestsAsync(assetPathGroup, regulations, _assetDatabaseAdapter))
                .ToList();

            var tests = Task.WhenAll(createTestsTasks).Result;
            foreach (var test in tests)
            {
                _testStore.AddTests(test);
            }
        }

        private static Task<string[]> GetMatchedAssetPathsAsync(IList<string> assetPaths,
            Regex[] assetPathFilterRegexes)
        {
            return Task.Run(() =>
            {
                return assetPaths.Where(x => { return assetPathFilterRegexes.Any(y => y.IsMatch(x)); }).ToArray();
            });
        }

        private static Task<AssetRegulationTest[]> CreateTestsAsync(IList<string> assetPaths,
            IList<AssetRegulation> regulations, IAssetDatabaseAdapter assetDatabaseAdapter)
        {
            return Task.Run(() =>
            {
                var result = new List<AssetRegulationTest>();
                for (var i = 0; i < assetPaths.Count; i++)
                {
                    var assetPath = assetPaths[i];
                    var test = new AssetRegulationTest(assetPath, assetDatabaseAdapter);
                    foreach (var regulation in regulations)
                    {
                        if (!regulation.IsTargetAsset(assetPath))
                        {
                            continue;
                        }

                        foreach (var constraint in regulation.Constraints.Values)
                        {
                            test.AddEntry(constraint);
                        }
                    }

                    result.Add(test);
                }

                return result.ToArray();
            });
        }
    }
}
