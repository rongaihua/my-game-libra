//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-23
// Desc: 音乐管理器。同一时间只播放一首
//---------------------------------------------------------------------------------------

using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 音乐管理器。同一时间只留一首，换曲时停掉上一首。
    /// </summary>
    public sealed class MusicMgr : Module
    {

        /// <summary>
        /// 音乐根节点。
        /// </summary>
        private UnityEngine.GameObject mGameObject;

        /// <summary>
        /// 音源挂在这个节点下。
        /// </summary>
        private Transform mTransform;

        /// <summary>
        /// 音乐开关。关掉后实际音量为 0。
        /// </summary>
        private bool mEnable;

        /// <summary>
        /// 总音量。
        /// </summary>
        private float mVolume;

        /// <summary>
        /// 当前这首。没有在播时为空。
        /// </summary>
        private Music mMusic;

        /// <summary>
        /// 当前曲目路径。没有在播时为空。
        /// </summary>
        public string PlayingPath
        {
            get
            {
                return mMusic == null ? null : mMusic.Path;
            }
        }

        /// <summary>
        /// 创建不随场景销毁的音乐根节点，并打开音乐。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        protected override bool OnInit()
        {
            mGameObject = new UnityEngine.GameObject("Audio.Music");
            UnityEngine.Object.DontDestroyOnLoad(mGameObject);
            mTransform = mGameObject.transform;
            mEnable = true;
            mVolume = 1f;
            return true;
        }

        /// <summary>
        /// 正在淡入淡出时推进当前这首。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        protected override void OnUpdate(int deltaMs)
        {
            if (mMusic == null || !mMusic.Fading || deltaMs <= 0)
            {
                return;
            }

            mMusic.Update(deltaMs * 0.001f);
        }

        /// <summary>
        /// 停掉当前这首并销毁根节点。
        /// </summary>
        protected override void OnDestroy()
        {
            ReleaseMusic();
            if (mGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(mGameObject);
            }

            mGameObject = null;
            mTransform = null;
        }

        /// <summary>
        /// 播放一首。同一路径已经在播时只改音量，否则换掉当前这首。
        /// </summary>
        /// <param name="path">剪辑路径。</param>
        /// <param name="volume">这首曲子的音量。</param>
        /// <param name="fadeInTime">淡入秒数。</param>
        public void Play(string path, float volume = 1f, float fadeInTime = 0f)
        {
            if (string.IsNullOrEmpty(path) || mTransform == null)
            {
                return;
            }

            if (mMusic != null)
            {
                if (path == mMusic.Path)
                {
                    if (!mMusic.Playing)
                    {
                        mMusic.Play(volume, true, 0f, fadeInTime);
                    }
                    else
                    {
                        mMusic.SetVolume(volume, fadeInTime);
                    }

                    return;
                }

                ReleaseMusic();
            }

            var music = App.Objects.Create<Music>();
            if (!music.Init(this, App.Find<ResMgr>(), path, mTransform))
            {
                App.Objects.Release(music);
                return;
            }

            mMusic = music;
            mMusic.Play(volume, true, 0f, fadeInTime);
        }

        /// <summary>
        /// 停止当前这首。
        /// </summary>
        /// <param name="fadeOutTime">淡出秒数。为 0 时立刻停止。</param>
        public void Stop(float fadeOutTime = 0f)
        {
            if (mMusic == null)
            {
                return;
            }

            mMusic.Stop(fadeOutTime);
        }

        /// <summary>
        /// 开关和音量合成后的实际音量。关掉时为 0。
        /// </summary>
        internal float FinalVolume
        {
            get
            {
                return mEnable ? mVolume : 0f;
            }
        }

        /// <summary>
        /// 音乐开关。变化后立刻写到正在播的音源上。
        /// </summary>
        public bool Enable
        {
            get
            {
                return mEnable;
            }
            set
            {
                if (value == mEnable)
                {
                    return;
                }

                mEnable = value;
                ApplyVolume();
            }
        }

        /// <summary>
        /// 总音量。变化后立刻写到正在播的音源上。
        /// </summary>
        public float Volume
        {
            get
            {
                return mVolume;
            }
            set
            {
                if (Mathf.Abs(mVolume - value) < 0.01f)
                {
                    return;
                }

                mVolume = value;
                ApplyVolume();
            }
        }

        /// <summary>
        /// 把总音量写到当前这首上。
        /// </summary>
        private void ApplyVolume()
        {
            if (mMusic != null)
            {
                mMusic.ApplyVolume();
            }
        }

        /// <summary>
        /// 归还当前这首。没有在播时直接返回。
        /// </summary>
        private void ReleaseMusic()
        {
            if (mMusic == null)
            {
                return;
            }

            App.Objects.Release(mMusic);
            mMusic = null;
        }
    }
}
