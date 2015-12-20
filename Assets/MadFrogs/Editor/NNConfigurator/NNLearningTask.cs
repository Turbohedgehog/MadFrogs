using System.Collections.Generic;
using System.Threading;

public class NNLearningTask
{
    private readonly AI.NNConfig _nnConfig;
    private readonly AI.NNExamples _nnExamples;
    private Thread _thread;
    private volatile bool _isStoped;
    private AI.FrogNN _nn;
    public float Error { get; private set; }
    public int Iterations { get; private set; }
    public bool IsRunning { get { return _thread != null && _thread.IsAlive; } }
    public IEnumerable<float> Weights { get { return _nn == null ? null : _nn.Weights; } }

    public NNLearningTask(AI.NNConfig nnConfig, AI.NNExamples nnExamples)
    {
        _nnConfig = nnConfig;
        _nnExamples = nnExamples;
    }

    public bool Start()
    {
        if (_nnConfig == null || _nnExamples == null) return false;

        if (IsRunning) return true;
        _nnConfig.CheckWeights();
        _nn = new AI.FrogNN(_nnConfig);

        _thread = new Thread(CalculateNN);
        _thread.Start();

        return true;

    }

    public void Stop()
    {
        if (IsRunning == false) return;

        _isStoped = true;
        _thread = null;

    }

    private void CalculateNN()
    {
        Iterations = 0;

        do
        {
            var error = 0f;

            foreach (var example in _nnExamples.Examples)
            {
                var input = example.Input;
                var data = new[]
                {
                    input.Health, input.NearMosquitoDistance, input.NearEnemyDistance,(float) input.EnemiesCount, input.IsSomebodyAttacksMe ? 1f : 0f
                };

                _nn.PushData(data);
                error += _nn.GetError((int)example.Action);
                _nn.DoBackPropagate((int)example.Action, _nnConfig.LearningStep);
            }

            ++Iterations;

            error /= _nnExamples.Examples.Count;
            Error = error;

        } while (!_isStoped && Error > _nnConfig.MaxLearningError);

        Stop();
    }
}
