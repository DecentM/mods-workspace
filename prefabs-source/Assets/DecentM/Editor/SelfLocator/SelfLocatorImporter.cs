using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

using DecentM.Icons;
using DecentM.Subtitles;

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
                ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), errorAsset, MaterialIcons.GetIcon(Icon.Close));
                ctx.SetMainObject(errorAsset);
                return;
            }

            TextAsset asset = new TextAsset(File.ReadAllText(ctx.assetPath));

            ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), asset, MaterialIcons.GetIcon(Icon.MapMarker));
            ctx.SetMainObject(asset);
        }
    }
}
