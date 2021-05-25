using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFramework.Common
{
    public class Utility
    {
        public static readonly DateTime _localOrgTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

#if UNITY_EDITOR
        [Conditional("UNITY_EDITOR")]
        public static void AddDefineSymbol(BuildTargetGroup buildTargetGroup, params string[] defineSymbols)
        {
            SetDefineSymbol(buildTargetGroup, true, defineSymbols);
        }

        [Conditional("UNITY_EDITOR")]
        public static void RemoveDefineSymbol(BuildTargetGroup buildTargetGroup, params string[] defineSymbols)
        {
            SetDefineSymbol(buildTargetGroup, false, defineSymbols);
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetDefineSymbol(BuildTargetGroup buildTargetGroup, bool isAdd, params string[] defineSymbols)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            HashSet<string> symbol = new HashSet<string>(symbolStr.Split(';'));
            foreach (var item in defineSymbols)
            {
                if (isAdd)
                {
                    symbol.Add(item);
                }
                else
                {
                    symbol.Remove(item);
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", new List<string>(symbol).ToArray()));
        }
#endif

        public static string GetMD5Hash(string str)
        {
            return GetMD5Hash(System.Text.Encoding.UTF8.GetBytes(str));
        }

        public static string GetMD5HashByPath(string path)
        {
            if (File.Exists(path))
            {
                string md5 = string.Empty;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    md5 = GetMD5Hash(fs);
                }
                return md5;
            }
            else
            {
                Log.LogE("Utility.GetMD5HashByPath:文件不存在,path:{0}", path);
                return string.Empty;
            }
        }

        public static string GetMD5Hash(byte[] _bytes)
        {
            byte[] hashData = null;
            using (var md5Hash = System.Security.Cryptography.MD5.Create())
            {
                hashData = md5Hash.ComputeHash(_bytes);
            }
            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            System.Text.StringBuilder stringBuider = new System.Text.StringBuilder();
            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            foreach (byte b in hashData)
            {
                stringBuider.Append(b.ToString("x2"));
            }
            // Return the hexadecimal string. 
            return stringBuider.ToString();
        }

        public static string GetMD5Hash(System.IO.Stream _stream)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] data = md5Hasher.ComputeHash(_stream);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            System.Text.StringBuilder stringBuider = new System.Text.StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                stringBuider.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return stringBuider.ToString();
        }

        public static string CombinePaths(params string[] values)
        {
            if (values.Length <= 0)
            {
                return string.Empty;
            }
            else if (values.Length == 1)
            {
                return values[0];
            }
            else if (values.Length > 1)
            {
                string path = Path.Combine(values[0], values[1]);

                for (int i = 2; i < values.Length; i++)
                {
                    path = Path.Combine(path, values[i]);
                }

                return path.Replace("\\","/");
            }
            return string.Empty;
        }

        /// <summary>
        /// 返回自1970年1月1日 00:00:00 GMT 的时间戳
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static long GetTimeTicksSecond(DateTime time)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(_localOrgTimeUtc);
            TimeSpan toNow = time.Subtract(dtStart);
            return (long)(toNow.TotalSeconds);
        }

        /// <summary>
        /// 返回自1970年1月1日 00:00:00 GMT 的时间戳
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static long GetTimeTicksSecond()
        {
            return GetTimeTicksSecond(DateTime.Now);
        }

        //----------------------------------------------
        /// 写入文件
        /// @filePath
        /// @data
        //----------------------------------------------
        public static bool WriteFile(string filePath, byte[] data, bool createDirectory = false)
        {
            int tryCount = 0;

            if (createDirectory)
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception e)
                    {
                        Log.LogE("Create folder " + directory + " failed. Exception: " + e.ToString());
                        return false;
                    }
                }
            }

            while (true)
            {
                try
                {
                    System.IO.File.WriteAllBytes(filePath, data);
                    return true;
                }
                catch (System.Exception ex)
                {
                    tryCount++;

                    if (tryCount >= 3)
                    {
                        Log.LogE("Write File " + filePath + " Error! Exception = " + ex.ToString());

                        //这里应该删除文件以防止数据错误
                        DeleteFile(filePath);

                        return false;
                    }
                }
            }
        }

        public static bool WriteFile(string filePath, string data, bool createDirectory = false)
        {
            return WriteFile(filePath, System.Text.Encoding.UTF8.GetBytes(data), createDirectory);
        }

        //----------------------------------------------
        /// 读取文件
        /// @filePath
        //----------------------------------------------
        public static byte[] ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.LogE("Read File " + filePath + " Is Not Exist");
                return null;
            }

            byte[] data = null;
            int tryCount = 0;

            while (true)
            {
                try
                {
                    data = System.IO.File.ReadAllBytes(filePath);
                }
                catch (System.Exception ex)
                {
                    Log.LogE("Read File " + filePath + " Error! Exception = " + ex.ToString() + ", TryCount = " + tryCount);
                    data = null;
                }

                if (data == null || data.Length <= 0)
                {
                    tryCount++;

                    if (tryCount >= 3)
                    {
                        Log.LogE("Read File " + filePath + " Fail!, TryCount = " + tryCount);

                        return null;
                    }
                }
                else
                {
                    return data;
                }
            }
        }

        //----------------------------------------------
        /// 删除文件
        /// @filePath
        //----------------------------------------------
        public static bool DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return true;
            }

            int tryCount = 0;

            while (true)
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    return true;
                }
                catch (System.Exception ex)
                {
                    tryCount++;
                    if (tryCount >= 3)
                    {
                        Log.LogE("Delete File " + filePath + " Error! Exception = " + ex.ToString());
                        return false;
                    }
                }
            }
        }

        //----------------------------------------------
        /// 删除目录
        /// @directory
        //----------------------------------------------
        public static bool DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return true;
            }

            int tryCount = 0;

            while (true)
            {
                try
                {
                    System.IO.Directory.Delete(directory, true);
                    return true;
                }
                catch (System.Exception ex)
                {
                    tryCount++;

                    if (tryCount >= 3)
                    {
                        Log.LogE("Delete Directory " + directory + " Error! Exception = " + ex.ToString());

                        return false;
                    }
                }
            }
        }

        public static bool MoveFile(string path,string destPath, bool overwrite = true)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            int tryCount = 0;

            while (true)
            {
                try
                {
                    if (overwrite && File.Exists(destPath))
                    {
                        DeleteFile(destPath);
                    }
                    System.IO.File.Move(path, destPath);
                    return true;
                }
                catch (System.Exception ex)
                {
                    tryCount++;

                    if (tryCount >= 3)
                    {
                        Log.LogE("MoveFile {0} To {1} Error! Exception = {2}",path, destPath, ex.ToString());
                        return false;
                    }
                }
            }
        }

        //----------------------------------------------
        /// 创建目录
        /// @directory
        //----------------------------------------------
        public static bool CreateDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                return true;
            }

            int tryCount = 0;

            while (true)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                    return true;
                }
                catch (Exception ex)
                {
                    tryCount++;

                    if (tryCount >= 3)
                    {
                        Log.LogE("Create Directory " + directory + " Error! Exception = " + ex.ToString());

                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 获取系统剪切板内容
        /// </summary>
        /// <returns></returns>
        public static string GetSystemCopyBuffer()
        {
            return UniClipboard.GetText();
        }

        /// <summary>
        /// 设置系统剪切板内容
        /// </summary>
        /// <param name="str"></param>
        public static void SetSystemCopyBuffer(string str)
        {
            UniClipboard.SetText(str);
        }

        public static string GetFilePathWithoutExtension(string v)
        {
            string parentPath = Path.GetDirectoryName(v);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(v);
            return CombinePaths(parentPath, fileNameWithoutExtension);
        }
    }
}
