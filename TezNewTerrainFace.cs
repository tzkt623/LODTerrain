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

        public enum Position
        {
            Error = -1,
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
            Error = -1,
            /// <summary>
            /// 北
            /// </summary>
            North = 0,
            /// <summary>
            /// 东
            /// </summary>
            East,
            /// <summary>
            /// 南
            /// </summary>
            South,
            /// <summary>
            /// 西
            /// </summary>
            West
        }

        public GameObject gameObject;
        public Transform transform;
        public Mesh mesh;


        public TezNewTerrain terrainSystem;
        public TezNewTerrainFace north;
        public TezNewTerrainFace east;
        public TezNewTerrainFace south;
        public TezNewTerrainFace west;
        public TezNewTerrainFace parentTerrainFace;


        public int LOD = -1;
        public sbyte stitchMask = 0;
        public Vector3 center = Vector3.negativeInfinity;
        public TezNewTerrainUtility.CubeDirection direction = TezNewTerrainUtility.CubeDirection.Error;
        public Position facePosition = Position.Error;

        bool m_MeshExist = false;
        bool m_MeshVisible = false;
        bool m_IsSplit = false;
        bool m_UpdateIndex = false;

        TezNewTerrainFace[] m_Children = new TezNewTerrainFace[4];
        public ushort neighborMask = 0;

        /// <summary>
        /// 生成单个面
        /// </summary>
        public TezNewTerrainFace(TezNewTerrain terrain, TezNewTerrainUtility.CubeDirection direction)
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
        public TezNewTerrainFace(TezNewTerrainFace parentTerrainFace, Position facePosition)
        {
            this.LOD = parentTerrainFace.LOD - 1;

            this.terrainSystem = parentTerrainFace.terrainSystem;
            this.parentTerrainFace = parentTerrainFace;
            this.direction = parentTerrainFace.direction;
            this.facePosition = facePosition;

            this.calculateCenter(facePosition);
            this.calculateLocalPosition();
        }

        public void setNeighbor(TezNewTerrainFace north, TezNewTerrainFace east, TezNewTerrainFace south, TezNewTerrainFace west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

        private void calculateCenter(Position facePosition)
        {
            var config = terrainSystem.config;
            var half_length = config.meshSideHalfSize * config.scaleFactorTable[this.LOD];

            ///4个象限中
            switch (facePosition)
            {
                ///第3象限
                ///向此方向偏移
                ///需要 X轴减少,Y轴减少
                case Position.SouthWest:
                    this.calculateCenter(this.parentTerrainFace.center, -half_length, -half_length);
                    break;
                ///第4象限
                ///向此方向偏移
                ///需要 X轴增加,Y轴减少
                case Position.SouthEast:
                    this.calculateCenter(this.parentTerrainFace.center, half_length, -half_length);
                    break;
                ///第2象限
                ///向此方向偏移
                ///需要 X轴减少,Y轴增加
                case Position.NorthWest:
                    this.calculateCenter(this.parentTerrainFace.center, -half_length, half_length);
                    break;
                ///第1象限
                ///向此方向偏移
                ///需要 X轴增加,Y轴增加
                case Position.NorthEast:
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
                case TezNewTerrainUtility.CubeDirection.Top:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z + half_b;
                    break;
                case TezNewTerrainUtility.CubeDirection.Bottom:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z - half_b;
                    break;
                // Z轴定位
                case TezNewTerrainUtility.CubeDirection.Front:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y - half_b;
                    center.z = parentCenter.z;
                    break;
                case TezNewTerrainUtility.CubeDirection.Back:
                    center.x = parentCenter.x + half_a;
                    center.y = parentCenter.y + half_b;
                    center.z = parentCenter.z;
                    break;
                // X轴定位
                case TezNewTerrainUtility.CubeDirection.Left:
                    center.x = parentCenter.x;
                    center.y = parentCenter.y + half_a;
                    center.z = parentCenter.z + half_b;
                    break;
                case TezNewTerrainUtility.CubeDirection.Right:
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
            this.terrainSystem.addCMD_CreateMesh(this);
        }

        private void calculateLocalPosition()
        {
            this.terrainSystem.createGameObject(this);
        }

        public void scan(ref Vector3 flagWorldPosition)
        {
            //             var rate = Vector3.Dot(flagWorldPosition - this.terrainSystem.transform.position, this.transform.position - this.terrainSystem.transform.position);
            // 
            //             if (rate <= 0)
            //             {
            //                 if (m_MeshVisible)
            //                 {
            //                     m_MeshVisible = false;
            //                     this.gameObject.SetActive(m_MeshVisible);
            //                 }
            // 
            //                 return;
            //             }


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
                ///或者扫描分裂后的Face
                if (Vector3.Distance(flagWorldPosition, this.transform.position) < this.terrainSystem.splitThreshold[this.LOD])
                {
                    ///如果此Face没有分裂
                    ///则分裂
                    if (!m_IsSplit)
                    {
                        ///如果此Face有Mesh
                        ///把他的Mesh隐藏/删除
                        if (m_MeshExist)
                        {
                            m_MeshVisible = false;
                            this.gameObject.SetActive(m_MeshVisible);
                        }

                        ///分裂
                        this.split();
                    }

                    this.scanChildren(ref flagWorldPosition);
                }
                ///如果距离没有超过阈值
                else
                {
                    ///这是根节点
                    ///如果有Subface
                    ///就需要全部合并
                    if (m_IsSplit)
                    {
                        this.combineChildren();
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

                this.updateIndex();
            }
            ///如果达到最高LOD
            ///此Face必须有Mesh
            else
            {
                if (!m_MeshExist)
                {
                    this.createFace();
                }

                this.updateIndex();
            }
        }

        private void updateIndex()
        {
            if (m_UpdateIndex && m_MeshVisible)
            {
                m_UpdateIndex = false;
                terrainSystem.addCMD_UpdateIndex(this);
            }
        }

        private void scanChildren(ref Vector3 flagWorldPosition)
        {
            m_Children[(int)Position.SouthWest].scan(ref flagWorldPosition);
            m_Children[(int)Position.SouthEast].scan(ref flagWorldPosition);
            m_Children[(int)Position.NorthWest].scan(ref flagWorldPosition);
            m_Children[(int)Position.NorthEast].scan(ref flagWorldPosition);
        }

        private void split()
        {
            m_IsSplit = true;

            var sw = new TezNewTerrainFace(this, Position.SouthWest);
            var se = new TezNewTerrainFace(this, Position.SouthEast);
            var nw = new TezNewTerrainFace(this, Position.NorthWest);
            var ne = new TezNewTerrainFace(this, Position.NorthEast);

            m_Children[(int)Position.SouthWest] = sw;
            m_Children[(int)Position.SouthEast] = se;
            m_Children[(int)Position.NorthWest] = nw;
            m_Children[(int)Position.NorthEast] = ne;

            ///设定初始邻居
            ///
            sw.north = nw;
            sw.east = se;

            se.north = ne;
            se.west = sw;

            nw.south = sw;
            nw.east = ne;

            ne.south = se;
            ne.west = nw;

            /*
             * 这里需要注意一点
             * 要设定此Face的邻居
             * 其邻居Face的面积大小
             * 只能大于等于当前Face的面积
             * 
             * 原因很简单
             * 因为如果接壤的Face是两个小面积Face
             * 一个变量是装不下的
             * 
             * 所以只能反过来
             * 小面积Face设定大面积Face作为邻居
             */

            ///如果North的LOD等级等于当前face的等级
            if (this.north.LOD == this.LOD && this.north.m_IsSplit)
            {
                ///如果有,则把这两个face找出来
                ///以north的角度来看
                ///当前块是south
                ///所以为northface的sw和se
                this.north.findSubFaceByDirection(Direction.South, out var left, out var right);
                m_Children[(int)Position.NorthWest].setNeighbor(Direction.North, left);
                m_Children[(int)Position.NorthEast].setNeighbor(Direction.North, right);
                left.setNeighbor(Direction.South, m_Children[(int)Position.NorthWest]);
                right.setNeighbor(Direction.South, m_Children[(int)Position.NorthEast]);


                //                 left.setNeighbor(this, m_Children[(int)Position.NorthWest]);
                //                 right.setNeighbor(this, m_Children[(int)Position.NorthEast]);
            }
            else
            {
                ///如果LOD不同
                ///northface的面积必然大于当前face
                ///所以直接设定subface的NW和NE的north为当前块的northface
                m_Children[(int)Position.NorthWest].setNeighbor(Direction.North, this.north);
                m_Children[(int)Position.NorthEast].setNeighbor(Direction.North, this.north);
            }

            if (this.south.LOD == this.LOD && this.south.m_IsSplit)
            {

                ///以south的角度来看
                ///当前块是north
                ///所以为southface的NW和NE
                this.south.findSubFaceByDirection(Direction.North, out var left, out var right);
                m_Children[(int)Position.SouthWest].setNeighbor(Direction.South, left);
                m_Children[(int)Position.SouthEast].setNeighbor(Direction.South, right);
                left.setNeighbor(Direction.North, m_Children[(int)Position.SouthWest]);
                right.setNeighbor(Direction.North, m_Children[(int)Position.SouthEast]);

                //                 left.setNeighbor(this, m_Children[(int)Position.SouthWest]);
                //                 right.setNeighbor(this, m_Children[(int)Position.SouthEast]);
            }
            else
            {
                m_Children[(int)Position.SouthWest].setNeighbor(Direction.South, this.south);
                m_Children[(int)Position.SouthEast].setNeighbor(Direction.South, this.south);
            }

            if (this.west.LOD == this.LOD && this.west.m_IsSplit)
            {
                ///以west的角度来看
                ///当前块是esat
                ///所以为westface的SE和NE
                this.west.findSubFaceByDirection(Direction.East, out var left, out var right);
                m_Children[(int)Position.SouthWest].setNeighbor(Direction.West, left);
                m_Children[(int)Position.NorthWest].setNeighbor(Direction.West, right);
                left.setNeighbor(Direction.East, m_Children[(int)Position.SouthWest]);
                right.setNeighbor(Direction.East, m_Children[(int)Position.NorthWest]);

                //                 left.setNeighbor(this, m_Children[(int)Position.SouthWest]);
                //                 right.setNeighbor(this, m_Children[(int)Position.NorthWest]);
            }
            else
            {
                m_Children[(int)Position.SouthWest].setNeighbor(Direction.West, this.west);
                m_Children[(int)Position.NorthWest].setNeighbor(Direction.West, this.west);
            }

            if (this.east.LOD == this.LOD && this.east.m_IsSplit)
            {
                ///以east的角度来看
                ///当前块是west
                ///所以为eastface的SW和NW
                this.east.findSubFaceByDirection(Direction.West, out var left, out var right);
                m_Children[(int)Position.SouthEast].setNeighbor(Direction.East, left);
                m_Children[(int)Position.NorthEast].setNeighbor(Direction.East, right);
                left.setNeighbor(Direction.West, m_Children[(int)Position.SouthEast]);
                right.setNeighbor(Direction.West, m_Children[(int)Position.NorthEast]);

                //                 left.setNeighbor(this, m_Children[(int)Position.SouthEast]);
                //                 right.setNeighbor(this, m_Children[(int)Position.NorthEast]);
            }
            else
            {
                m_Children[(int)Position.SouthEast].setNeighbor(Direction.East, this.east);
                m_Children[(int)Position.NorthEast].setNeighbor(Direction.East, this.east);
            }
        }

        private void setNeighbor(Direction direction, TezNewTerrainFace terrainFace)
        {
            switch (direction)
            {
                case Direction.North:
                    this.north = terrainFace;
                    break;
                case Direction.East:
                    this.east = terrainFace;
                    break;
                case Direction.South:
                    this.south = terrainFace;
                    break;
                case Direction.West:
                    this.west = terrainFace;
                    break;
                default:
                    m_UpdateIndex = false;
                    break;
            }
            m_UpdateIndex = true;
        }

        private void findSubFaceByDirection(Direction direction, out TezNewTerrainFace left, out TezNewTerrainFace right)
        {
            left = null;
            right = null;

            switch (direction)
            {
                case Direction.North:
                    left = m_Children[(int)Position.NorthWest];
                    right = m_Children[(int)Position.NorthEast];
                    break;
                case Direction.East:
                    left = m_Children[(int)Position.SouthEast];
                    right = m_Children[(int)Position.NorthEast];
                    break;
                case Direction.South:
                    left = m_Children[(int)Position.SouthWest];
                    right = m_Children[(int)Position.SouthEast];
                    break;
                case Direction.West:
                    left = m_Children[(int)Position.SouthWest];
                    right = m_Children[(int)Position.NorthWest];
                    break;
                default:
                    break;
            }
        }

        private void setNeighbor(TezNewTerrainFace oldNeighborFace, TezNewTerrainFace newNeighborFace)
        {
            if (newNeighborFace == this)
            {
                return;
            }

            ///如果找到需要替换的位置
            if (oldNeighborFace == this.north)
            {
                this.north = newNeighborFace;
                return;
            }

            if (oldNeighborFace == this.south)
            {
                this.south = newNeighborFace;
                return;
            }

            if (oldNeighborFace == this.west)
            {
                this.west = newNeighborFace;
                return;
            }

            if (oldNeighborFace == this.east)
            {
                this.east = newNeighborFace;
                return;
            }
        }

        public TezNewTerrainUtility.StitchState calculateStitchMask()
        {
            TezNewTerrainUtility.StitchState stitchMask = TezNewTerrainUtility.StitchState.Normal;

            if (this.north.LOD > this.LOD)
            {
                stitchMask |= TezNewTerrainUtility.StitchState.NorthStitch;
            }

            if (this.south.LOD > this.LOD)
            {
                stitchMask |= TezNewTerrainUtility.StitchState.SouthStitch;
            }

            if (this.east.LOD > this.LOD)
            {
                stitchMask |= TezNewTerrainUtility.StitchState.EastStitch;
            }

            if (this.west.LOD > this.LOD)
            {
                stitchMask |= TezNewTerrainUtility.StitchState.WestStitch;
            }

            return stitchMask;
        }

        public int[] calculateMeshIndex()
        {
            return terrainSystem.config.getIndexTemplate((int)this.calculateStitchMask());
        }

        private void findNeighborFaceByNeighbor(TezNewTerrainFace flagFace, out TezNewTerrainFace left, out TezNewTerrainFace right)
        {
            left = null;
            right = null;

            ///north为标准
            if (this.north == flagFace)
            {
                ///找到north方向上分裂的subface
                left = m_Children[(int)Position.NorthWest];
                right = m_Children[(int)Position.NorthEast];
                return;
            }

            ///找到south方向上分裂的subface
            if (this.south == flagFace)
            {
                left = m_Children[(int)Position.SouthWest];
                right = m_Children[(int)Position.SouthEast];
                return;
            }

            ///找到east方向上分裂的subface
            if (this.east == flagFace)
            {
                left = m_Children[(int)Position.SouthEast];
                right = m_Children[(int)Position.NorthEast];
                return;
            }

            ///
            if (this.west == flagFace)
            {
                left = m_Children[(int)Position.SouthWest];
                right = m_Children[(int)Position.NorthWest];
                return;
            }

            /*
            ///north为标准
            if (this.north == flagFace.parentTerrainFace)
            {
                ///找到north方向上分裂的subface
                left = m_Children[(int)Position.NorthWest];
                right = m_Children[(int)Position.NorthEast];
                return;
            }

            ///找到south方向上分裂的subface
            if (this.south == flagFace.parentTerrainFace)
            {
                left = m_Children[(int)Position.SouthWest];
                right = m_Children[(int)Position.SouthEast];
                return;
            }

            ///找到east方向上分裂的subface
            if (this.east == flagFace.parentTerrainFace)
            {
                left = m_Children[(int)Position.SouthEast];
                right = m_Children[(int)Position.NorthEast];
                return;
            }

            ///
            if (this.west == flagFace.parentTerrainFace)
            {
                left = m_Children[(int)Position.SouthWest];
                right = m_Children[(int)Position.NorthWest];
                return;
            }
            */
        }

        private void split(Position facePosition, Vector3 flagWorldPosition)
        {
            var face = new TezNewTerrainFace(this, facePosition);
            face.scan(ref flagWorldPosition);
            m_Children[(int)facePosition] = face;
        }

        public void setChild(Position facePosition)
        {
            m_Children[(int)facePosition] = new TezNewTerrainFace(this, facePosition);
        }

        private void combine()
        {
            ///合并Subface
            if (m_IsSplit)
            {
                this.combineChildren();
            }
            ///
            else
            {
                m_MeshVisible = false;
                this.gameObject.SetActive(m_MeshVisible);
            }
        }

        private void combineChildren()
        {
            m_UpdateIndex = true;
            m_IsSplit = false;
            m_Children[(int)Position.SouthWest].combine();
            m_Children[(int)Position.SouthEast].combine();
            m_Children[(int)Position.NorthWest].combine();
            m_Children[(int)Position.NorthEast].combine();
        }

        #region Tool
        public void test_split()
        {
            if (this.LOD > 0)
            {
                this.test_split(Position.SouthWest);
                this.test_split(Position.SouthEast);
                this.test_split(Position.NorthWest);
                this.test_split(Position.NorthEast);
            }
            else
            {
                this.createFace();
            }
        }

        private void test_split(Position facePosition)
        {
            var face = new TezNewTerrainFace(this, facePosition);
            face.test_split();
            m_Children[(int)facePosition] = face;
        }
        #endregion
    }
}
