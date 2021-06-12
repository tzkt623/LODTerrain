using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewPlanetFaceGMO : MonoBehaviour
    {
        public TezNewTerrainFace face;


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(face.north.transform.position, 2);
            Gizmos.DrawSphere(face.south.transform.position, 2);
            Gizmos.DrawSphere(face.west.transform.position, 2);
            Gizmos.DrawSphere(face.east.transform.position, 2);
        }
    }
}