#define UseThread1

using UnityEngine;

namespace tezcat.Framework.Universe
{
    public class TezNewTerrainFace
    {
        public enum Position
        {
            NoSplit = -1,
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

        //         public enum Direction : int
        //         {
        //             Error = -1,
        //             /// <summary>
        //             /// 北
        //             /// </summary>
        //             North = 0,
        //             /// <summary>
        //             /// 东
        //             /// </summary>
        //             East,
        //             /// <summary>
        //             /// 南
        //             /// </summary>
        //             South,
        //             /// <summary>
        //             /// 西
        //             /// </summary>
        //             West
        //         }

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
        public TezNewTerrainUtility.StitchState stitchMask = TezNewTerrainUtility.StitchState.Reset;
        public TezNewTerrainUtility.CubeFace cubeFace = TezNewTerrainUtility.CubeFace.Error;
        public Position facePosition = Position.NoSplit;
        public Vector3 center = Vector3.negativeInfinity;

        bool m_MeshExist = false;
        bool m_MeshVisible = false;
        bool m_IsSplit = false;
        bool m_UpdateIndex = false;

        TezNewTerrainFace[] m_Children = null;

        /// <summary>
        /// 生成单个面
        /// </summary>
        public TezNewTerrainFace(TezNewTerrain terrain, TezNewTerrainUtility.CubeFace cubeFace)
        {
            this.LOD = terrain.maxLODLevel - 1;
            this.terrainSystem = terrain;
            this.cubeFace = cubeFace;
            this.center = TezNewTerrainUtility.CubeFaceVectors[(int)cubeFace];

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
            this.cubeFace = parentTerrainFace.cubeFace;
            this.facePosition = facePosition;

            this.calculateCenter(facePosition);
            this.calculateLocalPosition();
        }

        public void initCubeNeighbor(TezNewTerrainFace north, TezNewTerrainFace east, TezNewTerrainFace south, TezNewTerrainFace west)
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

        private void calculateCenter(Vector3 parentCenter, float halfA, float halfB)
        {
            ///以Top为标准面
            ///即X轴Z轴为标准
            ///旋转到各个面计算坐标轴的变换

            switch (this.cubeFace)
            {
                // Y轴定位
                case TezNewTerrainUtility.CubeFace.Top:
                    center.x = parentCenter.x + halfA;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z + halfB;
                    break;
                case TezNewTerrainUtility.CubeFace.Down:
                    center.x = parentCenter.x + halfA;
                    center.y = parentCenter.y;
                    center.z = parentCenter.z - halfB;
                    break;
                // Z轴定位
                case TezNewTerrainUtility.CubeFace.Front:
                    center.x = parentCenter.x + halfA;
                    center.y = parentCenter.y - halfB;
                    center.z = parentCenter.z;
                    break;
                case TezNewTerrainUtility.CubeFace.Back:
                    center.x = parentCenter.x + halfA;
                    center.y = parentCenter.y + halfB;
                    center.z = parentCenter.z;
                    break;
                // X轴定位
                case TezNewTerrainUtility.CubeFace.Left:
                    center.x = parentCenter.x;
                    center.y = parentCenter.y + halfA;
                    center.z = parentCenter.z + halfB;
                    break;
                case TezNewTerrainUtility.CubeFace.Right:
                    center.x = parentCenter.x;
                    center.y = parentCenter.y - halfA;
                    center.z = parentCenter.z + halfB;
                    break;
                default:
                    break;
            }
        }

        private void calculateLocalPosition()
        {
            this.terrainSystem.createGameObject(this);
        }

        private void split()
        {
            m_IsSplit = true;
            if (m_Children == null)
            {
                m_Children = new TezNewTerrainFace[4]
                {
                    new TezNewTerrainFace(this, Position.SouthWest),
                    new TezNewTerrainFace(this, Position.SouthEast),
                    new TezNewTerrainFace(this, Position.NorthWest),
                    new TezNewTerrainFace(this, Position.NorthEast)
                };
            }

            this.calculateNeighbor();
        }

