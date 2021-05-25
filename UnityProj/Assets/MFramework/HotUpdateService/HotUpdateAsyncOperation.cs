using MFramework.AssetService;
using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.DownloadService;
using System.Text;
using System.IO;

namespace MFramework.HotUpdateService
{
    public class HotUpdateAsyncOperation : CustomAsyncOperation
    {
        private float progress = 0.0f;
        private HotUpdateConfig hotUpdateConfig;
        private string resBaseUrl;
        private string buildInfoUrl;
        private float downloadSpeed = 0.0f;
        private string error;
        private Dictionary<DownloadAsyncOperation, BundleInfo> bundleDownloadInfo;
        private Dictionary<BundleInfo, string> needUpdateInfo;
        private uint bundleDownloadCompleteCnt;
        private string localBuildInfoPath;
        private BuildInfo remoteBuildInfo;

        public float DownloadSpeed
        {
            get
            {
                return downloadSpeed;
            }

            private set
            {
                downloadSpeed = value;
            }
        }

        public string Error
        {
            get
            {
                return error;
            }

            protected set
            {
                error = value;
            }
        }

        public HotUpdateAsyncOperation()
        {
            hotUpdateConfig = HotUpdateConfig.LoadConfig();
            resBaseUrl = Utility.CombinePaths(hotUpdateConfig.UpdateUrl, BuildConfig.GetBuildPlatformName(BuildConfig.GetCurrentPlatform()));
            buildInfoUrl = Utility.CombinePaths(resBaseUrl, BuildInfo.BuildInfoName);
            localBuildInfoPath = Utility.CombinePaths(BundleAssetLoader.BundleBasePath, BuildInfo.BuildInfoName);

            HotUpdateService.GetInstance().StartCoroutine(Start());
        }

        private IEnumerator Start()
        {
            if (!CheckAssetMode())
            {
                Log.LogD("本地加载资源，无需热更");
                Complete();
                yield break;
            }
            //下载版本信息
            DownloadAsyncOperation downloadResponseAsyncOperation = DownloadService.DownloadService.GetInstance().Download(buildInfoUrl);
            yield return downloadResponseAsyncOperation;
            if (!string.IsNullOrEmpty(downloadResponseAsyncOperation.DownloadResponse.Error))
            {
                Error = string.Format("资源版本信息下载失败,Error:{0},Url:{1}", downloadResponseAsyncOperation.DownloadResponse.Error, buildInfoUrl);
                Log.LogE(Error);
                Complete();
                yield break;
            }
            if (downloadResponseAsyncOperation.DownloadResponse.Data == null)
            {
                Error = string.Format("资源版本信息为空");
                Log.LogE(Error);
                Complete();
                yield break;
            }
            remoteBuildInfo = JsonUtility.FromJson<BuildInfo>(Encoding.UTF8.GetString(downloadResponseAsyncOperation.DownloadResponse.Data));
            BuildInfo localBuildInfo = null;
            if (File.Exists(localBuildInfoPath))
            {
                var localdata = Utility.ReadFile(localBuildInfoPath);
                if (localdata != null)
                {
                    localBuildInfo = JsonUtility.FromJson<BuildInfo>(Encoding.UTF8.GetString(localdata));
                }
            }
            //这里将下载的bundle的MD5置空并备份，下载成功后又设置回来
            needUpdateInfo = CompareBuildInfo(remoteBuildInfo, localBuildInfo);
            if (needUpdateInfo == null)
            {
                Log.LogD("无需更新");
                Complete();
                yield break;
            }
            // 下载资源包
            bundleDownloadInfo = new Dictionary<DownloadAsyncOperation, BundleInfo>();
            var remoteVersionBck = remoteBuildInfo.Version;
            remoteBuildInfo.Version--;
            foreach (var info in needUpdateInfo)
            {
                var bundleInfo = info.Key;
                var fileName = bundleInfo.BundleName + "." + BundleInfo.BundleExt;
                var url = Utility.CombinePaths(resBaseUrl, fileName);
                DownloadAsyncOperation downloadAsyncOperation = DownloadService.DownloadService.GetInstance()
                    .Download(url, BundleAssetLoader.BundleBasePath, fileName, bundleInfo.MD5);
                bundleDownloadInfo.Add(downloadAsyncOperation, bundleInfo);
                downloadAsyncOperation.Completed += BundleCompleted;
            }
            while (true)
            {
                progress = 0.0f;
                DownloadSpeed = 0.0f;
                foreach (var item in bundleDownloadInfo)
                {
                    var responseAsyncOperation = item.Key;
                    progress += responseAsyncOperation.Progress;
                    DownloadSpeed += responseAsyncOperation.DownloadSpeed;
                }
                progress /= bundleDownloadInfo.Count;
                if (bundleDownloadCompleteCnt == bundleDownloadInfo.Count)
                {
                    break;
                }
                yield return new WaitForSecondsRealtime(1.0f);
            }
            if (string.IsNullOrEmpty(Error))
            {
                remoteBuildInfo.Version = remoteVersionBck;
            }
            Utility.WriteFile(localBuildInfoPath, JsonUtility.ToJson(remoteBuildInfo));
            Complete();
        }

