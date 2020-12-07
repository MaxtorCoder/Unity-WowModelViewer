using IO.M2;
using IO.SKIN;
using UnityEngine;

namespace World.Model
{
    public static class M2Loader
    {
        public static bool Working = false;

        public static void Load(uint fileDataId, Vector3 position, Quaternion rotation, Vector3 scale, GameObject parent = null)
        {
            Working = true;

            var model = new M2Model
            {
                FileDataId = fileDataId,
                Position = position,
                Rotation = rotation,
                Scale = scale,
                Parent = parent
            };

            // Parse the M2 format
            M2Reader.ReadM2(fileDataId, model);

            // Parse the SKIN format.
            foreach (var skinFile in M2Reader.SkinFileIds)
                SkinReader.ReadSkin(skinFile, model);

            M2Data.EnqueuedModels.Enqueue(model);

            Working = false;
        }

        public static void EnqueueDoodad(M2QueueItem queueItem)
        {
            Working = true;

            var model = new M2Model
            {
                FileDataId  = queueItem.FileDataId,
                Position    = queueItem.Position,
                Rotation    = queueItem.Rotation,
                Scale       = queueItem.Scale,
                Parent      = queueItem.AdtParent,
                UniqueId    = queueItem.UniqueId
            };

            // Parse the M2 format
            M2Reader.ReadM2(queueItem.FileDataId, model);

            // Parse the SKIN format.
            foreach (var skinFile in M2Reader.SkinFileIds)
                SkinReader.ReadSkin(skinFile, model);

            M2Data.EnqueuedModels.Enqueue(model);

            Working = false;
        }
    }

    public struct M2QueueItem
    {
        public uint FileDataId;
        public int UniqueId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public GameObject AdtParent;
    }
}