using Assets.Scripts.Constants;
using System.IO;

namespace IO.WDT
{
    public static partial class WDTReader
    {
        /// <summary>
        /// Read the MAID chunks from the WDT.
        /// </summary>
        public static void ReadMAID(BinaryReader reader, uint wdtFileDataId)
        {
            for (var x = 0u; x < 64u; ++x)
            {
                for (var y = 0u; y < 64u; ++y)
                {
                    var maid = new MAIDEntry
                    {
                        RootAdt = reader.ReadUInt32(),
                        Obj0Adt = reader.ReadUInt32(),
                        Obj1Adt = reader.ReadUInt32(),
                        Tex0Adt = reader.ReadUInt32(),
                        LodAdt =  reader.ReadUInt32(),
                        MapTexture = reader.ReadUInt32(),
                        MapTextureN = reader.ReadUInt32(),
                        MinimapTexture = reader.ReadUInt32()
                    };

                    WDTData.MAIDs.Add((wdtFileDataId, y, x), maid);
                }
            }
        }

        /// <summary>
        /// Read the MPHD chunk which contains flags and additional info.
        /// </summary>
        public static void ReadMPHD(BinaryReader reader, uint wdtFileDataId)
        {
            var mphdEntry = new MPHDEntry
            {
                Flags           = (MPHDFlags)reader.ReadUInt32(),
                LgtFileDataId   = reader.ReadUInt32(),
                OccFileDataId   = reader.ReadUInt32(),
                FogsFileDataId  = reader.ReadUInt32(),
                MpvFileDataId   = reader.ReadUInt32(),
                TexFileDataId   = reader.ReadUInt32(),
                WdlFileDataId   = reader.ReadUInt32(),
                Pd4FileDataId   = reader.ReadUInt32(),
            };

            WDTData.MPHDs.Add(wdtFileDataId, mphdEntry);
        }
    }
}