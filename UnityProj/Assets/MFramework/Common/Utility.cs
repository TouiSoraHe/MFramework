using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace MFramework.Common
{
    public class Utility
    {
        public static readonly DateTime _localOrgTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

                return path;
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
    }
}
