//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 普通资源
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// 普通资源（预制、贴图、音频等），卸载时交给后端处理。
    /// </summary>
    public class ResObject : ResBase
    {
        /// <summary>
        /// 资源本体。
        /// </summary>
        public UnityEngine.Object Asset { get; private set; }

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="provider">加载后端。</param>
        public ResObject(IResProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// 取资源本体。
        /// </summary>
        /// <returns>资源。</returns>
        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }

        /// <summary>
        /// 粗估内存占用。
        /// </summary>
        /// <returns>字节数。</returns>
        public override long GetMemorySize()
        {
            return Asset ? 4096L : 0L;
        }

        /// <summary>
        /// 执行加载。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        internal override bool Load()
        {
            Status = EResStatus.Loading;
            Asset = Provider.LoadAsset(ResName);
            Status = Asset ? EResStatus.Loaded : EResStatus.Failed;
            return Asset;
        }

        /// <summary>
        /// 卸载。
        /// </summary>
        protected override void OnDestroy()
        {
            if (Asset)
            {
                Provider.UnloadAsset(Asset);
            }

            Asset = null;
            base.OnDestroy();
        }
    }
}
