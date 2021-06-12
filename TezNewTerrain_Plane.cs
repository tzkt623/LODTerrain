#define UseThread1


using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrain_Plane : TezNewTerrain
    {
        TezNewTerrainFace m_Face = null;

        public override void init(int maxLOD, float sideLength)
        {
            base.init(maxLOD, sideLength);
            this.config = new TezNewTerrainConfig(sideLength, this.maxLOD);
            m_Face = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Top);
        }

        public override void addCMD_CreateMesh(TezNewTerrainFace terrainFace)
        {
            TezNewTerrainCMD_CeateMesh render_cmd = new TezNewTerrainCMD_CeateMesh();
            render_cmd.terrainFace = terrainFace;

            render_cmd.onCreateData = (TezNewTerrainCMD_CeateMesh cmd) =>
            {
                var vc = this.config.vertexCount;
                var vertex_array = new Vector3[vc];
                this.config.templateVertexTable[(int)terrainFace.cubeFace].CopyTo(vertex_array, 0);

                var scale = Mathf.Pow(0.5f, this.maxLOD - terrainFace.LOD);
                ///这里可以用缩放矩阵传进shader里面计算
                ///速度更快
                for (int i = 0; i < vc; i++)
                {
                    vertex_array[i].x *= scale;
                    vertex_array[i].y *= scale;
                    vertex_array[i].z *= scale;
                }

//                 vertexData.vertices = vertex_array;
//                 vertexData.triangles = this.config.getIndexTemplate(terrainFace.calculateStitchMask());
//                 vertexData.terrainFace = terrainFace;
            };

            this.addCMD(render_cmd);
        }

        public override void update(Vector3 flagWorldPosition)
        {
            m_Face.update(ref flagWorldPosition);
        }

        public override void test_split()
        {
            m_Face.test_split();
        }
    }
}
