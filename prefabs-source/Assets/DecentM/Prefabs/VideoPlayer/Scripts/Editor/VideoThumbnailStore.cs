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
    struct YTDLJson
    {
        public string duration_string;
        public string title;
        public string uploader;
        public string view_count;
        public string like_count;
        public string thumbnail;
        public string resolution;
        public int fps;
        public string original_url;
    }

    public struct VideoMetadata
    {
        public string duration;
        public string title;
        public string uploader;
        public string viewCount;
        public string likeCount;
        public Texture2D thumbnail;
        public string resolution;
        public int fps;
    }

    public class VideoMetadataStore
    {
        /*
        private static string GetVideoIdFromUrl(string url)
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
        } */

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

        /* private static Texture2D GetThumbnail(string url)
        {
            if (!ValidateUrl(url)) return EditorAssets.FallbackVideoThumbnail;

            string hash = GetHash(url);
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        } */

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

        private static void SetMetadata(string hash, string filename, byte[] bytes)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            ApplyThumbnailImportSettings(path);
        }

        private static void SetMetadata(string hash, Texture2D texture)
        {
            SetMetadata(hash, "thumbnail.jpg", texture.EncodeToJPG());
        }

        private static Texture2D GetThumbnail(string hash, string url)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (result == null)
            {
                FetchThumbnail(hash, url);
                result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            return result;
        }

        public static VideoMetadata GetMetadata(string url)
        {
            VideoMetadata metadata = new VideoMetadata();

            if (!ValidateUrl(url)) return metadata;

            string hash = GetHash(url);
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            
            if (textAsset == null)
            {
                FetchMetadata(hash, url);
                textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }

            if (textAsset == null) return metadata;

            try
            {
                YTDLJson json = JsonUtility.FromJson<YTDLJson>(textAsset.text);

                metadata.fps = json.fps;
                metadata.duration = json.duration_string;
                metadata.title = json.title;
                metadata.resolution = json.resolution;
                metadata.likeCount = json.like_count;
                metadata.uploader = json.uploader;
                metadata.viewCount = json.view_count;
                metadata.thumbnail = GetThumbnail(hash, json.thumbnail);
            } catch
            {
                return metadata;
            }

            return metadata;
        }

        private static void FetchThumbnail(string hash, string url)
        {
            if (!ValidateUrl(url)) return;

            WebRequest request = HttpWebRequest.Create(url);
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
                    SetMetadata(hash, "thumbnail.jpg", bytes);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                UnityEngine.Debug.LogError($"Error while downloading thumbnail for {url.ToString()}, using fallback thumbnail");
                SetFallbackThumbnail(hash);
            }
        }

        private static void FetchMetadata(string hash, string url)
        {
            ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J {url}", 10000);

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            byte[] bytes = Encoding.UTF8.GetBytes(ytdl.stdout);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
        }

        private static void SetFallbackThumbnail(string hash)
        {
            SetMetadata(hash, EditorAssets.FallbackVideoThumbnail);
        }

        /* private static void FetchThumbnail(string url)
        {
            if (url == null || url.ToString() == "") return;
            if (!ValidateUrl(url.ToString())) return;

            FetchMetadataJson(url.ToString());
            string videoId = GetVideoId(url.ToString());
            string hash = GetHash(url.ToString());

            if (videoId == null || videoId == "")
            {
                SetFallbackThumbnail(hash);
                return;
            }

            string thumbnailUrl = GetThumbnailUrl(videoId);
            FetchThumbnailRaw(hash, thumbnailUrl);
        } */

        /* public static void FetchMetadataJson(string url)
        {
            ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J {url}", 10000);
            string resolvedJson = ytdl.stdout;

            // If a URL fails to resolve, YTDL will send error to stderror and nothing will be output to stdout
            if (string.IsNullOrEmpty(resolvedJson)) return;

            YTDLJson metadata = JsonUtility.FromJson<YTDLJson>(resolvedJson);
            UnityEngine.Debug.Log($"Got some metadata: {metadata.title}");
        }

        public static VideoMetadata GetMetadata(string url)
        {


            VideoMetadata result = new VideoMetadata();

            ProcessResult ytdl = ProcessManager.RunProcess(EditorAssets.YtDlpPath, $"--no-check-certificate -J {url}", 10000);
            string resolvedJson = ytdl.stdout;

            // If a URL fails to resolve, YTDL will send error to stderror and nothing will be output to stdout
            if (string.IsNullOrEmpty(resolvedJson)) return result;

            YTDLJson metadata = JsonUtility.FromJson<YTDLJson>(resolvedJson);
        } */
    }
}
