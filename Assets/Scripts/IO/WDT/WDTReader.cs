using System.Collections.Generic;
using System.IO;
using Casc;
using Constants;
using Util;

namespace IO.WDT
{
    public static partial class WDTReader
    {
        public static List<uint> ReadWDTFiles = new List<uint>();

        public static void ReadWDT(uint fileDataId)
        {
            if (ReadWDTFiles.Contains(fileDataId))
                return;

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
                        case Chunks.MPHD:
                            ReadMPHD(reader, fileDataId);
                            break;
                        case Chunks.MAID:
                            ReadMAID(reader, fileDataId);
                            break;
                        default:
                            reader.Skip(chunkSize);
                            // Debug.Log($"Skipping {chunkId} (0x{chunkId:X}) with size: {chunkSize}..");
                            break;
                    }
                }
            }

            ReadWDTFiles.Add(fileDataId);
        }
    }
}