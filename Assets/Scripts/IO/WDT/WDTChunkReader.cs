using System.IO;

namespace IO.WDT
{
    public static partial class WDTReader
    {
        /// <summary>
        /// Read the MAID chunks from the WDT.
        /// </summary>
        public static void ReadMAID(BinaryReader reader)
        {
            for (var x = 0u; x < 64u; ++x)
            {
                for (var y = 0u; y < 64u; ++y)
                {
                    var maid = new MAIDChunk
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

                    WDTData.MAIDs.Add((y, x), maid);
                }
            }
        }
    }
}