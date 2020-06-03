using System.Collections.Generic;

namespace IO.WDT
{
    public static class WDTData
    {
        public static Dictionary<(uint x, uint y), MAIDChunk> MAIDs = new Dictionary<(uint x, uint y), MAIDChunk>();
    }
    
    public struct MAIDChunk
    {
        public uint RootAdt;
        public uint Obj0Adt;
        public uint Obj1Adt;
        public uint Tex0Adt;
        public uint LodAdt;
        public uint MapTexture;
        public uint MapTextureN;
        public uint MinimapTexture;
    }
}