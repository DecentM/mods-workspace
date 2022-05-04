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
using SubtitlesParser.Classes.Parsers;

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
            File.WriteAllBytes(path, bytes);
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

        private static void FetchCommentsCallback(string hash, YTDLVideoJson json)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/comments.json";
            string data = JsonUtility.ToJson(json);
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(data));
        }

        private static void FetchComments(string url, Action<YTDLVideoJson> callback)
        {
            string hash = GetHash(url);

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";

            YTDLCommands.GetMetadataWithComments(url, callback);
        }

        private static void FetchSubtitlesCallback(string hash)
        {
            IEnumerable<string> files = Directory
                .GetFiles(string.Join(Application.dataPath, $"Assets/Editor/DecentM/VideoMetadata/{hash}/Subtitles"), "*.*", SearchOption.TopDirectoryOnly)
                .Where(name => !name.EndsWith(".srt"));

            /* List<string> paths = files.ToList();

            for (int i = 0; i < paths.Count; i++)
            {
                var parser = new SubParser();
                using (var fileStream = File.OpenRead(paths[i]))
                {
                    var items = parser.ParseStream(fileStream);

                    var writer = new SubtitlesParser.Classes.Writers.SrtWriter();
                    using (var fileStream = File.OpenWrite(pathToSrtFile))
                    {
                        writer.WriteStream(fileStream, yourListOfSubtitleItems);
                    }
                }
            } */
        }

        private static void FetchSubtitles(string url, string hash, Action callback)
        {
            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}";

            YTDLCommands.DownloadSubtitles(url, $"{path}/Subtitles", false, (string stdout) => callback());
        }

        private static void FetchThumbnailCallback(string path, byte[] data)
        {
            if (string.IsNullOrEmpty(path) || data == null) return;

            File.WriteAllBytes(path, data);
        }

        private static void FetchThumbnail(YTDLVideoJson json, Action<byte[]> callback)
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
                        Debug.LogWarning($"The web server hosting {request.RequestUri.AbsolutePath} has responded with {(int)response.StatusCode}");
                        return;
                    }

                    Stream data = response.GetResponseStream();
                    MemoryStream ms = new MemoryStream();
                    data.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    callback(bytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError($"Error while downloading thumbnail from {json.thumbnail.ToString()}, using fallback thumbnail");
                callback(null);
            }
        }

        private static void FetchMetadataCallback(string hash, YTDLVideoJson? jsonOrNull)
        {
            if (jsonOrNull == null) return;

            YTDLVideoJson json = (YTDLVideoJson)jsonOrNull;

            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/metadata.json";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(json));
            File.WriteAllBytes(path, bytes);
            // AssetDatabase.ImportAsset(path);
        }

        private static void FetchMetadata(string[] urls)
        {
            try
            {
                for (int i = 0; i < urls.Length; i++)
                {
                    string url = urls[i];
                    if (string.IsNullOrEmpty(url)) continue;
                    if (!ValidateUrl(url)) continue;

                    EditorUtility.DisplayProgressBar("Creating folders...", url, 1f * i / urls.Length);

                    string hash = GetHash(url);
                    CreateFolder($"{EditorAssets.VideoMetadataFolder}/{hash}", "Subtitles");
                }

                EditorUtility.ClearProgressBar();

                for (int i = 0; i < urls.Length; i++)
                {
                    string url = urls[i];
                    if (string.IsNullOrEmpty(url)) continue;
                    if (!ValidateUrl(url)) continue;

                    string hash = GetHash(url);

                    YTDLCommands.GetMetadata(url, (YTDLVideoJson json) =>
                    {
                        FetchMetadataCallback(hash, json);

                        FetchThumbnail(json, (byte[] data) =>
                        {
                            string path = $"{EditorAssets.VideoMetadataFolder}/{hash}/thumbnail.jpg";
                            FetchThumbnailCallback(path, data);
                        });
                    });

                    FetchSubtitles(url, hash, () => FetchSubtitlesCallback(hash));
                    FetchComments(url, (YTDLVideoJson json) => FetchCommentsCallback(hash, json));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError("Exception in multithreaded code");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                // AsyncProgress.Clear();
            }
        }
    }
}
