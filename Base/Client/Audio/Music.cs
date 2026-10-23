//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-23
// Desc: 用音源播放一首音乐，并处理淡入淡出
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 一首音乐。加载剪辑、循环播放，音量可以在一段时间里过渡。
    /// </summary>
    internal sealed class Music : MyGame.GameObject
    {

        /// <summary>
        /// 挂音源的对象。回收后留着，下次取出再打开。
        /// </summary>
        private UnityEngine.GameObject mGameObject;

        /// <summary>
        /// 音源所在的变换。
        /// </summary>
        private Transform mTransform;

        /// <summary>
        /// 音源。
        /// </summary>
        private AudioSource mAudioSource;

        /// <summary>
        /// 当前剪辑。
        /// </summary>
        private AudioClip mAudioClip;

        /// <summary>
        /// 剪辑的资源持有。
        /// </summary>
        private ResHandle mResHandle;

        /// <summary>
        /// 加载这首曲子的资源管理器。
        /// </summary>
        private ResMgr mResources;

        /// <summary>
        /// 提供总音量的音乐管理器。
        /// </summary>
        private MusicMgr mMusicMgr;

        /// <summary>
        /// 资源路径。
        /// </summary>
        private string mPath;

        /// <summary>
        /// 正在播放。
        /// </summary>
        private bool mPlaying;

        /// <summary>
        /// 正在过渡音量。
        /// </summary>
        private bool mFading;

        /// <summary>
        /// 是否循环。
        /// </summary>
        private bool mLoop;

        /// <summary>
        /// 这首曲子自己的音量，再乘管理器的总音量。
        /// </summary>
        private float mVolume;

        /// <summary>
        /// 延迟播放的秒数。
        /// </summary>
        private float mDelay;

        /// <summary>
        /// 过渡开始时的音量。
        /// </summary>
        private float mFadeSrcVolume;

        /// <summary>
        /// 过渡结束时的音量。
        /// </summary>
        private float mFadeDstVolume;

        /// <summary>
        /// 过渡时长，单位秒。
        /// </summary>
        private float mFadeDuration;

        /// <summary>
        /// 过渡已经过的秒数。
        /// </summary>
        private float mFadeElapsedTime;

        /// <summary>
        /// 正在播放。
        /// </summary>
        internal bool Playing
        {
            get
            {
                return mPlaying;
            }
        }

        /// <summary>
        /// 正在过渡音量。
        /// </summary>
        internal bool Fading
        {
            get
            {
                return mFading;
            }
        }

        /// <summary>
        /// 资源路径。
        /// </summary>
        internal string Path
        {
            get
            {
                return mPath;
            }
        }

        /// <summary>
        /// 挂到父节点下并加载剪辑。
        /// </summary>
        /// <param name="musicMgr">提供总音量的管理器。</param>
        /// <param name="resources">用来加载剪辑的资源管理器。</param>
        /// <param name="path">剪辑路径。</param>
        /// <param name="parent">音源的父节点。</param>
        /// <returns>剪辑加载成功时为 true。</returns>
        internal bool Init(MusicMgr musicMgr, ResMgr resources, string path, Transform parent)
        {
            if (musicMgr == null || resources == null || string.IsNullOrEmpty(path) || parent == null)
            {
                return false;
            }

            mMusicMgr = musicMgr;
            mResources = resources;
            mPath = path;
            mPlaying = false;
            mFading = false;
            mLoop = false;
            mVolume = 1f;
            mDelay = 0f;
            mFadeSrcVolume = 1f;
            mFadeDstVolume = 0f;
            mFadeDuration = 0f;
            mFadeElapsedTime = 0f;
            EnsureSource(parent);
            mResHandle = resources.Load(path);
            mAudioClip = mResHandle == null ? null : mResHandle.Asset as AudioClip;
            if (mAudioClip == null)
            {
                resources.Release(mResHandle);
                mResHandle = null;
                return false;
            }

            mAudioSource.clip = mAudioClip;
            return true;
        }

        /// <summary>
        /// 停掉播放，关掉对象，并归还剪辑。
        /// </summary>
        protected override void OnDestroy()
        {
            Stop();
            if (mAudioSource != null)
            {
                mAudioSource.clip = null;
            }

            if (mGameObject != null)
            {
                mGameObject.SetActive(false);
            }

            mVolume = 1f;
            mDelay = 0f;
            mFadeSrcVolume = 1f;
            mFadeDstVolume = 0f;
            mFadeDuration = 0f;
            mFadeElapsedTime = 0f;
            mAudioClip = null;
            mPath = null;
            if (mResources != null)
            {
                mResources.Release(mResHandle);
            }

            mResHandle = null;
            mResources = null;
            mMusicMgr = null;
        }

        /// <summary>
        /// 开始播放。淡入时音量从 0 收到目标值。
        /// </summary>
        /// <param name="volume">这首曲子的音量。</param>
        /// <param name="loop">为 true 时循环。</param>
        /// <param name="delay">延迟秒数。不大于 0.01 时立刻播放。</param>
        /// <param name="fadeInTime">淡入秒数。不大于 0.001 时不做过渡。</param>
        internal void Play(float volume, bool loop = false, float delay = 0f, float fadeInTime = 0f)
        {
            mPlaying = true;
            mLoop = loop;
            mDelay = delay;
            mVolume = volume;
            mFading = fadeInTime > 0.001f;
            if (mFading)
            {
                mFadeSrcVolume = 0f;
                mFadeDstVolume = mVolume;
                mFadeDuration = fadeInTime;
                mVolume = 0f;
                mFadeElapsedTime = 0f;
            }

            if (mAudioClip != null)
            {
                PlayImpl();
            }
        }

        /// <summary>
        /// 停止播放。给出淡出时间且当前还有音量时，先把音量收到 0。
        /// </summary>
        /// <param name="fadeOutTime">淡出秒数。不大于 0.001 时立刻停止。</param>
        internal void Stop(float fadeOutTime = 0f)
        {
            if (!mPlaying)
            {
                return;
            }

            if (fadeOutTime > 0.001f && mVolume > 0.001f)
            {
                mFading = true;
                mFadeSrcVolume = mVolume;
                mFadeDstVolume = 0f;
                mFadeElapsedTime = 0f;
                mFadeDuration = fadeOutTime;
                return;
            }

            mFading = false;
            mPlaying = false;
            mLoop = false;
            if (mAudioSource != null && mAudioClip != null)
            {
                mAudioSource.Stop();
            }
        }

        /// <summary>
        /// 改这首曲子的音量。给出过渡时间时从当前值收到新值。
        /// </summary>
        /// <param name="volume">目标音量。</param>
        /// <param name="fadeTime">过渡秒数。不大于 0 时立刻生效。</param>
        internal void SetVolume(float volume, float fadeTime = 0f)
        {
            mFading = fadeTime > 0f;
            if (mFading)
            {
                mFadeSrcVolume = mVolume;
                mFadeDstVolume = volume;
                mFadeDuration = fadeTime;
                mFadeElapsedTime = 0f;
                return;
            }

            mVolume = volume;
            ApplyVolume();
        }

        /// <summary>
        /// 按管理器总音量和这首曲子的音量写到音源上。
        /// </summary>
        internal void ApplyVolume()
        {
            if (mAudioClip == null || mAudioSource == null)
            {
                return;
            }

            mAudioSource.volume = FinalVolume;
        }

        /// <summary>
        /// 推进淡入淡出。非循环的曲子在播完后清掉播放标记。
        /// </summary>
        /// <param name="dt">距上一帧的秒数。</param>
        internal void Update(float dt)
        {
            if (mAudioClip == null || mAudioSource == null)
            {
                return;
            }

            if (mFading)
            {
                mFadeElapsedTime += dt;
                var duration = mFadeDuration > 0.001f ? mFadeDuration : 0.001f;
                var t = Smooth(mFadeElapsedTime / duration);
                mVolume = Mathf.Lerp(mFadeSrcVolume, mFadeDstVolume, t);
                if (mFadeElapsedTime > mFadeDuration)
                {
                    mVolume = mFadeDstVolume;
                    mFadeElapsedTime = 0f;
                    mFading = false;
                    if (mFadeDstVolume < 0.001f)
                    {
                        Stop();
                    }
                }

                ApplyVolume();
            }

            if (mLoop || !mPlaying)
            {
                return;
            }

            mPlaying = mAudioSource.isPlaying;
        }

        /// <summary>
        /// 总音量乘上这首曲子自己的音量。管理器已经销毁时为 0。
        /// </summary>
        private float FinalVolume
        {
            get
            {
                if (mMusicMgr == null)
                {
                    return 0f;
                }

                return mMusicMgr.FinalVolume * mVolume;
            }
        }

        /// <summary>
        /// 准备音源并挂到父节点下。对象还在就重新打开。
        /// </summary>
        /// <param name="parent">音源的父节点。</param>
        private void EnsureSource(Transform parent)
        {
            if (mGameObject != null)
            {
                mGameObject.name = mPath;
                mGameObject.SetActive(true);
            }
            else
            {
                mGameObject = new UnityEngine.GameObject(mPath);
                mTransform = mGameObject.transform;
                mAudioSource = mGameObject.AddComponent<AudioSource>();
            }

            mTransform.SetParent(parent, false);
            mAudioSource.playOnAwake = false;
            mAudioSource.spatialBlend = 0f;
        }

        /// <summary>
        /// 立刻按当前音量和循环设置播放。
        /// </summary>
        private void PlayImpl()
        {
            if (mAudioSource == null)
            {
                return;
            }

            mAudioSource.loop = mLoop;
            mAudioSource.volume = FinalVolume;
            if (mDelay > 0.01f)
            {
                mAudioSource.PlayDelayed(mDelay);
                return;
            }

            mAudioSource.Play();
        }

        /// <summary>
        /// 平滑插值。两端速度为 0。
        /// </summary>
        /// <param name="t">0 到 1 的进度。</param>
        /// <returns>平滑后的进度。</returns>
        private static float Smooth(float t)
        {
            if (t <= 0f)
            {
                return 0f;
            }

            if (t >= 1f)
            {
                return 1f;
            }

            return t * t * (3f - 2f * t);
        }
    }
}
