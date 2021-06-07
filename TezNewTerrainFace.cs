#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrainFace
    {
        public static readonly Vector3[] DirectionVectors = new Vector3[]
        {
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right
        };


        public enum SubFacePosition
        {
            Null = -1,
            /// <summary>
            /// 北西
            /// </summary>
            SouthWest,
            /// <summary>
            /// 北东
            /// </summary>
            SouthEast,
            /// <summary>
            /// 南西
            /// </summary>
            NorthWest,
            /// <summary>
            /// 南东
            /// </summary>
            NorthEast,
        }

        public enum Direction
        {
            None = -1,
            Top = 0,
            Bottom,
            Front,
            Back,
            Left,
            Right
        }

        public GameObject gameObject;
        public Transform transform;

        public TezNewTerrain terrainSystem;
        public TezNewTerrainFace parentTerrainFace;

        public int LOD = -1;
        public sbyte stitchMask = 0;
        public Vector3 center = Vector3.negativeInfinity;
        public Direction direction = Direction.Top;

        bool m_MeshExist = false;
        bool m_MeshVisible = false;
        TezNewTerrainFace[] m_Children = null;

        /// <summary>
        /// 生成单个面
        /// </summary>
        /// <param name="terrain"></param>
        public TezNewTerrainFace(TezNewTerrain terrain)
        {
            this.LOD = terrain.maxLOD;
            this.terrainSystem = terrain;
            this.center = Vector3.up;

            this.calculateLocalPosition();
        }

        /// <summary>
        /// 生成球体的6个根节点面之一
        /// </summary>
        public TezNewTerrainFace(TezNewTerrain terrain, Direction direction)
        {
            this.LOD = terrain.maxLOD;
            this.terrainSystem = terrain;
            this.direction = direction;
            this.center = DirectionVectors[(int)direction];

            this.calculateLocalPosition();
        }

        /// <summary>
        /// 生成四叉树面
        /// </summary>
        public TezNewTerrainFace(TezNewTerrainFace parentTerrainFace, SubFacePosition facePosition)
        {
            this.LOD = parentTerrainFace.LOD - 1;

            this.terrainSystem = parentTerrainFace.terrainSystem;
            this.parentTerrainFace = parentTerrainFace;
            this.direction = parentTerrainFace.direction;

            this.calculateCenter(facePosition);
            this.calculateLocalPosition();
        }

        private void calculateCenter(SubFacePosition facePosition)
        {
            var config = terrainSystem.cellConfig;
            var half_length = config.meshSideHalfSize * config.scaleFactorTable[this.LOD];

            ///4个象限中
            switch (facePosition)
            {
                ///第3象限
                ///向此方向偏移
                ///需要 X轴减少,Y轴减少
                case SubFacePosition.SouthWest:
                    this.calculateCenter(this.parentTerrainFace.center, -half_length, -half_length);
                    break;
                ///第4象限
                ///向此方向偏移
                ///需要 X轴增加,Y轴减少
                case SubFacePosition.SouthEast:
                    this.calculateCenter(this.parentTerrainFace.center, half_length, -half_length);
                    break;
                ///第2象限
                ///向此方向偏移
                ///需要 X轴减少,Y轴增加
                case SubFacePosition.NorthWest:
                    this.calculateCenter(this.parentTerrainFace.center, -half_length, half_length);
                    break;
                ///第1象限
                ///向此方向偏移
                ///需要 X轴增加,Y轴增加
                case SubFacePosition.NorthEast:
                    this.calculateCenter(this.parentTerrainFace.center, half_length, half_length);
                    break;
                default:
                    break;
            }
        }

        private void calculateCenter(Vector3 parentCenter, float half_a, float half_b)
        {
            ///以Top为标准面
            ///即X轴Z轴为标准
            ///旋转到各个面计算坐标轴的变换

            switch (this.direction)
            {
                // Y轴定位
                case Direction.Top:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z + half_b;
                    break;
                case Direction.Bottom:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z - half_b;
                    break;
                // Z轴定位
                case Direction.Front:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y - half_b;
                    center.z = parentCenter.z;
                    break;
                case Direction.Back:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y + half_b;
                    center.z = parentCenter.z;
                    break;
                // X轴定位
                case Direction.Left:
                    center.x = parentCenter.x;
                    center.y = parentCenter.y + half_a;
                    center.z = parentCenter.z + half_b;
                    break;
                case Direction.Right:
                    center.x = parentCenter.x;
                    center.y = parentCenter.y - half_a;
                    center.z = parentCenter.z + half_b;
                    break;
                default:
                    break;
            }
        }

        public void createFace()
        {
            m_MeshExist = true;
            m_MeshVisible = true;
            this.terrainSystem.addFace(this);
        }

        private void calculateLocalPosition()
        {
            terrainSystem.createGameObject(this);
        }

        public void scan(Vector3 flagWorldPosition)
        {
            if (this.LOD > 0)
            {
                //                 if (!m_MeshVisible)
                //                 {
                //                     ///如果已经分裂了
                //                     ///就继续扫描Subface
                //                     m_Children[(int)SubFacePosition.SouthWest].scan(flagWorldPosition);
                //                     m_Children[(int)SubFacePosition.SouthEast].scan(flagWorldPosition);
                //                     m_Children[(int)SubFacePosition.NorthWest].scan(flagWorldPosition);
                //                     m_Children[(int)SubFacePosition.NorthEast].scan(flagWorldPosition);
                //                 }

                ///如果距离超过阈值
                ///就应该分裂
                if (Vector3.Distance(flagWorldPosition, this.transform.position) < terrainSystem.splitThreshold[this.LOD])
                {
                    ///如果此Face没有分裂
                    ///则分裂
                    if (m_Children == null)
                    {
                        ///如果此Face有Mesh
                        ///把他的Mesh隐藏/删除
                        if (m_MeshExist)
                        {
                            m_MeshVisible = false;
                            this.gameObject.SetActive(m_MeshVisible);
                        }

                        ///分裂
                        m_Children = new TezNewTerrainFace[4];
                        this.split(SubFacePosition.SouthWest, flagWorldPosition);
                        this.split(SubFacePosition.SouthEast, flagWorldPosition);
                        this.split(SubFacePosition.NorthWest, flagWorldPosition);
                        this.split(SubFacePosition.NorthEast, flagWorldPosition);
                    }
                    else
                    {
                        ///如果已经分裂了
                        ///就继续扫描Subface
                        m_Children[(int)SubFacePosition.SouthWest].scan(flagWorldPosition);
                        m_Children[(int)SubFacePosition.SouthEast].scan(flagWorldPosition);
                        m_Children[(int)SubFacePosition.NorthWest].scan(flagWorldPosition);
                        m_Children[(int)SubFacePosition.NorthEast].scan(flagWorldPosition);
                    }
                }
                ///如果距离没有超过阈值
                else
                {
                    ///这是根节点
                    ///如果有Subface
                    ///就需要全部合并
                    if (m_Children != null)
                    {
                        m_Children[(int)SubFacePosition.SouthWest].combine();
                        m_Children[(int)SubFacePosition.SouthEast].combine();
                        m_Children[(int)SubFacePosition.NorthWest].combine();
                        m_Children[(int)SubFacePosition.NorthEast].combine();
                        m_Children = null;
                    }

                    ///检查此Face是否生成了Mesh
                    if (m_MeshExist)
                    {
                        ///如果有
                        ///但是没有显示出来
                        ///则把他显示出来
                        if (!m_MeshVisible)
                        {
                            m_MeshVisible = true;
                            this.transform.localRotation = Quaternion.identity;
                            this.transform.localScale = Vector3.one;
                            this.gameObject.SetActive(m_MeshVisible);
                        }
                    }
                    else
                    {
                        ///将自身生成出来
                        this.createFace();
                    }
                }
            }
            ///如果达到最高LOD
            ///此Face必须有Mesh
            else
            {
                if (!m_MeshExist)
                {
                    this.createFace();
                }
            }
        }

        private void split(SubFacePosition facePosition, Vector3 flagWorldPosition)
        {
            var face = new TezNewTerrainFace(this, facePosition);
            face.scan(flagWorldPosition);
            m_Children[(int)facePosition] = face;
        }

        private void combine()
        {
            ///合并Subface
            if (m_Children != null)
            {
                m_Children[(int)SubFacePosition.SouthWest].combine();
                m_Children[(int)SubFacePosition.SouthEast].combine();
                m_Children[(int)SubFacePosition.NorthWest].combine();
                m_Children[(int)SubFacePosition.NorthEast].combine();
                m_Children = null;
            }
            ///
            else
            {
                m_MeshVisible = false;
                this.gameObject.SetActive(m_MeshVisible);
            }
        }

        #region Tool
        public void test_split()
        {
            if (this.LOD > 0)
            {
                this.test_split(SubFacePosition.SouthWest);
                this.test_split(SubFacePosition.SouthEast);
                this.test_split(SubFacePosition.NorthWest);
                this.test_split(SubFacePosition.NorthEast);
            }
            else
            {
                this.createFace();
            }
        }

        private void test_split(SubFacePosition facePosition)
        {
            var face = new TezNewTerrainFace(this, facePosition);
            face.test_split();
            m_Children[(int)facePosition] = face;
        }
        #endregion
    }
}
