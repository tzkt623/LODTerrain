#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public static class TezNewTerrainUtility
    {
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
    }
}
