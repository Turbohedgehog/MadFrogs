using Gameplay;
using UnityEngine;

public class FrogAttack : MonoBehaviour
{
    public enum State
    {
        Idle,
        Preparing,
        Attacking,
        Resting,
        
    }

    private Frog _frog;
    private System.Action _onAttackCompleteCallback;
    private float _timer;
    private Frog _target;
    private Vector3 _attackPosition;
    private bool _isTargetAttacked = false;

    public State CurrentState { get; private set; }

    public void Init()
    {
        _frog = gameObject.GetComponent<Frog>();
    }


    public void Attack(Frog attackTarget, System.Action onAttackComplete)
    {
        _onAttackCompleteCallback = onAttackComplete;
        _target = attackTarget;
        CurrentState = State.Preparing;
        _timer = _frog.Profile.AttackData.AttackPreparingDuration;
        _isTargetAttacked = false;
    }

    private void UpdateState(float deltaTime)
    {
        _timer -= deltaTime;

        switch (CurrentState)
        {
            case State.Preparing:
                if (_timer <= 0f)
                {
                    CurrentState = State.Attacking;
                    _timer = _frog.Profile.AttackData.AttackDuration;
                    _attackPosition = transform.position;
                }

                break;

            case State.Attacking:
                UpdateAttackingPos();
                if (_timer <= 0f)
                {
                    CurrentState = State.Resting;
                    _timer = _frog.Profile.AttackData.PostAttackWaitDuration;
                }

                break;

            case State.Resting:
                if (_timer <= 0f)
                {
                    transform.position = _attackPosition;
                    CurrentState = State.Idle;
                    if (_onAttackCompleteCallback != null) _onAttackCompleteCallback();
                }
                break;
        }
    }

    private void UpdateAttackingPos()
    {
        if (_target == null) return;

        RotateToTarget();

        var value = 1f - _timer/_frog.Profile.AttackData.AttackDuration;
        value = Mathf.Clamp01(value);

        if (value >= 0.5f && !_isTargetAttacked)
        {
            _isTargetAttacked = true;
            _target.Hurt(_frog.Profile.AttackData.Damage, _frog);
        }

        var dir = _target.transform.position - transform.position;
        dir.Normalize();
        value = Mathf.PingPong(value, 0.5f);
        transform.position = _attackPosition + dir*value*_frog.Profile.AttackData.AttackRange;
    }

    private void Update()
    {
        UpdateState(Time.deltaTime);
    }

    private void RotateToTarget()
    {
        if (_target == null) return;

        transform.rotation.SetLookRotation(_target.transform.position - transform.position, transform.up);

    }
}
