// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AssetRegulationManager.Editor.Core.Model.AssetRegulations;
using AssetRegulationManager.Editor.Foundation.TinyRx.ObservableProperty;
using Object = UnityEngine.Object;

namespace AssetRegulationManager.Editor.Core.Model.AssetRegulationTests
{
    public sealed class AssetRegulationTestEntry
    {
        private readonly ObservableProperty<string> _message = new ObservableProperty<string>(string.Empty);

        private readonly ObservableProperty<AssetRegulationTestStatus> _status =
            new ObservableProperty<AssetRegulationTestStatus>(AssetRegulationTestStatus.None);

        public AssetRegulationTestEntry(IAssetConstraint constraint)
        {
            Id = Guid.NewGuid().ToString();
            Constraint = constraint;
        }

        public string Id { get; }
        public string Description => Constraint.GetDescription();
        public IAssetConstraint Constraint { get; }

        public IReadOnlyObservableProperty<AssetRegulationTestStatus> Status => _status;
        public IReadOnlyObservableProperty<string> Message => _message;

        internal void RunFixed(Object obj)
        {
            try
            {
                var success = Constraint.Fixed(obj);
                if (success)
                    _status.Value = AssetRegulationTestStatus.Success;
                else
                    _status.Value = AssetRegulationTestStatus.Failed;

                _message.Value = $"Actual Value: {Constraint.GetLatestValueAsText()}";
            }
            catch (InvalidCastException)
            {
                // If the cast fails, the constraint does not support the type of obj.
                // It may be a configuration error so make it a warning.
                _status.Value = AssetRegulationTestStatus.Warning;
                _message.Value = $"This test cannot be used for {obj.GetType()}.";
            }
        }

        internal void Run(Object obj)
        {
            try
            {
                var success = Constraint.Check(obj);
                if (success)
                    _status.Value = AssetRegulationTestStatus.Success;
                else
                    _status.Value = AssetRegulationTestStatus.Failed;

                _message.Value = $"Actual Value: {Constraint.GetLatestValueAsText()}";
            }
            catch (InvalidCastException)
            {
                // If the cast fails, the constraint does not support the type of obj.
                // It may be a configuration error so make it a warning.
                _status.Value = AssetRegulationTestStatus.Warning;
                _message.Value = $"This test cannot be used for {obj.GetType()}.";
            }
        }

        internal void Reset()
        {
            _status.Value = AssetRegulationTestStatus.None;
            _message.Value = string.Empty;
        }
    }
}