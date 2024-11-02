// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace AssetRegulationManager.Editor.Core.Model.AssetRegulations.AssetConstraintImpl
{
    [Serializable]
    [AssetConstraint("Particle System/Max Particle System Count in GameObject",
        "Max Particle System Count in GameObject")]
    public sealed class MaxParticleSystemCountConstraint : MaxComponentCountConstraint<ParticleSystem>
    {
        protected override bool FixedInternal(GameObject asset)
        {
            Assert.IsNotNull(asset);
            return true; 
        }
    }
}
