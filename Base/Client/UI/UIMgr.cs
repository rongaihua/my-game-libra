//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 界面管理器（分层 + 缓存 + 开闭动画钩子）
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 界面管理器。相机和根节点由入口绑定，打开的预制挂在对应层级的根节点下。
    /// 关闭的界面默认进缓存，下次打开复用实例，不再重复实例化和加载。
    /// </summary>
    public sealed class UIMgr : Module
    {
        /// <summary>
        /// 一个界面实例的全部信息。
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

            /// <summary>
            /// 所在层级。
            /// </summary>
            public EUILayer Layer;

            /// <summary>
            /// 打开的预制路径，复用时用来核对是不是同一个预制。
            /// </summary>
            public string PrefabPath;
        }

        /// <summary>
        /// 关闭动画结束后的回调。用具名类而不是闭包，避免捕获带来的生命周期问题。
        /// </summary>
        private sealed class CloseCallback
        {
            /// <summary>
            /// 界面管理器。
            /// </summary>
            public UIMgr Manager;

            /// <summary>
            /// 要关闭的窗口。
            /// </summary>
            public Window Window;

            /// <summary>
            /// 窗口类型，回收进缓存时当键用。
            /// </summary>
            public Type WindowType;

            /// <summary>
            /// 关掉后是否直接销毁，不进缓存。
            /// </summary>
            public bool DestroyImmediately;

            /// <summary>
            /// 动画结束后由界面调用。
            /// </summary>
            public void Invoke()
            {
                Manager.CompleteClose(Window, WindowType, DestroyImmediately);
            }
        }

        /// <summary>
        /// 正在显示的界面，键是挂在预制上的组件类型。
        /// </summary>
        private readonly Dictionary<Type, Window> mWindows = new Dictionary<Type, Window>();

        /// <summary>
        /// 已关闭待复用的界面，键同上。
        /// </summary>
        private readonly Dictionary<Type, Window> mCache = new Dictionary<Type, Window>();

        /// <summary>
        /// 各层级的根节点，按需创建。
        /// </summary>
        private readonly Dictionary<EUILayer, RectTransform> mLayers = new Dictionary<EUILayer, RectTransform>();

        /// <summary>
        /// UI 相机。
        /// </summary>
        public Camera UICamera { get; private set; }

        /// <summary>
        /// UI 根节点。
        /// </summary>
        public RectTransform UIRoot { get; private set; }

        /// <summary>
        /// 是否启用关闭后缓存。关掉则关闭即销毁。
        /// </summary>
        public bool EnableCache { get; set; } = true;

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
        /// 打开界面，放进默认窗口层。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <param name="prefabPath">预制路径。</param>
        /// <returns>实例上的组件。打不开时为空。</returns>
        public T Open<T>(string prefabPath) where T : Component
        {
            return Open<T>(prefabPath, EUILayer.Window, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <param name="prefabPath">预制路径。</param>
        /// <param name="layer">层级。</param>
        /// <param name="args">打开参数，会转给 <see cref="UIView.OnOpen"/>。</param>
        /// <returns>实例上的组件。打不开时为空。</returns>
        public T Open<T>(string prefabPath, EUILayer layer, object args = null) where T : Component
        {
            Close<T>();

            var parent = GetLayerRoot(layer);
            if (parent == null)
            {
                return null;
            }

            Window window;
            if (!mCache.TryGetValue(typeof(T), out window) || window.PrefabPath != prefabPath)
            {
                window = CreateWindow<T>(prefabPath, layer, parent);
                if (window == null)
                {
                    return null;
                }
            }
            else
            {
                mCache.Remove(typeof(T));
                window.Layer = layer;
                window.Instance.transform.SetParent(parent, false);
                window.Instance.SetActive(true);
            }

            var view = window.Instance.GetComponent<T>();
            if (view == null)
            {
                DestroyWindow(window);
                return null;
            }

            mWindows[typeof(T)] = window;

            var uiView = view as UIView;
            if (uiView != null)
            {
                uiView.PlayOpen(args);
            }

            return view;
        }

        /// <summary>
        /// 关掉一种界面。默认进缓存等待复用。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        public void Close<T>() where T : Component
        {
            Close(typeof(T), false);
        }

        /// <summary>
        /// 关掉一种界面并销毁实例，不进缓存。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        public void CloseAndDestroy<T>() where T : Component
        {
            Close(typeof(T), true);
        }

        /// <summary>
        /// 关掉全部正在显示的界面。
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
                Close(types[i], false);
            }
        }

        /// <summary>
        /// 清掉缓存里的界面，收回预制引用。切场景前调用。
        /// </summary>
        public void ClearCache()
        {
            foreach (var pair in mCache)
            {
                DestroyWindow(pair.Value);
            }

            mCache.Clear();
        }

        /// <summary>
        /// 按类型关掉界面。
        /// </summary>
        /// <param name="type">预制上的组件类型。</param>
        /// <param name="destroyImmediately">是否直接销毁。</param>
        public void Close(Type type, bool destroyImmediately = false)
        {
            Window window;
            if (!mWindows.Remove(type, out window))
            {
                return;
            }

            var request = new CloseCallback();
            request.Manager = this;
            request.Window = window;
            request.WindowType = type;
            request.DestroyImmediately = destroyImmediately;

            var view = window.Instance == null ? null : window.Instance.GetComponent<UIView>();
            if (view == null)
            {
                request.Invoke();
                return;
            }

            view.PlayClose(request.Invoke);
        }

        /// <summary>
        /// 取已经打开的界面。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <param name="view">打开着的界面实例。没打开时为空。</param>
        /// <returns>开着时为 true。</returns>
        public bool TryGetWindow<T>(out T view) where T : Component
        {
            view = null;
            Window window;
            if (!mWindows.TryGetValue(typeof(T), out window))
            {
                return false;
            }

            view = window.Instance == null ? null : window.Instance.GetComponent<T>();
            return view != null;
        }

        /// <summary>
        /// 某类界面是否已经打开。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <returns>开着时为 true。</returns>
        public bool IsWindowOpened<T>() where T : Component
        {
            T view;
            return TryGetWindow(out view);
        }

        /// <summary>
        /// 当前打开的界面数量。
        /// </summary>
        /// <returns>界面数量。</returns>
        public int GetOpenedUICount()
        {
            return mWindows.Count;
        }

        /// <summary>
        /// 缓存里的界面数量。
        /// </summary>
        /// <returns>缓存数量。</returns>
        public int GetCachedUICount()
        {
            return mCache.Count;
        }

        /// <summary>
        /// 取 UI 根节点。
        /// </summary>
        /// <returns>根节点。未绑定时为空。</returns>
        public RectTransform GetUIRoot()
        {
            return UIRoot;
        }

        /// <summary>
        /// 取某层级的根节点，没有就建一个。根节点铺满 UI 根节点，
        /// 兄弟序号取层级枚举值，保证层与层的前后关系。
        /// </summary>
        /// <param name="layer">层级。</param>
        /// <returns>层级根节点。UI 根节点未绑定时为空。</returns>
        public RectTransform GetLayerRoot(EUILayer layer)
        {
            RectTransform root;
            if (mLayers.TryGetValue(layer, out root))
            {
                return root;
            }

            if (UIRoot == null)
            {
                return null;
            }

            var layerObject = new UnityEngine.GameObject($"Layer_{layer}", typeof(RectTransform));
            root = (RectTransform)layerObject.transform;
            root.SetParent(UIRoot, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.SetSiblingIndex((int)layer);
            mLayers.Add(layer, root);
            return root;
        }

        /// <summary>
        /// 销毁还开着的界面。资源缓存由资源管理器自己清空。
        /// </summary>
        protected override void OnDestroy()
        {
            CloseAll();
            ClearCache();
            mLayers.Clear();
            UICamera = null;
            UIRoot = null;
        }

        /// <summary>
        /// 关闭动画结束后的收尾：隐藏并按配置缓存或销毁。
        /// </summary>
        /// <param name="window">窗口。</param>
        /// <param name="destroyImmediately">是否直接销毁。</param>
        private void CompleteClose(Window window, Type windowType, bool destroyImmediately)
        {
            if (window == null || window.Instance == null)
            {
                return;
            }

            window.Instance.SetActive(false);

            if (destroyImmediately || !EnableCache)
            {
                DestroyWindow(window);
                return;
            }

            mCache[windowType] = window;
        }

        /// <summary>
        /// 实例化一个新窗口。
        /// </summary>
        /// <typeparam name="T">预制上的组件。</typeparam>
        /// <param name="prefabPath">预制路径。</param>
        /// <param name="layer">层级。</param>
        /// <param name="parent">父节点。</param>
        /// <returns>窗口。失败时为空。</returns>
        private Window CreateWindow<T>(string prefabPath, EUILayer layer, RectTransform parent) where T : Component
        {
            var handle = App.Find<ResMgr>().Load(prefabPath);
            var prefab = handle == null ? null : handle.Asset as UnityEngine.GameObject;
            if (prefab == null)
            {
                App.Find<ResMgr>().Release(handle);
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            if (instance.GetComponent<T>() == null)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                App.Find<ResMgr>().Release(handle);
                return null;
            }

            var window = new Window();
            window.Instance = instance;
            window.Handle = handle;
            window.Layer = layer;
            window.PrefabPath = prefabPath;
            return window;
        }

        /// <summary>
        /// 销毁实例并归还预制。
        /// </summary>
        /// <param name="window">窗口。</param>
        private void DestroyWindow(Window window)
        {
            if (window == null)
            {
                return;
            }

            if (window.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(window.Instance);
            }

            var resources = App.Find<ResMgr>();
            if (resources != null)
            {
                resources.Release(window.Handle);
            }

            window.Instance = null;
            window.Handle = null;
        }
    }
}
