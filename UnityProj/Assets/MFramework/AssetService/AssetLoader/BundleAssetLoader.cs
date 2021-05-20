using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.Common;
using System.IO;
using System;

namespace MFramework.AssetService
{
    public class BundleAssetLoader : AssetLoader
    {
#if !UNITY_EDITOR || !LOCAL_BUNDLE
        public static readonly string bundleBasePath = Application.persistentDataPath + "/Build/" + BuildConfig.GetBuildPlatformName(BuildConfig.GetCurrentPlatform());
#else
        public static readonly string bundleBasePath = Application.dataPath + "/../../Build/" + BuildConfig.GetBuildPlatformName(BuildConfig.GetCurrentPlatform());
#endif

        private BuildInfo buildInfo;
        private Dictionary<string, BundleInfo> nameToBundleInfo = new Dictionary<string, BundleInfo>();
        private Dictionary<string, ResInfo> pathToResInfo = new Dictionary<string, ResInfo>();
        private Dictionary<string, List<BundleAsset>> pathToBundleAsset = new Dictionary<string, List<BundleAsset>>();
        private Dictionary<string, BundleCreateAssetRequest> BundleSyncQueue = new Dictionary<string, BundleCreateAssetRequest>();
        private Dictionary<string, BundleAssetRequest> AssetSyncQueue = new Dictionary<string, BundleAssetRequest>();

        public BundleAssetLoader()
        {
            AssetBase.AssetManager.AssetUnload += AssetManagerAssetUnload;
            string buildInfoPath = Utility.CombinePaths(bundleBasePath, BuildInfo.BuildInfoName);
            if (File.Exists(buildInfoPath))
            {
                buildInfo = JsonUtility.FromJson<BuildInfo>(System.Text.Encoding.UTF8.GetString(Utility.ReadFile(buildInfoPath)));
                foreach (var bundleInfo in buildInfo.BundleInfos)
                {
                    nameToBundleInfo.Add(bundleInfo.BundleName, bundleInfo);
                    foreach (var resInfo in bundleInfo.ResInfos)
                    {
                        pathToResInfo.Add(resInfo.Path, resInfo);
                    }
                }
            }
            else
            {
                throw new System.Exception("BundleAssetLoader,file:" + buildInfoPath + " not found");
            }
        }

        private void AssetManagerAssetUnload(AssetBase obj)
        {
            if (pathToBundleAsset.ContainsKey(obj.ResPath))
            {
                foreach (BundleAsset item in pathToBundleAsset[obj.ResPath])
                {
                    item.Unload();
                }
                pathToBundleAsset.Remove(obj.ResPath);
            }
        }

        public override AssetBase LoadAsset(string path)
        {
            UnityAsset unityAsset = AssetBase.AssetManager.TryCopy<UnityAsset>(path);
            if (unityAsset != null) return unityAsset;
            string bundleName = BuildConfig.GetBundleName(path);
            List<string> allBundleName = GetAllDependencyBundleName(path);
            BundleAsset mainBundleAsset = LoadBundleAsset(bundleName);
            List<BundleAsset> bundleAssets = LoadAllBundleAsset(allBundleName);
            if (mainBundleAsset != null)
            {
                AssetBundle assetBundle = mainBundleAsset.Data as AssetBundle;
                UnityEngine.Object data = assetBundle.LoadAsset(path);
                bundleAssets.Add(mainBundleAsset);
                pathToBundleAsset.Add(path, bundleAssets);
                return AssetBase.AssetManager.Create<UnityAsset>(path, data);
            }
            else
            {
                foreach (var item in bundleAssets)
                {
                    item.Unload();
                }
            }
            return null;
        }

