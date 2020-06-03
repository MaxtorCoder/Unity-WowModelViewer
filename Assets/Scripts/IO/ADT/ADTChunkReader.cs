using System.Collections;
using System.Collections.Generic;
using System.IO;
using Constants;
using UnityEngine;
using Util;

namespace IO.ADT
{
    public static partial class ADTReader
    {
        /// <summary>
        /// Read the MCNK chunk and fills the <see cref="ADTModel"/> and <see cref="MCNKChunk"/>
        /// </summary>
        public static void ReadMCNK(BinaryReader reader, ADTModel model, uint mcnkSize)
        {
            var startPosition = reader.BaseStream.Position;
            var mcnk = new MCNKChunk();

            mcnk.Flags = reader.ReadUInt32();
            mcnk.IndexX = reader.ReadUInt32();
            mcnk.IndexY = reader.ReadUInt32();
            mcnk.LayerCount = reader.ReadUInt32();
            var doodadRefCount = reader.ReadUInt32();

            mcnk.HolesHighRes = new byte[8];
            for (var i = 0; i < 8; ++i)
                mcnk.HolesHighRes[i] = reader.ReadByte();

            var layerOffset = reader.ReadUInt32();
            var refOffset = reader.ReadUInt32();
            var alphaOffset = reader.ReadUInt32();
            var alphaSize = reader.ReadUInt32();
            var shadowOffset = reader.ReadUInt32();
            var shadowSize = reader.ReadUInt32();
            var areaId = reader.ReadUInt32();
            var mapObjectRefs = reader.ReadUInt32();
            mcnk.HolesLowRes = reader.ReadUInt16();
            var unknown1 = reader.ReadUInt16();

            var lowQualityTexture = new uint[4];
            for (var i = 0; i < 4; ++i)
                lowQualityTexture[i] = reader.ReadUInt32();

            var predTex = reader.ReadUInt32();
            var noEffectDoodad = reader.ReadUInt32();
            var soundEmitterOffset = reader.ReadInt32();
            var soundEmitterCount = reader.ReadInt32();
            var liquidOffset = reader.ReadInt32();
            var liquidSize = reader.ReadInt32();
            
            mcnk.MeshPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var mccvOffset = reader.ReadInt32();
            var mclvOffset = reader.ReadInt32();
            reader.ReadInt32(); // Unused

            while (reader.BaseStream.Position < startPosition + mcnkSize)
            {
                var chunkId = (Chunks) reader.ReadUInt32();
                var chunkSize = reader.ReadUInt32();

                switch (chunkId)
                {
                    case Chunks.MCVT:        // Vertex Heights
                        mcnk.VertexHeights = ReadMCVT(reader);
                        break;
                    case Chunks.MCNR:        // Normals
                        mcnk.VertexNormals = ReadMCNR(reader);
                        break;
                    default:
                        reader.Skip(chunkSize);
                        // Debug.Log($"MCNK - Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                        break;
                }
            }

            model.MCNKs.Add(mcnk);
        }

        public static void ReadTexMCNK(BinaryReader reader, uint mcnkSize, ADTModel model)
        {
            var startPosition = reader.BaseStream.Position;
            var texMcnk = new TexMCNKChunk();

            while (reader.BaseStream.Position < startPosition + mcnkSize)
            {
                var chunkId = (Chunks)reader.ReadUInt32();
                var chunkSize = reader.ReadUInt32();

                switch (chunkId)
                {
                    case Chunks.MCLY:       // Texture Layers.
                        ReadMCLY(reader, chunkSize, texMcnk);
                        break;
                    default:
                        reader.Skip(chunkSize);
                        // Debug.Log($"MCNK - Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                        break;
                }
            }

            model.TexMCNKs.Add(texMcnk);
        }

        /// <summary>
        /// Read the Textures from the Tex0 adt.
        /// </summary>
        public static void ReadTextures(BinaryReader reader, uint chunkSize, ADTModel data)
        {
            var textureSize = chunkSize / 4;
            for (var i = 0; i < textureSize; ++i)
            {
                var texFileDataId = reader.ReadUInt32();
                if (!data.TextureFileDataId.Contains(texFileDataId))
                {
                    var textureData = BLP.Open(texFileDataId);
                    if (textureData == null)
                        continue;

                    data.TextureFileDataId.Add(texFileDataId);
                    data.TextureDatas.Add(texFileDataId, textureData);
                }
            }
        }

        /// <summary>
        /// Read the MTXP chunk.
        /// </summary>
        public static void ReadMTXP(BinaryReader reader, uint chunkSize, ADTModel model)
        {
            model.HasMTXP = true;

            for (var i = 0; i < chunkSize / 16; ++i)
            {
                var flags = reader.ReadUInt32();
                var heightScale = reader.ReadSingle();
                var heightOffset = reader.ReadSingle();
                reader.ReadInt32(); // Padding

                model.TextureFlags.Add(model.TextureFileDataId[i], flags);
                model.HeightScales.Add(model.TextureFileDataId[i], heightScale);
                model.HeightOffsets.Add(model.TextureFileDataId[i], heightOffset);
            }
        }

        #region MCNK Readers

        /// <summary>
        /// Read MCVT chunk which contain VertexHeights
        /// </summary>
        public static float[] ReadMCVT(BinaryReader reader)
        {
            var vertices = new float[145];
            for (var i = 0; i < 145; ++i)
                vertices[i] = reader.ReadSingle();

            return vertices;
        }

        /// <summary>
        /// Read the MCNR chunk which contains all Normals.
        /// </summary>
        public static Vector3[] ReadMCNR(BinaryReader reader)
        {
            var vertexNormals = new Vector3[145];
            for (var i = 0; i < 145; ++i)
                vertexNormals[i] = new Vector3(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

            // Skip the unused bytes.
            reader.BaseStream.Position += 13;

            return vertexNormals;
        }

        #endregion

        #region Tex MCNK Readers

        public static void ReadMCLY(BinaryReader reader, uint chunkSize, TexMCNKChunk chunk)
        {
            var layerCount = chunkSize / 16;
            chunk.LayerCount = layerCount;
            chunk.TextureIds = new uint[layerCount];
            chunk.LayerOffsetInMCAL = new uint[layerCount];
            chunk.AlphaMapCompressed = new bool[layerCount];

            for (var i = 0; i < layerCount; ++i)
            {
                chunk.TextureIds[i] = reader.ReadUInt32();

                var bitArray = new byte[4];
                reader.Read(bitArray, 0, 4);
                var flags = new BitArray(bitArray);
                chunk.AlphaMapCompressed[i] = flags[9];

                chunk.LayerOffsetInMCAL[i] = reader.ReadUInt32();
                var effectId = reader.ReadUInt32();
            }
        }

        #endregion
    }
}