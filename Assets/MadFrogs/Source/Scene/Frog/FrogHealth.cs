using UnityEngine;

namespace Gameplay
{

    public class FrogHealth : MonoBehaviour
    {
        private Frog _frog;
        public float Health { get; private set; }

        public void Init()
        {
            _frog = gameObject.GetComponent<Frog>();

            Health = _frog.Profile.PersonalData.Health;
        }

        public float Hurt(float damage)
        {
            Health -= damage;
            ClampHealth();

            return Health;
        }

        public float Heal(float health)
        {
            Health += health;
            ClampHealth();

            return Health;
        }

        public float Restore()
        {
            Health = _frog.Profile.PersonalData.Health;
            ClampHealth();

            return Health;
        }

        private void ClampHealth()
        {
            Health = Mathf.Clamp(Health, 0f, _frog.Profile.PersonalData.Health);
        }

        private void Update()
        {
            RestoreHealth(Time.deltaTime);
        }

        private void RestoreHealth(float deltaTime)
        {
            if (_frog == null || _frog.CurrentState != Frog.State.Capricioning) return;
            Heal(deltaTime * _frog.Profile.PersonalData.HealthRegenerationDuration);

        }
    }

}