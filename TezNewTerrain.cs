#define UseThread1

using System;
using System.Collections.Generic;
using UnityEngine;

namespace tezcat.Framework.Universe
{

    public abstract class TezNewTerrain
    {
        public event System.Action<TezNewTerrainVertexData> onCreateMesh;
        public event System.Action<GameObject, Vector3> onCreateGameObject;

        public Transform transform;
        /// <summary>
        /// Cell的配置信息
        /// </summary>
        public TezNewTerrainCellConfig cellConfig;

        /// <summary>
        /// 最大LOD
        /// </summary>
        public int maxLOD;

        /// <summary>
        /// 分裂阈值
        /// </summary>
        public float[] splitThreshold;

        public float sideLength => m_Length;
        public float radius => m_Length;

        float m_Length;

        List<TezNewTerrainVertexData> m_UpdateList = new List<TezNewTerrainVertexData>();

        public virtual void init(int maxLOD, float length)
        {
            this.maxLOD = maxLOD;
            this.splitThreshold = new float[maxLOD + 1];
            m_Length = length;
        }

        public void sendData()
        {
            if (m_UpdateList.Count > 0)
            {
                for (int i = 0; i < m_UpdateList.Count; i++)
                {
                    this.createMesh(m_UpdateList[i]);
                }
                m_UpdateList.Clear();
            }
        }

        protected virtual void createMesh(TezNewTerrainVertexData vertexData)
        {
            var mask = vertexData.terrainFace.stitchMask;
            string name = "Cell-";
            if ((mask & (sbyte)TezNewTerrainCellConfig.StitchState.NorthStitch) != (sbyte)TezNewTerrainCellConfig.StitchState.Normal)
            {
                name += "North";
            }

            if ((mask & (sbyte)TezNewTerrainCellConfig.StitchState.EastStitch) != (sbyte)TezNewTerrainCellConfig.StitchState.Normal)
            {
                name += "East";
            }

            if ((mask & (sbyte)TezNewTerrainCellConfig.StitchState.SouthStitch) != (sbyte)TezNewTerrainCellConfig.StitchState.Normal)
            {
                name += "South";
            }

            if ((mask & (sbyte)TezNewTerrainCellConfig.StitchState.WestStitch) != (sbyte)TezNewTerrainCellConfig.StitchState.Normal)
            {
                name += "West";
            }
            vertexData.name = name;

            onCreateMesh(vertexData);
        }

        public void addVertexData(TezNewTerrainVertexData vertexData)
        {
            m_UpdateList.Add(vertexData);
        }

        public virtual void createGameObject(TezNewTerrainFace terrainFace)
        {
            GameObject go = new GameObject(terrainFace.direction.ToString());
            var local = terrainFace.center;
            TezNewTerrainUtility.smoothNormalized(ref local);
            local *= terrainFace.terrainSystem.radius;

            terrainFace.gameObject = go;
            terrainFace.transform = go.transform;

            onCreateGameObject(go, local);
        }


        public abstract void addFace(TezNewTerrainFace terrainFace);

        public virtual void split() { }
        public virtual void scan(Vector3 flagWorldPosition) { }

    }
}
