namespace Assets.Scripts.Constants
{
    public enum MPHDFlags
    {
        WdtUsesGlobalMapObj             = 0x0001,
        AdtHasMccv                      = 0x0002,
        AdtHasBigAlpha                  = 0x0004,
        AdtHasDoodadRefsSortedBySize    = 0x0008,
        AdyHasLightingVertices          = 0x0010,
        AdtHasUpsideDownGround          = 0x0020,
        Unk0x0040                       = 0x0040,
        AdtHasHeightTexturing           = 0x0080,
        Unk0x0100                       = 0x0100,
        WdtHasMaid                      = 0x0200,
        Unk0x0400                       = 0x0400,
        Unk0x0800                       = 0x0800,
        Unk0x1000                       = 0x1000,
        Unk0x2000                       = 0x2000,
        Unk0x4000                       = 0x4000,
        Unk0x8000                       = 0x8000,

        MaskVertexBufferFormat          = AdtHasMccv,
        MaskRenderChunkSomething        = AdtHasHeightTexturing | AdtHasBigAlpha
    }

    public enum MDDFFlags
    {
        Biodome             = 0x001,
        Shrubbery           = 0x002,
        Unk4                = 0x004,
        Unk8                = 0x008,
        LiquidKnown         = 0x020,
        EntryIsFileDataId   = 0x040,
        Unk100              = 0x100,
    }
}