        /// <summary>
        /// 检查资源加载模式
        /// </summary>
        /// <returns>true:需要热更，false：不需要热更</returns>
        private bool CheckAssetMode()
        {
#if !UNITY_EDITOR || (USE_BUNDLE && !LOCAL_BUNDLE)
            return true;
#else
			return false;
#endif
        }

        private Dictionary<BundleInfo,string> CompareBuildInfo(BuildInfo remote, BuildInfo local)
        {
            if (local == null)
            {
                local = new BuildInfo();
            }
            if (remote.Version.Equals(local.Version))
            {
                return null;
            }
            var localBundleNameToInfo = new Dictionary<string, BundleInfo>();
            foreach (var item in local.BundleInfos)
            {
                localBundleNameToInfo.Add(item.BundleName, item);
            }
            Dictionary<BundleInfo, string> needUpdate = new Dictionary<BundleInfo, string>();
            foreach (var remoteBundleInfo in remote.BundleInfos)
            {
                if (!localBundleNameToInfo.ContainsKey(remoteBundleInfo.BundleName))
                {
                    needUpdate.Add(remoteBundleInfo, remoteBundleInfo.MD5);
                    remoteBundleInfo.MD5 = string.Empty;
                    continue;
                }
                if (!remoteBundleInfo.MD5.Equals(localBundleNameToInfo[remoteBundleInfo.BundleName].MD5))
                {
                    needUpdate.Add(remoteBundleInfo, remoteBundleInfo.MD5);
                    remoteBundleInfo.MD5 = string.Empty;
                }
            }
            return needUpdate;
        }

        private void BundleCompleted(CustomAsyncOperation o)
        {
            DownloadAsyncOperation downloadAsyncOperation = o as DownloadAsyncOperation;
            if (!string.IsNullOrEmpty(downloadAsyncOperation.DownloadResponse.Error))
            {
                var e = string.Format("资源包下载失败,Error:{0},Url:{1}", downloadAsyncOperation.DownloadResponse.Error, downloadAsyncOperation.DownloadRequest.Url);
                if (!string.IsNullOrEmpty(Error))
                {
                    Error = Error + "\n" + e;
                }
                Log.LogE(e);
            }
            else
            {
                bundleDownloadInfo[downloadAsyncOperation].MD5 = needUpdateInfo[bundleDownloadInfo[downloadAsyncOperation]];
            }
            bundleDownloadCompleteCnt++;
            if (bundleDownloadCompleteCnt % 10 == 0)
            {
                Utility.WriteFile(localBuildInfoPath, JsonUtility.ToJson(remoteBuildInfo));
            }
        }

        private void Complete()
        {
            progress = 1.0f;
            CompletedInvoke();
        }

        protected override float OnProgress()
        {
            return progress;
        }
    }
}
