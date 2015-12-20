using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{

    public class CombatSceneProfile : ScriptableObject
    {
        [System.Serializable]
        public class CameraPofile
        {
            public float AngularRotationVelocity = 15f;
            public Vector3 Monopod = new Vector3(0f, 2f, 4f);
            public float Tangent = -10f;

        }

        public float Width;
        public float Lenght;
        public int MaxMosquitos;
        public float SpawnMosquitoPeriod;
        public float Gravity = 10f;

        public CameraPofile CameraInfo;

        public List<FrogProfile> FrogProfiles = new List<FrogProfile>();
        public List<MosquitoProfile> MosquitoProfiles = new List<MosquitoProfile>();

    }

}   