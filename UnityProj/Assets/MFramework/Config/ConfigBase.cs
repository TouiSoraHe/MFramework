using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using MFramework.Common;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFramework.Config
{
    public abstract class ConfigBase
    {
        public static T LoadConfig<T>() where T : ConfigBase,new()
        {
            string path = GetConfigPath(typeof(T));
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                return new T();
            }
            return Utility.DeSerialize<T>(asset.bytes);
        }

        [Conditional("UNITY_EDITOR")]
        public void SaveConfig()
        {
#if UNITY_EDITOR
            if (IsSerialize())
            {
                byte[] bytes = Utility.Serialize(this);
                string path = Utility.CombinePaths(Application.dataPath, "Resources", GetConfigPath(this.GetType()) + ".txt");
                Utility.WriteFile(path, bytes, true);
                AssetDatabase.Refresh();
            }
#endif
        }

        protected virtual bool IsSerialize()
        {
            return true;
        }

        private static string GetConfigPath(Type type)
        {
            return "MFramework_Config/" + type.Name;
        }
    }
}
