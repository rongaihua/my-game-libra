//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 资源状态
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// 资源状态。对照 ChatRoom 的 <c>EStatus</c>。
    /// </summary>
    public enum EResStatus
    {
        /// <summary>
        /// 尚未加载。
        /// </summary>
        None = 0,

        /// <summary>
        /// 加载中。
        /// </summary>
        Loading = 1,

        /// <summary>
        /// 已加载。
        /// </summary>
        Loaded = 2,

        /// <summary>
        /// 加载失败。
        /// </summary>
        Failed = 3,

        /// <summary>
        /// 已释放。
        /// </summary>
        Released = 4,
    }
}
