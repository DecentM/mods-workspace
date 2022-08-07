using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace DecentM.EditorTools.SelfLocator
{
    [ScriptedImporter(1, SelfLocatorAsset.SelfLocatorId)]
    public class SelfLocatorImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!File.Exists(ctx.assetPath))
            {
                ctx.LogImportError($"Imported file disappeared from {ctx.assetPath}");
                TextAsset errorAsset = new TextAsset("");
                ctx.AddObjectToAsset(
                    Path.GetFileName(ctx.assetPath),
                    errorAsset
                );
                ctx.SetMainObject(errorAsset);
                return;
            }

            SelfLocatorAsset asset = SelfLocatorAsset.CreateInstance(
                File.ReadAllText(ctx.assetPath)
            );

            ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), asset);
            ctx.SetMainObject(asset);
        }
    }
}
