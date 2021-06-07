using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewPlaneGMO : MonoBehaviour
    {
        TezNewTerrain_Plane terrain = new TezNewTerrain_Plane();
        Material material;
        Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            int maxLOD = 1;
            terrain.init(maxLOD, 100f);
            terrain.onCreateMesh += onMeshCreate;

            material = new Material(Shader.Find("Standard"));
        }

        private void onMeshCreate(TezNewTerrainVertexData vertexData)
        {
            vertexData.createMesh();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.G))
            {
                terrain.split();
                terrain.scan(mainCamera.transform.position);
                terrain.sendData();
            }
        }
    }
}