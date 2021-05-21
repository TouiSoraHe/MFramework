using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework.DownloadService
{
    public class DownloadService : SingletonMB<DownloadService>
    {
        private Dictionary<string, DownloadResponseAsyncOperation> DownloadQueue = new Dictionary<string, DownloadResponseAsyncOperation>();

        public DownloadResponseAsyncOperation Download(string url)
        {
            return Download(new DownloadRequest(url));
        }

        public DownloadResponseAsyncOperation Download(string url, string saveDir, string saveName)
        {
            return Download(new DownloadRequest(url, saveDir, saveName));
        }

        public DownloadResponseAsyncOperation Download(string url, string saveDir, string saveName, string md5)
        {
            return Download(new DownloadRequest(url, saveDir, saveName, md5));
        }

        public DownloadResponseAsyncOperation Download(DownloadRequest downloadRequest)
        {
            if (DownloadQueue.ContainsKey(downloadRequest.FullTempPath))
            {
                Log.LogD("下载文件 {0} 已存在下载队列，直接返回", downloadRequest.Url);
                return DownloadQueue[downloadRequest.FullTempPath];
            }
            UnityWebRequest unityWebRequest = new UnityWebRequest();
            unityWebRequest.method = DownloadRequest.HttpMethodsToString(downloadRequest.HttpMethod);
            unityWebRequest.url = downloadRequest.Url;
            unityWebRequest.timeout = downloadRequest.Timeout;
            foreach (var item in downloadRequest.Header)
            {
                unityWebRequest.SetRequestHeader(item.Key, item.Value);
            }
            if (downloadRequest.SaveToFile)
            {
                //下载到文件
                unityWebRequest.downloadHandler = new DownloadHandlerFile(downloadRequest.FullTempPath);
            }
            else
            {
                //下载到内存
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            }
            DownloadResponseAsyncOperation downloadResponseAsyncOperation = new DownloadResponseAsyncOperation(unityWebRequest.SendWebRequest(), downloadRequest);
            DownloadQueue.Add(downloadRequest.FullTempPath, downloadResponseAsyncOperation);
            downloadResponseAsyncOperation.Completed += (o) => {
                DownloadQueue.Remove(downloadRequest.FullTempPath);
            };
            return downloadResponseAsyncOperation;
        }
    }
}
