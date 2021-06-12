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
            LNF = 1 << CubeFace.Left | Direction.North | 1 << CubeFace.Front,
            FSL = 1 << CubeFace.Left | Direction.South | 1 << CubeFace.Front,
            LDW = 1 << CubeFace.Left | Direction.West | 1 << CubeFace.Down,
            LDE = 1 << CubeFace.Left | Direction.East | 1 << CubeFace.Down,
            LSB = 1 << CubeFace.Left | Direction.South | 1 << CubeFace.Back,
            BNL = 1 << CubeFace.Left | Direction.North | 1 << CubeFace.Back,

            RNF = 1 << CubeFace.Right | Direction.North | 1 << CubeFace.Front,
            FSR = 1 << CubeFace.Right | Direction.South | 1 << CubeFace.Front,
            RDW = 1 << CubeFace.Right | Direction.West | 1 << CubeFace.Down,
            RDE = 1 << CubeFace.Right | Direction.East | 1 << CubeFace.Down,
            RSB = 1 << CubeFace.Right | Direction.South | 1 << CubeFace.Back,
            BNR = 1 << CubeFace.Right | Direction.North | 1 << CubeFace.Back,
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