        private void calculateNeighbor()
        {
            var sw = m_Children[(int)Position.SouthWest];
            var se = m_Children[(int)Position.SouthEast];
            var nw = m_Children[(int)Position.NorthWest];
            var ne = m_Children[(int)Position.NorthEast];

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

            TezNewTerrainFace low, high;
            int mask;
            ///如果North的LOD等级等于当前face的等级
            ///north则有可能是分裂过的Face
            if (this.north.LOD == this.LOD && this.north.m_IsSplit)
            {
                ///如果有,则把这两个face找出来
                ///以north的角度来看
                ///当前块是south
                ///所以为northface的sw和se
                ///
                mask = 1 << (int)this.north.cubeFace | 1 << (int)this.cubeFace;
                this.north.findSubFaceBy(mask, TezNewTerrainUtility.Direction.South, out low, out high);
                this.setNeighbor(mask, TezNewTerrainUtility.Direction.North, low, high);
                this.north.setNeighbor(mask, TezNewTerrainUtility.Direction.South, nw, ne);
            }
            else
            {
                ///如果LOD不同
                ///northface的面积必然大于等于当前face
                ///所以直接设定subface的NW和NE的north为当前块的northface
                nw.setNeighbor(TezNewTerrainUtility.Direction.North, this.north);
                ne.setNeighbor(TezNewTerrainUtility.Direction.North, this.north);
            }

            if (this.south.LOD == this.LOD && this.south.m_IsSplit)
            {
                ///以south的角度来看
                ///当前块是north
                mask = 1 << (int)this.south.cubeFace | 1 << (int)this.cubeFace;
                this.south.findSubFaceBy(mask, TezNewTerrainUtility.Direction.North, out low, out high);
                this.setNeighbor(mask, TezNewTerrainUtility.Direction.South, low, high);
                this.south.setNeighbor(mask, TezNewTerrainUtility.Direction.North, sw, se);
            }
            else
            {
                sw.setNeighbor(TezNewTerrainUtility.Direction.South, this.south);
                se.setNeighbor(TezNewTerrainUtility.Direction.South, this.south);
            }

            if (this.west.LOD == this.LOD && this.west.m_IsSplit)
            {
                ///以west的角度来看
                ///当前块是esat
                mask = 1 << (int)this.west.cubeFace | 1 << (int)this.cubeFace;
                this.west.findSubFaceBy(mask, TezNewTerrainUtility.Direction.East, out low, out high);
                this.setNeighbor(mask, TezNewTerrainUtility.Direction.West, low, high);
                this.west.setNeighbor(mask, TezNewTerrainUtility.Direction.East, sw, nw);
            }
            else
            {
                sw.setNeighbor(TezNewTerrainUtility.Direction.West, this.west);
                nw.setNeighbor(TezNewTerrainUtility.Direction.West, this.west);
            }

            if (this.east.LOD == this.LOD && this.east.m_IsSplit)
            {
                ///以east的角度来看
                ///当前块是west
                ///所以为eastface的SW和NW
                mask = 1 << (int)this.east.cubeFace | 1 << (int)this.cubeFace;
                this.east.findSubFaceBy(mask, TezNewTerrainUtility.Direction.West, out low, out high);
                this.setNeighbor(mask, TezNewTerrainUtility.Direction.East, low, high);
                this.east.setNeighbor(mask, TezNewTerrainUtility.Direction.West, se, ne);
            }
            else
            {
                se.setNeighbor(TezNewTerrainUtility.Direction.East, this.east);
                ne.setNeighbor(TezNewTerrainUtility.Direction.East, this.east);
            }

            sw.updateMeshIndex();
            se.updateMeshIndex();
            nw.updateMeshIndex();
            ne.updateMeshIndex();
        }

