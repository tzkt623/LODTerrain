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
            this.cellConfig = new TezNewTerrainCellConfig(sideLength, this.maxLOD);
            m_Face = new TezNewTerrainFace(this);
        }

        public override void addFace(TezNewTerrainFace terrainFace)
        {
            var vc = this.cellConfig.vertexCount;
            var vertex_array = new Vector3[vc];
            this.cellConfig.templateVertexTable[(int)terrainFace.direction].CopyTo(vertex_array, 0);

            var scale = Mathf.Pow(0.5f, this.maxLOD - terrainFace.LOD);
            ///这里可以用缩放矩阵传进shader里面计算
            ///速度更快
            for (int i = 0; i < vc; i++)
            {
                vertex_array[i].x *= scale;
                vertex_array[i].y *= scale;
                vertex_array[i].z *= scale;
            }

            TezNewTerrainVertexData vertexData = new TezNewTerrainVertexData();
            vertexData.vertices = vertex_array;
            vertexData.triangles = this.cellConfig.getIndexTemplate(terrainFace.stitchMask);
            vertexData.terrainFace = terrainFace;

            this.addVertexData(vertexData);
        }

        public override void scan(Vector3 flagWorldPosition)
        {
            m_Face.scan(flagWorldPosition);
        }

        public override void split()
        {
            m_Face.test_split();
        }
    }
}
