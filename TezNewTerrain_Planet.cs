#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrain_Planet : TezNewTerrain
    {
        TezNewTerrainFace[] m_Faces = null;

        public override void init(int maxLOD, float radius)
        {
            base.init(maxLOD, radius);
            ///边长只能为2
            ///因为需要在半径为1的标准球体上进行计算
            ///其他非标准球体都不行,需要Normlize
            this.config = new TezNewTerrainConfig(2, this.maxLOD);

            float r = this.radius * 2f;
            for (int i = this.maxLOD; i >= 0; i--)
            {
                this.splitThreshold[i] = r;
                r *= 0.5f;
            }
        }

        public void createCubeFace()
        {
            var top = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Top);
            var bottom = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Bottom);
            var front = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Front);
            var back = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Back);
            var left = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Left);
            var right = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeDirection.Right);


            m_Faces = new TezNewTerrainFace[6] { top, bottom, front, back, left, right };

            ///设置邻居
            top.setNeighbor(front, right, back, left);
            bottom.setNeighbor(back, right, front, left);
            front.setNeighbor(bottom, right, top, left);
            back.setNeighbor(top, right, bottom, left);
            left.setNeighbor(front, top, back, bottom);
            right.setNeighbor(front, bottom, back, top);

            top.createFace();
            bottom.createFace();
            front.createFace();
            back.createFace();
            left.createFace();
            right.createFace();
        }

        public override void scan(Vector3 flagWorldPosition)
        {
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Top].scan(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Bottom].scan(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Front].scan(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Back].scan(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Left].scan(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Right].scan(ref flagWorldPosition);
        }


        public override void addCMD_CreateMesh(TezNewTerrainFace terrainFace)
        {
            TezNewTerrainCMD_CeateMesh render_cmd = new TezNewTerrainCMD_CeateMesh()
            {
                terrainFace = terrainFace,
                onCreateData = (TezNewTerrainCMD_CeateMesh cmd) =>
                {
                    /*
                        用此方法可以传入shader计算

                        var translation = terrainFace.center;
                        var scale3 = new Vector3(scale, scale, scale);
                        var rotation = Quaternion.identity;
                        glUniform3fv(vertices);

                        {
                            glUniform3f(translation);
                            glUniform3f(scale3);
                            glUniform3f(rotation);
                        }
                        Or
                        {
                            Vector4 v4 = vertices[i];
                            v4.w = 1;
                            var mat = Matrix4x4.TRS(translation, rotation, scale3);
                            glUniformMatrix4fv(mat);
                        }
                    */

                    var face = cmd.terrainFace;
                    var config = face.terrainSystem.config;

                    config.copyVertices(face.direction);
                    var vertices = config.shardeVertices;
                    var scale = config.scaleFactorTable[face.LOD];

                    var local_position = face.transform.localPosition;
                    var vertex_count = config.vertexCount;
                    for (int i = 0; i < vertex_count; i++)
                    {
                        var temp = vertices[i];

                        ///先按比例缩放点
                        temp.x *= scale;
                        temp.y *= scale;
                        temp.z *= scale;

                        ///把点移动到正确的子面位置上
                        temp += face.center;

                        ///标准化成球面点
                        ///得到的是球面上每个点的朝向方向
                        TezNewTerrainUtility.smoothNormalized(ref temp);

                        ///乘以半径后得到当前面点的模型局部坐标
                        ///减去当前GO所在的局部坐标
                        ///将所有点拉回原点以适应GO的局部坐标位置
                        vertices[i] = temp * radius - local_position;


                        /*
                         * 惊了
                         * 点歪了
                           GameObject number = new GameObject();
                           number.name = i.ToString();
                           number.transform.parent = face.transform;
                           number.transform.localPosition = vertices[i];
                        */
                    }

                    Mesh mesh = new Mesh();
                    mesh.name = "Cell";
                    mesh.vertices = vertices;
                    mesh.triangles = face.calculateMeshIndex();
                    mesh.RecalculateNormals();
                    face.mesh = mesh;

                    GameObject go = face.gameObject;
                    go.name = TezNewTerrainUtility.generateNameWithStitch(face.calculateStitchMask());
                    var mf = go.GetComponent<MeshFilter>();
                    mf.mesh = mesh;
                }
            };

            this.addCMD(render_cmd);
        }

        public override void test_split()
        {
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Top].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Bottom].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Left].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Right].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Front].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeDirection.Back].test_split();
        }

    }
}
