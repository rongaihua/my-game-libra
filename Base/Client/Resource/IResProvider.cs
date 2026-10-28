//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源加载后端接口
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 资源加载后端。编辑器读 AssetDatabase，真机读 AssetBundle / Resources，
    /// 上层缓存与引用计数不关心具体从哪来。
    /// </summary>
    public interface IResProvider
    {
        /// <summary>
        /// 后端名字，打日志用。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 资源是否存在。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>存在时为 true。</returns>
        bool Exists(string resName);

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="resName">资源名。</param>
        /// <returns>资源。加载不到时为空。</returns>
        Object LoadAsset(string resName);

        /// <summary>
        /// 卸载一个已经没人用的资源。
        /// </summary>
        /// <param name="asset">资源。</param>
        void UnloadAsset(Object asset);

        /// <summary>
        /// 加载一个资源包。
        /// </summary>
        /// <param name="bundleName">包名。</param>
        /// <returns>资源包。加载不到时为空。</returns>
        AssetBundle LoadBundle(string bundleName);

        /// <summary>
        /// 卸载资源包。
        /// </summary>
        /// <param name="bundle">资源包。</param>
        /// <param name="unloadAllObjects">是否连带卸载包内已实例化的对象。</param>
        void UnloadBundle(AssetBundle bundle, bool unloadAllObjects);
    }
}
