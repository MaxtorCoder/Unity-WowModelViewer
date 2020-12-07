using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Assets.Scripts.Constants;
using Constants;
using IO.WDT;
using UnityEngine;
using Util;

namespace IO.ADT
{
    public static partial class ADTReader
    {
        #region Root Chunk Readers

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
                var chunkId = (Chunks)reader.ReadUInt32();
                var chunkSize = reader.ReadUInt32();

                switch (chunkId)
                {
                    case Chunks.MCVT:        // Vertex Heights
                        mcnk.Vertices = ReadMCVT(reader);
                        break;
                    case Chunks.MCNR:        // Normals
                        mcnk.Normals = ReadMCNR(reader);
                        break;
                    default:
                        reader.Skip(chunkSize);
                        // Debug.Log($"MCNK - Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                        break;
                }
            }

            model.MCNKs.Add(mcnk);
        }

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
            {
                var rawNormal = new Vector3(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                var calcX = rawNormal.x.NormalizeValue(); if (calcX <= 0) { calcX = 1 + calcX; } else if (calcX > 0) { calcX = (1 - calcX) * (-1); }
                var calcY = rawNormal.y.NormalizeValue(); if (calcY <= 0) { calcY = 1 + calcY; } else if (calcY > 0) { calcY = (1 - calcY) * (-1); }
                var calcZ = rawNormal.z.NormalizeValue(); if (calcZ <= 0) { calcZ = 1 + calcZ; } else if (calcZ > 0) { calcZ = (1 - calcZ) * (-1); }

                vertexNormals[i] = new Vector3(calcX, calcZ, calcY);
            }

            // Skip the unused bytes.
            reader.BaseStream.Position += 13;

            return vertexNormals;
        }

        #endregion

        #region Texture Chunk Readers

        /// <summary>
        /// Read the Texture MCNK from the texture ADT.
        /// </summary>
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
                    case Chunks.MCAL:       // Alpha Layers.
                        ReadMCAL(reader, texMcnk, model.WdtFileDataId, chunkSize);
                        break;
                    default:
                        reader.Skip(chunkSize);
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
        /// Read texture data from MCLY.
        /// </summary>
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

        /// <summary>
        /// Read Alpha Layers from MCAL.
        /// </summary>
        public static void ReadMCAL(BinaryReader reader, TexMCNKChunk chunk, uint wdtFileDataId, uint chunkSize)
        {
            var mphd = WDTData.MPHDs[wdtFileDataId];

            var mcal = new MCAL[chunk.LayerCount];
            mcal[0] = new MCAL
            {
                Layer = new byte[64 * 64]
            };

            Parallel.For(0, 64 * 64, i => { mcal[0].Layer[i] = 255; });

            var readOffset = 0;
            for (var i = 1; i < chunk.LayerCount; ++i)
            {
                if (chunk.LayerOffsetInMCAL[i] != readOffset)
                    Debug.LogError("Mismatch: layer boefre required more/less bytes than expected");

                if (chunk.AlphaMapCompressed[i])
                {
                    mcal[i] = new MCAL
                    {
                        Layer = new byte[64 * 64]
                    };

                    var inOffset = 0;
                    var outOffset = 0;
                    while (outOffset < 4096)
                    {
                        var info = reader.ReadByte(); ++inOffset;
                        var mode = (uint)(info & 0x80) >> 7;
                        var count = (uint)(info & 0x7F);

                        if (mode != 0)
                        {
                            var val = reader.ReadByte(); ++inOffset;
                            while (count-- > 0 && outOffset < 4096)
                            {
                                mcal[i].Layer[outOffset] = val;
                                ++outOffset;
                            }
                        }
                        else
                        {
                            while (count-- > 0 && outOffset < 4096)
                            {
                                var val = reader.ReadByte(); ++inOffset;
                                mcal[i].Layer[outOffset] = val;
                                ++outOffset;
                            }
                        }
                    }

                    readOffset += inOffset;
                    if (outOffset != 4096)
                        Debug.LogError($"OutOffset is not 4096! {outOffset}");
                }
                else if (mphd.Flags.HasFlag(MPHDFlags.AdtHasBigAlpha) || mphd.Flags.HasFlag(MPHDFlags.AdtHasHeightTexturing))
                {
                    mcal[i] = new MCAL
                    {
                        Layer = reader.ReadBytes(4096)
                    };

                    readOffset += 4096;
                }
                else
                {
                    mcal[i] = new MCAL
                    {
                        Layer = new byte[64 * 64]
                    };

                    var mcalData = reader.ReadBytes(2048);
                    readOffset += 2048;

                    for (var j = 0; j < 2048; ++j)
                    {
                        mcal[i].Layer[2 * j + 0] = (byte)(((mcalData[j] & 0x0F) >> 0) * 17);
                        mcal[i].Layer[2 * j + 1] = (byte)(((mcalData[j] & 0xF0) >> 4) * 17);
                    }
                }
            }

            if (readOffset != chunkSize)
                throw new Exception($"ReadOffset is not 4096! {readOffset}");

            chunk.AlphaLayers = mcal;
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
            }
        }

        #endregion

        #region Object Chunk Readers

        /// <summary>
        /// Read the doodad instances from the Object ADT.
        /// </summary>
        public static void ReadMDDF(BinaryReader reader, uint chunkSize, ADTModel model)
        {
            var mddfSize = chunkSize / 36;
            for (var i = 0; i < mddfSize; ++i)
            {
                var mddfEntry = new MDDF();
                mddfEntry.FileDataId    = reader.ReadUInt32();
                mddfEntry.UniqueId      = reader.ReadInt32();

                var rawY                = (reader.ReadSingle() - 17066) * -1 / WorldConstants.WorldScale;
                var rawZ                = reader.ReadSingle() / WorldConstants.WorldScale;
                var rawX                = (reader.ReadSingle() - 17066) * -1 / WorldConstants.WorldScale;
                mddfEntry.Position      = new Vector3(rawX, rawZ, rawY);

                rawX                    = reader.ReadSingle();
                rawZ                    = 180 - reader.ReadSingle();
                rawY                    = reader.ReadSingle();
                mddfEntry.Rotation      = Quaternion.Euler(new Vector3(rawX, rawZ, rawY));

                mddfEntry.Scale         = reader.ReadUInt16() / 1024.0f;
                mddfEntry.Flags         = (MDDFFlags)reader.ReadUInt16();

                model.DoodadInstances.Add(mddfEntry.UniqueId, mddfEntry);
            }
        }

        #endregion
    }
}