//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 弹窗基类
//---------------------------------------------------------------------------------------

namespace MyGame
{
    /// <summary>
    /// 弹窗基类。在 <see cref="UIView"/> 之上补了显示、隐藏和打开参数。
    /// 关掉并回收实例仍由 <see cref="UIMgr.Close"/> 负责。
    /// </summary>
    public abstract class UIWindow : UIView
    {
        /// <summary>
        /// 本次打开的参数。
        /// </summary>
        public object Args { get; private set; }

        /// <summary>
        /// 显示并回调 <see cref="UIView.OnOpen"/>。
        /// </summary>
        /// <param name="args">打开参数，可为空。</param>
        public void Show(object args = null)
        {
            Args = args;
            gameObject.SetActive(true);
            OnOpen(args);
        }

        /// <summary>
        /// 隐藏并回调 <see cref="UIView.OnClose"/>。
        /// </summary>
        public void Hide()
        {
            OnClose();
            gameObject.SetActive(false);
            Args = null;
        }

        /// <summary>
        /// 关闭并让界面管理器销毁实例。
        /// </summary>
        public void Close()
        {
            G.UIMgr?.Close(GetType());
        }
    }
}
