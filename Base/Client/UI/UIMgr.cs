//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 把界面预制挂到 UI 根节点上
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 界面管理器。相机和根节点由入口绑定，打开的预制挂在根节点下。
    /// </summary>
    public sealed class UIMgr : Module
    {

        /// <summary>
        /// 一个已打开的界面。
        /// </summary>
        private sealed class Window
        {

            /// <summary>
            /// 界面实例。
            /// </summary>
            public UnityEngine.GameObject Instance;

            /// <summary>
            /// 预制的资源持有。
            /// </summary>
            public ResHandle Handle;
        }

        /// <summary>
        /// 已打开的界面，键是挂在预制上的组件类型。
        /// </summary>
        private readonly Dictionary<Type, Window> mWindows = new Dictionary<Type, Window>();

        /// <summary>
        /// UI 相机。
        /// </summary>
        public Camera UICamera { get; private set; }

        /// <summary>
        /// UI 根节点。
        /// </summary>
        public RectTransform UIRoot { get; private set; }

        /// <summary>
        /// 绑定入口上的 UI 相机和根节点，画布改成这台相机渲染。
        /// </summary>
        /// <param name="camera">UI 相机。</param>
        /// <param name="root">UI 根节点。</param>
        /// <returns>相机、根节点和画布都在时为 true。</returns>
        public bool Bind(Camera camera, RectTransform root)
        {
            if (camera == null || root == null)
            {
                return false;
            }

            var canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            UICamera = camera;
            UIRoot = root;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            return true;
        }

        /// <summary>
        /// 打开界面。同类型已经打开时先关掉。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <param name="prefabPath">预制路径。</param>
        /// <returns>实例上的组件。打不开时为空。</returns>
        public T Open<T>(string prefabPath) where T : Component
        {
            Close<T>();
            if (UIRoot == null)
            {
                return null;
            }

            var handle = App.Find<ResMgr>().Load(prefabPath);
            var prefab = handle == null ? null : handle.Asset as UnityEngine.GameObject;
            if (prefab == null)
            {
                App.Find<ResMgr>().Release(handle);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, UIRoot, false);
            var view = instance.GetComponent<T>();
            if (view == null)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                App.Find<ResMgr>().Release(handle);
                return null;
            }

            var window = new Window();
            window.Instance = instance;
            window.Handle = handle;
            mWindows.Add(typeof(T), window);
            return view;
        }

        /// <summary>
        /// 关掉一种界面，销毁实例并归还预制。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        public void Close<T>() where T : Component
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 关掉全部界面。
        /// </summary>
        public void CloseAll()
        {
            if (mWindows.Count == 0)
            {
                return;
            }

            var types = new Type[mWindows.Count];
            var index = 0;
            foreach (var pair in mWindows)
            {
                types[index] = pair.Key;
                index++;
            }

            for (var i = 0; i < types.Length; i++)
            {
                Close(types[i]);
            }
        }

        /// <summary>
        /// 销毁还开着的界面。资源缓存由资源管理器自己清空。
        /// </summary>
        protected override void OnDestroy()
        {
            CloseAll();
            UICamera = null;
            UIRoot = null;
        }

        /// <summary>
        /// 按类型关掉界面。
        /// </summary>
        /// <param name="type">预制上的组件类型。</param>
        private void Close(Type type)
        {
            Window window;
            if (!mWindows.TryGetValue(type, out window))
            {
                return;
            }

            mWindows.Remove(type);
            if (window.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(window.Instance);
            }

            var resources = App.Find<ResMgr>();
            if (resources != null)
            {
                resources.Release(window.Handle);
            }
        }
    }
}
