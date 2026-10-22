using System.Collections.Generic;

namespace MyGame
{
    public struct PlayerJoined
    {
        public int PlayerId;
        public string Name;
    }

    public struct PlayerLeft
    {
        public int PlayerId;
    }

    public struct PlayerMoved
    {
        public int PlayerId;
        public float X;
        public float Y;
        public float Z;
    }

    /// <summary>
    /// 局域网房间里的玩家名单和最近一次位置。
    /// </summary>
    public sealed class RoomMgr : Module
    {
        private readonly Dictionary<int, string> mNames = new Dictionary<int, string>();
        private readonly Dictionary<int, PlayerMoved> mMoves = new Dictionary<int, PlayerMoved>();

        public int LocalPlayerId { get; private set; }
        public int PlayerCount { get { return mNames.Count; } }

        public void SetLocalPlayer(int playerId, string name)
        {
            LocalPlayerId = playerId;
            Join(playerId, name);
        }

        public void Join(int playerId, string name)
        {
            mNames[playerId] = name ?? string.Empty;
            App.Events.Emit(new PlayerJoined { PlayerId = playerId, Name = mNames[playerId] });
        }

        public void Leave(int playerId)
        {
            if (!mNames.Remove(playerId))
            {
                return;
            }

            mMoves.Remove(playerId);
            App.Events.Emit(new PlayerLeft { PlayerId = playerId });
        }

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

        public bool TryGetMove(int playerId, out PlayerMoved move)
        {
            return mMoves.TryGetValue(playerId, out move);
        }

        protected override void OnDestroy()
        {
            mNames.Clear();
            mMoves.Clear();
            LocalPlayerId = 0;
        }
    }
}
