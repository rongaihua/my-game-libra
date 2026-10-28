//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 子节点自动绑定标记
//---------------------------------------------------------------------------------------

using System;

namespace MyGame
{
    /// <summary>
    /// 标记界面上的子节点引用，<see cref="UIBinder"/> 在 Awake 时按路径自动赋值。
    /// 路径写相对界面根节点的层级，用 <c>/</c> 分隔，例如 <c>Top/BtnClose</c>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class UIBindAttribute : Attribute
    {
        /// <summary>
        /// 相对界面根节点的节点路径。
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 构造。
        /// </summary>
        /// <param name="path">相对界面根节点的节点路径。</param>
        public UIBindAttribute(string path)
        {
            Path = path;
        }
    }
}
