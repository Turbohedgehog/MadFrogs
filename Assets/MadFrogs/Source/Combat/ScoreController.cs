using UnityEngine;

namespace Gameplay
{
    public class ScoreController : ControllerBase<ScoreController>
    {
        public void OnGUI()
        {
            var frogs = FrogController.Instance.GetFrogsList();

            GUI.Box(new Rect(10, 10, 100, 30 + frogs.Count * 30), "Score");

            for (var i = 0; i < frogs.Count; ++i)
            {
                var frog = frogs[i];
                var score = string.Format("{0}: {1}", frog.Profile.name, frog.Score);
                GUI.Box(new Rect(12, 35 + i * 30, 96, 27), score);
            }

            
        }
    }

}