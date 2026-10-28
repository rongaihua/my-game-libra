//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-28
// Desc: 界面基类（自动绑定 + 开闭渐变）
//---------------------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 界面基类。挂在界面预制根节点上，由 <see cref="UIMgr"/> 打开。
    /// 生命周期：Awake（自动绑定 → OnCreate）→ OnOpen → OnClose。
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        /// <summary>
        /// 根节点的 RectTransform。
        /// </summary>
        public RectTransform Rect { get; private set; }

        /// <summary>
        /// 透明控制，开闭动画默认作用在它上面。
        /// </summary>
        public CanvasGroup Group { get; private set; }

        /// <summary>
        /// 根节点。
        /// </summary>
        public Transform Root => Rect;

        /// <summary>
        /// 根节点物体。
        /// </summary>
        public UnityEngine.GameObject Go => Rect.gameObject;

        /// <summary>
        /// 打开渐变时长（秒）。派生类可改。
        /// </summary>
        protected virtual float OpenDuration => 0.12f;

        /// <summary>
        /// 关闭渐变时长（秒）。派生类可改。
        /// </summary>
        protected virtual float CloseDuration => 0.12f;

        /// <summary>
        /// 唤醒。派生类重写时必须调用基类实现。
        /// </summary>
        protected virtual void Awake()
        {
            Rect = transform as RectTransform;
            Group = gameObject.GetComponent<CanvasGroup>();
            if (Group == null)
            {
                Group = gameObject.AddComponent<CanvasGroup>();
            }

            UIBinder.Bind(this);
            OnCreate();
        }

        /// <summary>
        /// 创建时调用一次，用来取子节点引用、绑按钮。
        /// 子节点引用优先用 <see cref="UIBindAttribute"/> 自动绑定，剩下的再在这里手写。
        /// </summary>
        protected virtual void OnCreate()
        {
        }

        /// <summary>
        /// 打开时调用。由界面管理器触发，不要自己调。
        /// </summary>
        /// <param name="args">打开参数，可为空。</param>
        public virtual void OnOpen(object args)
        {
        }

        /// <summary>
        /// 关闭时调用。
        /// </summary>
        public virtual void OnClose()
        {
        }

        /// <summary>
        /// 关闭动画。默认按 <see cref="CloseDuration"/> 淡出，结束后回调。
        /// 派生类要实现位移、缩放等效果时重写 <see cref="OnPlayCloseAnimation"/>。
        /// </summary>
        /// <param name="onCompleted">动画结束回调。</param>
        public void PlayClose(Action onCompleted)
        {
            OnPlayCloseAnimation(onCompleted);
        }

        /// <summary>
        /// 打开动画。默认按 <see cref="OpenDuration"/> 淡入。
        /// </summary>
        public void PlayOpen(object args)
        {
            Group.alpha = 0f;
            gameObject.SetActive(true);
            OnOpen(args);
            OnPlayOpenAnimation();
            StartCoroutine(FadeRoutine(Group.alpha, 1f, OpenDuration, null));
        }

        /// <summary>
        /// 打开动画实现。
        /// </summary>
        protected virtual void OnPlayOpenAnimation()
        {
        }

        /// <summary>
        /// 关闭动画实现。默认淡出后回调，重写时记得调 <paramref name="onCompleted"/>。
        /// </summary>
        /// <param name="onCompleted">动画结束回调。</param>
        protected virtual void OnPlayCloseAnimation(Action onCompleted)
        {
            StartCoroutine(FadeRoutine(Group.alpha, 0f, CloseDuration, onCompleted));
        }

        /// <summary>
        /// 透明度渐变。用 unscaledDeltaTime，暂停时也能正常播完。
        /// </summary>
        /// <param name="from">起始透明度。</param>
        /// <param name="to">目标透明度。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="onCompleted">结束回调。</param>
        /// <returns>协程。</returns>
        private IEnumerator FadeRoutine(float from, float to, float duration, Action onCompleted)
        {
            if (duration <= 0f)
            {
                Group.alpha = to;
                onCompleted?.Invoke();
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                Group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            Group.alpha = to;
            onCompleted?.Invoke();
        }
    }
}
