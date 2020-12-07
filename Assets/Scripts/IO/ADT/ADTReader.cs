using System.IO;
using Casc;
using Constants;
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
        /// Read the Obj0.adt format.
        /// </summary>
        /// <param name="fileDataId"></param>
        /// <param name="model"></param>
        public static void ReadObjADT(uint fileDataId, ADTModel model)
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
                        case Chunks.MDDF:   // Doodad Instances
                            ReadMDDF(reader, chunkSize, model);
                            break;
                        default:
                            reader.Skip(chunkSize);
                            // Debug.Log($"ADTOBJ: Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                            break;
                    }
                }
            }
        }
    }
}