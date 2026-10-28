//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 场景资源
//---------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGame
{
    /// <summary>
    /// 场景资源。对照 ChatRoom 的 <c>ResScene</c>，只保留本地加载。
    /// </summary>
    public class ResScene : ResBase
    {
        /// <summary>
        /// 已加载的场景。
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// 加载方式。
        /// </summary>
        public LoadSceneMode Mode { get; private set; }

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="provider">加载后端。</param>
        /// <param name="mode">加载方式。</param>
        public ResScene(IResProvider provider, LoadSceneMode mode) : base(provider)
        {
            Mode = mode;
        }

        /// <summary>
        /// 场景没有资产对象。
        /// </summary>
        /// <returns>恒为空。</returns>
        public override Object GetAsset()
        {
            return null;
        }

        /// <summary>
        /// 场景是否已经加载。
        /// </summary>
        /// <returns>加载完成时为 true。</returns>
        public bool IsLoaded => Scene.IsValid() && Scene.isLoaded;

        /// <summary>
        /// 按资源名加载场景。名字可以是场景名，也可以是工程路径。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        internal override bool Load()
        {
            Status = EResStatus.Loading;

            var sceneName = ResName;
            if (sceneName != null && sceneName.EndsWith(".unity"))
            {
                sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
            }

            SceneManager.LoadScene(sceneName, Mode);
            Scene = Mode == LoadSceneMode.Single ? SceneManager.GetActiveScene() : SceneManager.GetSceneByName(sceneName);
            Status = Scene.IsValid() ? EResStatus.Loaded : EResStatus.Failed;
            return Scene.IsValid();
        }

        /// <summary>
        /// 卸载时把场景卸掉。
        /// </summary>
        protected override void OnDestroy()
        {
            if (Scene.IsValid() && Scene.isLoaded && SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(Scene);
                Logger.Debug($"UnloadScene: {ResName}");
            }

            base.OnDestroy();
        }
    }
}