        public override AssetRequest LoadAssetAsync(string path)
        {
            UnityAsset unityAsset = AssetBase.AssetManager.TryCopy<UnityAsset>(path);
            if (unityAsset != null)
            {
                return new BundleAssetRequest(path, unityAsset);
            }
            if (AssetSyncQueue.ContainsKey(path))
            {
                Log.LogD("BundleAssetLoader.LoadAssetAsync:加载队列中已存在，直接返回");
                return AssetSyncQueue[path].Clone();
            }
            string mainBundleName = BuildConfig.GetBundleName(path);
            List<string> allBundleName = GetAllDependencyBundleName(path);
            BundleCreateAssetRequest mainBundleCreateAssetRequest = LoadBundleAssetSync(mainBundleName);
            List<BundleCreateAssetRequest> bundleCreateAssetRequests = LoadAllBundleAssetSync(allBundleName);
            if (mainBundleCreateAssetRequest != null)
            {
                List<BundleAsset> bundleAssets = new List<BundleAsset>();
                int loadingCount = 1 + bundleCreateAssetRequests.Count;
                Action<CustomAsyncOperation> complete = (obj) => 
                {
                    loadingCount--;
                    BundleCreateAssetRequest bundleCreateAsset = obj as BundleCreateAssetRequest;
                    if (bundleCreateAsset != null && bundleCreateAsset.BundleAsset != null)
                    {
                        bundleAssets.Add(bundleCreateAsset.BundleAsset);
                    }
                    if (loadingCount == 0)
                    {
                        pathToBundleAsset.Add(path, bundleAssets);
                    }
                };
                mainBundleCreateAssetRequest.Completed += complete;
                foreach (var item in bundleCreateAssetRequests)
                {
                    item.Completed += complete;
                }
                BundleAssetRequest bundleAssetRequest = new BundleAssetRequest(path, mainBundleCreateAssetRequest, bundleCreateAssetRequests);
                AssetSyncQueue.Add(path, bundleAssetRequest);
                bundleAssetRequest.Completed += (o) => {
                    AssetSyncQueue.Remove(path);
                };
                return bundleAssetRequest;
            }
            else
            {
                foreach (var item in bundleCreateAssetRequests)
                {
                    item.Completed += (obj) =>
                    {
                        if (item.BundleAsset != null)
                        {
                            item.BundleAsset.Unload();
                        }
                    };
                }
                //这里需要停止资源加载
            }
            return null;
        }

        private static string GetBundleFullPath(string bundleName)
        {
            return Utility.CombinePaths(bundleBasePath, bundleName + "." + BundleInfo.BundleExt);
        }

        private List<string> GetAllDependencyBundleName(string path)
        {
            List<string> bundlePaths = new List<string>();
            ResInfo resInfo;
            if (pathToResInfo.TryGetValue(path, out resInfo))
            {
                bundlePaths.AddRange(resInfo.DependencyBundleName);
            }
            else
            {
                Log.LogE("can not get res dependency info,{0}", path);
            }
            return bundlePaths;
        }

        private List<BundleAsset> LoadAllBundleAsset(List<string> bundleNames)
        {
            List<BundleAsset> allBundleAsset = new List<BundleAsset>();
            foreach (var bundleName in bundleNames)
            {
                BundleAsset bundleAsset = LoadBundleAsset(bundleName);
                if (bundleAsset != null)
                {
                    allBundleAsset.Add(bundleAsset);
                }
            }
            return allBundleAsset;
        }

        private BundleAsset LoadBundleAsset(string bundleName)
        {
            BundleAsset bundleAsset = AssetBase.AssetManager.TryCopy<BundleAsset>(bundleName);
            if (bundleAsset != null) return bundleAsset;
            var bundleFullPath = GetBundleFullPath(bundleName);
            if (File.Exists(bundleFullPath))
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleFullPath);
                bundleAsset = AssetBase.AssetManager.Create<BundleAsset>(bundleName, assetBundle);
                return bundleAsset;
            }
            else
            {
                Log.LogE("bundle file not exits,{0}", bundleFullPath);
            }
            return null;
        }


        private List<BundleCreateAssetRequest> LoadAllBundleAssetSync(List<string> bundleNames)
        {
            List<BundleCreateAssetRequest> bundleCreateAssetRequests = new List<BundleCreateAssetRequest>();
            foreach (var bundleName in bundleNames)
            {
                BundleCreateAssetRequest bundleCreateAssetRequest = LoadBundleAssetSync(bundleName);
                if (bundleCreateAssetRequest != null)
                {
                    bundleCreateAssetRequests.Add(bundleCreateAssetRequest);
                }
            }
            return bundleCreateAssetRequests;
        }

        private BundleCreateAssetRequest LoadBundleAssetSync(string bundleName)
        {
            BundleAsset bundleAsset = AssetBase.AssetManager.TryCopy<BundleAsset>(bundleName);
            if (bundleAsset != null) return new BundleCreateAssetRequest(bundleName, bundleAsset);
            if (BundleSyncQueue.ContainsKey(bundleName))
            {
                Log.LogD("BundleAssetLoader.LoadBundleAssetSync:加载队列中已存在，直接返回");
                return BundleSyncQueue[bundleName].Clone() as BundleCreateAssetRequest;
            }
            var bundleFullPath = GetBundleFullPath(bundleName);
            if (File.Exists(bundleFullPath))
            {
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(bundleFullPath);
                BundleCreateAssetRequest bundleCreateAssetRequest = new BundleCreateAssetRequest(bundleName, assetBundleCreateRequest);
                BundleSyncQueue.Add(bundleName, bundleCreateAssetRequest);
                bundleCreateAssetRequest.Completed += (o) => {
                    BundleSyncQueue.Remove(bundleName);
                };
                return bundleCreateAssetRequest;
            }
            else
            {
                Log.LogE("bundle file not exits,{0}", bundleFullPath);
            }
            return null;
        }
    }
}
