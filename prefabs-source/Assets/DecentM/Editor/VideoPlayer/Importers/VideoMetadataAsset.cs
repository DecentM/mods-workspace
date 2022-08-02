using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DecentM.EditorTools;

namespace DecentM.VideoPlayer.EditorTools.Importers
{
    public sealed class VideoMetadataAsset : ScriptableObject
    {
        public static new VideoMetadataAsset CreateInstance(string input)
        {
            VideoMetadataAsset asset = ScriptableObject.CreateInstance<VideoMetadataAsset>();
            asset.SetMetadata(JsonUtility.FromJson<YTDLVideoJson>(input));

            return asset;
        }

        [SerializeField]
        private YTDLVideoJson _metadata;

        private void SetMetadata(YTDLVideoJson metadata)
        {
            this._metadata = metadata;
        }

        public YTDLVideoJson? metadata
        {
            get { return this._metadata; }
        }
    }
}
