﻿#define UseThread1

using System;
using UnityEngine;

namespace tezcat.Framework.Universe
{
    public abstract class TezNewTerrainCMD
    {
        public TezNewTerrainFace terrainFace;
        public abstract void sendData();
    }

    public class TezNewTerrainCMD_IndexUpdate : TezNewTerrainCMD
    {
        public override void sendData()
        {
            terrainFace.mesh.triangles = terrainFace.calculateMeshIndex();
            terrainFace.mesh.RecalculateNormals();
        }
    }

    public class TezNewTerrainCMD_CeateMesh : TezNewTerrainCMD
    {
        public System.Action<TezNewTerrainCMD_CeateMesh> onCreateData;

        public override void sendData()
        {
            this.onCreateData(this);
        }
    }
}
