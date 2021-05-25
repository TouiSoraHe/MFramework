using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFramework.Common
{
    public abstract class ConfigBase<T> : ScriptableObject where T : ConfigBase<T>
    {
        public static T LoadConfig()
        {
            string path = GetConfigPath();
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                asset = CreateInstance<T>();
#if UNITY_EDITOR
                Utility.CreateDirectory(System.IO.Path.GetDirectoryName(Utility.CombinePaths(Application.dataPath, "Resources", GetConfigPath())));
                AssetDatabase.CreateAsset(asset, Utility.CombinePaths("Assets/Resources", GetConfigPath(), ".asset"));
#endif
            }
            return asset;
        }

        [Conditional("UNITY_EDITOR")]
        protected void SaveConfig()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        private static string GetConfigPath()
        {
            return "A_Config_MFramework/" + typeof(T).Name;
        }
    }
}
