using System;
using System.Collections.Generic;
using IO.M2;
using UnityEngine;

namespace World.Model
{
    public class M2Handler : MonoBehaviour
    {
        public GameObject M2Parent;
        public WorldLoader World;

        private Dictionary<uint, Texture2D> activeTextures = new Dictionary<uint, Texture2D>();

        public void Update()
        {
            if (M2Data.EnqueuedModels.Count > 0)
            {
                if (!M2Loader.Working && M2Data.EnqueuedModels.TryDequeue(out var model))
                {
                    if (!World.LoadedM2s.ContainsKey(model.FileDataId))
                    {
                        World.LoadedM2s.Add(model.FileDataId, null);
                        CreateM2Object(model, model.Parent);
                    }
                    else
                        CloneM2Object(model);
                }
            }

            if (M2Data.EnqueuedCloneModels.Count > 0)
            {
                if (!M2Data.EnqueuedCloneModels.TryDequeue(out var model))
                    throw new Exception("Something went wrong!");

                if (World.LoadedM2s[model.FileDataId] == null)
                    throw new Exception($"{model.FileDataId} has no existing GameObject");
                
                var instance = Instantiate(World.LoadedM2s[model.FileDataId]);
                instance.transform.position = model.Position;
                instance.transform.rotation = model.Rotation;
                instance.transform.localScale = model.Scale;
                instance.transform.SetParent(model.Parent.transform);
            }
        }

        /// <summary>
        /// Create the M2 Mesh and <see cref="GameObject"/>.
        /// </summary>
        /// <param name="model">The <see cref="M2Model"/> instance</param>
        /// <param name="parent">The <see cref="GameObject"/> parent</param>
        public void CreateM2Object(M2Model model, GameObject parent = null)
        {
            var m2Object = new GameObject();
            m2Object.transform.SetParent(parent != null ? parent.transform : M2Parent.transform);

            World.LoadedM2s[model.FileDataId] = m2Object;
            World.LoadedM2s[model.FileDataId].name = model.ModelName;

            // Bones
            var m2Bone = new GameObject();
            m2Bone.name = "bones";
            m2Bone.transform.SetParent(m2Object.transform);

            // Mesh
            var m2MeshObject = new GameObject();
            m2MeshObject.name = "mesh";
            m2MeshObject.transform.position = Vector3.zero;
            m2MeshObject.transform.rotation = Quaternion.identity;
            m2MeshObject.transform.SetParent(m2Object.transform);

            var mesh = new Mesh();
            mesh.vertices = model.MeshData.Vertices.ToArray();
            mesh.normals = model.MeshData.Normals.ToArray();
            mesh.uv = model.MeshData.TexCoords.ToArray();
            mesh.uv2 = model.MeshData.TexCoords2.ToArray();
            mesh.subMeshCount = model.Submeshes.Count;

            var meshRenderer = m2MeshObject.AddComponent<SkinnedMeshRenderer>();
            meshRenderer.sharedMesh = mesh;
            
            var materials = new Material[model.Submeshes.Count];
            for (var i = 0; i < model.Submeshes.Count; ++i)
            {
                mesh.SetTriangles(model.Submeshes[i].Triangles, i, true);
                materials[i] = new Material(Shader.Find("Shader Graphs/ModelShader"));

                var skinSectionIndex = model.BatchIndices[i].SkinSectionId;
                var textureFileDataId = model.Textures[model.TextureLookupTable[model.BatchIndices[skinSectionIndex].TextureComboIndex]].FileDataId;
                var textureData = model.Textures[model.TextureLookupTable[model.BatchIndices[skinSectionIndex].TextureComboIndex]].TextureData;

                if (textureFileDataId != 0 && textureData != null)
                {
                    if (!activeTextures.ContainsKey(textureFileDataId))
                    {
                        var texture = new Texture2D(textureData.Width, textureData.Height, textureData.TextureFormat, textureData.HasMipmaps);
                        texture.LoadRawTextureData(textureData.RawData);
                        texture.Apply();

                        activeTextures[textureFileDataId] = texture;
                    }
                    
                    materials[i].SetTexture("_baseMap", activeTextures[textureFileDataId]);
                }
            }
            meshRenderer.materials = materials;

            World.LoadedM2s[model.FileDataId].transform.position = model.Position;
            World.LoadedM2s[model.FileDataId].transform.rotation = model.Rotation;
            World.LoadedM2s[model.FileDataId].transform.localScale = model.Scale;
        }


        /// <summary>
        /// Clone the generated M2 <see cref="GameObject"/>.
        /// </summary>
        /// <param name="model"></param>
        public void CloneM2Object(M2Model model)
        {
            M2Data.EnqueuedCloneModels.Enqueue(model);
        }
    }
}