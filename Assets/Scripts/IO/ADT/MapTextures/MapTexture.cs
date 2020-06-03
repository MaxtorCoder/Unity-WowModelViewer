using IO.Shared;
using IO.WDT;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IO.ADT.MapTextures
{
    public static class MapTexture
    {
        public static ConcurrentQueue<MapTextureBlock> EnqueuedMapTextures = new ConcurrentQueue<MapTextureBlock>();

        public static void Load(Vector2 coords)
        {
            var textureBlock = new MapTextureBlock();
            var textureData = BLP.Open(WDTData.MAIDs[((uint)coords.x, (uint)coords.y)].MapTexture);
            if (textureData == null)
                return;

            textureBlock.Data       = textureData;
            textureBlock.FileDataId = WDTData.MAIDs[((uint)coords.x, (uint)coords.y)].MapTexture;
            textureBlock.Coords     = coords;

            EnqueuedMapTextures.Enqueue(textureBlock);
        }
    }

    public struct MapTextureBlock
    {
        public uint FileDataId;
        public Vector2 Coords;
        public TextureData Data;
    }
}
