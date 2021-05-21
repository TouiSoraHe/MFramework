using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.DownloadService
{
    public class DownloadResponse
    {
        private string error;
        private string filePath;
        private byte[] data;
        private long responseCode;
        private Dictionary<string, string> header;

        public string Error
        {
            get
            {
                return error;
            }

            private set
            {
                error = value;
            }
        }

        public string FilePath
        {
            get
            {
                return filePath;
            }

            private set
            {
                filePath = value;
            }
        }

        public byte[] Data
        {
            get
            {
                return data;
            }

            private set
            {
                data = value;
            }
        }

        public long ResponseCode
        {
            get
            {
                return responseCode;
            }

            private set
            {
                responseCode = value;
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

        public DownloadResponse(string error, string filePath, byte[] data, long responseCode, Dictionary<string, string> header)
        {
            this.Error = error;
            this.FilePath = filePath;
            this.Data = data;
            this.ResponseCode = responseCode;
            this.Header = header;
        }
    }
}
