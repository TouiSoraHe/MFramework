using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.DownloadService
{
    public class DownloadRequest
    {
        public enum HttpMethods
        {
            GET,
            HEAD,
            POST,
            PUT,
            CREATE,
            DELETE,
        }

        private HttpMethods httpMethod = HttpMethods.GET;
        private string url;
        private string saveDir;
        private string saveName;
        private int timeout = 0;
        private string md5;
        private Dictionary<string, string> header = new Dictionary<string, string>();

        public HttpMethods HttpMethod
        {
            get
            {
                return httpMethod;
            }

            set
            {
                httpMethod = value;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new System.Exception("DownloadRequest.Url 不能为空");
                }
                url = value;
            }
        }

        public string SaveDir
        {
            get
            {
                if (string.IsNullOrEmpty(saveDir))
                {
                    return string.Empty;
                }
                return saveDir;
            }

            set
            {
                saveDir = value;
            }
        }

        public string SaveName
        {
            get
            {
                if (string.IsNullOrEmpty(saveName))
                {
                    return string.Empty;
                }
                return saveName;
            }

            set
            {
                saveName = value;
            }
        }

        public bool SaveToFile
        {
            get
            {
                return !string.IsNullOrEmpty(SaveDir) && !string.IsNullOrEmpty(SaveName);
            }
        }

        public string SavePath
        {
            get
            {
                if (SaveToFile)
                {
                    return Utility.CombinePaths(SaveDir, SaveName);
                }
                return null;
            }
        }

        public string FullTempPath
        {
            get
            {
                return Utility.CombinePaths(Application.persistentDataPath, "DownloadRequest_DownloadTemp", Utility.GetMD5Hash(Url + SaveDir + SaveName));
            }
        }

        public int Timeout
        {
            get
            {
                return timeout;
            }

            set
            {
                if (timeout < 0)
                {
                    Log.LogE("DownloadRequest.Timeout 不能小于 0");
                    return;
                }
                timeout = value;
            }
        }

        public Dictionary<string, string> Header
        {
            get
            {
                return header;
            }

            private set
            {
                header = value;
            }
        }

        public string Md5
        {
            get
            {
                return md5;
            }

            set
            {
                md5 = value;
            }
        }

        public DownloadRequest(string url)
        {
            Url = url;
        }

        public DownloadRequest(string url, string saveDir, string saveName) : this(url)
        {
            if (string.IsNullOrEmpty(saveDir))
            {
                throw new System.Exception("DownloadRequest saveDir 不能为空");
            }
            if (string.IsNullOrEmpty(saveName))
            {
                throw new System.Exception("DownloadRequest saveName 不能为空");
            }
            SaveDir = saveDir;
            SaveName = saveName;
        }

        public DownloadRequest(string url, string saveDir, string saveName, string md5) : this(url, saveDir, saveName)
        {
            Md5 = md5;
        }

        public static string HttpMethodsToString(HttpMethods httpMethods)
        {
            return httpMethods.ToString();
        }
    }
}
