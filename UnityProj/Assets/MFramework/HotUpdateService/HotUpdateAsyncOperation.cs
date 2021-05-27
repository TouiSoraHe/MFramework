using MFramework.AssetService;
using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.DownloadService;
using System.Text;
using System.IO;
using MFramework.Config;
using MFramework.Build;

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
        private Dictionary<DownloadAsyncOperation, BuildConfig.FileInfo> fileDownloadInfo;
        private Dictionary<BuildConfig.FileInfo, string> needUpdateFile;
        private uint fileDownloadCompleteCnt;
        private string localBuildInfoPath;
        private BuildConfig.BuildInfo remoteBuildInfo;

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
            hotUpdateConfig = HotUpdateConfig.LoadConfig<HotUpdateConfig>();
            resBaseUrl = Utility.CombinePaths(hotUpdateConfig.UpdateUrl, Platform.GetCurBuildPlatformName());
            buildInfoUrl = Utility.CombinePaths(resBaseUrl, BuildConfig.BuildInfo.BuildInfoFileName);
            localBuildInfoPath = Utility.CombinePaths(PathMgr.Runtime_BuildFullPath, BuildConfig.BuildInfo.BuildInfoFileName);

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
            remoteBuildInfo = Utility.DeSerialize<BuildConfig.BuildInfo>(downloadResponseAsyncOperation.DownloadResponse.Data);
            BuildConfig.BuildInfo localBuildInfo = null;
            if (File.Exists(localBuildInfoPath))
            {
                var localdata = Utility.ReadFile(localBuildInfoPath);
                if (localdata != null)
                {
                    localBuildInfo = Utility.DeSerialize<BuildConfig.BuildInfo>(localdata);
                }
            }
            //这里将下载的bundle的MD5置空并备份，下载成功后又设置回来
            needUpdateFile = CompareBuildInfo(remoteBuildInfo, localBuildInfo);
            if (needUpdateFile == null)
            {
                Log.LogD("无需更新");
                Complete();
                yield break;
            }
            // 下载资源包
            fileDownloadInfo = new Dictionary<DownloadAsyncOperation, BuildConfig.FileInfo>();
            var remoteVersionBck = remoteBuildInfo.Version;
            remoteBuildInfo.Version--;
            foreach (var info in needUpdateFile)
            {
                var fileInfo = info.Key;
                var filePath = Utility.CombinePaths(PathMgr.Runtime_BuildFullPath, fileInfo.RelativePath);
                var fileDir = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);
                var url = Utility.CombinePaths(resBaseUrl, fileInfo.RelativePath);
                DownloadAsyncOperation downloadAsyncOperation = DownloadService.DownloadService.GetInstance()
                    .Download(url, fileDir, fileName, fileInfo.MD5);
                fileDownloadInfo.Add(downloadAsyncOperation, fileInfo);
                downloadAsyncOperation.Completed += BundleCompleted;
            }
            while (true)
            {
                progress = 0.0f;
                DownloadSpeed = 0.0f;
                foreach (var item in fileDownloadInfo)
                {
                    var responseAsyncOperation = item.Key;
                    progress += responseAsyncOperation.Progress;
                    DownloadSpeed += responseAsyncOperation.DownloadSpeed;
                }
                progress /= fileDownloadInfo.Count;
                if (fileDownloadCompleteCnt == fileDownloadInfo.Count)
                {
                    break;
                }
                yield return new WaitForSecondsRealtime(1.0f);
            }
            if (string.IsNullOrEmpty(Error))
            {
                remoteBuildInfo.Version = remoteVersionBck;
            }
            Utility.WriteFile(localBuildInfoPath, Utility.Serialize(remoteBuildInfo));
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

        private Dictionary<BuildConfig.FileInfo, string> CompareBuildInfo(BuildConfig.BuildInfo remote, BuildConfig.BuildInfo local)
        {
            if (local == null)
            {
                local = new BuildConfig.BuildInfo(remote.Version - 1);
            }
            if (remote.Version.Equals(local.Version))
            {
                return null;
            }
            Dictionary<BuildConfig.FileInfo, string> needUpdate = new Dictionary<BuildConfig.FileInfo, string>();
            foreach (var item in remote.FileInfos)
            {
                var remoteFileInfo = item.Value;
                if (!local.FileInfos.ContainsKey(remoteFileInfo.RelativePath))
                {
                    needUpdate.Add(remoteFileInfo, remoteFileInfo.MD5);
                    remoteFileInfo.MD5 = string.Empty;
                    continue;
                }
                if (!remoteFileInfo.MD5.Equals(local.FileInfos[remoteFileInfo.RelativePath].MD5))
                {
                    needUpdate.Add(remoteFileInfo, remoteFileInfo.MD5);
                    remoteFileInfo.MD5 = string.Empty;
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
                fileDownloadInfo[downloadAsyncOperation].MD5 = needUpdateFile[fileDownloadInfo[downloadAsyncOperation]];
            }
            fileDownloadCompleteCnt++;
            if (fileDownloadCompleteCnt % 10 == 0)
            {
                Utility.WriteFile(localBuildInfoPath, Utility.Serialize(remoteBuildInfo));
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
