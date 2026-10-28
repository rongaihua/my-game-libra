//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: UI 层级
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// UI 层级。层级枚举值同时用作根节点的兄弟序号，越大越靠前显示。
    /// </summary>
    public enum EUILayer
    {
        /// <summary>
        /// 场景内嵌界面，压在最底下。
        /// </summary>
        Scene = 0,

        /// <summary>
        /// 主界面窗口。
        /// </summary>
        Window = 1,

        /// <summary>
        /// 弹窗。
        /// </summary>
        Popup = 2,

        /// <summary>
        /// 提示、飘字。
        /// </summary>
        Tip = 3,

        /// <summary>
        /// 顶层遮挡，比如加载遮罩。
        /// </summary>
        Top = 4,
    }
}
