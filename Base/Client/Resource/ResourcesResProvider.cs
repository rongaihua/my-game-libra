//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: Resources 资源后端
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// Resources 后端：真机没有资源包时用 <see cref="Resources.Load(string)"/>，
    /// 卸载走 <see cref="Resources.UnloadAsset"/>。
    /// </summary>
    public sealed class ResourcesResProvider : IResProvider
    {
        /// <summary>
        /// 后端名字。
        /// </summary>
        public string Name => "Resources";

        /// <summary>
        /// 资源是否存在。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>存在时为 true。</returns>
        public bool Exists(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return false;
            }

            var asset = Resources.Load(resName);
            return asset != null;
        }

        /// <summary>
        /// 加载资源。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>资源。加载不到时为空。</returns>
        public Object LoadAsset(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            return Resources.Load(resName);
        }

        /// <summary>
        /// 卸载资源。只对没有被实例化的资源有效。
        /// </summary>
        /// <param name="asset">资源。</param>
        public void UnloadAsset(Object asset)
        {
            if (!asset)
            {
                return;
            }

            Resources.UnloadAsset(asset);
        }

        /// <summary>
        /// 从文件加载资源包。
        /// </summary>
        /// <param name="bundleName">包名。</param>
        /// <returns>资源包。加载不到时为空。</returns>
        public AssetBundle LoadBundle(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                return null;
            }

            return AssetBundle.LoadFromFile(bundleName);
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
