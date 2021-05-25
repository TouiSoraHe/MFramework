using MFramework.Common;
using MFramework.ScheduleService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework.DownloadService
{
    public class DownloadService : SingletonMB<DownloadService>
    {
        public static readonly uint MAX_DOWNLOAD_CNT = 1;
        private Dictionary<string, DownloadAsyncOperation> DownloadList = new Dictionary<string, DownloadAsyncOperation>();
        private Queue<DownloadAsyncOperation.Inner> waitDownload = new Queue<DownloadAsyncOperation.Inner>();
        private uint curDownloadingCnt = 0;

        public DownloadAsyncOperation Download(string url)
        {
            return Download(new DownloadRequest(url));
        }

        public DownloadAsyncOperation Download(string url, string saveDir, string saveName)
        {
            return Download(new DownloadRequest(url, saveDir, saveName));
        }

        public DownloadAsyncOperation Download(string url, string saveDir, string saveName, string md5)
        {
            return Download(new DownloadRequest(url, saveDir, saveName, md5));
        }

        public DownloadAsyncOperation Download(DownloadRequest downloadRequest)
        {
            if (DownloadList.ContainsKey(downloadRequest.FullTempPath))
            {
                Log.LogD("下载文件 {0} 已存在下载队列，直接返回", downloadRequest.Url);
                return DownloadList[downloadRequest.FullTempPath];
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
            DownloadAsyncOperation.Inner downloadAsyncOperationInner= new DownloadAsyncOperation.Inner(unityWebRequest, downloadRequest);
            DownloadList.Add(downloadRequest.FullTempPath, downloadAsyncOperationInner.DownloadAsyncOperation);
            if (curDownloadingCnt >= MAX_DOWNLOAD_CNT)
            {
                waitDownload.Enqueue(downloadAsyncOperationInner);
            }
            else
            {
                curDownloadingCnt++;
                downloadAsyncOperationInner.Send();
            }
            downloadAsyncOperationInner.Completed += DownloadAsyncOperationInner_Completed;
            return downloadAsyncOperationInner.DownloadAsyncOperation;
        }

        private void DownloadAsyncOperationInner_Completed(CustomAsyncOperation obj)
        {
            DownloadAsyncOperation.Inner downloadAsyncOperationInner = obj as DownloadAsyncOperation.Inner;
            DownloadList.Remove(downloadAsyncOperationInner.DownloadRequest.FullTempPath);
            curDownloadingCnt--;
            if (curDownloadingCnt < MAX_DOWNLOAD_CNT && waitDownload.Count > 0)
            {
                curDownloadingCnt++;
                waitDownload.Dequeue().Send();
            }
        }
    }
}
