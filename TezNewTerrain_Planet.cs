#define UseThread1

using System.Collections.Generic;
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
//                Debug.Log(r);
                r *= 0.8f - (this.maxLOD - i) * 0.2f;
            }
        }

        public void createCubeFace()
        {
            var top = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Top);
            var bottom = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Down);
            var front = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Front);
            var back = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Back);
            var left = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Left);
            var right = new TezNewTerrainFace(this, TezNewTerrainUtility.CubeFace.Right);


            m_Faces = new TezNewTerrainFace[6] { top, bottom, front, back, left, right };

            ///设置邻居
            top.initCubeNeighbor(front, right, back, left);
            bottom.initCubeNeighbor(back, right, front, left);
            front.initCubeNeighbor(bottom, right, top, left);
            back.initCubeNeighbor(top, right, bottom, left);
            left.initCubeNeighbor(front, top, back, bottom);
            right.initCubeNeighbor(front, bottom, back, top);
        }

        public override void update(Vector3 flagWorldPosition)
        {
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Top].update(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Down].update(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Front].update(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Back].update(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Left].update(ref flagWorldPosition);
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Right].update(ref flagWorldPosition);

            this.updateFace(ref flagWorldPosition);
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

                    config.copyVerticesBy(face.cubeFace);
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

                    terrainFace.needChangeStitchMask(out var mask);

                    var mesh = new Mesh();
                    mesh.name = "Cell";
                    mesh.vertices = vertices;
                    mesh.triangles = terrainFace.calculateMeshIndex(mask);
                    mesh.RecalculateNormals();
                    face.mesh = mesh;

                    var go = face.gameObject;
                    go.name = terrainFace.cubeFace.ToString() + "[" + terrainFace.facePosition.ToString() + "]" + TezNewTerrainUtility.generateNameWithStitch(mask);
                    go.GetComponent<MeshFilter>().mesh = mesh;

                }
            };

            this.addCMD(render_cmd);
        }

        public override void test_split()
        {
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Top].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Down].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Left].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Right].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Front].test_split();
            m_Faces[(int)TezNewTerrainUtility.CubeFace.Back].test_split();
        }
    }
}
