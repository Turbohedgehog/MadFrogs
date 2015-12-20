using UnityEngine;

namespace Gameplay
{

    public class SceneObjectFollower : MonoBehaviour
    {
        private CombatSceneProfile.CameraPofile _profile;
        private SceneObjectBase _target;

        public SceneObjectBase Target
        {
            get { return _target; }
            set
            {
                _target = value;
                if (_target != null)
                {
                    this.transform.rotation = value.transform.rotation;
                }
            }
        }
        private Vector3 _monopod = new Vector3();

        private void Awake()
        {
            _profile = GameStarter.Instance.SceneProfile.CameraInfo;

            var cam = Camera.main;
            cam.transform.parent = transform;
            cam.transform.localRotation = Quaternion.Euler(_profile.Tangent, 0f, 0f);
            cam.transform.localPosition = Vector3.zero;
        }

        private void Update()
        {
            UpdateTarget(Time.deltaTime);
        }

        private void UpdateTarget(float deltaTime)
        {
            if (Target == null) return;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Target.transform.rotation, deltaTime * _profile.AngularRotationVelocity);
            var monopod = transform.rotation * _profile.Monopod;
            transform.position = new Vector3(Target.transform.position.x, 0f, Target.transform.position.z) + monopod;

        }
    }

}