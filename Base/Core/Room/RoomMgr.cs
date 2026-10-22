//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 房间里的玩家名单和最近位置
//---------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace MyGame
{

    /// <summary>
    /// 有玩家加入房间。
    /// </summary>
    public struct PlayerJoined
    {

        /// <summary>
        /// 玩家 id。
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// 玩家名字。
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// 有玩家离开房间。
    /// </summary>
    public struct PlayerLeft
    {

        /// <summary>
        /// 玩家 id。
        /// </summary>
        public int PlayerId;
    }

    /// <summary>
    /// 玩家位置更新。
    /// </summary>
    public struct PlayerMoved
    {

        /// <summary>
        /// 玩家 id。
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// 世界坐标 X。
        /// </summary>
        public float X;

        /// <summary>
        /// 世界坐标 Y。
        /// </summary>
        public float Y;

        /// <summary>
        /// 世界坐标 Z。
        /// </summary>
        public float Z;
    }

    /// <summary>
    /// 局域网房间里的玩家名单和最近一次位置。
    /// </summary>
    public sealed class RoomMgr : Module
    {

        /// <summary>
        /// 玩家 id 到名字。
        /// </summary>
        private readonly Dictionary<int, string> mNames = new Dictionary<int, string>();

        /// <summary>
        /// 玩家 id 到最近一次位置。
        /// </summary>
        private readonly Dictionary<int, PlayerMoved> mMoves = new Dictionary<int, PlayerMoved>();

        /// <summary>
        /// 本机玩家 id。尚未确定时为 -1。
        /// </summary>
        public int LocalPlayerId { get; private set; } = -1;

        /// <summary>
        /// 当前房间人数。
        /// </summary>
        public int PlayerCount { get { return mNames.Count; } }

        /// <summary>
        /// 登记本机玩家并广播加入。
        /// </summary>
        /// <param name="playerId">本机玩家 id。</param>
        /// <param name="name">名字。</param>
        public void SetLocalPlayer(int playerId, string name)
        {
            LocalPlayerId = playerId;
            Join(playerId, name);
        }

        /// <summary>
        /// 记录一名玩家并派发 <see cref="PlayerJoined"/>。
        /// </summary>
        /// <param name="playerId">玩家 id。</param>
        /// <param name="name">名字。空则记成空字符串。</param>
        public void Join(int playerId, string name)
        {
            mNames[playerId] = name ?? string.Empty;
            App.Events.Emit(new PlayerJoined { PlayerId = playerId, Name = mNames[playerId] });
        }

        /// <summary>
        /// 移除一名玩家并派发 <see cref="PlayerLeft"/>。不在名单里则不做事。
        /// </summary>
        /// <param name="playerId">玩家 id。</param>
        public void Leave(int playerId)
        {
            if (!mNames.Remove(playerId))
            {
                return;
            }

            mMoves.Remove(playerId);
            App.Events.Emit(new PlayerLeft { PlayerId = playerId });
        }

        /// <summary>
        /// 更新一名已在房间里的玩家位置并派发 <see cref="PlayerMoved"/>。
        /// </summary>
        /// <param name="playerId">玩家 id。</param>
        /// <param name="x">世界坐标 X。</param>
        /// <param name="y">世界坐标 Y。</param>
        /// <param name="z">世界坐标 Z。</param>
        public void Move(int playerId, float x, float y, float z)
        {
            if (!mNames.ContainsKey(playerId))
            {
                return;
            }

            var move = new PlayerMoved { PlayerId = playerId, X = x, Y = y, Z = z };
            mMoves[playerId] = move;
            App.Events.Emit(move);
        }

        /// <summary>
        /// 读取玩家最近一次位置。
        /// </summary>
        /// <param name="playerId">玩家 id。</param>
        /// <param name="move">读到的位置。</param>
        /// <returns>该玩家有位置记录时为 true。</returns>
        public bool TryGetMove(int playerId, out PlayerMoved move)
        {
            return mMoves.TryGetValue(playerId, out move);
        }

        /// <summary>
        /// 读取玩家名字。
        /// </summary>
        /// <param name="playerId">玩家 id。</param>
        /// <param name="name">读到的名字。</param>
        /// <returns>该玩家在房间里时为 true。</returns>
        public bool TryGetName(int playerId, out string name)
        {
            return mNames.TryGetValue(playerId, out name);
        }

        /// <summary>
        /// 把当前玩家 id 按升序写入调用方的列表。
        /// </summary>
        /// <param name="ids">接收 id 的列表，会被先清空。</param>
        public void CopyPlayerIds(List<int> ids)
        {
            ids.Clear();
            foreach (var pair in mNames)
            {
                ids.Add(pair.Key);
            }

            for (var i = 1; i < ids.Count; i++)
            {
                var key = ids[i];
                var j = i - 1;
                while (j >= 0 && ids[j] > key)
                {
                    ids[j + 1] = ids[j];
                    j--;
                }

                ids[j + 1] = key;
            }
        }

        /// <summary>
        /// 只改本机玩家 id，不改名单。
        /// </summary>
        /// <param name="playerId">本机玩家 id。</param>
        public void SetLocalPlayerId(int playerId)
        {
            LocalPlayerId = playerId;
        }

        /// <summary>
        /// 清空名单和位置，不改本机 id。
        /// </summary>
        public void Clear()
        {
            mNames.Clear();
            mMoves.Clear();
        }

        /// <summary>
        /// 清空房间并把本机 id 置回 -1。
        /// </summary>
        protected override void OnDestroy()
        {
            Clear();
            LocalPlayerId = -1;
        }
    }
}
