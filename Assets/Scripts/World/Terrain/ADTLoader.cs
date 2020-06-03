using System;
using IO.ADT.MapTextures;
using Casc;
using Constants;
using DB2.Structures;
using DBFileReaderLib;
using IO.ADT;
using IO.WDT;
using UnityEngine;

namespace World.Terrain
{
    public static class ADTLoader
    {
        public static void Load(uint mapId, uint x, uint y)
        {
            var stream = CASC.OpenFile(DB2Constants.MapFileDataId);
            if (stream == null)
                throw new Exception("Map.db2 is null!");
            
            var mapStorage = new DBReader(stream).GetRecords<Map>();
            if (!mapStorage.TryGetValue((int)mapId, out var map))
                throw new Exception($"{mapId} cannot be found in Map.db2");
            
            // Read the WDT file
            WDTReader.ReadWDT(map.WdtFileDataId);

            var adtModel = new ADTModel
            {
                X = x,
                Y = y,
                MapId = mapId,
                FileDataId = WDTData.MAIDs[(x, y)].RootAdt
            };

            // Parse the associated ADTs
            ADTReader.ReadRootADT(WDTData.MAIDs[(x, y)].RootAdt, adtModel);
            ADTReader.ReadTexADT(WDTData.MAIDs[(x, y)].Tex0Adt, adtModel);

            // Load textures.
            ADTReader.LoadHeightTextures(adtModel);
            MapTexture.Load(new Vector2(adtModel.X, adtModel.Y));

            ADTData.EnqueuedADTs.Enqueue(adtModel);
        }
    }
}