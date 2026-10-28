//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 编辑器资源后端
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 编辑器资源后端：直接用工程路径读 AssetDatabase，不打资源包。
    /// </summary>
    public sealed class EditorResProvider : IResProvider
    {
        /// <summary>
        /// 后端名字。
        /// </summary>
        public string Name => "Editor";

        /// <summary>
        /// 资源是否存在。
        /// </summary>
        /// <param name="resName">资源路径。</param>
        /// <returns>存在时为 true。</returns>
        public bool Exists(string resName)
        {
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(resName) && UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(resName) != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// 按工程路径加载资源。
        /// </summary>
        /// <param name="resName">资源路径。</param>
        /// <returns>资源。加载不到时为空。</returns>
        public Object LoadAsset(string resName)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(resName);
#else
            return null;
#endif
        }

        /// <summary>
        /// 编辑器下的资源归 AssetDatabase 管，不主动卸载。
        /// </summary>
        /// <param name="asset">资源。</param>
        public void UnloadAsset(Object asset)
        {
        }

        /// <summary>
        /// 编辑器不加载资源包。
        /// </summary>
        /// <param name="bundleName">包名。</param>
        /// <returns>恒为空。</returns>
        public AssetBundle LoadBundle(string bundleName)
        {
            return null;
        }

        /// <summary>
        /// 卸载资源包。
        /// </summary>
        /// <param name="bundle">资源包。</param>
        /// <param name="unloadAllObjects">是否连带卸载包内对象。</param>
        public void UnloadBundle(AssetBundle bundle, bool unloadAllObjects)
        {
            if (bundle)
            {
                bundle.Unload(unloadAllObjects);
            }
        }
    }
}
