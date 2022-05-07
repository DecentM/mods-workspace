using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using JetBrains.Annotations;

using UnityEngine;
using UnityEditor;

namespace DecentM.EditorTools
{
    public class ImageStore : AutoSceneFixer
    {
        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private static void ApplyImportSettings(string path)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer == null) return;

            TextureImporter textureImporter = (TextureImporter)importer;

            // If it's imported as a sprite, we assume it's done by us so that we don't keep re-crunching already imported images.
            // This also lets the user change the other settings without us resetting them every time.
            if (textureImporter.textureType == TextureImporterType.Sprite) return;

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.mipmapEnabled = false;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.maxTextureSize = 1024;
            textureImporter.textureCompression = TextureImporterCompression.Compressed;
            textureImporter.compressionQuality = 50;
            textureImporter.crunchedCompression = true;
            textureImporter.SaveAndReimport();
        }

        private static void ReapplyImportSettings(string[] urls)
        {
            for (int i = 0; i < urls.Length; i++)
            {
                EditorUtility.DisplayProgressBar($"Reapplying import settings... ({i} of {urls.Length})", urls[i], 1f * i / urls.Length);
                string path = GetPathFromUrl(urls[i]);
                ApplyImportSettings(path);
            }

            EditorUtility.ClearProgressBar();
        }

        private static string GetPathFromUrl(string url)
        {
            if (!ValidateUrl(url)) return null;

            string hash = Hash.String(url);
            string filename = "image.jpg";
            return $"{EditorAssets.ImageCacheFolder}/{hash}/{filename}";
        }

        private static void CreateFolders(string[] urls)
        {
            List<Tuple<string, string>> folders = new List<Tuple<string, string>>();

            foreach (string url in urls)
            {
                string hash = Hash.String(url);
                folders.Add(new Tuple<string, string>($"{EditorAssets.ImageCacheFolder}", hash));
            }

            Assets.CreateFolders(folders);
        }

        private static List<string> pendingUrls = new List<string>();

        protected override bool OnPerformFixes()
        {
            string[] urls = pendingUrls.ToArray();
            if (urls.Length == 0) return true;

            CreateFolders(pendingUrls.ToArray());

            foreach (string url in pendingUrls)
            {
                FetchSync(url);
            }

            AssetDatabase.Refresh();
            ReapplyImportSettings(urls);

            return true;
        }

        [PublicAPI]
        public static Texture2D GetFromCache(string url)
        {
            if (!ValidateUrl(url)) return null;

            string path = GetPathFromUrl(url);
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (result == null && !pendingUrls.Contains(url))
            {
                pendingUrls.Add(url);
            }

            return result;
        }

        private static void WriteImageFromResponse(HttpWebResponse response, string path)
        {
            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
            {
                throw new Exception($"The web server hosting {response.ResponseUri} has responded with {(int)response.StatusCode}");
            }

            Stream data = response.GetResponseStream();
            MemoryStream ms = new MemoryStream();
            data.CopyTo(ms);
            byte[] bytes = ms.ToArray();
            File.WriteAllBytes(path, bytes);
        }

        private static void FetchSync(string url)
        {
            if (string.IsNullOrEmpty(url) || !ValidateUrl(url)) return;

            WebRequest request = HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string path = GetPathFromUrl(url);
            WriteImageFromResponse(response, path);
        }
    }
}
