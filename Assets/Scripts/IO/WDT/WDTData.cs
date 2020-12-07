using Assets.Scripts.Constants;
using System.Collections.Generic;

namespace IO.WDT
{
    public static class WDTData
    {
        public static Dictionary<(uint wdtFileDataId, uint x, uint y), MAIDEntry> MAIDs = new Dictionary<(uint wdtFileDataId, uint x, uint y), MAIDEntry>();
        public static Dictionary<uint, MPHDEntry> MPHDs = new Dictionary<uint, MPHDEntry>();
    }
    
    public struct MPHDEntry
    {
        public MPHDFlags Flags;
        public uint LgtFileDataId;
        public uint OccFileDataId;
        public uint FogsFileDataId;
        public uint MpvFileDataId;
        public uint TexFileDataId;
        public uint WdlFileDataId;
        public uint Pd4FileDataId;
    }

    public struct MAIDEntry
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