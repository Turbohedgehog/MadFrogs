using UnityEngine;

namespace Gameplay
{
    public class Mosquito : SceneObjectBase
    {
        private float _floatingTimer = 0f;
        public bool IsCatched { get; private set; }
        public MosquitoProfile Profile { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            MosquitoController.Instance.RegisterMosquito(this);
        }

        public override void CallDestroy()
        {
            base.CallDestroy();

            MosquitoController.Instance.UnregisterMosquito(this);
        }

        public void Init(MosquitoProfile profile)
        {
            Profile = profile;

            _floatingTimer = Random.Range(0f, profile.FloatingPeriodDuration);
        }

        protected override void Update()
        {
            base.Update();

            UpdateFloating(Time.deltaTime);
        }

        private void UpdateFloating(float deltaTime)
        {
            if (Profile.FloatingPeriodDuration < 0f) return;
            if (_floatingTimer < 0f) _floatingTimer = 0f;
            
            _floatingTimer += deltaTime;

            while (_floatingTimer >= Profile.FloatingPeriodDuration)
            {
                _floatingTimer -= Profile.FloatingPeriodDuration;
            }

            var value = 2f * _floatingTimer/Profile.FloatingPeriodDuration * Mathf.PI;
            value = Mathf.Sin(value) * Profile.FloatingAmplitude;

            transform.position = new Vector3(transform.position.x, Profile.FlightHeight + value, transform.position.z);
        }

        public void Catch(Frog who)
        {
            IsCatched = true;
            FrogController.Instance.OnMosquitoRemoved(this);

            CallDestroy();
        }

    }

}