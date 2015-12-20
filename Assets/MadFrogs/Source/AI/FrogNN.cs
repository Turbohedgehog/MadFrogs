

using System;

namespace AI
{
    public class FrogNN : NNBase
    {
        [Serializable]
        public class NNInputs
        {
            public float Health; // [0-100]%
            public float NearMosquitoDistance; // [0-Inf]
            public float NearEnemyDistance; // [0-Inf]
            public int EnemiesCount; // [0-Inf]
            public bool IsSomebodyAttacksMe; // [true, false]

        }

        public enum FrogAction
        {
            CatchMosquito,
            Attack,
            Hide,
            BeCapricious,

        }

        public enum FrogInput
        {
            Health,
            NearMosquitoDistance,
            NearEnemyDistance,
            EnemiesCount,
            IsSomebodyAttacksMe,

        }

        public static readonly int InputsCount = Enum.GetValues(typeof (FrogInput)).Length;
        public static readonly int OutputsCount = Enum.GetValues(typeof(FrogAction)).Length;

        public FrogNN(int inputsCount, int outputCount, int[] hiddenLayers) : base(inputsCount, outputCount, hiddenLayers)
        {
        }

        public FrogNN(NNConfig config) : base(config)
        {
        }

    }

}
