using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFramework.Common
{
    public class ConfigBase<T> : ScriptableObject where T : ConfigBase<T>
    {
        public static T LoadAssetAtPath(string path)
        {
#if UNITY_EDITOR
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
#else
        return null;
#endif
        }

        [Conditional("UNITY_EDITOR")]
        protected void SaveAsset()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}
