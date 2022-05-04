using System;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

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
            Debug.Log($"create folder {basePath} {name}");
            if (!basePath.StartsWith("Assets/")) return false;

            Debug.Log($"create folder - 1");
            if (AssetDatabase.IsValidFolder($"{basePath}/{name}")) return true;
            Debug.Log($"create folder - 2");
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                Debug.Log($"create folder - 3");
                string[] paths = basePath.Split('/');
                Debug.Log($"create folder - 4");
                CreateFolder(string.Join("/", paths.Take(paths.Length - 1)), paths[paths.Length - 1]);
                Debug.Log($"create folder - 5");
            }

            Debug.Log($"create folder - 6");
            if (AssetDatabase.CreateFolder(basePath, name) == "") return false;
            Debug.Log($"create folder returns");
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
            // importer.SaveAndReimport();
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
            // CreateFolder(EditorAssets.VideoMetadataFolder, hash);
            File.WriteAllBytes(path, bytes);
            // AssetDatabase.ImportAsset(path);
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

        private static Texture2D GetThumbnail(string hash)
        {
            string filename = "thumbnail.jpg";
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/{filename}";
            Texture2D result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            return result;
        }

        public static void RefreshMetadata(string url)
        {
            FetchMetadata(new string[] { url });
        }

        public static void RefreshMetadata(string[] urls)
        {
            FetchMetadata(urls);
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

        Queue<Task> postFetchTasks = new Queue<Task>();

        private static void FetchSubtitlesCallback(string url)
        {
            UnityEngine.Debug.Log("FetchSubtitlesCallback");

            UnityEngine.Debug.Log($"Subtitles downloaded for {url}");
        }

        private static void FetchSubtitles(string url, Action callback)
        {
            string hash = GetHash(url);

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";
            // if (!CreateFolder(path, "Subtitles")) return;

            YTDLCommands.DownloadSubtitles(url, $"{path}/Subtitles", false, (string stdout) => callback());
        }

        private static void FetchThumbnailCallback(string hash, byte[] data)
        {
            Debug.Log("FetchThumbnailCallback");

            if (string.IsNullOrEmpty(hash) || data == null) return;

            SetThumbnail(hash, data);
        }

        private static IEnumerator FetchThumbnail(YTDLVideoJson json, Action<byte[]> callback)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(json.thumbnail);
            yield return www.SendWebRequest();

            while (!www.isDone && !www.isNetworkError)
                yield return new WaitForSeconds(0.1f);

            if (www.isNetworkError)
            {
                Debug.LogError($"Network error while fetching thumbnail: {www.error}");
                callback(null);
            }
            else
            {
                callback(www.downloadHandler.data);
            }
        }

        private static void FetchMetadataCallback(string hash, YTDLVideoJson? jsonOrNull)
        {
            Debug.Log("FetchMetadataCallback");

            if (jsonOrNull == null)
            {
                Debug.Log("jsonOrNull is null");
                return;
            }

            Debug.Log("FetchMetadataCallback - 1");
            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            Debug.Log("FetchMetadataCallback - 2");
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            Debug.Log("FetchMetadataCallback - 3");
            
            Debug.Log("FetchMetadataCallback - 4");
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            Debug.Log("FetchMetadataCallback - 5");
            File.WriteAllBytes(path, bytes);
            Debug.Log("FetchMetadataCallback - 6");
            // AssetDatabase.ImportAsset(path);
        }

        private static void FetchMetadata(string[] urls)
        {
            try
            {
                EditorUtility.DisplayProgressBar($"Refreshing metadata...", "Dispatching threads...", 0);

                for (int i = 0; i < urls.Length; i++)
                {
                    string url = urls[i];
                    if (string.IsNullOrEmpty(url)) continue;

                    /* if (EditorUtility.DisplayCancelableProgressBar($"Refreshing metadata... ({i} of {urls.Length})", url, 1f * i / urls.Length))
                    {
                        break;
                    } */

                    if (!ValidateUrl(url)) continue;

                    string hash = GetHash(url);
                    CreateFolder($"{EditorAssets.VideoMetadataFolder}/{hash}", "Subtitles");

                    YTDLCommands.GetVideoMetadata(url, (YTDLVideoJson json) =>
                    {
                        Debug.Log($"{i} - 1");
                        FetchMetadataCallback(hash, json);
                        Debug.Log($"{i} - 1.1");

                        FetchThumbnail(json, (byte[] data) =>
                        {
                            Debug.Log($"{i} - 2");
                            FetchThumbnailCallback(hash, data);
                        });
                    });

                    FetchSubtitles(url, () =>
                    {
                        Debug.Log($"{i} - 3");
                        FetchSubtitlesCallback(url);
                        // EditorUtility.DisplayProgressBar($"Refreshed metadata {i} of {urls.Length}", url, 1f * i / urls.Length);
                    });

                    /* Task.Run(() =>
                    {
                        // return YTDLCommands.GetVideoMetadata(url);
                        // (result) => FetchMetadataCallback(url, result)
                    }); */
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError("Exception in multithreaded code");
            }
            finally
            {
                // EditorUtility.ClearProgressBar();
                AsyncProgress.Clear();
            }
        }
    }
}
