using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class ScoreController : ControllerBase<ScoreController>
    {
        public void OnGUI()
        {
            var frogs = FrogController.Instance.GetFrogsList().OrderByDescending(x => x.Score).ToList();

            GUI.Box(new Rect(10, 10, 400, 30 + frogs.Count * 30), "Score");

            for (var i = 0; i < frogs.Count; ++i)
            {
                var frog = frogs[i];
                var score = string.Format("{0}: {1} [H:{2:0.0}%, S:{3}]", frog.Profile.name, frog.Score, frog.Health, frog.CurrentNNState);
                GUI.Box(new Rect(12, 35 + i * 30, 396, 27), score);
            }

            
        }
    }

}