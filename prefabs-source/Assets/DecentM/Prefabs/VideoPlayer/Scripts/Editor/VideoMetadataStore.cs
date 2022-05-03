using System;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer
{
    public struct VideoMetadata
    {
        public string duration;
        public string title;
        public string uploader;
        public int viewCount;
        public int likeCount;
        public Texture2D thumbnail;
        public string resolution;
        public int fps;
        public string siteName;
        public string description;
    }

    public class VideoMetadataStore
    {
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
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
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

        private static void ApplyThumbnailImportSettings(string path)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 50;
            importer.crunchedCompression = true;
            importer.SaveAndReimport();
        }

        public static void ReapplyImportSettings()
        {
            string[] files = Directory.GetFiles(string.Join(Application.dataPath, "Assets/Editor/DecentM/VideoMetadata"), "*.jpg", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar($"Reapplying import settings... ({i} of {files.Length})", files[i], 1f * i / files.Length)) break;
                ApplyThumbnailImportSettings(files[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void SetMetadata(string hash, string filename, byte[] bytes)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
        }

        private static void SetThumbnail(string hash, Texture2D texture)
        {
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            SetMetadata(hash, filename, texture.EncodeToJPG());
            ApplyThumbnailImportSettings(path);
        }

        private static void SetThumbnail(string hash, byte[] bytes)
        {
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            SetMetadata(hash, filename, bytes);
            ApplyThumbnailImportSettings(path);
        }

        private static void SetFallbackThumbnail(string hash)
        {
            SetThumbnail(hash, EditorAssets.FallbackVideoThumbnail);
        }

        private static Texture2D GetThumbnail(string hash)
        {
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            return result;
        }

        public static void RefreshMetadata(string url)
        {
            if (!ValidateUrl(url)) return;

            string hash = GetHash(url);
            FetchMetadata(hash, url);

            YTDLVideoJson? jsonOrNull = GetYTDLJson(url);
            if (jsonOrNull == null) return;
            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            FetchThumbnail(hash, json);
        }

        private static YTDLVideoJson? GetYTDLJson(string url)
        {
            if (!ValidateUrl(url)) return null;

            string filename = "metadata.json";
            string hash = GetHash(url);
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (textAsset == null) return null;

            try
            {
                return JsonUtility.FromJson<YTDLVideoJson>(textAsset.text);
            }
            catch
            {
                return null;
            }
        }

        public static VideoMetadata GetMetadata(string url)
        {
            VideoMetadata metadata = new VideoMetadata();

            if (!ValidateUrl(url)) return metadata;

            string hash = GetHash(url);

            try
            {
                YTDLVideoJson? jsonOrNull = GetYTDLJson(url);
                if (jsonOrNull == null) return metadata;
                YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

                metadata.fps = json.fps;
                metadata.duration = json.duration_string;
                metadata.title = json.title;
                metadata.resolution = json.resolution;
                metadata.uploader = json.uploader;
                metadata.viewCount = 0;
                metadata.likeCount = 0;
                metadata.thumbnail = GetThumbnail(hash);
                metadata.siteName = json.extractor_key;
                metadata.description = json.description;
                metadata.duration = json.duration_string;

                int.TryParse(json.like_count, out metadata.likeCount);
                int.TryParse(json.view_count, out metadata.viewCount);
            } catch
            {
                return metadata;
            }

            return metadata;
        }

        private static void FetchThumbnail(string hash, YTDLVideoJson json)
        {
            if (!ValidateUrl(json.thumbnail)) return;

            WebRequest request = HttpWebRequest.Create(json.thumbnail);
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
                    {
                        UnityEngine.Debug.LogWarning($"The web server hosting {request.RequestUri.AbsolutePath} has responded with {(int)response.StatusCode}");
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
                UnityEngine.Debug.LogError(ex);
                UnityEngine.Debug.LogError($"Error while downloading thumbnail from {json.thumbnail.ToString()}, using fallback thumbnail");
                SetFallbackThumbnail(hash);
            }
        }

        private static void FetchMetadata(string hash, string url)
        {
            // ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J {url}", 10000);
            YTDLVideoJson? jsonOrNull = YTDLCommands.GetVideoMetadata(url);
            if (jsonOrNull == null) return;
            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
        }
    }
}
