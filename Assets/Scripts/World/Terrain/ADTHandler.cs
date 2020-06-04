using Constants;
using IO.ADT;
using IO.ADT.MapTextures;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Terrain
{
    public class ADTHandler
    {
        private GameObject adtParent;
        private GameObject chunkPrefab;

        private Dictionary<uint, Texture2D> activeDiffuseTextures = new Dictionary<uint, Texture2D>();
        private Dictionary<uint, Texture2D> activeHeightTextures = new Dictionary<uint, Texture2D>();

        public ADTHandler(GameObject adtParent, GameObject chunkPrefab)
        {
            this.chunkPrefab = chunkPrefab;
            this.adtParent = adtParent;
        }

        public void Update()
        {
            if (ADTData.EnqueuedADTs.TryDequeue(out var adtModel))
                CreateADTBlock(adtModel);
        }

        public void CreateADTBlock(ADTModel model)
        {
            var adtObject = new GameObject();
            adtObject.name = $"{model.MapId}_{model.X}_{model.Y}";
            adtObject.transform.SetParent(adtParent.transform);

            var adtMesh = new GameObject();
            adtMesh.name = "mesh";
            adtMesh.transform.position = Vector3.zero;
            adtMesh.transform.rotation = Quaternion.identity;
            adtMesh.transform.SetParent(adtObject.transform);

            if (!MapTexture.EnqueuedMapTextures.TryDequeue(out var mapBlock))
                return;

            var adtStartX = model.MCNKs[0].MeshPosition.x;
            var adtStartY = model.MCNKs[0].MeshPosition.y;
            var initialChunkX = adtStartX + model.MCNKs[0].IndexX * WorldConstants.ChunkSize * -1;
            var initialChunkY = adtStartY + model.MCNKs[0].IndexY * WorldConstants.ChunkSize * -1;

            var chunkIndex = 0;
            var gobs = new List<GameObject>();
            for (var x = 0; x < 16; ++x)
            {
                for (var y = 0; y < 16; ++y)
                {
                    var mcnkChunk = model.MCNKs[chunkIndex];

                    var genX = (initialChunkX + (WorldConstants.ChunkSize * x) * -1);
                    var genY = (initialChunkY + (WorldConstants.ChunkSize * y) * -1);

                    var verticeList = new List<Vector3>();
                    var normalsList = new List<Vector3>();
                    var uvList = new List<Vector2>();

                    for (uint i = 0, index = 0; i < 17u; ++i)
                    {
                        var isSmallRow = i % 2 != 0;
                        var rowLength = isSmallRow ? 8 : 9;

                        for (var j = 0; j < rowLength; ++j)
                        {
                            normalsList.Add(new Vector3(
                                -mcnkChunk.Normals[index].x / 127.0f,
                                mcnkChunk.Normals[index].z / 127.0f,
                                mcnkChunk.Normals[index].y / 127.0f));

                            var X = genY - (j * WorldConstants.UnitSize);
                            var Y = mcnkChunk.Vertices[index++] + mcnkChunk.MeshPosition.z;
                            var Z = genX - (i * WorldConstants.UnitSizeHalf);

                            var verticeVector = new Vector3(X * -1, Y, Z);
                            if (isSmallRow)
                                verticeVector.x = X - WorldConstants.UnitSizeHalf;

                            verticeList.Add(verticeVector);

                            var texX = -(verticeVector.x - initialChunkY) / WorldConstants.TileSize;
                            var texY = -(verticeVector.z - initialChunkX) / WorldConstants.TileSize;
                            uvList.Add(new Vector2(texX, texY));
                        }
                    }

                    var indiceList = new List<int>();
                    for (int i = 9; i < 145; ++i)
                    {
                        // Triangles
                        // indiceList.AddRange(new int[] { i + 8, i - 9, i - 8 });
                        // indiceList.AddRange(new int[] { i - 9, i - 8, i + 9 });
                        // indiceList.AddRange(new int[] { i - 8, i + 9, i + 8 });
                        // indiceList.AddRange(new int[] { i + 9, i + 8, i - 9 });

                        indiceList.AddRange(new int[] { i + 8, i - 9, i - 8 });
                        indiceList.AddRange(new int[] { i - 8, i + 9, i + 8 });

                        if ((i + 1) % (9 + 8) == 0)
                            i += 9;
                    }

                    mcnkChunk.VertexArray = verticeList.ToArray();
                    mcnkChunk.Normals = normalsList.ToArray();
                    mcnkChunk.UVs = uvList.ToArray();
                    mcnkChunk.TriangleArray = indiceList.ToArray();

                    GameObject chunk = GameObject.Instantiate(chunkPrefab, adtMesh.transform, true);
                    chunk.isStatic = true;
                    chunk.name = $"chunk_{chunkIndex++}";

                    var mesh = new Mesh();
                    mesh.vertices = mcnkChunk.VertexArray;
                    mesh.triangles = mcnkChunk.TriangleArray;
                    mesh.normals = mcnkChunk.Normals;
                    mesh.uv = mcnkChunk.UVs;

                    chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
                    chunk.GetComponent<MeshCollider>().sharedMesh = mesh;

                    gobs.Add(chunk);
                    GameObject.Destroy(chunk);
                }
            }

            var combinedMeshes = new CombineInstance[gobs.Count];
            for (var i = 0; i < gobs.Count; ++i)
            {
                combinedMeshes[i].mesh      = gobs[i].GetComponent<MeshFilter>().sharedMesh;
                combinedMeshes[i].transform = gobs[i].GetComponent<MeshFilter>().transform.localToWorldMatrix;
            }

            var mainMesh = new Mesh();
            mainMesh.CombineMeshes(combinedMeshes);

            var mainChunk = GameObject.Instantiate(chunkPrefab, adtMesh.transform, true);
            mainChunk.name = "mainChunk";
            mainChunk.GetComponent<MeshFilter>().sharedMesh = mainMesh;
            mainChunk.GetComponent<MeshCollider>().sharedMesh = mainMesh;

            var lowTexture = new Texture2D(mapBlock.Data.Width, mapBlock.Data.Height, mapBlock.Data.TextureFormat, false);
            lowTexture.LoadRawTextureData(mapBlock.Data.RawData);
            lowTexture.Apply();
            lowTexture.wrapMode = TextureWrapMode.Clamp;

            var lowMaterial = new Material(Shader.Find("Shader Graphs/WowShader"));
            lowMaterial.SetTexture("_layer0", lowTexture);

            if (model.X == mapBlock.Coords.x && model.Y == mapBlock.Coords.y)
                mainChunk.GetComponent<MeshRenderer>().material = lowMaterial;
        }
    }
}