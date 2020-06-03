using UnityEngine;

namespace World.Terrain
{
    public class ADTChunk : MonoBehaviour
    {
        public Material High;
        public Material Low;
        public Mesh Mesh;

        public void UpdateDistance(uint lod)
        {
            if (lod == 1)
            {
                if (Low != null)
                    GetComponent<Renderer>().material = Low;
            }
            else if (lod == 0)
            {
                if (High != null)
                    GetComponent<Renderer>().material = High;
                else if (Low != null)
                    GetComponent<Renderer>().material = Low;
            }
        }

        public void SetMaterial(uint lod, Material material)
        {
            if (lod == 0)
                High = material;
            else if (lod == 1)
                Low = material;

            UpdateDistance(lod);
        }
    }
}
