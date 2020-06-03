namespace Constants
{
    public static class WorldConstants
    {
        public const float MaxSize = 51200.0f / 3.0f;
        public const float TileSize = MaxSize / 32.0f;
        public const float WorldScale = 10.0f;
        public const float BlockSize = TileSize / WorldScale;
        public const float ChunkSize = TileSize / 16.0f;
        public const float UnitSize = ChunkSize / 8.0f;
        public const float UnitSizeHalf = UnitSize / 2.0f;
    }

    public static class DB2Constants
    {
        public const uint MapFileDataId = 1349477u;
    }

    public static class GuiConstants
    {
        public static bool IsInModelPreview = true;
    }
}