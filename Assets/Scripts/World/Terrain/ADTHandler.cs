using Constants;
using IO.ADT;
using System;
using System.Linq;
using UnityEngine;
using World.Model;

namespace World.Terrain
{
    public class ADTHandler : MonoBehaviour
    {
        public GameObject ADTParent;
        public GameObject ChunkPrefab;

        public WorldLoader World;
        public M2Handler DoodadHandler;

        private bool isWorking;

        /// <summary>
        /// Update the ADTHandler thread and Dequeue all enqueued ADTs.
        /// </summary>
        public void Update()
        {
            if (!isWorking && ADTData.EnqueuedADTs.TryDequeue(out var adtModel))
            {
                isWorking = true;
                CreateADTBlock(adtModel);
                isWorking = false;

                // CreateDoodadInstances(adtModel);
            }
        }

        /// <summary>
        /// Create the ADT block with textures.
        /// </summary>
        public void CreateADTBlock(ADTModel model)
        {
            var adtObject = new GameObject();
            adtObject.name = $"{model.MapId}_{model.X}_{model.Y}";
            adtObject.transform.SetParent(ADTParent.transform);

            var adtMesh = new GameObject();
            adtMesh.name = "mesh";
            adtMesh.transform.SetParent(adtObject.transform);

            var chunkIndex = 0;
            for (var x = 0; x < 256; ++x)
            {
                var mcnkChunk = model.MCNKs[chunkIndex];

                GameObject chunk = GameObject.Instantiate(ChunkPrefab, adtMesh.transform, true);
                chunk.isStatic = true;
                chunk.name = $"chunk_{chunkIndex}";
                chunk.transform.position = mcnkChunk.MeshPosition;

                var mesh = new Mesh
                {
                    vertices    = mcnkChunk.VertexArray,
                    triangles   = mcnkChunk.TriangleArray,
                    normals     = mcnkChunk.Normals,
                    uv          = mcnkChunk.UVs
                };

                if (model.TexMCNKs[chunkIndex].TextureIds.Length != 0)
                {
                    var fileDataId = model.TextureFileDataId[(int)model.TexMCNKs[chunkIndex].TextureIds[0]];
                    var textureData = model.TextureDatas[fileDataId];

                    var texture = new Texture2D(textureData.Width, textureData.Height, textureData.TextureFormat, textureData.HasMipmaps);
                    texture.LoadRawTextureData(textureData.RawData);
                    texture.Apply();

                    var material = new Material(Shader.Find("Shader Graphs/WowShader"));
                    material.SetTexture($"_layer0", texture);

                    for (var i = 1; i < model.TexMCNKs[chunkIndex].LayerCount; ++i)
                    {
                        if (model.TexMCNKs[chunkIndex].AlphaLayers == null)
                            continue;

                        var alphaWorker = new Texture2D(64, 64, TextureFormat.Alpha8, false);
                        alphaWorker.LoadRawTextureData(model.TexMCNKs[chunkIndex].AlphaLayers[i].Layer);
                        alphaWorker.Apply();
                        alphaWorker.wrapMode = TextureWrapMode.Clamp;

                        fileDataId = model.TextureFileDataId[(int)model.TexMCNKs[chunkIndex].TextureIds[i]];
                        textureData = model.TextureDatas[fileDataId];

                        texture = new Texture2D(textureData.Width, textureData.Height, textureData.TextureFormat, textureData.HasMipmaps);
                        texture.LoadRawTextureData(textureData.RawData);
                        texture.Apply();

                        material.SetTexture($"_layer{i}", texture);
                        material.SetTexture($"_blend{i}", alphaWorker);
                    }

                    chunk.GetComponent<MeshRenderer>().material = material;
                }

                chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
                chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
                chunkIndex++;
            }
        }

        /// <summary>
        /// Parse all Doodad instances and render them onto the ADT
        /// </summary>
        public void CreateDoodadInstances(ADTModel model)
        {
            var parent = GameObject.Find($"{model.MapId}_{model.X}_{model.Y}");
            if (parent == null)
                throw new Exception($"Parent cannot be found: '{model.MapId}_{model.X}_{model.Y}'");

            var doodadParent = new GameObject("doodads");
            doodadParent.transform.position = new Vector3((32 - model.Y) * WorldConstants.BlockSize, 0, (32 - model.X) * WorldConstants.BlockSize);
            doodadParent.transform.SetParent(parent.transform);

            //foreach (var doodad in model.DoodadInstances.Values)
            for (var i = 0; i < 5; ++i)
            {
                var doodad = model.DoodadInstances.Values.ToList()[i];
                if (!World.LoadedUniqueIds.Contains(doodad.UniqueId))
                {
                    World.LoadedUniqueIds.Add(doodad.UniqueId);
                    M2Loader.EnqueueDoodad(new M2QueueItem
                    {
                        FileDataId  = doodad.FileDataId,
                        AdtParent   = doodadParent,
                        Position    = doodad.Position,
                        Rotation    = doodad.Rotation,
                        Scale       = new Vector3(doodad.Scale, doodad.Scale, doodad.Scale),
                        UniqueId    = doodad.UniqueId
                    });
                }
            }
        }
    }
}