using System;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;

using VRC.SDKBase;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer
{
    public class VideoThumbnailStore
    {
        private static string GetVideoId(string url)
        {
            if (url == "") return null;

            try
            {
                Uri uri = new Uri(url);
                string[] queryParams = uri.Query.Split(new char[] { '?', '&' });

                foreach (string param in queryParams)
                {
                    string[] parts = param.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0];
                    string value = parts[1];

                    // Look for the query parameter "v", as that's where YouTube stores the video id
                    // https://www.youtube.com/watch?v=q-fgx5ktr2s
                    if (key != "v") continue;

                    return value;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string GetThumbnailUrl(string videoId)
        {
            return $"https://i.ytimg.com/vi/{videoId}/maxresdefault.jpg";
        }

        private static string GetHash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private static bool ValidateUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            } catch
            {
                return false;
            }
        }

        private static bool CreateFolder(string basePath, string name)
        {
            if (!basePath.StartsWith("Assets/")) return false;

            if (AssetDatabase.IsValidFolder($"{basePath}/{name}")) return true;
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                string[] paths = basePath.Split('/');
                CreateFolder(string.Join("/", paths.Take(paths.Length - 1)), paths[paths.Length - 1]);
            }

            if (AssetDatabase.CreateFolder(basePath, name) == "") return false;
            return true;
        }

        private static Texture2D GetThumbnail(string url)
        {
            string hash = GetHash(url);
            string path = $"Assets/Editor/DecentM/VideoThumbnails/{hash}/maxresdefault.jpg";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static Texture2D GetThumbnail(VRCUrl url)
        {
            if (!ValidateUrl(url.ToString())) return EditorAssets.FallbackVideoThumbnail;

            return GetThumbnail(url.ToString());
        }

        private static void ApplyThumbnailImportSettings(string path)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 50;
            importer.SaveAndReimport();
        }

        private static void SetThumbnail(string hash, byte[] bytes)
        {
            string path = $"Assets/Editor/DecentM/VideoThumbnails/{hash}/maxresdefault.jpg";
            CreateFolder("Assets/Editor/DecentM/VideoThumbnails", hash);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            ApplyThumbnailImportSettings(path);
        }

        private static void SetThumbnail(string hash, Texture2D texture)
        {
            string path = $"Assets/Editor/DecentM/VideoThumbnails/{hash}/maxresdefault.jpg";
            CreateFolder("Assets/Editor/DecentM/VideoThumbnails", hash);
            File.WriteAllBytes(path, texture.EncodeToJPG());
            AssetDatabase.ImportAsset(path);
            ApplyThumbnailImportSettings(path);
        }

        private static void FetchThumbnailRaw(string hash, string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
                    {
                        Debug.LogWarning($"The web server hosting {request.RequestUri.AbsolutePath} has responded with {(int)response.StatusCode}");
                        return;
                    }

                    Stream data = response.GetResponseStream();
                    MemoryStream ms = new MemoryStream();
                    data.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    SetThumbnail(hash, bytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError($"Error while downloading thumbnail for {url.ToString()}, using fallback thumbnail");
                SetFallbackThumbnail(hash);
            }
        }

        private static void SetFallbackThumbnail(string hash)
        {
            SetThumbnail(hash, EditorAssets.FallbackVideoThumbnail);
        }

        public static void FetchThumbnail(VRCUrl url)
        {
            if (url == null || url.ToString() == "") return;
            if (!ValidateUrl(url.ToString())) return;

            string videoId = GetVideoId(url.ToString());
            string hash = GetHash(url.ToString());

            if (videoId == null || videoId == "")
            {
                SetFallbackThumbnail(hash);
                return;
            }

            string thumbnailUrl = GetThumbnailUrl(videoId);
            FetchThumbnailRaw(hash, thumbnailUrl);
        }
    }
}
