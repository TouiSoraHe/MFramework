

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace MFramework.Common
{
    public class Log : Singleton<Log>
    {
        public enum Type
        {
            Debug,
            Warning,
            Error,
            None
        }

        private StringBuilder _stringBuilder;
        private StringBuilder _stringHelper;
        private StreamWriter _writer;

        public static Type LogLevel = Type.Debug;

        public Log()
        {
#if UNITY_EDITOR
                LogLevel = (Type)PlayerPrefs.GetInt("LogLevel", (int)LogLevel);
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR || LOG_ENABLED
                _stringBuilder = new StringBuilder(128);
                _stringHelper = new StringBuilder(128);
#endif
        }

        ~Log()
        {
            if (_writer != null)
            {
                try
                {
                    _writer.Flush();
                    _writer.BaseStream.Flush();
                    _writer.Close();
                    _writer = null;
                }
                catch
                {
                    // ignored
                }
            }

            _stringBuilder = null;
            _stringHelper = null;
        }

        public static void SetLogLevel(Log.Type type)
        {
#if UNITY_EDITOR
            PlayerPrefs.SetInt("LogLevel", (int)type);
#endif
            LogLevel = type;
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogD(string content)
        {
            GetInstance().OutputLog(Type.Debug, content);
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogD(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Debug, format, args);
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogW(string content)
        {
            GetInstance().OutputLog(Type.Warning, content);
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogW(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Warning, format, args);
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogE(string content)
        {
            GetInstance().OutputLog(Type.Error, content);
        }

        [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("LOG_ENABLED")]
        public static void LogE(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Error, format, args);
        }

        private void OutputLog(Type type, string content)
        {
            if (type < LogLevel)
            {
                return;
            }
            OutputConsole(type, content);
            OutputFile(type, content);
        }

        private void OutputLog(Type type, string format, params object[] args)
        {
            if (type < LogLevel)
            {
                return;
            }

            _stringHelper.Length = 0;
            _stringHelper.AppendFormat(format, args);
            OutputLog(type, _stringHelper.ToString());
        }

        private void OutputConsole(Type type, string content)
        {
            switch (type)
            {
                case Type.Debug:
                    UnityEngine.Debug.Log(content);
                    break;

                case Type.Warning:
                    UnityEngine.Debug.LogWarning(content);
                    break;

                case Type.Error:
                    UnityEngine.Debug.LogError(content);
                    break;
            }
        }

        private void OutputFile(Type type, string content)
        {
            if (_writer == null)
            {
                OpenFile();
            }

            if (_writer != null)
            {
                BuildLog(type);
                _stringBuilder.Append(content);

                try
                {
                    _writer.WriteLine(_stringBuilder.ToString());
                    _writer.Flush();
                    _writer.BaseStream.Flush();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void OpenFile()
        {
            try
            {
#if UNITY_EDITOR
                    string path = Path.GetFullPath(Application.dataPath + "/../../Log");
#else
                string path = Application.persistentDataPath + "/Log";
#endif
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] files = Directory.GetFiles(path);
                if (files.Length > 3)
                {
                    Array.Sort(files);
                    for (int i = 0; i < files.Length - 3; i++)
                    {
                        File.Delete(files[i]);
                    }
                }

                string logFile = string.Format("{0}/{1}.log", path, DateTime.Now.ToString("yyyyMMdd"));
                if (!File.Exists(logFile))
                {
                    FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate);
                    _writer = new StreamWriter(fs);
                }
                else
                {
                    var fs = new FileStream(logFile, FileMode.Append);
                    _writer = new StreamWriter(fs);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void BuildLog(Type type)
        {
            _stringBuilder.Length = 0;

            DateTime now = DateTime.Now;
            _stringBuilder.Append('[');

            AppendInt(_stringBuilder, now.Day, 2);
            _stringBuilder.Append(':');

            AppendInt(_stringBuilder, now.Hour, 2);
            _stringBuilder.Append(':');

            AppendInt(_stringBuilder, now.Minute, 2);
            _stringBuilder.Append(':');

            AppendInt(_stringBuilder, now.Second, 2);

            switch (type)
            {
                case Type.Debug:
                    _stringBuilder.Append("]D:");
                    break;

                case Type.Warning:
                    _stringBuilder.Append("]W:");
                    break;

                case Type.Error:
                    _stringBuilder.Append("]E:");
                    break;
            }
        }

        private static void AppendInt(StringBuilder s, int number, int digitCount)
        {
            if (number < 0)
            {
                s.Append('-');
                number = -number;
            }

            int length = s.Length;
            int num = 0;
            do
            {
                s.Append((char)(number % 10 + 48));
                number /= 10;
                num++;
            } while (number > 0);

            while (num++ < digitCount)
            {
                s.Append('0');
            }

            Reverse(s, length, s.Length - 1);
        }

        private static void Reverse(StringBuilder s, int firstIndex, int lastIndex)
        {
            while (firstIndex < lastIndex)
            {
                char value = s[firstIndex];
                s[firstIndex++] = s[lastIndex];
                s[lastIndex--] = value;
            }
        }
    }
}