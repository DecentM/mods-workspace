using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using DecentM.EditorTools.SelfLocator;

namespace DecentM.Icons
{
    public static class MaterialIcons
    {
        private static int IconSize = 96;

        private static Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();

        public static Texture2D GetIcon(Icon icon)
        {
            int index = (int)icon;

            if (cache.ContainsKey(index) && cache.TryGetValue(index, out Texture2D cachedResult))
            {
                return cachedResult;
            }

            string path = $"{SelfLocatorAsset.LocateSelf()}/Editor/MaterialIcons/spritesheet.png";
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            int iconX = index % (sheet.width / IconSize);
            int iconY = Mathf.FloorToInt(index * IconSize / sheet.height);

            int x = iconX * IconSize;
            int y = sheet.height - (iconY * IconSize) - IconSize;

            Color[] colours = sheet.GetPixels(x, y, IconSize, IconSize);

            Texture2D result = new Texture2D(IconSize, IconSize);
            result.alphaIsTransparency = true;
            result.wrapMode = TextureWrapMode.Clamp;

            result.SetPixels(colours);
            result.Apply();

            cache.Add(index, result);

            return result;
        }
    }
}
