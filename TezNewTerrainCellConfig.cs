#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrainCellConfig
    {
        public enum StitchState : sbyte
        {
            Reset = -1,

            /// <summary>
            /// 不缝合
            /// </summary>
            Normal = 0,

            /// <summary>
            /// 北缝合
            /// </summary>
            NorthStitch = 1 << 0,

            /// <summary>
            /// 东缝合
            /// </summary>
            EastStitch = 1 << 1,

            /// <summary>
            /// 南缝合
            /// </summary>
            SouthStitch = 1 << 2,

            /// <summary>
            /// 西缝合
            /// </summary>
            WestStitch = 1 << 3
        }

        /// <summary>
        /// 边缘点数量一定要为基数
        /// </summary>
        public int sideVertexCount = 17;

        /// <summary>
        /// mesh边长
        /// </summary>
        public float meshSideSize = 1f;

        /// <summary>
        /// 半边长
        /// </summary>
        public float meshSideHalfSize;

        /// <summary>
        /// 切分系数
        /// </summary>
        public float splitFactor = 0.5f;

        /// <summary>
        /// 方形mesh块数量
        /// </summary>
        public int rectMeshCount;

        /// <summary>
        /// 索引总数
        /// </summary>
        public int indexCount;

        /// <summary>
        /// 顶点总数
        /// </summary>
        public int vertexCount;

        /// <summary>
        /// 三角形总数
        /// </summary>
        public int triangleCount;

        /// <summary>
        /// 顶点模板数组
        /// </summary>
        public Vector3[][] templateVertexTable = new Vector3[6][];

        /// <summary>
        ///为什么是17
        ///数组下标0-16
        ///0号为正常不缝合模板
        /// 
        ///除开0号模板之外
        ///Cell还有15种缝合结构
        ///即(缝合方向)
        ///单个系列>>北-东-南-西
        ///两个连续系列>>北东-东南-南西-西北
        ///两个不连续系列>>北南-东西
        ///三个连续系列>>北东南-东南西-南西北-西北东
        ///四面系列>>北东南西
        ///总共15种情况
        ///
        ///每种情况与应枚举State中的数据相对应
        ///即1,2,4,8,可以生成1-15中所有的数
        /// </summary>
        public const int indexTemplateCount = 17;

        /// <summary>
        /// 索引模板数组
        /// </summary>
        public int[][] templateIndexTable = new int[indexTemplateCount][];

        /// <summary>
        /// 缩放表
        /// </summary>
        public float[] scaleFactorTable = null;

        public TezNewTerrainCellConfig(float sideLength, int maxLOD)
        {
            this.init(sideLength, maxLOD);
        }

        private void init(float sideLength, int maxLOD)
        {
            this.meshSideSize = sideLength;
            this.meshSideHalfSize = sideLength * 0.5f;

            ///单行/列方块数等于边点数-1
            this.rectMeshCount = sideVertexCount - 1;
            ///索引数量等于总方块数*6(每个方块6个索引)
            this.indexCount = this.rectMeshCount * this.rectMeshCount * 6;
            ///顶点数量等于边点数*边点数
            this.vertexCount = this.sideVertexCount * this.sideVertexCount;
            ///三角形数量等于索引数量除以3
            this.triangleCount = this.indexCount / 3;

            ///创建scale表
            this.scaleFactorTable = new float[maxLOD + 1];
            for (int i = 0; i <= maxLOD; i++)
            {
                this.scaleFactorTable[i] = Mathf.Pow(this.splitFactor, maxLOD - i);
            }

            this.createMesh();
            this.createIndex();
        }

        private void createMesh()
        {
            for (int i = 0; i < 6; i++)
            {
                this.templateVertexTable[i] = new Vector3[vertexCount];
            }

            ///  1/4 = 0.25
            var step = this.meshSideSize / this.rectMeshCount;

            /// 1/2 = 0.5
            var half = this.meshSideSize / 2f;

            int index = 0;
            for (int y = 0; y < this.sideVertexCount; y++)
            {
                for (int x = 0; x < this.sideVertexCount; x++)
                {
                    ///Top
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Top][index] = new Vector3(-half + x * step, 0, half - y * step);
                    ///Back
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Back][index] = new Vector3(-half + x * step, half - y * step, 0);
                    ///Bottom
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Bottom][index] = new Vector3(-half + x * step, 0, -half + y * step);
                    ///Front
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Front][index] = new Vector3(-half + x * step, -half + y * step, 0);
                    ///Left
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Left][index] = new Vector3(0, half - x * step, -half + y * step);
                    ///Right
                    this.templateVertexTable[(int)TezNewTerrainFace.Direction.Right][index] = new Vector3(0, -half + x * step, -half + y * step);
                    index++;
                }
            }
        }

        private void createIndex()
        {
            this.templateIndexTable[0] = new int[indexCount];

            int vertex_index = 0;

            ///Mesh中小矩形的标记点
            ///通常为矩形左上角的点的ID
            ///
            ///最右和最下的矩形会跳掉一个点
            ///因为矩形的边
            ///是朝右和下延伸的
            ///
            ///偶数矩形和奇数矩形的链接方式不同
            ///所以有两种链接方式
            int rect_flag_index = 0;
            for (int y = 0; y < this.rectMeshCount; y++)
            {
                for (int x = 0; x < this.rectMeshCount; x++)
                {
                    if ((rect_flag_index & 1) == 1)
                    {
                        ///(2,6,1)-(i+1, i+r, i)
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + 1;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + this.sideVertexCount;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index;

                        ///(2,7,6)-(i+1, i+r+1, i+r)
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + 1;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + 1 + this.sideVertexCount;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + this.sideVertexCount;
                    }
                    else
                    {
                        ///(0,6,5)-(i,i+r+1,i+r)
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + this.sideVertexCount + 1;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + this.sideVertexCount;

                        ///(0,1,6)-(i,i+1,i+r+1)
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + 1;
                        this.templateIndexTable[0][vertex_index++] = rect_flag_index + this.sideVertexCount + 1;
                    }

                    rect_flag_index++;
                }
                rect_flag_index++;
            }

            for (int i = 1; i < indexTemplateCount; i++)
            {
                this.templateIndexTable[i] = new int[indexCount];
                this.templateIndexTable[0].CopyTo(this.templateIndexTable[i], 0);
                this.createIndexWithStitchState((StitchState)i, this.templateIndexTable[i], this.templateIndexTable[0]);
            }
        }

        private void createIndexWithStitchState(StitchState state, int[] current, int[] template)
        {
            if ((state & StitchState.NorthStitch) != StitchState.Normal)
            {
                for (int i = 1; i < this.rectMeshCount; i += 2)
                {
                    var pos = calculateIndexPosition(i, 0);

                    current[pos - 2] = template[pos - 3];
                    current[pos + 2] = template[pos - 3];
                }
            }

            if ((state & StitchState.EastStitch) != StitchState.Normal)
            {
                for (int i = 1; i < this.rectMeshCount; i += 2)
                {
                    var pos = calculateIndexPosition(this.rectMeshCount - 1, i - 1);
                    var next_pos = calculateIndexPosition(this.rectMeshCount - 1, i);

                    current[pos + 4] = template[pos];
                    current[next_pos + 4] = template[pos];
                }
            }

            if ((state & StitchState.SouthStitch) != StitchState.Normal)
            {
                for (int i = 1; i < this.rectMeshCount; i += 2)
                {
                    var pos = calculateIndexPosition(i, this.rectMeshCount - 1);

                    current[pos - 2] = template[pos + 1];
                    current[pos + 2] = template[pos + 1];
                }
            }

            if ((state & StitchState.WestStitch) != StitchState.Normal)
            {
                for (int i = 1; i < this.rectMeshCount; i += 2)
                {
                    var pos = calculateIndexPosition(0, i - 1);
                    var next_pos = calculateIndexPosition(0, i);

                    current[pos + 2] = template[next_pos + 1];
                    current[next_pos + 2] = template[next_pos + 1];
                }
            }
        }

        private int calculateIndexPosition(int x, int y)
        {
            return (x * 6) + y * 6 * this.rectMeshCount;
        }

        public int[] getIndexTemplate(int mask)
        {
            return this.templateIndexTable[mask];
        }
    }
}