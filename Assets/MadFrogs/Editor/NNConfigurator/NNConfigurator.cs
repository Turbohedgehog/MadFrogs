using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NNConfigurator : EditorWindow
{
    private static NNConfigurator _configurator;
    private static AI.NNConfig _nnConfig;
    private static AI.NNExamples _nnExamples;
    public static bool IsLearning { get { return _nnLearningTask != null; } }
    private static NNLearningTask _nnLearningTask;
    private DateTime _lastUpdateLearningTime = DateTime.Now;
    private float _learningError;
    private int _iterations;

    [MenuItem("Window/Mad Frogs/NNEditor")]
    public static void ShowWindow()
    {
        _configurator = EditorWindow.GetWindow<NNConfigurator>();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        GUI.enabled = !IsLearning;

        EditorGUILayout.Separator();

        _nnConfig = EditorGUILayout.ObjectField("NN Config", _nnConfig, typeof (AI.NNConfig), true) as AI.NNConfig;

        var nnExamples = EditorGUILayout.ObjectField("Examples", _nnExamples, typeof(AI.NNExamples), true) as AI.NNExamples;
        if (nnExamples != _nnExamples) _selectedExample = 0;
        _nnExamples = nnExamples;

        GUI.enabled = true;

        DrawNNConfig();

        EditorGUILayout.EndVertical();

    }

    private Vector2 _exampleScrollPosition;
    private int _selectedExample;
    private GUIStyle _toggleButtonStyleNormal;
    private GUIStyle _toggleButtonStyleToggled;

    private void DrawNNConfig()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        if (IsLearning)
        {
            DrawLearning(0.23f);
            GUI.enabled = false;
        }
        else
        {
            DrawNNOptions(0.23f);
        }

        DrawExamplesListPanel(0.23f);
        DrawInputPanel(0.23f);
        DrawOutputPanel(0.23f);

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLearning(float widthPercent)
    {
        if (_configurator != null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_configurator.position.width * widthPercent));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }

        GUILayout.Label(string.Format("Learning error: {0}", _learningError));
        GUILayout.Label(string.Format("Iterations: {0}", _iterations));

        if (GUILayout.Button(">>> Stop Learning <<<"))
        {
            StopLearning();
        }


        EditorGUILayout.EndVertical();
    }

    private void DrawNNOptions(float widthPercent)
    {
        if (_nnConfig == null) return;

        if (_configurator != null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_configurator.position.width*widthPercent));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }

        var learningStepText = _nnConfig.LearningStep.ToString();
        var newLearningStepText = EditorGUILayout.TextField("Learning step", learningStepText);
        if (newLearningStepText != learningStepText)
        {
            var newLearningStep = 0f;
            if (float.TryParse(newLearningStepText, out newLearningStep) && newLearningStep > 0f)
            {
                _nnConfig.LearningStep = newLearningStep;
                EditorUtility.SetDirty(_nnConfig);
            }
        }

        var learningErrorText = _nnConfig.MaxLearningError.ToString();
        var newLearningErrorText = EditorGUILayout.TextField("Max learning error", learningErrorText);
        if (newLearningErrorText != learningErrorText)
        {
            var newLearningError = 0f;
            if (float.TryParse(newLearningErrorText, out newLearningError) && newLearningError > 0f)
            {
                _nnConfig.MaxLearningError = newLearningError;
                EditorUtility.SetDirty(_nnConfig);
            }
        }

        EditorGUILayout.Separator();

        var idxToDelete = -1;

        for(var i = 0; i < _nnConfig.HiddenLayers.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            var countText = _nnConfig.HiddenLayers[i].ToString();
            var count = EditorGUILayout.TextField(string.Format("Hidden layer #{0} width", i + 1), countText);

            if (countText != count)
            {
                var newCount = 0;
                if (int.TryParse(count, out newCount) && newCount > 1)
                {
                    _nnConfig.HiddenLayers[i] = newCount;
                }
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(20f));
            if (GUILayout.Button("X")) idxToDelete = i;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            if (idxToDelete >= 0)
            {
                _nnConfig.HiddenLayers.RemoveAt(idxToDelete);
                EditorUtility.SetDirty(_nnConfig);

            }

        }

        EditorGUILayout.Separator();
        if (GUILayout.Button("+"))
        {
            _nnConfig.HiddenLayers.Add(1);
            EditorUtility.SetDirty(_nnConfig);
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("One step"))
        {
            MakeStep();
        }

        if (GUILayout.Button("!!!! Reset NN !!!!"))
        {
            ResetNN();
        }

        if (GUILayout.Button(">>> Start Learning <<<"))
        {
            StartLearning();
        }

        EditorGUILayout.EndVertical();

    }

    private void ResetNN()
    {
        if (_nnConfig == null) return;

        _nnConfig.ResetWeights();
    }

    private void InitToggleButtonStyles()
    {
        if (_toggleButtonStyleNormal != null) return;

        _toggleButtonStyleNormal = "Button";
        _toggleButtonStyleToggled = new GUIStyle(_toggleButtonStyleNormal);
        _toggleButtonStyleToggled.normal.background = _toggleButtonStyleToggled.active.background;
    }

    private void DrawExamplesListPanel(float widthPercent)
    {
        if (_nnExamples == null) return;

        InitToggleButtonStyles();

        if (_configurator != null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_configurator.position.width * widthPercent));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }

        _exampleScrollPosition = EditorGUILayout.BeginScrollView(_exampleScrollPosition);
        var idxToDelete = -1;

        for (var i = 0; i < _nnExamples.Examples.Count; ++i)
        {
            var curStyle = i == _selectedExample ? _toggleButtonStyleToggled : _toggleButtonStyleNormal;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(string.Format("#{0}", i + 1), curStyle))
            {
                _selectedExample = i;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(20f));
            if (GUILayout.Button("X")) idxToDelete = i;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();
        if (GUILayout.Button("+")) AddExample();

        DeleteExample(idxToDelete);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

    }

    private void AddExample()
    {
        if (_nnExamples == null) return;

        _nnExamples.Examples.Add(new AI.NNExamples.Example {Input = new AI.FrogNN.NNInputs()});
        if (_selectedExample < 0) _selectedExample = 0;
        EditorUtility.SetDirty(_nnExamples);
    }

    private void DeleteExample(int idx)
    {
        if (_nnExamples == null) return;

        if (idx < 0 || idx >= _nnExamples.Examples.Count) return;
        _nnExamples.Examples.RemoveAt(idx);
        if (_selectedExample >= _nnExamples.Examples.Count) _selectedExample = _nnExamples.Examples.Count - 1;
        EditorUtility.SetDirty(_nnExamples);
    }

    public bool IsExampleValid
    {
        get
        {
            if (_nnExamples == null) return false;
            if (_selectedExample < 0 || _selectedExample >= _nnExamples.Examples.Count) return false;
            var example = _nnExamples.Examples[_selectedExample];
            if (example == null) return false;

            return true;
        }
    }

    private void DrawInputPanel(float widthPercent)
    {
        if (!IsExampleValid) return;

        var example = _nnExamples.Examples[_selectedExample];

        if (_configurator != null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_configurator.position.width * widthPercent));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }

        var healthText = example.Input.Health.ToString();
        var newHealthText = EditorGUILayout.TextField("Heath", healthText);
        if (healthText != newHealthText)
        {
            var heath = 0f;
            if (float.TryParse(newHealthText, out heath))
            {
                example.Input.Health = Mathf.Clamp(heath, 0f, 100f);
                EditorUtility.SetDirty(_nnExamples);
            }
        }

        var nearMosquitoDistanceText = example.Input.NearMosquitoDistance.ToString();
        var newNearMosquitoDistanceText = EditorGUILayout.TextField("Distance to mosquito", nearMosquitoDistanceText);
        if (nearMosquitoDistanceText != newNearMosquitoDistanceText)
        {
            var nearMosquitoDistance = 0f;
            if (float.TryParse(newNearMosquitoDistanceText, out nearMosquitoDistance))
            {
                example.Input.NearMosquitoDistance = Mathf.Max(0f, nearMosquitoDistance);
                EditorUtility.SetDirty(_nnExamples);
            }
        }

        var nearEnemyDistanceText = example.Input.NearEnemyDistance.ToString();
        var newNearEnemyDistanceText = EditorGUILayout.TextField("Distance to near enemy", nearEnemyDistanceText);
        if (newNearEnemyDistanceText != nearEnemyDistanceText)
        {
            var nearEnemyDistance = 0f;
            if (float.TryParse(newNearEnemyDistanceText, out nearEnemyDistance))
            {
                example.Input.NearEnemyDistance = Mathf.Max(0f, nearEnemyDistance);
                EditorUtility.SetDirty(_nnExamples);
            }
        }

        var enemiesText = example.Input.EnemiesCount.ToString();
        var newEnemiesText = EditorGUILayout.TextField("Visible enemies", enemiesText);
        if (enemiesText != newEnemiesText)
        {
            var enemies = 0;
            if (int.TryParse(newEnemiesText, out enemies))
            {
                example.Input.EnemiesCount = Mathf.Max(0, enemies);
                EditorUtility.SetDirty(_nnExamples);
            }
        }

        var isSomebodyAttacksMe = EditorGUILayout.Toggle("Is somebody attacks me", example.Input.IsSomebodyAttacksMe);
        if (example.Input.IsSomebodyAttacksMe != isSomebodyAttacksMe)
        {
            example.Input.IsSomebodyAttacksMe = isSomebodyAttacksMe;
            EditorUtility.SetDirty(_nnExamples);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawOutputPanel(float widthPercent)
    {
        if (!IsExampleValid) return;

        InitToggleButtonStyles();
        var example = _nnExamples.Examples[_selectedExample];

        var values = Enum.GetValues(typeof (AI.FrogNN.FrogAction));
        if (_configurator != null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_configurator.position.width * widthPercent));
        }
        else
        {
            EditorGUILayout.BeginVertical();
        }

        foreach (var value in values)
        {
            var val = (AI.FrogNN.FrogAction) value;
            var textValue = val.ToString();
            var curStyle = val == example.Action ? _toggleButtonStyleToggled : _toggleButtonStyleNormal;

            if (GUILayout.Button(textValue, curStyle))
            {
                example.Action = val;
                EditorUtility.SetDirty(_nnExamples);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private AI.FrogNN _oneStepNN;

    private void MakeStep()
    {
        if (_nnConfig == null || _nnExamples == null) return;
        if (!_nnExamples.Examples.Any()) return;

        var example = _nnExamples.Examples[_selectedExample];
        if (example == null || example.Input == null) return;

        _nnConfig.CheckWeights();

        _oneStepNN = new AI.FrogNN(_nnConfig);

        var input = example.Input;
        var data = new[]
            {
                input.Health, input.NearMosquitoDistance, input.NearEnemyDistance, (float) input.EnemiesCount, input.IsSomebodyAttacksMe ? 1f : 0f
            };

        _oneStepNN.PushData(data);
        Debug.Log("Result: " + _oneStepNN.MaxIndex);

    }


    private void StartLearning()
    {
        if (_nnConfig == null || _nnExamples == null) return;
        if (_nnLearningTask != null) return;

        _nnConfig.InputsCount = Enum.GetValues(typeof (AI.FrogNN.FrogInput)).Length;
        _nnConfig.OutputsCount = Enum.GetValues(typeof (AI.FrogNN.FrogAction)).Length;
        _nnLearningTask = new NNLearningTask(_nnConfig, _nnExamples);
        EditorUtility.SetDirty(_nnConfig);

        _nnLearningTask.Start();
    }

    private void StopLearning()
    {
        if (_nnLearningTask == null) return;
        _nnLearningTask.Stop();
        var weights = _nnLearningTask.Weights;
        if (weights != null)
        {
            _nnConfig.Weights = weights.ToArray();
        }
        
        _nnLearningTask = null;
        EditorUtility.SetDirty(_nnConfig);

    }

    private void Update()
    {
        UpdateLearningProgress();
    }

    private void UpdateLearningProgress()
    {
        if (!IsLearning) return;

        var time = DateTime.Now;
        if ((time - _lastUpdateLearningTime).TotalSeconds < 1f) return;
        _lastUpdateLearningTime = time;

        _learningError = _nnLearningTask.Error;
        _iterations = _nnLearningTask.Iterations;
        if (!_nnLearningTask.IsRunning) StopLearning();
        Repaint();

    }
}
