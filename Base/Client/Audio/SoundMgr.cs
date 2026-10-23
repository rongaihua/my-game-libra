//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-23
// Desc: 音效管理器。按钮点击等短音效从这里播放
//---------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    /// <summary>
    /// 音效管理器。可以同时播放多条，不循环的播完后回收。
    /// </summary>
    public sealed class SoundMgr : Module
    {

        /// <summary>
        /// 音效根节点。
        /// </summary>
        private UnityEngine.GameObject mGameObject;

        /// <summary>
        /// 音源挂在这个节点下。
        /// </summary>
        private Transform mTransform;

        /// <summary>
        /// 音效开关。关掉后不再播放，正在播的实际音量为 0。
        /// </summary>
        private bool mEnable;

        /// <summary>
        /// 总音量。
        /// </summary>
        private float mVolume;

        /// <summary>
        /// 下一编号。0 留给播放失败。
        /// </summary>
        private long mNextId;

        /// <summary>
        /// 正在播放的音效，键是编号。
        /// </summary>
        private readonly Dictionary<long, Sound> mSounds = new Dictionary<long, Sound>();

        /// <summary>
        /// 本帧播完、等回收的编号。
        /// </summary>
        private readonly List<long> mPending = new List<long>();

        /// <summary>
        /// 创建不随场景销毁的音效根节点，并打开音效。
        /// </summary>
        /// <returns>成功时为 true。</returns>
        protected override bool OnInit()
        {
            mGameObject = new UnityEngine.GameObject("Audio.Sound");
            UnityEngine.Object.DontDestroyOnLoad(mGameObject);
            mTransform = mGameObject.transform;
            mEnable = true;
            mVolume = 1f;
            mNextId = 1;
            return true;
        }

        /// <summary>
        /// 推进正在播的音效，并回收已经播完的。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。</param>
        protected override void OnUpdate(int deltaMs)
        {
            if (mSounds.Count == 0 || deltaMs <= 0)
            {
                return;
            }

            var dt = deltaMs * 0.001f;
            foreach (var pair in mSounds)
            {
                pair.Value.Update(dt);
                if (!pair.Value.Playing)
                {
                    mPending.Add(pair.Key);
                }
            }

            for (var i = 0; i < mPending.Count; i++)
            {
                ReleaseSound(mPending[i]);
            }

            mPending.Clear();
        }

        /// <summary>
        /// 停掉全部音效并销毁根节点。
        /// </summary>
        protected override void OnDestroy()
        {
            if (mSounds.Count > 0)
            {
                var ids = new long[mSounds.Count];
                mSounds.Keys.CopyTo(ids, 0);
                for (var i = 0; i < ids.Length; i++)
                {
                    ReleaseSound(ids[i]);
                }
            }

            mPending.Clear();
            if (mGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(mGameObject);
            }

            mGameObject = null;
            mTransform = null;
        }

        /// <summary>
        /// 播放一条 2D 音效。
        /// </summary>
        /// <param name="path">剪辑路径。</param>
        /// <param name="volume">这条音效的音量。</param>
        /// <param name="loop">为 true 时循环。</param>
        /// <param name="delay">延迟秒数。</param>
        /// <returns>播放编号。未播放时为 0。</returns>
        public long Play2D(string path, float volume = 1f, bool loop = false, float delay = 0f)
        {
            if (!mEnable || string.IsNullOrEmpty(path) || mTransform == null)
            {
                return 0;
            }

            var id = mNextId;
            mNextId++;
            var sound = App.Objects.Create<Sound>();
            if (!sound.Init(this, App.Find<ResMgr>(), path, mTransform, id))
            {
                App.Objects.Release(sound);
                return 0;
            }

            sound.Play(volume, loop, delay);
            mSounds.Add(id, sound);
            return id;
        }

        /// <summary>
        /// 停止一条音效。播完回收留到下一帧。
        /// </summary>
        /// <param name="id">播放编号。无效编号直接返回。</param>
        public void Stop(long id)
        {
            Sound sound;
            if (id <= 0 || !mSounds.TryGetValue(id, out sound))
            {
                return;
            }

            sound.Stop();
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
        /// 音效开关。变化后立刻写到正在播的音源上。
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
        /// 把总音量写到正在播的音效上。
        /// </summary>
        private void ApplyVolume()
        {
            foreach (var pair in mSounds)
            {
                pair.Value.ApplyVolume();
            }
        }

        /// <summary>
        /// 归还一条音效。已经不在表里时直接返回。
        /// </summary>
        /// <param name="id">播放编号。</param>
        private void ReleaseSound(long id)
        {
            Sound sound;
            if (!mSounds.TryGetValue(id, out sound))
            {
                return;
            }

            mSounds.Remove(id);
            App.Objects.Release(sound);
        }
    }
}
