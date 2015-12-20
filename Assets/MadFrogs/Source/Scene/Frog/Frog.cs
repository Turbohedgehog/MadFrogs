using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class Frog : SceneObjectBase
    {
        public enum State
        {
            Idle,
            Fighting,
            Moving,
            Hiding,
            Capricioning,
            
        }

        private AI.FrogNN _nn;
        public FrogProfile Profile { get; private set; }
        private FrogMove _moving;
        private FrogHealth _health;
        private FrogVision _vision;
        private FrogAttack _attack;

        public bool IsSomebodyAttacksMe { get; private set; }
        public Frog LastDamageDealler { get; private set; }
        public Frog AttackTarget { get; private set; }
        public int Score { get; private set; }
        public State CurrentState { get; private set; }

        public bool IsVisible
        {
            get
            {
                return CurrentState != State.Hiding && CurrentState != State.Capricioning && CurrentState != State.Fighting;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            FrogController.Instance.RegisterFrog(this);
        }

        public override void CallDestroy()
        {
            base.CallDestroy();

            FrogController.Instance.UnregisterFrog(this);
        }

        public void Init(FrogProfile profile)
        {
            if (profile.AIData == null || profile.AIData.Config == null) return;

            _nn = new AI.FrogNN(profile.AIData.Config);
            Profile = profile;

            SetupMaterial();

            _moving = gameObject.AddComponent<FrogMove>();
            _health = gameObject.AddComponent<FrogHealth>();
            _vision = gameObject.AddComponent<FrogVision>();
            _attack = gameObject.AddComponent<FrogAttack>();

            _moving.Init();
            _health.Init();
            _vision.Init();
            _attack.Init();

        }

        public static List<T> FindComponents<T>(Transform transform) where T : Component
        {
            var components = transform.gameObject.GetComponents<T>();

            var res = new List<T>();

            if(components != null)
                res.AddRange(components);

            for (var i = 0; i < transform.childCount; ++i)
            {
                res.AddRange(FindComponents<T>(transform.GetChild(i)));
            }

            return res;

        }

        private void SetupMaterial()
        {

            var renderers = FindComponents<Renderer>(transform);

            for (var i = 0; i < renderers.Count; ++i)
            {
                var currentRenderer = renderers[i];
                if (currentRenderer.material == null || !currentRenderer.material.HasProperty("_EmissionColor")) continue;

                currentRenderer.material.SetColor("_EmissionColor", Profile.FrogColor);
            }

        }

        protected override void Update()
        {
            base.Update();

            UpdateState();
        }

        private void UpdateState()
        {
            if (CurrentState == State.Capricioning) CurrentState = State.Idle;
            if (CurrentState != State.Idle) return;

            ChooseState();
        }

        private void OnJumpToMosquitoComplete(bool complete)
        {
            var mosquito = MosquitoController.Instance.GetNearMosquito(transform.position);
            if (mosquito != null)
            {
                var mPos = new Vector3(mosquito.transform.position.x, 0f, mosquito.transform.position.z);
                var fPos = new Vector3(transform.position.x, 0f, transform.position.z);
                var dir = mPos - fPos;
                if (dir.sqrMagnitude <= 0.01f)
                {
                    Heal(mosquito.Profile.HealHealth);
                    mosquito.Catch(this);
                    ++Score;
                }
            }

            CurrentState = State.Idle;
        }

        public void Hurt(float damage, Frog damageDealler)
        {
            _health.Hurt(damage);

            if (damageDealler == null) return;

            IsSomebodyAttacksMe = true;
            LastDamageDealler = damageDealler;
        }

        public void Heal(float health)
        {
            _health.Heal(health);
        }

        public bool IsVisibleForMe(Frog me)
        {
            if (CurrentState == State.Hiding || CurrentState == State.Capricioning) return false;
            if (CurrentState != State.Fighting) return true;
            return AttackTarget == me;
        }

        private void ChooseState()
        {
            _vision.UpdateVision();
            var enemiesCount = _vision.VisibleFrogsCount;

            var enemies = FrogController.Instance.GetFrogs(x => x != null && x != this && x.IsVisibleForMe(this));
            var nearEnemyDistance = 1000f;
            if (enemies.Any())
            {
                var nearEnemy = enemies.OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).First();
                nearEnemyDistance = (transform.position - nearEnemy.transform.position).magnitude;
            }

            var mosquito = MosquitoController.Instance.GetNearMosquito(transform.position);
            var nearMosquitoDistance = mosquito == null
                ? 1000f
                : (transform.position - mosquito.transform.position).magnitude;

            var health = _health.Health;
            var isSomebodyAttacksMe = LastDamageDealler != null && LastDamageDealler.AttackTarget == this;

            var data = new[] { health, nearMosquitoDistance, nearEnemyDistance, (float)enemiesCount, isSomebodyAttacksMe ? 1f : 0f };
            _nn.PushData(data);

            CurrentState = UseState(_nn.MaxIndex);
        }

        private State UseState(int nnIndex)
        {
            var nnState = (AI.FrogNN.FrogAction) nnIndex;
            AttackTarget = null;

            switch (nnState)
            {
                case AI.FrogNN.FrogAction.Attack:
                    if (LastDamageDealler != null && LastDamageDealler.AttackTarget == this)
                    {
                        AttackTarget = LastDamageDealler;
                    }
                    else
                    {
                        AttackTarget = FrogController.Instance.GetTargetFor(this);
                    }

                    if (AttackTarget == null) return UseState((int) AI.FrogNN.FrogAction.CatchMosquito);

                    var lenToTarget = (AttackTarget.transform.position - transform.position).magnitude;
                    if (lenToTarget > Profile.AttackData.AttackRange)
                    {
                        if (!_moving.MoveTo(AttackTarget.transform.position, OnJumpToTargetComplete))
                        {
                            return UseState((int)AI.FrogNN.FrogAction.CatchMosquito);
                        }
                    }
                    else
                    {
                        OnJumpToTargetComplete(true);
                    }
                    return State.Fighting;

                case AI.FrogNN.FrogAction.BeCapricious:
                    // return State.Capricioning; // << ToDo: make realization

                case AI.FrogNN.FrogAction.CatchMosquito:
                    if (_moving == null) return State.Idle;
                    var mosquito = MosquitoController.Instance.GetNearMosquito(transform.position);
                    if (mosquito == null) return State.Idle;

                    CurrentState = _moving.MoveTo(mosquito.transform.position, OnJumpToMosquitoComplete) ? State.Moving : State.Idle;
                    return State.Moving;

                case AI.FrogNN.FrogAction.Hide:
                    return State.Capricioning; // << Temp solution
            }

            return State.Idle;
        }

        private void OnJumpToTargetComplete(bool done)
        {
            if (AttackTarget == null || !AttackTarget.IsVisibleForMe(this))
            {
                AttackTarget = null;
                CurrentState = State.Idle;
                return;
            }

            var len = (AttackTarget.transform.position - transform.position).magnitude;
            if (len > Profile.AttackData.AttackRange)
            {
                CurrentState = State.Idle;
                return;
            }

            _attack.Attack(AttackTarget, OnAttackComplete);
        }

        private void OnAttackComplete()
        {
            CurrentState = State.Idle;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this != null && SceneObjectController.Instance != null) SceneObjectController.Instance.UnregisterSceneObject(this);
        }

    }

}