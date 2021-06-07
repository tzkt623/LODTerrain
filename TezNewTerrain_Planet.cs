#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrain_Planet : TezNewTerrain
    {
        TezNewTerrainFace[] m_Faces = new TezNewTerrainFace[6];

        public override void init(int maxLOD, float radius)
        {
            base.init(maxLOD, radius);
            this.cellConfig = new TezNewTerrainCellConfig(2, this.maxLOD);

            float r = this.radius;
            for (int i = this.maxLOD; i >= 0; i--)
            {
                this.splitThreshold[i] = r;
                r *= 0.75f;
            }
        }

        public void createFace()
        {
            m_Faces[(int)TezNewTerrainFace.Direction.Top] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Top);
            m_Faces[(int)TezNewTerrainFace.Direction.Bottom] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Bottom);
            m_Faces[(int)TezNewTerrainFace.Direction.Left] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Left);
            m_Faces[(int)TezNewTerrainFace.Direction.Right] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Right);
            m_Faces[(int)TezNewTerrainFace.Direction.Front] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Front);
            m_Faces[(int)TezNewTerrainFace.Direction.Back] = new TezNewTerrainFace(this, TezNewTerrainFace.Direction.Back);

            m_Faces[(int)TezNewTerrainFace.Direction.Top].createFace();
            m_Faces[(int)TezNewTerrainFace.Direction.Bottom].createFace();
            m_Faces[(int)TezNewTerrainFace.Direction.Left].createFace();
            m_Faces[(int)TezNewTerrainFace.Direction.Right].createFace();
            m_Faces[(int)TezNewTerrainFace.Direction.Front].createFace();
            m_Faces[(int)TezNewTerrainFace.Direction.Back].createFace();
        }

        public override void addFace(TezNewTerrainFace terrainFace)
        {
            var vc = this.cellConfig.vertexCount;
            var vertices = new Vector3[vc];
            this.cellConfig.templateVertexTable[(int)terrainFace.direction].CopyTo(vertices, 0);
            var scale = this.cellConfig.scaleFactorTable[terrainFace.LOD];

            ///这里可以用矩阵传入shader计算
            for (int i = 0; i < vc; i++)
            {
                /*
                    用此方法可以传入shader计算
                    Vector4 v4 = vertex_array[i];
                    v4.w = 1;
                    var translation = dir_vector + terrainFace.center;
                    var scale3 = new Vector3(scale, scale, scale);
                    Vector3 temp = Matrix4x4.TRS(translation, Quaternion.identity, scale3) * v4;
                */

                var temp = vertices[i];

                ///先按比例缩放点
                temp.x *= scale;
                temp.y *= scale;
                temp.z *= scale;

                ///把点移动到正确的子面位置上
                temp += terrainFace.center;

                ///标准化成球面点
                ///得到的是球面上每个点的朝向方向
                TezNewTerrainUtility.smoothNormalized(ref temp);

                ///乘以半径后得到当前面点的模型局部坐标
                ///减去当前GO所在的局部坐标
                ///将所有点拉回原点以适应GO的局部坐标位置
                vertices[i] = temp * radius - terrainFace.transform.localPosition;
            }

            TezNewTerrainVertexData vertexData = new TezNewTerrainVertexData();
            vertexData.vertices = vertices;
            vertexData.triangles = this.cellConfig.getIndexTemplate(terrainFace.stitchMask);
            vertexData.terrainFace = terrainFace;

            this.addVertexData(vertexData);
        }

        public override void split()
        {
            m_Faces[(int)TezNewTerrainFace.Direction.Top].test_split();
            m_Faces[(int)TezNewTerrainFace.Direction.Bottom].test_split();
            m_Faces[(int)TezNewTerrainFace.Direction.Left].test_split();
            m_Faces[(int)TezNewTerrainFace.Direction.Right].test_split();
            m_Faces[(int)TezNewTerrainFace.Direction.Front].test_split();
            m_Faces[(int)TezNewTerrainFace.Direction.Back].test_split();
        }

        public override void scan(Vector3 flagWorldPosition)
        {
            m_Faces[(int)TezNewTerrainFace.Direction.Top].scan(flagWorldPosition);
            m_Faces[(int)TezNewTerrainFace.Direction.Bottom].scan(flagWorldPosition);
            m_Faces[(int)TezNewTerrainFace.Direction.Left].scan(flagWorldPosition);
            m_Faces[(int)TezNewTerrainFace.Direction.Right].scan(flagWorldPosition);
            m_Faces[(int)TezNewTerrainFace.Direction.Front].scan(flagWorldPosition);
            m_Faces[(int)TezNewTerrainFace.Direction.Back].scan(flagWorldPosition);
        }


    }
}
