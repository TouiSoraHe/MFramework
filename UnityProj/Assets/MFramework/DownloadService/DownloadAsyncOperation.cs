using MFramework.Common;
using MFramework.ScheduleService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MFramework.DownloadService
{
    public class DownloadAsyncOperation : CustomAsyncOperation
    {
        private Inner inner = null;

        public DownloadResponse DownloadResponse
        {
            get
            {
                return inner.DownloadResponse;
            }
        }

        public DownloadRequest DownloadRequest
        {
            get
            {
                return inner.DownloadRequest;
            }
        }

        public float DownloadSpeed
        {
            get
            {
                return inner.DownloadSpeed;
            }
        }

        public ulong DownloadedBytes
        {
            get
            {
                return inner.DownloadedBytes;
            }
        }

        protected override float OnProgress()
        {
            return inner.Progress;
        }

        public class Inner : CustomAsyncOperation
        {
            private UnityWebRequest webRequest;
            private DownloadResponse downloadResponse;
            private DownloadRequest downloadRequest;
            private float downloadSpeed;
            private ulong downloadedBytes;
            private DownloadAsyncOperation downloadAsyncOperation;

            private bool isSend = false;
            private float completedTime;

            public DownloadResponse DownloadResponse
            {
                get
                {
                    return downloadResponse;
                }
                private set
                {
                    downloadResponse = value;
                }
            }

            public DownloadRequest DownloadRequest
            {
                get
                {
                    return downloadRequest;
                }

                private set
                {
                    downloadRequest = value;
                }
            }

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

            public ulong DownloadedBytes
            {
                get
                {
                    if (!IsDone)
                    {
                        return webRequest.downloadedBytes;
                    }
                    return downloadedBytes;
                }

                private set
                {
                    downloadedBytes = value;
                }
            }

            public DownloadAsyncOperation DownloadAsyncOperation
            {
                get
                {
                    return downloadAsyncOperation;
                }
                private set
                {
                    downloadAsyncOperation = value;
                }
            }

            public Inner(UnityWebRequest unityWebRequest, DownloadRequest downloadRequest)
            {
                webRequest = unityWebRequest;
                DownloadAsyncOperation = new DownloadAsyncOperation();
                DownloadAsyncOperation.inner = this;
                DownloadRequest = downloadRequest;
            }

            public void Send()
            {
                if (!isSend)
                {
                    isSend = true;
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = webRequest.SendWebRequest();

                    unityWebRequestAsyncOperation.completed += OnCompleted;
                    unityWebRequestAsyncOperation.completed += CompletedInvoke;

                    DownloadService.GetInstance().StartCoroutine(DownloadEnumerator());
                }
            }

            private IEnumerator DownloadEnumerator()
            {
                float lastTime = Time.unscaledTime;
                float lastDownloadedBytes = DownloadedBytes;

                float downloadBytes;
                float time;
                while (!IsDone)
                {
                    downloadBytes = DownloadedBytes - lastDownloadedBytes;
                    time = Time.unscaledTime - lastTime;
                    DownloadSpeed = 0.0f;
                    if (time != 0)
                    {
                        DownloadSpeed = downloadBytes / time;
                    }
                    lastTime = Time.unscaledTime;
                    lastDownloadedBytes = DownloadedBytes;
                    yield return new WaitForSecondsRealtime(1.0f);
                }
                downloadBytes = DownloadedBytes - lastDownloadedBytes;
                time = completedTime - lastTime;
                if (time != 0)
                {
                    DownloadSpeed = downloadBytes / time;
                }
                yield return new WaitForSecondsRealtime(1.0f);
                DownloadSpeed = 0.0f;
            }

            protected void OnCompleted(AsyncOperation obj)
            {
                if (!webRequest.isDone)
                {
                    throw new System.Exception("意外的情况");
                }

                string error = null;
                string savePath = DownloadRequest.SavePath;
                byte[] data = null;
                if (!DownloadRequest.SaveToFile)
                {
                    data = webRequest.downloadHandler.data;
                }
                Dictionary<string, string> header = webRequest.GetResponseHeaders();

                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    error = webRequest.error;
                }
                else if (webRequest.responseCode >= 400)
                {
                    error = "HttpError,Response Code:" + webRequest.responseCode;
                }
                else
                {
                    if (MD5Verify())
                    {
                        if (DownloadRequest.SaveToFile)
                        {
                            if (!Common.Utility.MoveFile(DownloadRequest.FullTempPath, savePath))
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
                DownloadedBytes = webRequest.downloadedBytes;
                completedTime = Time.unscaledTime;

                DownloadResponse = new DownloadResponse(error, savePath, data, webRequest.responseCode, header);
                webRequest.Dispose();
                Common.Utility.DeleteFile(DownloadRequest.FullTempPath);
            }

            private bool MD5Verify()
            {
                if (!string.IsNullOrEmpty(DownloadRequest.Md5))
                {
                    string md5 = string.Empty;
                    if (DownloadRequest.SaveToFile)
                    {
                        md5 = Common.Utility.GetMD5HashByPath(DownloadRequest.FullTempPath);
                    }
                    else
                    {
                        md5 = Common.Utility.GetMD5Hash(webRequest.downloadHandler.data);
                    }
                    if (!DownloadRequest.Md5.Equals(md5))
                    {
                        return false;
                    }
                }
                return true;
            }

            private void CompletedInvoke(object obj)
            {
                CompletedInvoke();
                DownloadAsyncOperation.CompletedInvoke();
            }

            protected override float OnProgress()
            {
                if (IsDone)
                {
                    return 1.0f;
                }
                if (webRequest.downloadProgress < 0)
                {
                    return 0.0f;
                }
                return webRequest.downloadProgress;
            }
        }
    }
}
