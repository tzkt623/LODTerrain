#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public static class TezNewTerrainUtility
    {
        public enum Direction
        {
            Error = -1,
            /// <summary>
            /// 北
            /// </summary>
            North = 1 << (CubeFace.Right + 1),
            /// <summary>
            /// 东
            /// </summary>
            East = 1 << (CubeFace.Right + 2),
            /// <summary>
            /// 南
            /// </summary>
            South = 1 << (CubeFace.Right + 3),
            /// <summary>
            /// 西
            /// </summary>
            West = 1 << (CubeFace.Right + 4)
        }

        public enum CubeFace
        {
            Error = -1,
            Top = 0,
            Down,
            Front,
            Back,
            Left,
            Right
        }

        public enum StitchState : int
        {
            Reset = -1,

            /// <summary>
            /// 不缝合
            /// </summary>
            Normal = 0,

            /// <summary>
            /// 北缝合
            /// </summary>
            NorthStitch = 1 << 0,

            /// <summary>
            /// 东缝合
            /// </summary>
            EastStitch = 1 << 1,

            /// <summary>
            /// 南缝合
            /// </summary>
            SouthStitch = 1 << 2,

            /// <summary>
            /// 西缝合
            /// </summary>
            WestStitch = 1 << 3
        }

        public enum Group
        {
            /// <summary>
            /// Left-Front相接时,L分裂,North邻居(Front)查其理论South
            /// </summary>
            LN_FS = 1 << CubeFace.Left | Direction.South | 1 << CubeFace.Front,
            SelfUse_LN_FS = 1 << CubeFace.Left | Direction.North | 1 << CubeFace.Front,

            /// <summary>
            /// Left-Front相接时,F分裂,West邻居(Left)查其理论East方向
            /// </summary>
            FW_LE = 1 << CubeFace.Left | Direction.East | 1 << CubeFace.Front,
            SelfUse_FW_LE = 1 << CubeFace.Left | Direction.West | 1 << CubeFace.Front,

            /// <summary>
            /// Left-Down相接时
            /// L分裂,West邻居(Down)查其理论East方向
            /// 或者
            /// D分裂,West邻居(Left)查其理论East方向
            /// </summary>
            LW_DE_or_DW_LE = 1 << CubeFace.Left | Direction.East | 1 << CubeFace.Down,
            SelfUse_LW_DE_or_DW_LE = 1 << CubeFace.Left | Direction.West | 1 << CubeFace.Down,

            LS_BN = 1 << CubeFace.Left | Direction.North | 1 << CubeFace.Back,
            SelfUse_LS_BN = 1 << CubeFace.Left | Direction.South | 1 << CubeFace.Back,

            BW_LE = 1 << CubeFace.Left | Direction.East | 1 << CubeFace.Back,
            SelfUse_BW_LE = 1 << CubeFace.Left | Direction.West | 1 << CubeFace.Back,

            FE_RW = 1 << CubeFace.Right | Direction.West | 1 << CubeFace.Front,
            SelfUse_FE_RW = 1 << CubeFace.Right | Direction.East | 1 << CubeFace.Front,

            RN_FS = 1 << CubeFace.Right | Direction.South | 1 << CubeFace.Front,
            SelfUse_RN_FS = 1 << CubeFace.Right | Direction.North | 1 << CubeFace.Front,

            RE_DW_or_DE_RW = 1 << CubeFace.Right | Direction.West | 1 << CubeFace.Down,
            SelfUse_RE_DW_or_DE_RW = 1 << CubeFace.Right | Direction.East | 1 << CubeFace.Down,

            BE_RW = 1 << CubeFace.Right | Direction.West | 1 << CubeFace.Back,
            SelfUse_BE_RW = 1 << CubeFace.Right | Direction.East | 1 << CubeFace.Back,

            RS_BN = 1 << CubeFace.Right | Direction.North | 1 << CubeFace.Back,
            SelfUse_RS_BN = 1 << CubeFace.Right | Direction.South | 1 << CubeFace.Back,
        }

        public static readonly Vector3[] CubeFaceVectors = new Vector3[]
        {
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right
        };

        public static void smoothNormalized(ref Vector3 vector)
        {
            var x = vector.x;
            var y = vector.y;
            var z = vector.z;

            var x2 = x * x;
            var y2 = y * y;
            var z2 = z * z;

            vector.Set(
                x * Mathf.Sqrt(1f - y2 * 0.5f - z2 * 0.5f + y2 * z2 / 3),
                y * Mathf.Sqrt(1f - x2 * 0.5f - z2 * 0.5f + x2 * z2 / 3),
                z * Mathf.Sqrt(1f - x2 * 0.5f - y2 * 0.5f + x2 * y2 / 3));
        }


        public static string generateNameWithStitch(StitchState stitchState)
        {
            string name = "-";

            if ((stitchState & StitchState.NorthStitch) != StitchState.Normal)
            {
                name += "N";
            }
            if ((stitchState & StitchState.EastStitch) != StitchState.Normal)
            {
                name += "E";
            }
            if ((stitchState & StitchState.SouthStitch) != StitchState.Normal)
            {
                name += "S";
            }
            if ((stitchState & StitchState.WestStitch) != StitchState.Normal)
            {
                name += "W";
            }

            return name;
        }
    }
}
