using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework.DownloadService
{
    public class DownloadResponseAsyncOperation : CustomAsyncOperation
    {
        private DownloadResponse downloadResponse;
        private UnityWebRequestAsyncOperation unityWebRequestAsyncOperation;
        private DownloadRequest downloadRequest;

        public DownloadResponse DownloadResponse
        {
            get
            {
                return downloadResponse;
            }
            protected set
            {
                downloadResponse = value;
            }
        }

        public DownloadResponseAsyncOperation(UnityWebRequestAsyncOperation unityWebRequestAsyncOperation, DownloadRequest downloadRequest)
        {
            this.unityWebRequestAsyncOperation = unityWebRequestAsyncOperation;
            this.downloadRequest = downloadRequest;

            this.unityWebRequestAsyncOperation.completed += OnCompleted;
            this.unityWebRequestAsyncOperation.completed += CompletedInvoke;
        }

        protected void OnCompleted(AsyncOperation obj)
        {
            if (!unityWebRequestAsyncOperation.webRequest.isDone)
            {
                throw new System.Exception("意外的情况");
            }

            string error = null;
            string savePath = downloadRequest.SavePath;
            byte[] data = null;
            if (!downloadRequest.SaveToFile)
            {
                data = unityWebRequestAsyncOperation.webRequest.downloadHandler.data;
            }
            Dictionary<string, string> header = unityWebRequestAsyncOperation.webRequest.GetResponseHeaders();

            if (!string.IsNullOrEmpty(unityWebRequestAsyncOperation.webRequest.error))
            {
                error = unityWebRequestAsyncOperation.webRequest.error;
            }
            else if (unityWebRequestAsyncOperation.webRequest.responseCode >= 400)
            {
                error = "HttpError,Response Code:" + unityWebRequestAsyncOperation.webRequest.responseCode;
            }
            else
            {
                if (MD5Verify())
                {
                    if (downloadRequest.SaveToFile)
                    {
                        if (!Common.Utility.MoveFile(downloadRequest.FullTempPath, savePath))
                        {
                            error = "拷贝文件到目标路径失败";
                        }
                    }
                }
                else
                {
                    error = "MD5校验失败";
                }
            }
            DownloadResponse = new DownloadResponse(error, savePath, data, unityWebRequestAsyncOperation.webRequest.responseCode, header);
            unityWebRequestAsyncOperation.webRequest.Dispose();
            Common.Utility.DeleteFile(downloadRequest.FullTempPath);
        }

        private bool MD5Verify()
        {
            if (!string.IsNullOrEmpty(downloadRequest.Md5))
            {
                string md5 = string.Empty;
                if (downloadRequest.SaveToFile)
                {
                    md5 = Common.Utility.GetMD5HashByPath(downloadRequest.FullTempPath);
                }
                else
                {
                    md5 = Common.Utility.GetMD5Hash(unityWebRequestAsyncOperation.webRequest.downloadHandler.data);
                }
                if (!downloadRequest.Md5.Equals(md5))
                {
                    return false;
                }
            }
            return true;
        }

        private void CompletedInvoke(object obj)
        {
            CompletedInvoke();
        }

        protected override float OnProgress()
        {
            throw new System.NotImplementedException();
        }
    }
}