        private void findSubFaceBy(int mask, TezNewTerrainUtility.Direction direction, out TezNewTerrainFace low, out TezNewTerrainFace high)
        {
            low = null;
            high = null;

            var temp = (TezNewTerrainUtility.Group)(mask | (int)direction);
            switch (temp)
            {
                ///Left-Front相接时,L分裂,North邻居(Front)查其理论South
                case TezNewTerrainUtility.Group.LN_FS:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    break;
                ///Left-Front相接时,F分裂,West邻居(Left)查其理论East
                case TezNewTerrainUtility.Group.FW_LE:
                    low = m_Children[(int)Position.NorthWest];
                    high = m_Children[(int)Position.NorthEast];
                    break;
                ///Left-Down相接时,D分裂,West邻居(Left)查其理论East
                case TezNewTerrainUtility.Group.LW_DE_or_DW_LE:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    break;
                ///Left-Back相接时,L分裂,South邻居(Back)查其理论North
                case TezNewTerrainUtility.Group.LS_BN:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    break;
                ///Left-Back相接时,B分裂,West邻居(Left)查其理论East
                case TezNewTerrainUtility.Group.BW_LE:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.SouthEast];
                    break;


                ///Right-Front相接时,F分裂,East邻居(Right)查其理论West
                case TezNewTerrainUtility.Group.FE_RW:
                    low = m_Children[(int)Position.NorthWest];
                    high = m_Children[(int)Position.NorthEast];
                    break;
                ///Right-Front相接时,R分裂,North邻居(Front)查其理论South
                case TezNewTerrainUtility.Group.RN_FS:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    break;
                ///Right-Down相接时
                ///R分裂,East邻居(Down)查其理论West
                ///D分裂,East邻居(Right)查其理论West
                case TezNewTerrainUtility.Group.RE_DW_or_DE_RW:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    break;
                ///Right-Back相接时,B分裂,East邻居(Right)查其理论West
                case TezNewTerrainUtility.Group.BE_RW:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.SouthEast];
                    break;
                ///Right-Back相接时,R分裂,South邻居(Back)查其理论North
                case TezNewTerrainUtility.Group.RS_BN:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    break;
                default:
                    switch (direction)
                    {
                        case TezNewTerrainUtility.Direction.North:
                            low = m_Children[(int)Position.NorthWest];
                            high = m_Children[(int)Position.NorthEast];
                            break;
                        case TezNewTerrainUtility.Direction.East:
                            low = m_Children[(int)Position.SouthEast];
                            high = m_Children[(int)Position.NorthEast];
                            break;
                        case TezNewTerrainUtility.Direction.South:
                            low = m_Children[(int)Position.SouthWest];
                            high = m_Children[(int)Position.SouthEast];
                            break;
                        case TezNewTerrainUtility.Direction.West:
                            low = m_Children[(int)Position.SouthWest];
                            high = m_Children[(int)Position.NorthWest];
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        private void setNeighbor(TezNewTerrainUtility.Direction direction, TezNewTerrainFace terrainFace)
        {
            switch (direction)
            {
                case TezNewTerrainUtility.Direction.North:
                    this.north = terrainFace;
                    break;
                case TezNewTerrainUtility.Direction.East:
                    this.east = terrainFace;
                    break;
                case TezNewTerrainUtility.Direction.South:
                    this.south = terrainFace;
                    break;
                case TezNewTerrainUtility.Direction.West:
                    this.west = terrainFace;
                    break;
                default:
                    break;
            }
        }

        public void setNeighbor(int faceMask, TezNewTerrainUtility.Direction direction, TezNewTerrainFace inLow, TezNewTerrainFace inHigh)
        {
            TezNewTerrainFace low = null, high = null;
            TezNewTerrainUtility.Group temp = (TezNewTerrainUtility.Group)(faceMask | (int)direction);
            switch (temp)
            {
                ///Left-Front相接时,设置Left的North方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_LN_FS:
                case TezNewTerrainUtility.Group.FW_LE:
                    low = m_Children[(int)Position.NorthWest];
                    high = m_Children[(int)Position.NorthEast];
                    low.north = inHigh;
                    high.north = inLow;
                    break;

                ///Front-Left相接时,设置Front的West方向的Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_FW_LE:
                case TezNewTerrainUtility.Group.LN_FS:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    low.west = inHigh;
                    high.west = inLow;
                    break;

                ///Left-Down相接时,设置Left/Down的West方向的Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_LW_DE_or_DW_LE:
                case TezNewTerrainUtility.Group.LW_DE_or_DW_LE:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    low.west = inHigh;
                    high.west = inLow;
                    break;

                ///Left-Back相接时,设置Back的West方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_BW_LE:
                case TezNewTerrainUtility.Group.LS_BN:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.NorthWest];
                    low.west = inLow;
                    high.west = inHigh;
                    break;

                ///Left-Back相接时,设置Left的South方向的Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_LS_BN:
                case TezNewTerrainUtility.Group.BW_LE:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.SouthEast];
                    low.south = inLow;
                    high.south = inHigh;
                    break;

                ///Right-Front相接时,设置Right的North方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_RN_FS:
                case TezNewTerrainUtility.Group.FE_RW:
                    low = m_Children[(int)Position.NorthWest];
                    high = m_Children[(int)Position.NorthEast];
                    low.north = inLow;
                    high.north = inHigh;
                    break;
                ///Right-Front相接时,设置Front的East方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_FE_RW:
                case TezNewTerrainUtility.Group.RN_FS:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    low.east = inLow;
                    high.east = inHigh;
                    break;
                ///Right-Down相接时,设置Front/Down的East方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_RE_DW_or_DE_RW:
                case TezNewTerrainUtility.Group.RE_DW_or_DE_RW:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    low.east = inHigh;
                    high.east = inLow;
                    break;
                ///Right-Back相接时,设置Back的East方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_BE_RW:
                case TezNewTerrainUtility.Group.RS_BN:
                    low = m_Children[(int)Position.SouthEast];
                    high = m_Children[(int)Position.NorthEast];
                    low.east = inHigh;
                    high.east = inLow;
                    break;
                ///Right-Back相接时,设置Right的South方向Subface的邻居
                case TezNewTerrainUtility.Group.SelfUse_RS_BN:
                case TezNewTerrainUtility.Group.BE_RW:
                    low = m_Children[(int)Position.SouthWest];
                    high = m_Children[(int)Position.SouthEast];
                    low.south = inHigh;
                    high.south = inLow;
                    break;
                default:
                    switch (direction)
                    {
                        case TezNewTerrainUtility.Direction.North:
                            low = m_Children[(int)Position.NorthWest];
                            high = m_Children[(int)Position.NorthEast];
                            low.north = inLow;
                            high.north = inHigh;
                            break;
                        case TezNewTerrainUtility.Direction.East:
                            low = m_Children[(int)Position.SouthEast];
                            high = m_Children[(int)Position.NorthEast];
                            low.east = inLow;
                            high.east = inHigh;
                            break;
                        case TezNewTerrainUtility.Direction.South:
                            low = m_Children[(int)Position.SouthWest];
                            high = m_Children[(int)Position.SouthEast];
                            low.south = inLow;
                            high.south = inHigh;
                            break;
                        case TezNewTerrainUtility.Direction.West:
                            low = m_Children[(int)Position.SouthWest];
                            high = m_Children[(int)Position.NorthWest];
                            low.west = inLow;
                            high.west = inHigh;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            low.updateMeshIndex();
            high.updateMeshIndex();
        }
        private void combine()
        {
            if (m_IsSplit)
            {
                m_IsSplit = false;

                this.stitchMask = TezNewTerrainUtility.StitchState.Normal;

                int mask;
                if (this.north.LOD == this.LOD && this.north.m_IsSplit)
                {
                    mask = 1 << (int)this.north.cubeFace | 1 << (int)this.cubeFace;
                    this.north.setNeighbor(mask, TezNewTerrainUtility.Direction.South, this, this);
                }

                if (this.south.LOD == this.LOD && this.south.m_IsSplit)
                {
                    mask = 1 << (int)this.south.cubeFace | 1 << (int)this.cubeFace;
                    this.south.setNeighbor(mask, TezNewTerrainUtility.Direction.North, this, this);
                }

                if (this.east.LOD == this.LOD && this.east.m_IsSplit)
                {
                    mask = 1 << (int)this.east.cubeFace | 1 << (int)this.cubeFace;
                    this.east.setNeighbor(mask, TezNewTerrainUtility.Direction.West, this, this);
                }

                if (this.west.LOD == this.LOD && this.west.m_IsSplit)
                {
                    mask = 1 << (int)this.west.cubeFace | 1 << (int)this.cubeFace;
                    this.west.setNeighbor(mask, TezNewTerrainUtility.Direction.East, this, this);
                }

                this.updateMeshIndex();
                this.combineChildren();
            }
        }

        private void combineSelf()
        {
            this.stitchMask = TezNewTerrainUtility.StitchState.Normal;
            ///合并Subface
            if (m_IsSplit)
            {
                m_IsSplit = false;
                this.combineChildren();
            }
            ///
            else
            {
                m_MeshVisible = false;
                this.gameObject.SetActive(m_MeshVisible);
            }
        }

        /// <summary>
        /// 合并此Face的Subface
        /// 使当前Face成为根Face
        /// </summary>
        private void combineChildren()
        {
            m_Children[(int)Position.SouthWest].combineSelf();
            m_Children[(int)Position.SouthEast].combineSelf();
            m_Children[(int)Position.NorthWest].combineSelf();
            m_Children[(int)Position.NorthEast].combineSelf();
        }

        public void addUpdateChildren()
        {
            terrainSystem.addUpdateFace(m_Children[(int)Position.SouthWest]);
            terrainSystem.addUpdateFace(m_Children[(int)Position.SouthEast]);
            terrainSystem.addUpdateFace(m_Children[(int)Position.NorthWest]);
            terrainSystem.addUpdateFace(m_Children[(int)Position.NorthEast]);
        }

        public void update(ref Vector3 flagWorldPosition)
        {
            if(terrainSystem.clipping)
            {
                var rate = Vector3.Dot(flagWorldPosition - this.terrainSystem.transform.position, this.transform.position - this.terrainSystem.transform.position);

                if (rate <= 0)
                {
                    if (m_MeshVisible)
                    {
                        m_MeshVisible = false;
                        this.gameObject.SetActive(m_MeshVisible);
                    }

                    return;
                }
            }


            if (this.LOD > 0)
            {
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
                        ///
                        this.split();
                    }

                    this.addUpdateChildren();
                }
                ///如果距离没有超过阈值
                else
                {
                    ///这是根节点
                    ///如果有Subface
                    ///就需要全部合并
                    this.combine();
                    this.updateSelf();
                }
            }
            ///如果达到最高LOD
            ///此Face必须有Mesh
            else
            {
                this.updateSelf();
            }
        }

