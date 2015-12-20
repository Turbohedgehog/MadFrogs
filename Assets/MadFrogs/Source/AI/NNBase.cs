using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public abstract class NNBase
    {
        private abstract class NNItem
        {
            // public bool IsDirty;
            public abstract float Value { get; set; }
            public float Weight { get; set; }

            public NNItem(float weight)
            {
                Weight = weight;
            }

        }

        private class Synapsis : NNItem
        {
            public bool IsDirty;
            public Neuron Input { get; private set; }
            public Neuron Output { get; private set; }

            private float _value;
            public override float Value
            {
                get
                {
                    if (Input == null)
                    {
                        return 0f;
                    }

                    if (IsDirty)
                    {
                        _value = Input.Value * Weight;
                        IsDirty = false;
                    }

                    return _value;
                }

                set
                {
                    _value = value;
                    IsDirty = false;
                }

            }

            public Synapsis(Neuron input, Neuron output, float weight) : base(weight)
            {
                Input = input;
                Output = output;

                IsDirty = true;
            }

            public void DoWeightCorrection(float learnRate)
            {
                Weight += (learnRate * Output.Error * Input.Value);
            }
        }

        private class Neuron : NNItem
        {
            private float _value;
            public bool IsDirty;
            public int Id;
            public override float Value
            {
                get
                {
                    if (!IsDirty)
                    {
                        return _value;
                    }

                    IsDirty = false;
                    _value = 0f;

                    foreach (var input in Inputs)
                    {
                        _value += input.Value;
                    }

                    _value += Weight;
                    _value = SigmoidFunction(_value);

                    return _value;
                }

                set
                {
                    _value = value;
                    IsDirty = false;
                }
            }

            private bool _isErrorCalculated;
            private float _error;

            public float Error
            {
                get
                {
                    if (_isErrorCalculated)
                    {
                        return _error;
                    }

                    _error = 0f;
                    _isErrorCalculated = true;
                    foreach (var synapsis in Outputs)
                    {
                        _error += synapsis.Output.Error * synapsis.Weight;
                    }

                    _error *= SigmoidDerivative(Value);
                    return _error;
                }

                private set
                {
                    _error = value;
                    _isErrorCalculated = true;
                }
            }

            public void ResetError()
            {
                Error = 0f;
                _isErrorCalculated = false;
            }

            public void SetTarget(float target)
            {
                Error = (target - Value) * SigmoidDerivative(Value);
            }

            public void DoWeightCorrection(float learnRate)
            {
                Weight += (learnRate * Error);
            }

            public readonly List<Synapsis> Inputs = new List<Synapsis>();
            public readonly List<Synapsis> Outputs = new List<Synapsis>();

            public Neuron(float weight) : base(weight)
            {
            }

            public static float SigmoidFunction(float value)
            {
                return 1f / (1f + Mathf.Exp(-value));
            }

            public static float SigmoidDerivative(float value)
            {
                return value * (1f - value);
            }

            public Synapsis AttachChildNeuron(Neuron neuron)
            {
                neuron.Inputs.RemoveAll(x => x.Input == this);
                Outputs.RemoveAll(x => x.Output == neuron);
                var synapsis = new Synapsis(this, neuron, Random.Range(-0.5f, 0.5f));
                Outputs.Add(synapsis);
                neuron.Inputs.Add(synapsis);

                return synapsis;
            }

        }

        private readonly List<Synapsis> _synapsises = new List<Synapsis>();
        private readonly List<Neuron> _inputs = new List<Neuron>();
        private readonly List<Neuron> _outputs = new List<Neuron>();
        private readonly List<Neuron> _neurons = new List<Neuron>();
        private readonly List<NNItem> _nnItems = new List<NNItem>();
        public float[] Outputs { get; private set; }
        public int MaxIndex { get; private set; }

        public IEnumerable<float> Weights
        {
            get
            {
                return _nnItems.Select(x => x.Weight);
            }

            set
            {
                var i = 0;
                var nnCount = _nnItems.Count;
                foreach (var weigth in value)
                {
                    if (i >= nnCount) break;

                    _nnItems[i++].Weight = weigth;
                }
            }
        }

        protected NNBase(int inputsCount, int outputCount, int[] hiddenLayers)
        {
            // Debug.Assert(inputsCount > 0, "Wrong inputs count");
            // Debug.Assert(outputCount > 0, "Wrong outputs count")


            Outputs = new float[outputCount];

            _inputs = CreateLayer(inputsCount);
            _neurons.AddRange(_inputs);
            var prevLayer = _inputs;

            if (hiddenLayers != null && hiddenLayers.Length > 0)
            {
                foreach (var hiddenLayer in hiddenLayers)
                {
                    var newLayer = CreateLayer(hiddenLayer);

                    ConnectLayers(prevLayer, newLayer);
                    _neurons.AddRange(newLayer);
                    prevLayer = newLayer;
                                            
                }

            }

            _outputs = CreateLayer(outputCount);
            _neurons.AddRange(_outputs);
            ConnectLayers(prevLayer, _outputs);

            _nnItems.AddRange(_neurons.Select(x => (NNItem)x));
            _nnItems.AddRange(_synapsises.Select(x => (NNItem)x));
        }

        protected NNBase(NNConfig config)
            : this(config.InputsCount, config.OutputsCount, config.HiddenLayers.ToArray())
        {
            Weights = config.Weights;
        }

        private static int _uniqueId;

        private static List<Neuron> CreateLayer(int count)
        {
            var neurons = new List<Neuron>();
            for (var i = 0; i < count; ++i)
            {
                var neuron = new Neuron(Random.Range(-0.5f, 0.5f));
                neuron.Id = _uniqueId++;

                neurons.Add(neuron);
            }

            return neurons;
        }

        private void ConnectLayers(IEnumerable<Neuron> firstLayer, IEnumerable<Neuron> secondLayer)
        {
            foreach (var neuron1 in firstLayer)
            {
                foreach (var neuron2 in secondLayer)
                {
                    var synapsis = neuron1.AttachChildNeuron(neuron2);
                    _synapsises.Add(synapsis);
                }
            }
        }

        public float[] PushData(float[] input)
        {
            if (input.Length != _inputs.Count)
            {
                throw new Exception("Wrong inputs count");
            }

            foreach (var neuron in _neurons)
            {
                neuron.IsDirty = true;
            }

            foreach (var synapsis in _synapsises)
            {
                synapsis.IsDirty = true;
            }

            for (var i = 0; i < input.Length; ++i)
            {
                _inputs[i].Value = input[i];
            }

            for (var i = 0; i < _outputs.Count; ++i)
            {
                Outputs[i] = _outputs[i].Value;
            }

            MaxIndex = 0;
            var maxValue = Outputs[0];
            for (var i = 1; i < Outputs.Length; ++i)
            {
                if (Outputs[i] < maxValue)
                {
                    continue;
                }

                maxValue = Outputs[i];
                MaxIndex = i;
            }

            return Outputs;
        }

        public void DoBackPropagate(int targetIndex, float learnRate = 0.2f)
        {
            // Debug.Assert(targetIndex == _outputs.Count, "Wrong outputs count");

            foreach (var neuron in _neurons)
            {
                neuron.ResetError();
            }

            for (var i = 0; i < _outputs.Count; ++i)
            {
                _outputs[i].SetTarget(targetIndex == i ? 1f : 0f);
            }

            foreach (var synapsis in _synapsises)
            {
                synapsis.DoWeightCorrection(learnRate);
            }

            foreach (var neuron in _inputs)
            {
                neuron.DoWeightCorrection(learnRate);
            }

        }

        public float GetError(int targetIndex)
        {
            var error = 0f;
            for (var i = 0; i < _outputs.Count; ++i)
            {
                var diff = _outputs[i].Value - (i == targetIndex ? 1f : 0f);
                error += diff*diff;
            }

            error /= 2f;

            return error;
        }

    }

}
