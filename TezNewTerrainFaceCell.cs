#define UseThread1


namespace tezcat.Framework.Universe
{
    public class TezNewTerrainFaceCell
    {
        public int LOD;
        public float stdSide;
        public float scale;

        public TezNewTerrainFaceCell(int LOD, float stdSide, int resolution, float rectSize)
        {
            this.LOD = LOD;
            this.stdSide = stdSide;
            this.scale = rectSize / (float)resolution;
        }

        #region Info
        static TezNewTerrainFaceCell[] LODList = null;

        public static void initFaceData(int maxLod, float stdSide, int resolution, float rectSize)
        {
            int temp = maxLod + 1;

            LODList = new TezNewTerrainFaceCell[temp];
            for (int i = 0; i < temp; i++)
            {
                LODList[i] = new TezNewTerrainFaceCell(i, stdSide, resolution, rectSize);
                rectSize *= 0.5f;
            }
        }

        public static TezNewTerrainFaceCell getFaceData(int lod)
        {
            return LODList[lod];
        }
        #endregion

    }
}