        private void updateSelf()
        {
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
                    this.updateMeshIndex();
                }
            }
            else
            {
                ///将自身生成出来
                this.createMesh();
            }
        }

        public void updateChildren(ref Vector3 flagWorldPosition)
        {
            m_Children[(int)Position.SouthWest].update(ref flagWorldPosition);
            m_Children[(int)Position.SouthEast].update(ref flagWorldPosition);
            m_Children[(int)Position.NorthWest].update(ref flagWorldPosition);
            m_Children[(int)Position.NorthEast].update(ref flagWorldPosition);
        }

        public bool needChangeStitchMask(out TezNewTerrainUtility.StitchState newStitchMask)
        {
            newStitchMask = this.calculateStitchMask();
            return newStitchMask != this.stitchMask;
        }

        private TezNewTerrainUtility.StitchState calculateStitchMask()
        {
            TezNewTerrainUtility.StitchState stitch_mask = TezNewTerrainUtility.StitchState.Normal;

            if (this.north.LOD > this.LOD)
            {
                stitch_mask |= TezNewTerrainUtility.StitchState.NorthStitch;
            }

            if (this.south.LOD > this.LOD)
            {
                stitch_mask |= TezNewTerrainUtility.StitchState.SouthStitch;
            }

            if (this.east.LOD > this.LOD)
            {
                stitch_mask |= TezNewTerrainUtility.StitchState.EastStitch;
            }

            if (this.west.LOD > this.LOD)
            {
                stitch_mask |= TezNewTerrainUtility.StitchState.WestStitch;
            }

            return stitch_mask;
        }

        public int[] calculateMeshIndex(TezNewTerrainUtility.StitchState mask)
        {
            this.stitchMask = mask;
            return terrainSystem.config.getIndexTemplate((int)this.stitchMask);
        }

        #region CMD
        public void createMesh()
        {
            m_MeshExist = true;
            m_MeshVisible = true;
            this.terrainSystem.addCMD_CreateMesh(this);
        }

        /// <summary>
        /// 每次更新时自动扫描
        /// </summary>
        private void updateMeshIndex()
        {
            ///Mesh必须可见
            ///才能更新
            if (m_MeshVisible && !m_UpdateIndex)
            {
                m_UpdateIndex = true;
                terrainSystem.addCMD_UpdateIndex(this);
            }
        }

        public void updateMeshIndexComplete()
        {
            m_UpdateIndex = false;
        }
        #endregion

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
                this.createMesh();
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
