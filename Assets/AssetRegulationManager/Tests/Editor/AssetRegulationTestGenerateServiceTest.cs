﻿// --------------------------------------------------------------
// Copyright 2021 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AssetRegulationManager.Tests.Editor
{
    public class AssetRegulationTestGenerateServiceTest
    {
        /*
        [Test]
        public void Run_GenerateSuccessfully()
        {
            var regulation1 = new AssetRegulation
            {
                AssetPathRegex = "^Assets/Tests/.+"
            };
            regulation1.Entries.Add(new FakeAssetLimitation(true));

            var regulation2 = new AssetRegulation
            {
                AssetPathRegex = "^Assets/Tests/Test01.asset"
            };
            regulation2.Entries.Add(new TextureSizeLimitation());

            var regulations = new List<AssetRegulation>
            {
                regulation1, regulation2
            };

            var store = new AssetRegulationManagerStore(regulations);
            var assetDatabaseAdapter = new FakeAssetDatabaseAdapter();
            var service = new AssetRegulationTestGenerateService(store, assetDatabaseAdapter);

            Assert.That(store.Tests.Count, Is.EqualTo(0));

            service.Run(string.Empty);

            var test01 = store.Tests.Values.First(x => x.AssetPath.Equals("Assets/Tests/Test01.asset"));
            var test02 = store.Tests.Values.First(x => x.AssetPath.Equals("Assets/Tests/Test02.asset"));
            Assert.That(store.Tests.Count, Is.EqualTo(2));
            Assert.That(test01.Entries.Count, Is.EqualTo(2));
            Assert.That(test01.Entries.Values.Count(x => x.Limitation is FakeAssetLimitation), Is.EqualTo(1));
            Assert.That(test01.Entries.Values.Count(x => x.Limitation is TextureSizeLimitation), Is.EqualTo(1));
            Assert.That(test02.Entries.Count, Is.EqualTo(1));
            Assert.That(test02.Entries.Values.Count(x => x.Limitation is FakeAssetLimitation), Is.EqualTo(1));
        }

        private class FakeAssetDatabaseAdapter : IAssetDatabaseAdapter
        {
            public IEnumerable<string> FindAssetPaths(string filter)
            {
                yield return "Assets/Tests/Test01.asset";
                yield return "Assets/Tests/Test02.asset";
            }

            public TAsset LoadAssetAtPath<TAsset>(string assetPath) where TAsset : Object
            {
                throw new NotImplementedException();
            }
        }
        */
    }
}
