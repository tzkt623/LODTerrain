using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewPlanetGMO : MonoBehaviour
    {

        public bool rotating = false;
        public Vector3 rotateVector = new Vector3(0, 0, 0);

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
            terrain.createCubeFace();
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

        private void onMeshCreate(TezNewTerrainCMD_CeateMesh vertexData)
        {
            vertexData.sendData();
        }

        private void Update()
        {
            terrain.update(flagObject.position);
            terrain.sendData();

            if (rotating)
            {
                this.transform.Rotate(rotateVector * Time.deltaTime);
            }


            //             if (Input.GetKeyUp(KeyCode.G))
            //             {
            //                 terrain.split();
            //                 terrain.sendData();
            //             }
        }
    }
}