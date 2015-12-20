using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI
{

    public class NNConfig : ScriptableObject
    {
        public int InputsCount;
        public int OutputsCount;
        public List<int> HiddenLayers;
        public float[] Weights;
        public float LearningStep = 0.02f;
        public float MaxLearningError = 0.01f;

        public void ResetWeights()
        {
            var linksCount = 0;
            var layerCount = InputsCount;
            foreach (var hiddenLayer in HiddenLayers)
            {
                linksCount += layerCount*hiddenLayer;
                layerCount = hiddenLayer;
            }

            linksCount += layerCount*OutputsCount;
            linksCount += InputsCount;
            linksCount += OutputsCount;
            linksCount += HiddenLayers.Sum();

            Weights = new float[linksCount];

            for (var i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }

        public void CheckWeights()
        {
            if (Weights.Any(x => float.IsNaN(x) || float.IsInfinity(x)))
            {
                ResetWeights();
            }
        }
    }

}