using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class NNExamples : ScriptableObject
    {
        [Serializable]
        public class Example
        {
            public FrogNN.NNInputs Input;
            public FrogNN.FrogAction Action;

        }

        public List<Example> Examples = new List<Example>();

    }

}
