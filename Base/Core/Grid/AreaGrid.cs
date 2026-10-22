//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 5 行 9 列地盘，按配额铺色并在保持四块连通时交换
//---------------------------------------------------------------------------------------

using System;

namespace MyGame
{

    /// <summary>
    /// 5 行 9 列的地盘。按 9:11:12:13 顺序铺色，再交换格子；
    /// 一次交换后若不再是 4 个四邻域连通块，就换回去。
    /// </summary>
    public sealed class AreaGrid
    {

        /// <summary>
        /// 行数。
        /// </summary>
        public const int Rows = 5;

        /// <summary>
        /// 列数。
        /// </summary>
        public const int Cols = 9;

        /// <summary>
        /// 格子总数。
        /// </summary>
        public const int CellCount = Rows * Cols;

        /// <summary>
        /// 颜色块数量。
        /// </summary>
        public const int RegionCount = 4;

        /// <summary>
        /// 四种颜色的格子配额，顺序为红、蓝、黄、绿。
        /// </summary>
        private static readonly int[] Quotas = { 9, 11, 12, 13 };

        /// <summary>
        /// 四邻域的列偏移。
        /// </summary>
        private static readonly int[] NeighborCols = { 1, -1, 0, 0 };

        /// <summary>
        /// 四邻域的行偏移。
        /// </summary>
        private static readonly int[] NeighborRows = { 0, 0, 1, -1 };

        /// <summary>
        /// 每格所属颜色，下标为行优先。
        /// </summary>
        private readonly byte[] mOwners = new byte[CellCount];

        /// <summary>
        /// 连通块统计的访问标记。
        /// </summary>
        private readonly byte[] mVisited = new byte[CellCount];

        /// <summary>
        /// 连通块搜索用的栈。
        /// </summary>
        private readonly int[] mStack = new int[CellCount];

        /// <summary>
        /// 读取一格的颜色。
        /// </summary>
        /// <param name="index">行优先下标。</param>
        /// <returns>颜色编号。</returns>
        public byte Get(int index)
        {
            return mOwners[index];
        }

        /// <summary>
        /// 读取一种颜色的格子数。
        /// </summary>
        /// <param name="region">颜色编号。</param>
        /// <returns>配额。</returns>
        public int Quota(int region)
        {
            return Quotas[region];
        }

        /// <summary>
        /// 统计四邻域同色连通块的个数。
        /// </summary>
        /// <returns>连通块个数。</returns>
        public int CountComponents()
        {
            Array.Clear(mVisited, 0, CellCount);
            var groups = 0;
            for (var i = 0; i < CellCount; i++)
            {
                if (mVisited[i] != 0)
                {
                    continue;
                }

                groups++;
                var color = mOwners[i];
                var top = 0;
                mStack[top++] = i;
                mVisited[i] = 1;
                while (top > 0)
                {
                    var current = mStack[--top];
                    var col = current % Cols;
                    var row = current / Cols;
                    for (var n = 0; n < 4; n++)
                    {
                        var nextCol = col + NeighborCols[n];
                        var nextRow = row + NeighborRows[n];
                        if (nextCol < 0 || nextRow < 0 || nextCol >= Cols || nextRow >= Rows)
                        {
                            continue;
                        }

                        var next = nextRow * Cols + nextCol;
                        if (mVisited[next] != 0 || mOwners[next] != color)
                        {
                            continue;
                        }

                        mVisited[next] = 1;
                        mStack[top++] = next;
                    }
                }
            }

            return groups;
        }

        /// <summary>
        /// 按配额铺色，再做若干次仍保持四块连通的随机交换。
        /// </summary>
        /// <param name="seed">随机种子。</param>
        /// <param name="acceptedSwaps">希望成功的交换次数。</param>
        public void Generate(int seed, int acceptedSwaps)
        {
            FillInOrder();
            if (acceptedSwaps < 0)
            {
                acceptedSwaps = 0;
            }

            var random = new Random(seed);
            var accepted = 0;
            var attempts = 0;
            while (accepted < acceptedSwaps && attempts < 20000)
            {
                attempts++;
                if (TrySwap(random.Next(CellCount), random.Next(CellCount)))
                {
                    accepted++;
                }
            }
        }

        /// <summary>
        /// 按配额顺序把格子涂满。
        /// </summary>
        private void FillInOrder()
        {
            var index = 0;
            for (var region = 0; region < RegionCount; region++)
            {
                var quota = Quotas[region];
                for (var i = 0; i < quota; i++)
                {
                    mOwners[index++] = (byte)region;
                }
            }
        }

        /// <summary>
        /// 交换两格。交换后不是 4 个连通块就换回去。
        /// </summary>
        /// <param name="a">第一格下标。</param>
        /// <param name="b">第二格下标。</param>
        /// <returns>交换被保留时为 true。</returns>
        private bool TrySwap(int a, int b)
        {
            if (a == b || a < 0 || b < 0 || a >= CellCount || b >= CellCount)
            {
                return false;
            }

            var ownerA = mOwners[a];
            var ownerB = mOwners[b];
            if (ownerA == ownerB)
            {
                return false;
            }

            mOwners[a] = ownerB;
            mOwners[b] = ownerA;
            if (CountComponents() == RegionCount)
            {
                return true;
            }

            mOwners[a] = ownerA;
            mOwners[b] = ownerB;
            return false;
        }
    }
}
