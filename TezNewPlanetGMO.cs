using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewPlanetGMO : MonoBehaviour
    {
        TezNewTerrain_Planet terrain = new TezNewTerrain_Planet();
        Material material;
        Camera mainCamera;

        public Transform flagObject;

        private void Start()
        {
            material = new Material(Shader.Find("Standard"));
            mainCamera = Camera.main;

            int maxLOD = 4;

            terrain.onCreateMesh += onMeshCreate;
            terrain.onCreateGameObject += onCreateGameObject;

            terrain.transform = this.transform;
            terrain.init(maxLOD, 100.0f);
            terrain.createFace();
            terrain.sendData();
        }

        private void onCreateGameObject(GameObject go, Vector3 local)
        {
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().sharedMaterial = material;

            go.transform.parent = this.transform;
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.identity;
        }

        private void onMeshCreate(TezNewTerrainVertexData vertexData)
        {
            vertexData.createMesh();
        }

        private void Update()
        {
            terrain.scan(flagObject.position);
            terrain.sendData();

            float degrees = 10 * Time.deltaTime;
            this.transform.Rotate(degrees, degrees, degrees);

            //             if (Input.GetKeyUp(KeyCode.G))
            //             {
            //                 terrain.split();
            //                 terrain.sendData();
            //             }
        }
    }
}