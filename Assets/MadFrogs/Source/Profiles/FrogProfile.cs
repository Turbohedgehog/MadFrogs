using UnityEngine;

namespace Gameplay
{

    public class FrogProfile : ScriptableObject
    {
        [System.Serializable]
        public class Vision
        {
            public float VisionAngle = 60f;
            public float VisionRange = 2f;
            public float FeelEnemyRadius = 2f;
            public float VisionUpdateDuration = 0.2f;

        }

        [System.Serializable]
        public class AIConfig
        {
            public AI.NNConfig Config;
            public float UpdateStateDutation = 0.2f;

        }

        [System.Serializable]
        public class Moving
        {
            public float PrepareForJumpDuration = 0.1f;
            public float PostJumpWaitDuration = 0.1f;
            public float RotateDuration = 0.1f;
            public float MaxJumpAngle = 45f;
            public float MaxJumpDistance = 2f;
            public float Speed = 10f;

        }

        [System.Serializable]
        public class Attack
        {
            public float Damage = 1f;
            public float AttackRange = 0.5f;
            public float AttackPreparingDuration = 0.1f;
            public float AttackDuration = 0.1f;
            public float PostAttackWaitDuration = 0.8f;
        }

        [System.Serializable]
        public class PersonalAttributes
        {
            public int Health = 10;
            public float HealthRegenerationDuration = 10f;

        }

        public GameObject Prefab;
        public Color FrogColor = Color.green;
        public string Name;
        public AIConfig AIData;
        public PersonalAttributes PersonalData;
        public Vision VisionData;
        public Moving MovingData;
        public Attack AttackData;

    }

}