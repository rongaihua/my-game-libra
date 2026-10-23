//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-23
// Desc: 播放一次音效，播完后由音效管理器回收
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 一条音效。加载剪辑并播放，不循环的在播完后清掉播放标记。
    /// </summary>
    internal sealed class Sound : MyGame.GameObject
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
        /// 加载这条音效的资源管理器。
        /// </summary>
        private ResMgr mResources;

        /// <summary>
        /// 提供总音量的音效管理器。
        /// </summary>
        private SoundMgr mSoundMgr;

        /// <summary>
        /// 资源路径。
        /// </summary>
        private string mPath;

        /// <summary>
        /// 管理器分配的编号。
        /// </summary>
        private long mId;

        /// <summary>
        /// 正在播放。
        /// </summary>
        private bool mPlaying;

        /// <summary>
        /// 音源曾经进入播放，用来区分延迟还没开始和已经播完。
        /// </summary>
        private bool mHeardPlaying;

        /// <summary>
        /// 是否循环。
        /// </summary>
        private bool mLoop;

        /// <summary>
        /// 这条音效自己的音量，再乘管理器的总音量。
        /// </summary>
        private float mVolume;

        /// <summary>
        /// 延迟播放的秒数。
        /// </summary>
        private float mDelay;

        /// <summary>
        /// 管理器分配的编号。
        /// </summary>
        internal long Id
        {
            get
            {
                return mId;
            }
        }

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
        /// 挂到父节点下并加载剪辑。
        /// </summary>
        /// <param name="soundMgr">提供总音量的管理器。</param>
        /// <param name="resources">用来加载剪辑的资源管理器。</param>
        /// <param name="path">剪辑路径。</param>
        /// <param name="parent">音源的父节点。</param>
        /// <param name="id">管理器分配的编号。</param>
        /// <returns>剪辑加载成功时为 true。</returns>
        internal bool Init(SoundMgr soundMgr, ResMgr resources, string path, Transform parent, long id)
        {
            if (soundMgr == null || resources == null || string.IsNullOrEmpty(path) || parent == null)
            {
                return false;
            }

            mSoundMgr = soundMgr;
            mResources = resources;
            mPath = path;
            mId = id;
            mPlaying = false;
            mHeardPlaying = false;
            mLoop = false;
            mVolume = 1f;
            mDelay = 0f;
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
            mHeardPlaying = false;
            mAudioClip = null;
            mPath = null;
            mId = 0;
            if (mResources != null)
            {
                mResources.Release(mResHandle);
            }

            mResHandle = null;
            mResources = null;
            mSoundMgr = null;
        }

        /// <summary>
        /// 开始播放。
        /// </summary>
        /// <param name="volume">这条音效的音量。</param>
        /// <param name="loop">为 true 时循环。</param>
        /// <param name="delay">延迟秒数。不大于 0.01 时立刻播放。</param>
        internal void Play(float volume, bool loop = false, float delay = 0f)
        {
            mPlaying = true;
            mHeardPlaying = false;
            mLoop = loop;
            mDelay = delay;
            mVolume = volume;
            if (mAudioClip != null)
            {
                PlayImpl();
            }
        }

        /// <summary>
        /// 立刻停止。
        /// </summary>
        internal void Stop()
        {
            if (!mPlaying)
            {
                return;
            }

            mPlaying = false;
            mLoop = false;
            if (mAudioSource != null && mAudioClip != null)
            {
                mAudioSource.Stop();
            }
        }

        /// <summary>
        /// 按管理器总音量和这条音效的音量写到音源上。
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
        /// 非循环音效在播完后清掉播放标记。
        /// </summary>
        /// <param name="dt">距上一帧的秒数。</param>
        internal void Update(float dt)
        {
            if (!mPlaying || mAudioSource == null)
            {
                return;
            }

            if (mAudioSource.isPlaying)
            {
                mHeardPlaying = true;
                return;
            }

            if (mLoop || !mHeardPlaying)
            {
                return;
            }

            mPlaying = false;
        }

        /// <summary>
        /// 总音量乘上这条音效自己的音量。管理器已经销毁时为 0。
        /// </summary>
        private float FinalVolume
        {
            get
            {
                if (mSoundMgr == null)
                {
                    return 0f;
                }

                return mSoundMgr.FinalVolume * mVolume;
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
    }
}
