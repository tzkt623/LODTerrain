#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public static class TezNewTerrainUtility
    {
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

        public enum CubeDirection
        {
            Error = -1,
            Top = 0,
            Bottom,
            Front,
            Back,
            Left,
            Right
        }

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
            string name = "F-";

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
