using System;
using AssetRegulationManager.Editor.Core.Model.AssetRegulations;
using AssetRegulationManager.Editor.Foundation.CustomDrawers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AssetRegulationManager.Editor
{
    [Serializable]
    [AssetConstraint("Texture/Generate MipMaps", "Generate MipMaps")] // Required for GUI.
    public sealed class GenerateMipMapsConstraint : AssetConstraint<Texture2D>
    {
        [SerializeField] private bool generateMipMaps;
        private bool _latestValue;

        public bool GenerateMipMaps
        {
            get { return generateMipMaps; }
            set { generateMipMaps = value; }
        }

        // Get a description of this constraint.
        public override string GetDescription()
        {
            return $"Generate Mip Maps: {generateMipMaps}";
        }

        public override string GetLatestValueAsText()
        {
            return _latestValue.ToString();
        }


        protected override bool CheckInternal(Texture2D asset)
        {
            Assert.IsNotNull(asset);
            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
            var mipmapEnabled = textureImporter && textureImporter.mipmapEnabled;
            _latestValue = mipmapEnabled;
            return mipmapEnabled == generateMipMaps;
        }

        protected override bool FixedInternal(Texture2D asset)
        {
            Assert.IsNotNull(asset);
            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.mipmapEnabled = generateMipMaps;
                textureImporter.SaveAndReimport();
            }

            _latestValue = generateMipMaps;
            return true;
        }
    }

    [CustomGUIDrawer(typeof(GenerateMipMapsConstraint))]
    public sealed class GenerateMipMapConstraintDrawer : GUIDrawer<GenerateMipMapsConstraint>
    {
        public override void Setup(object target)
        {
            base.Setup(target);

            // If you want to setup this drawer, write here.
        }

        protected override void GUILayout(GenerateMipMapsConstraint target)
        {
            target.GenerateMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", target.GenerateMipMaps);
        }
    }
}