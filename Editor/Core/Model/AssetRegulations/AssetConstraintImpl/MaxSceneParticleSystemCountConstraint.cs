﻿// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AssetRegulationManager.Editor.Core.Model.AssetRegulations.AssetConstraintImpl
{
    [Serializable]
    [AssetConstraint("Particle System/Max Particle System Count in Scene", "Max Particle System Count in Scene")]
    public sealed class MaxSceneParticleSystemCountConstraint : MaxSceneComponentCountConstraint<ParticleSystem>
    {
        protected override bool FixedInternal(SceneAsset asset)
        {
            Assert.IsNotNull(asset);
            return true;
        }
    }
}
