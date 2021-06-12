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

        public Transform transform;
        /// <summary>
        /// Cell的配置信息
        /// </summary>
        public TezNewTerrainConfig config;

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

        List<TezNewTerrainCMD> m_UpdateList = new List<TezNewTerrainCMD>();
        List<TezNewTerrainCMD> m_UpdateIndexList = new List<TezNewTerrainCMD>();

        public virtual void init(int maxLOD, float length)
        {
            this.maxLOD = maxLOD;
            this.splitThreshold = new float[maxLOD + 1];
            m_Length = length;
        }

        public void sendData()
        {
            this.sendData(m_UpdateList);
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

        public virtual void createGameObject(TezNewTerrainFace terrainFace)
        {
            GameObject go = new GameObject(terrainFace.cubeFace.ToString());
            var local = terrainFace.center;
            TezNewTerrainUtility.smoothNormalized(ref local);
            local *= terrainFace.terrainSystem.radius;

            terrainFace.gameObject = go;
            terrainFace.transform = go.transform;

            onCreateGameObject(go, local);
        }


        public virtual void test_split() { }
        public virtual void update(Vector3 flagWorldPosition) { }

        public void addCMD(TezNewTerrainCMD cmd)
        {
            m_UpdateList.Add(cmd);
        }

        public void addCMD_UpdateIndex(TezNewTerrainFace terrainFace)
        {
            m_UpdateIndexList.Add(new TezNewTerrainCMD_IndexUpdate()
            {
                terrainFace = terrainFace
            });
        }

        public abstract void addCMD_CreateMesh(TezNewTerrainFace terrainFace);
    }
}
