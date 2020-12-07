using Assets.Scripts.Constants;
using IO.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace IO.ADT
{
    public static class ADTData
    {
        public static ConcurrentQueue<ADTModel> EnqueuedADTs = new ConcurrentQueue<ADTModel>();
    }

    public class ADTModel
    {
        public uint X;
        public uint Y;
        public uint MapId;
        public uint FileDataId;
        public uint WdtFileDataId;

        public bool HasMTXP = false;

        public List<MCNKChunk> MCNKs = new List<MCNKChunk>();
        public List<TexMCNKChunk> TexMCNKs = new List<TexMCNKChunk>();

        public List<uint> TextureFileDataId = new List<uint>();
        public Dictionary<uint, TextureData> TextureDatas = new Dictionary<uint, TextureData>();

        // Store Doodads and WMOs by UniqueId.
        public Dictionary<int, MDDF> DoodadInstances = new Dictionary<int, MDDF>();
    }

    public class MCNKChunk
    {
        public uint Flags;
        public uint IndexX;
        public uint IndexY;
        public uint LayerCount;
        public byte[] HolesHighRes;
        public ushort HolesLowRes;
        public Vector3 MeshPosition;

        public float[] Vertices;
        public Vector3[] Normals;
        public Vector3[] VertexArray;
        public int[] TriangleArray;
        public Vector2[] UVs;
    }

    public class TexMCNKChunk
    {
        public uint LayerCount;
        public uint[] TextureIds;
        public uint[] LayerOffsetInMCAL;
        public bool[] AlphaMapCompressed;
        public MCAL[] AlphaLayers;
    }

    public class MCAL
    {
        public byte[] Layer;
    }

    public class MDDF
    {
        public uint FileDataId;
        public int UniqueId;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        public MDDFFlags Flags;
    }
}