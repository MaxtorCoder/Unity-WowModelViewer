using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assets.Scripts.IO.ADT.MapTextures;
using Casc;
using Constants;
using UnityEngine;
using Util;

namespace IO.ADT
{
    public static partial class ADTReader
    {
        /// <summary>
        /// Read the root adt format.
        /// </summary>
        public static void ReadRootADT(uint fileDataId, ADTModel model)
        {
            var stream = CASC.OpenFile(fileDataId);
            if (stream == null)
                return;

            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (Chunks) reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    switch (chunkId)
                    {
                        case Chunks.MCNK:
                            ReadMCNK(reader, model, chunkSize);
                            break;
                        default:
                            reader.Skip(chunkSize);
                            // Debug.Log($"ADTROOT: Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Read the Tex0.adt format.
        /// </summary>
        public static void ReadTexADT(uint fileDataId, ADTModel model)
        {
            var stream = CASC.OpenFile(fileDataId);
            if (stream == null)
                return;

            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (Chunks)reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    switch (chunkId)
                    {
                        case Chunks.MDID:
                        case Chunks.MHID:
                            ReadTextures(reader, chunkSize, model);
                            break;
                        case Chunks.MCNK:
                            ReadTexMCNK(reader, chunkSize, model);
                            break;
                        case Chunks.MTXP:
                            ReadMTXP(reader, chunkSize, model);
                            break;
                        default:
                            reader.Skip(chunkSize);
                            // Debug.Log($"ADTTEX: Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Load the height textures.
        /// </summary>
        public static void LoadHeightTextures(ADTModel model)
        {
            if (!model.HasMTXP)
                return;

            foreach (var textureFileDataId in model.TextureFileDataId)
            {
                var textureData = BLP.Open(textureFileDataId);
                if (textureData == null)
                    continue;

                model.TerrainHeightTextures.Add(textureFileDataId, textureData);
            }
        }

        /// <summary>
        /// Load all lower textures.
        /// </summary>
        public static void LoadLowTextures()
        {

        }
    }
}