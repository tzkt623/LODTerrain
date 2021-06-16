#define UseThread1

using System;
using System.Collections.Generic;
using UnityEngine;

namespace tezcat.Framework.Universe
{

    public abstract class TezNewTerrain
    {
        public event System.Action<TezNewTerrainCMD_CeateMesh> onCreateMesh;
        public event System.Action<GameObject, Vector3> onCreateGameObject;

        Queue<TezNewTerrainFace> m_UpdateFaceQueue = new Queue<TezNewTerrainFace>();

        public bool clipping;

        public Transform transform;
        /// <summary>
        /// Cell的配置信息
        /// </summary>
        public TezNewTerrainConfig config;

        /// <summary>
        /// 最大LOD
        /// </summary>
        public int maxLODLevel;

        /// <summary>
        /// 分裂阈值
        /// </summary>
        public float[] splitThreshold;

        public float sideLength => m_Length;
        public float radius => m_Length;

        float m_Length;

        List<TezNewTerrainCMD> m_UpdateList = new List<TezNewTerrainCMD>();
        List<TezNewTerrainCMD> m_UpdateIndexList = new List<TezNewTerrainCMD>();

        public virtual void init(int maxLODLevel, float length)
        {
            this.maxLODLevel = maxLODLevel;
            this.splitThreshold = new float[maxLODLevel];
            m_Length = length;
        }

        public virtual void createGameObject(TezNewTerrainFace terrainFace)
        {
            GameObject go = new GameObject(terrainFace.cubeFace.ToString());
            go.AddComponent<TezNewPlanetFaceGMO>().face = terrainFace;

            var local = terrainFace.center;
            TezNewTerrainUtility.smoothNormalized(ref local);
            local *= terrainFace.terrainSystem.radius;

            terrainFace.gameObject = go;
            terrainFace.transform = go.transform;

            onCreateGameObject(go, local);
        }


        public void addUpdateFace(TezNewTerrainFace terrainFace)
        {
            m_UpdateFaceQueue.Enqueue(terrainFace);
        }

        public void addCMD(TezNewTerrainCMD cmd)
        {
            m_UpdateList.Add(cmd);
        }

        public void addCMD_UpdateIndex(TezNewTerrainFace terrainFace)
        {
            m_UpdateIndexList.Add(new TezNewTerrainCMD_IndexUpdate()
            {
                terrainFace = terrainFace,
            });
        }

        public abstract void addCMD_CreateMesh(TezNewTerrainFace terrainFace);
        public virtual void update(Vector3 flagWorldPosition)
        {

        }

        protected void updateFace(ref Vector3 flagWorldPosition)
        {
            while (m_UpdateFaceQueue.Count > 0)
            {
                m_UpdateFaceQueue.Dequeue().update(ref flagWorldPosition);
            }
        }

        public void sendData()
        {
            this.sendData(m_UpdateList);

//             if(m_UpdateIndexList.Count > 0)
//             {
//                 Debug.Log(m_UpdateIndexList.Count);
//             }
            this.sendData(m_UpdateIndexList);
        }

        private void sendData(List<TezNewTerrainCMD> cmds)
        {
            if (cmds.Count > 0)
            {
                for (int i = 0; i < cmds.Count; i++)
                {
                    cmds[i].sendData();
                }
                cmds.Clear();
            }
        }

        public virtual void test_split() { }
    }
}
