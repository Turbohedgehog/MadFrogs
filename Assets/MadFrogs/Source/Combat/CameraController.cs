using System.Linq;
using UnityEngine;

namespace Gameplay
{

    public class CameraController : ControllerBase<CameraController>
    {

        private SceneObjectFollower _objectFollower;

        protected override void Start()
        {
            base.Start();

            var cameraParent = new GameObject("TargetFollower");
            _objectFollower = cameraParent.AddComponent<SceneObjectFollower>();
            _objectFollower.Target = FrogController.Instance.GetFrogs().FirstOrDefault();

        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.RightArrow)) NextTarget();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevTarget();
        }

        public void NextTarget()
        {
            var targets = FrogController.Instance.GetFrogs().ToList();

            for (var i = 0; i < targets.Count; ++i)
            {
                if (targets[i] == _objectFollower.Target)
                {
                    _objectFollower.Target = targets[(i + 1)%targets.Count];

                    return;
                }
            }
        }

        public void PrevTarget()
        {
            var targets = FrogController.Instance.GetFrogs().ToList();

            for (var i = 0; i < targets.Count; ++i)
            {
                if (targets[i] == _objectFollower.Target)
                {
                    _objectFollower.Target = targets[(i - 1 + targets.Count) % targets.Count];

                    return;
                }
            }
        }
    }

}