using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class LocalAssetLoader : AssetLoader
    {
        private Dictionary<string, LocalAssetRequest> SyncQueue = new Dictionary<string, LocalAssetRequest>();

        public override AssetBase LoadAsset(string path)
        {
            AssetBase asset = AssetBase.AssetManager.TryCopy<UnityAsset>(path);
            if (asset != null) return asset;
            UnityEngine.Object data = null;
#if UNITY_EDITOR
            data = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
#endif
            if (data == null)
            {
                Log.LogE("资源加载失败,path:{0}", path);
                return null;
            }
            return AssetBase.AssetManager.Create<UnityAsset>(path, data);
        }

        public override AssetRequest LoadAssetAsync(string path)
        {
            UnityAsset asset = AssetBase.AssetManager.TryCopy<UnityAsset>(path);
            if (asset != null) return new LocalAssetRequest(path, asset);
            if (SyncQueue.ContainsKey(path))
            {
                Log.LogD("LocalAssetLoader.LoadAssetAsync:加载队列中已存在，直接返回");
                return SyncQueue[path].Clone();
            }
            LocalAssetRequest assetRequest = new LocalAssetRequest(path, LoadAsset(path) as UnityAsset);
            SyncQueue.Add(path, assetRequest);
            assetRequest.Completed += (o) => {
                SyncQueue.Remove(path);
            };
            return assetRequest;
        }
    }
}
