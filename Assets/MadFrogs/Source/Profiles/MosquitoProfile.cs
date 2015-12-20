using UnityEngine;

namespace Gameplay
{

    public class MosquitoProfile : ScriptableObject
    {
        public GameObject Prefab;
        public float FlightHeight = 1f;
        public float FloatingAmplitude = 0.3f;
        public float FloatingPeriodDuration = 1f;
        public float HealHealth = 3f;

    }

}