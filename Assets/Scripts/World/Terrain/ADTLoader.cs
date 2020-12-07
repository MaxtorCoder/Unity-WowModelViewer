using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Load the ADT by MapId and given coords.
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
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

            var maid = WDTData.MAIDs[(map.WdtFileDataId, x, y)];
            var adtModel = new ADTModel
            {
                X = x,
                Y = y,
                MapId = mapId,
                FileDataId = maid.RootAdt,
                WdtFileDataId = map.WdtFileDataId
            };

            // Parse the associated ADTs
            ADTReader.ReadRootADT(maid.RootAdt, adtModel);
            ADTReader.ReadTexADT(maid.Tex0Adt, adtModel);
            ADTReader.ReadObjADT(maid.Obj0Adt, adtModel);

            AssembleMeshData(adtModel);

            ADTData.EnqueuedADTs.Enqueue(adtModel);
        }

        /// <summary>
        /// Assemble the <see cref="ADTModel"/> mesh data.
        /// </summary>
        /// <param name="model"></param>
        public static void AssembleMeshData(ADTModel model)
        {
            foreach (var chunkData in model.MCNKs)
            {
                // Assembly Vertex Data.
                chunkData.VertexArray = new Vector3[145];
                var currentVertex = 0;
                for (var i = 0; i < 17; ++i)
                {
                    var isSmallRow = i % 2 != 0;
                    var rowLength = isSmallRow ? 8 : 9;

                    for (var j = 0; j < rowLength; ++j)
                    {
                        var calcX = (float)(-i * 2.08333125) / WorldConstants.WorldScale;
                        var calcY = chunkData.Vertices[currentVertex] / WorldConstants.WorldScale;

                        var calcZ = 0.0f;
                        if (!isSmallRow)
                            calcZ = (float)(-j * 4.1666625) / WorldConstants.WorldScale;
                        else
                            calcZ = (float)((-j - 0.5f) * 4.1666625) / WorldConstants.WorldScale;

                        var vertexVector = new Vector3(calcX, calcY, calcZ);
                        chunkData.VertexArray[currentVertex] = vertexVector;

                        ++currentVertex;
                    }
                }

                // Assemble Triangles.
                var indiceList = new List<int>();
                for (int i = 9; i < 145; ++i)
                {
                    // Triangles
                    indiceList.AddRange(new int[] { i + 8, i - 9, i - 8 });
                    indiceList.AddRange(new int[] { i - 9, i - 8, i + 9 });
                    indiceList.AddRange(new int[] { i - 8, i + 9, i + 8 });
                    indiceList.AddRange(new int[] { i + 9, i + 8, i - 9 });

                    if ((i + 1) % (9 + 8) == 0)
                        i += 9;
                }
                chunkData.TriangleArray = indiceList.ToArray();

                // Assemble UVs
                chunkData.UVs = new Vector2[145];
                for (var i = 0; i < 145; ++i)
                {
                    var uvVector = new Vector2(-(chunkData.VertexArray[i].z / (WorldConstants.ChunkSize / WorldConstants.WorldScale)),
                                               -(chunkData.VertexArray[i].x / (WorldConstants.ChunkSize / WorldConstants.WorldScale)));

                    chunkData.UVs[i] = uvVector;
                }

                // Scale chunk positions to World Scale.
                Vector3 newMapPosition = new Vector3(chunkData.MeshPosition.x / WorldConstants.WorldScale,
                                                     chunkData.MeshPosition.z / WorldConstants.WorldScale,
                                                     chunkData.MeshPosition.y / WorldConstants.WorldScale);
                chunkData.MeshPosition = newMapPosition;
            }
        }
    }
}