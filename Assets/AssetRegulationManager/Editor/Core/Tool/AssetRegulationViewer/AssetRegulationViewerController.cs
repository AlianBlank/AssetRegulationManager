// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetRegulationManager.Editor.Core.Data;
using AssetRegulationManager.Editor.Core.Model;
using AssetRegulationManager.Editor.Core.Model.Adapters;
using AssetRegulationManager.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine;

namespace AssetRegulationManager.Editor.Core.Tool.AssetRegulationViewer
{
    internal sealed class AssetRegulationViewerController : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly AssetRegulationTestExecuteService _executeService;
        private readonly AssetRegulationTestGenerateService _generateService;
        private readonly AssetRegulationManagerStore _store;

        private CancellationTokenSource _testExecuteTaskCancellationTokenSource;
        private AssetRegulationViewerTreeView _treeView;
        private AssetRegulationViewerState _viewerState;
        private AssetRegulationViewerWindow _window;

        public AssetRegulationViewerController(AssetRegulationManagerStore store)
        {
            _store = store;
            var assetDatabaseAdapter = new AssetDatabaseAdapter();
            _generateService = new AssetRegulationTestGenerateService(store, assetDatabaseAdapter);
            _executeService = new AssetRegulationTestExecuteService(store);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Setup(AssetRegulationViewerWindow window, AssetRegulationViewerState viewerState)
        {
            _window = window;
            _treeView = _window.TreeView;
            _viewerState = viewerState;

            window.AssetPathOrFilterChangedAsObservable.Subscribe(_generateService.Run).DisposeWith(_disposables);
            window.RefreshButtonClickedAsObservable.Subscribe(_generateService.Run).DisposeWith(_disposables);
            window.CheckAllButtonClickedAsObservable
                .Subscribe(_ =>
                {
                    var __ = CheckAllAsync();
                })
                .DisposeWith(_disposables);
            window.CheckSelectedAddButtonClickedAsObservable.Subscribe(_ =>
                {
                    if (_treeView.HasSelection())
                    {
                        return;
                    }

                    var ids = _treeView.GetSelection();
                    var __ = CheckAsync(ids);
                })
                .DisposeWith(_disposables);
            _treeView.ItemDoubleClicked += OnItemDoubleClicked;
            _treeView.OnSelectionChanged += OnSelectionChanged;
        }

        public void Cleanup()
        {
            _treeView.ItemDoubleClicked -= OnItemDoubleClicked;
            _treeView.OnSelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(IList<int> ids)
        {
            if (ids.Count == 0)
            {
                return;
            }

            var firstId = ids.First();
            var item = _treeView.GetItem(firstId);
            var testId = "";
            if (item is AssetRegulationTestTreeViewItem testItem)
            {
                testId = testItem.TestId;
            }
            else if (item is AssetRegulationTestEntryTreeViewItem entryItem)
            {
                var parent = (AssetRegulationTestTreeViewItem)entryItem.parent;
                testId = parent.TestId;
            }

            var test = _store.Tests[testId];
            _viewerState.SelectedAssetPath.Value = test.AssetPath;
        }

        private void OnItemDoubleClicked(int itemId)
        {
            var _ = CheckAsync(new[] { itemId });
        }

        private async Task CheckAllAsync()
        {
            if (_testExecuteTaskCancellationTokenSource != null)
            {
                _testExecuteTaskCancellationTokenSource.Cancel();
            }

            _testExecuteTaskCancellationTokenSource = new CancellationTokenSource();
            try
            {
                await CheckAllAsync(_testExecuteTaskCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                ShowUnexpectedErrorDialog();
            }
            finally
            {
                _testExecuteTaskCancellationTokenSource = null;
            }
        }

        private async Task CheckAsync(IEnumerable<int> ids)
        {
            if (_testExecuteTaskCancellationTokenSource != null)
            {
                _testExecuteTaskCancellationTokenSource.Cancel();
            }

            _testExecuteTaskCancellationTokenSource = new CancellationTokenSource();
            try
            {
                await CheckAsync(ids, _testExecuteTaskCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                ShowUnexpectedErrorDialog();
            }
            finally
            {
                _testExecuteTaskCancellationTokenSource = null;
            }
        }

        private async Task CheckAllAsync(CancellationToken cancellationToken)
        {
            var targets = _store.Tests.Values.ToArray();
            foreach (var test in targets)
            {
                _executeService.ClearAllResults(test.Id);
            }

            await Task.Delay(300, cancellationToken);

            foreach (var test in targets)
            {
                var sequence = _executeService.CreateRunAllSequence(test.Id);
                foreach (var _ in sequence)
                {
                    await Task.Delay(1, cancellationToken);
                }
            }
        }

        private async Task CheckAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var targetEntryIds = new Dictionary<string, HashSet<string>>();
            foreach (var selection in ids)
            {
                var item = _treeView.GetItem(selection);
                if (item is AssetRegulationTestTreeViewItem testItem)
                {
                    // If the root item is selected, all child test entries will be targeted.
                    var testId = testItem.TestId;
                    if (!targetEntryIds.TryGetValue(testId, out var entryIds))
                    {
                        entryIds = new HashSet<string>();
                        targetEntryIds.Add(testId, entryIds);
                    }

                    if (testItem.hasChildren)
                    {
                        foreach (var child in testItem.children)
                        {
                            var testEntryItem = (AssetRegulationTestEntryTreeViewItem)child;
                            entryIds.Add(testEntryItem.EntryId);
                        }
                    }
                }
                else
                {
                    var testEntryItem = (AssetRegulationTestEntryTreeViewItem)item;
                    var testId = ((AssetRegulationTestTreeViewItem)testEntryItem.parent).TestId;

                    if (!targetEntryIds.TryGetValue(testId, out var entryIds))
                    {
                        entryIds = new HashSet<string>();
                        targetEntryIds.Add(testId, entryIds);
                    }

                    entryIds.Add(testEntryItem.EntryId);
                }
            }

            // Clear results.
            foreach (var value in targetEntryIds)
            {
                _executeService.ClearResults(value.Key, value.Value.ToArray());
            }

            await Task.Delay(300, cancellationToken);

            // Run all the tests.
            foreach (var value in targetEntryIds)
            {
                var tests = _executeService.CreateRunSequence(value.Key, value.Value.ToArray());
                foreach (var _ in tests)
                {
                    await Task.Delay(1, cancellationToken);
                }
            }
        }

        private static void ShowUnexpectedErrorDialog()
        {
            const string message = "Unexpected error has occurred during testing. See the Console Window for details.";
            EditorUtility.DisplayDialog("Error", message, "OK");
        }
    }
}