﻿// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetRegulationManager.Editor.Core.Tool.AssetRegulationEditor
{
    [Serializable]
    internal sealed class AssetRegulationEditorTreeViewState : TreeViewState
    {
        [SerializeField] private MultiColumnHeaderState.Column[] _columnStates;

        public MultiColumnHeaderState.Column[] ColumnStates => _columnStates;

        public AssetRegulationEditorTreeViewState()
        {
            _columnStates = GetColumnStates();
        }

        private MultiColumnHeaderState.Column[] GetColumnStates()
        {
            var nameColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                canSort = true,
                width = 150,
                minWidth = 50,
                autoResize = false,
                allowToggleVisibility = false
            };
            var targetsColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Targets"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 200,
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = true
            };
            var constraintsColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Constraints"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 200,
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = true
            };
            return new[] { nameColumn, targetsColumn, constraintsColumn };
        }
    }
}
