using UnityEditor;
using UnityEngine;

namespace Game.Tools
{
    public class ModelImportProcessor : AssetPostprocessor
    {
        private const string SKELETAL_MESH_PREFIX = "SK_";
        private const string ANIM_LOOP_SUFFIX = "_Loop";

        private void OnPreprocessModel()
        {
            if (assetImporter is not ModelImporter importer)
                return;

            if (!importer.importSettingsMissing)
                return;

            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.bakeAxisConversion = true;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.isReadable = false;
            importer.addCollider = false;

            if (importer.name.StartsWith(SKELETAL_MESH_PREFIX))
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.importConstraints = false;
                importer.importAnimation = true;

                foreach (var animation in importer.clipAnimations)
                {
                    if (animation.name.EndsWith(ANIM_LOOP_SUFFIX))
                        animation.loop = true;

                    // remove armature prefix
                    if (animation.name.StartsWith(SKELETAL_MESH_PREFIX))
                    {
                        var split = animation.name.Split('|');
                        if (split.Length > 1)
                        {
                            animation.name = split[1];
                            Debug.Log($"Removed prefix from animation {animation.name}");
                        }
                    }
                }
            }
            else
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.importConstraints = false;
                importer.importAnimation = false;
            }
        }
    }
}