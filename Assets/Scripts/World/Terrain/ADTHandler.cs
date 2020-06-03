using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using World.Terrain;
using Constants;
using IO.ADT;
using IO.ADT.MapTextures;
using UnityEngine;

namespace World.Terrain
{
    public class ADTHandler
    {
        private GameObject adtParent;
        private GameObject chunkPrefab;
        private Material heightMaterial;
        private Shader shaderTerrainLow;

        private Dictionary<uint, Texture2D> activeDiffuseTextures = new Dictionary<uint, Texture2D>();
        private Dictionary<uint, Texture2D> activeHeightTextures = new Dictionary<uint, Texture2D>();

        public ADTHandler(GameObject adtParent, GameObject chunkPrefab, Material heightMaterial, Shader shaderTerrainLow)
        {
            this.chunkPrefab = chunkPrefab;
            this.adtParent = adtParent;
            this.heightMaterial = heightMaterial;
            this.shaderTerrainLow = shaderTerrainLow;
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

            var adtStartX = model.MCNKs[0].MeshPosition.x;
            var adtStartY = model.MCNKs[0].MeshPosition.y;
            var initialChunkX = adtStartX + (model.MCNKs[0].IndexX * WorldConstants.ChunkSize) * -1;
            var initialChunkY = adtStartY + (model.MCNKs[0].IndexY * WorldConstants.ChunkSize) * -1;

            var chunkId = 0;
            for (var x = 0; x < 16; ++x)
            {
                for (var y = 0; y < 16; ++y)
                {
                    var genX = initialChunkX + WorldConstants.ChunkSize * x * -1;
                    var genY = initialChunkY + WorldConstants.ChunkSize * y * -1;

                    var mcnkChunk = model.MCNKs[chunkId];

                    var verticeList = new List<Vector3>();
                    var normalsList = new List<Vector3>();
                    var uvList = new List<Vector2>();

                    for (uint i = 0, index = 0; i < 17u; ++i)
                    {
                        var isSmallRow = i % 2 != 0;
                        var rowLength = isSmallRow ? 8 : 9;

                        for (var j = 0; j < rowLength; ++j)
                        {
                            normalsList.Add(new Vector3(-mcnkChunk.VertexNormals[index].x / 127.0f,
                                mcnkChunk.VertexNormals[index].z / 127.0f,
                                mcnkChunk.VertexNormals[index].y / 127.0f));

                            var X = genY - j * WorldConstants.UnitSize;
                            var Y = mcnkChunk.VertexHeights[index++] + mcnkChunk.MeshPosition.z;
                            var Z = genX - i * WorldConstants.UnitSizeHalf;
                            
                            verticeList.Add(new Vector3(X * -1, Y, Z));

                            if (i % 2 != 0)
                                X = X - WorldConstants.UnitSizeHalf;

                            var offset = (double)j;
                            if (isSmallRow)
                                offset += 0.5;

                            var tx = offset / 8d;
                            var ty = 1 - i / 16d;
                            uvList.Add(new Vector2((float)tx, (float)-ty + 1));
                        }
                    }

                    var indiceList = new List<int>();
                    for (int i = 9, xx = 0, yy = 0; i < 145; ++i, ++xx)
                    {
                        if (xx >= 8)
                        {
                            xx = 0;
                            ++yy;
                        }

                        var isHole = true;
                        if ((mcnkChunk.Flags & 0x10000) == 0)
                        {
                            var currentHole = (int)Math.Pow(2, 
                                Math.Floor(xx / 2f) * 1f + 
                                Math.Floor(yy / 2f) * 4f);

                            if ((mcnkChunk.HolesLowRes & currentHole) == 0)
                                isHole = false;
                        }
                        else
                        {
                            if (((mcnkChunk.HolesHighRes[yy] >> xx) & 1) == 0)
                                isHole = false;
                        }

                        if (!isHole)
                        {
                            // Triangles
                            indiceList.AddRange(new int[] { i + 8, i - 9, i - 8 });
                            indiceList.AddRange(new int[] { i - 9, i - 8, i + 9 });
                            indiceList.AddRange(new int[] { i - 8, i + 9, i + 8 });
                            indiceList.AddRange(new int[] { i + 9, i + 8, i - 9 });
                        }

                        if ((i + 1) % (9 + 8) == 0)
                            i += 9;
                    }

                    var diffuseLayers = new uint[4];
                    var heightLayers = new uint[4];
                    var textureFlags = new uint[4];
                    var heightScales = new float[4];
                    var heightOffset = new float[4];

                    for (var i = 0; i < model.TexMCNKs[chunkId].LayerCount; ++i)
                    {
                        var textureFileDataId = model.TextureFileDataId[(int)model.TexMCNKs[chunkId].TextureIds[i]];

                        // Diffuse Textures
                        if (!activeDiffuseTextures.ContainsKey(textureFileDataId))
                        {
                            var textureData = model.TextureDatas[textureFileDataId];
                            var texture = new Texture2D(textureData.Width, textureData.Height, textureData.TextureFormat, textureData.HasMipmaps);
                            texture.LoadRawTextureData(textureData.RawData);
                            texture.mipMapBias = 0.5f;
                            texture.Apply();

                            activeDiffuseTextures[textureFileDataId] = texture;
                        }
                        diffuseLayers[i] = textureFileDataId;

                        // Height Textures
                        if (model.TerrainHeightTextures.ContainsKey(textureFileDataId))
                        {
                            if (!activeHeightTextures.ContainsKey(textureFileDataId))
                            {
                                var textureData = model.TerrainHeightTextures[textureFileDataId];
                                var texture = new Texture2D(textureData.Width, textureData.Height, textureData.TextureFormat, textureData.HasMipmaps);
                                texture.LoadRawTextureData(textureData.RawData);
                                texture.Apply();

                                activeHeightTextures[textureFileDataId] = texture;
                            }
                            heightLayers[i] = textureFileDataId;
                        }

                        // Height Values
                        model.HeightScales.TryGetValue(textureFileDataId, out heightScales[i]);
                        model.HeightOffsets.TryGetValue(textureFileDataId, out heightOffset[i]);
                        model.TextureFlags.TryGetValue(textureFileDataId, out textureFlags[i]);
                    }

                    var material = new Material(heightMaterial);
                    for (var i = 0; i < 4; ++i)
                    {
                        if (diffuseLayers[i] != 0)
                            material.SetTexture("_layer" + i, activeDiffuseTextures[diffuseLayers[i]]);
                        material.SetTextureScale("_layer" + i, new Vector2(1, 1));
                        if (heightLayers[i] != 0)
                            material.SetTexture("_height" + i, activeHeightTextures[heightLayers[i]]);
                        material.SetTextureScale("_height" + i, new Vector2(1, 1));
                    }

                    if (model.HasMTXP)
                    {
                        material.SetVector("heightScale", new Vector4(heightScales[0], heightScales[1], heightScales[2], heightScales[3]));
                        material.SetVector("heightOffset", new Vector4(heightOffset[0], heightOffset[1], heightOffset[2], heightOffset[3]));
                    }
                    else
                    {
                        material.SetVector("heightScale", new Vector4(.01f, .01f, .01f, .01f));
                        material.SetVector("heightOffset", new Vector4(.01f, .01f, .01f, .01f));
                    }

                    mcnkChunk.VertexArray = verticeList.ToArray();
                    mcnkChunk.VertexNormals = normalsList.ToArray();
                    mcnkChunk.UVs = uvList.ToArray();
                    mcnkChunk.TriangleArray = indiceList.ToArray();

                    GameObject chunk = GameObject.Instantiate(chunkPrefab, adtMesh.transform, true);
                    chunk.isStatic = true;
                    chunk.name = $"chunk_{chunkId++}";

                    var mesh = new Mesh();
                    mesh.vertices = mcnkChunk.VertexArray;
                    mesh.triangles = mcnkChunk.TriangleArray;
                    mesh.normals = mcnkChunk.VertexNormals;
                    mesh.uv = mcnkChunk.UVs;

                    chunk.GetComponent<ADTChunk>().Mesh = mesh;
                    chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
                    chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
                    // chunk.GetComponent<ADTChunk>().SetMaterial(0, material);
                }
            }

            if (!MapTexture.EnqueuedMapTextures.TryDequeue(out var mapBlock))
                return;

            var lowTexture = new Texture2D(mapBlock.Data.Width, mapBlock.Data.Height, mapBlock.Data.TextureFormat, false);
            lowTexture.wrapMode = TextureWrapMode.Clamp;
            lowTexture.LoadRawTextureData(mapBlock.Data.RawData);
            lowTexture.mipMapBias = 0.5f;
            lowTexture.Apply();

            var lowMaterial = new Material(shaderTerrainLow);
            lowMaterial.SetTexture("_MainTex2", lowTexture);
            lowMaterial.SetTextureScale("_MainTex2", Vector2.one);
            lowMaterial.enableInstancing = true;

            if (model.X == mapBlock.Coords.x && model.Y == mapBlock.Coords.y)
            {
                for (var i = 0; i < 256; ++i)
                {
                    if (adtMesh.transform.GetChild(i) != null)
                        adtMesh.transform.GetChild(i).GetComponent<ADTChunk>().SetMaterial(1, lowMaterial);
                }
            }
        }
    }
}