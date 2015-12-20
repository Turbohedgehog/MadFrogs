using UnityEngine;

namespace Gameplay
{

    public class FrogMove : MonoBehaviour
    {
        public enum JumpState
        {
            Idle,
            Preparing,
            Jumping,
            Resting,
            
        }

        private Frog _frog;
        private float _timer;
        private float _timeToJump;
        private Vector3 _jumpVelocity;
        private Vector3 _startJumpPosition;
        private System.Action<bool> _jumpCompleteCallback;
        public Vector3 JumpToPos { get; private set; }
        private Quaternion _jumpRotation;
        public Vector3 Gravity { get { return new Vector3(0f, GameStarter.Instance.SceneProfile.Gravity, 0f); } }

        public JumpState CurrentJumpState { get; private set; }

        public void Init()
        {
            _frog = GetComponent<Frog>();
        }

        public bool MoveTo(Vector3 pos, System.Action<bool> jumpCompleteCallback = null)
        {
            if (CurrentJumpState != JumpState.Idle)
            {
                if (jumpCompleteCallback != null) jumpCompleteCallback(false);

                return false;
            }

            _jumpCompleteCallback = jumpCompleteCallback;
            CurrentJumpState = JumpState.Preparing;
            JumpToPos = new Vector3(pos.x, 0f, pos.z);
            _timer = _frog.Profile.MovingData.PrepareForJumpDuration;

            return true;
        }

        public void Update()
        {
            UpdateJumpState(Time.deltaTime);
        }

        private void UpdateJumpState(float deltaTime)
        {
            if (CurrentJumpState == JumpState.Idle) return;

            switch (CurrentJumpState)
            {
                case JumpState.Preparing:
                    if (_timer <= 0f)
                    {
                        _timer = CalculateJump();
                        RotateToJumpDirection();
                        CurrentJumpState = JumpState.Jumping;
                    }
                    break;
                    

                case JumpState.Jumping:
                    UpdateJumpValue();
                    if (_timer <= 0f)
                    {
                        _timer = _frog.Profile.MovingData.PostJumpWaitDuration;
                        CurrentJumpState = JumpState.Resting;
                        transform.position = JumpToPos;
                    }
                    break;

                case JumpState.Resting:
                    if (_timer <= 0f)
                    {
                        _timer = 0f;
                        CurrentJumpState = JumpState.Idle;
                        if (_jumpCompleteCallback != null) _jumpCompleteCallback(true);
                        _jumpCompleteCallback = null;
                    }
                    break;
            }

            _timer -= deltaTime;
        }

        private float CalculateJump()
        {
            _startJumpPosition = transform.position;
            var dir = JumpToPos - transform.position;
            var angle = Vector3.Angle(transform.forward, dir);
            _jumpRotation = Quaternion.identity;
            if (angle > _frog.Profile.MovingData.MaxJumpAngle)
            {
                var axis = Vector3.Cross(transform.forward, dir.normalized);
                if (axis.sqrMagnitude < 0.001f) axis = transform.up;
                _jumpRotation = Quaternion.AngleAxis(_frog.Profile.MovingData.MaxJumpAngle, axis);

                var makeJump = dir.sqrMagnitude < _frog.Profile.MovingData.MaxJumpDistance*_frog.Profile.MovingData.MaxJumpDistance;
                JumpToPos = transform.position;

                if (!makeJump)
                {
                    _timeToJump = _frog.Profile.MovingData.RotateDuration;
                    CalculateStartJumpVelocity(Vector3.zero);
                    return _timeToJump;
                }

                _timeToJump = _frog.Profile.MovingData.MaxJumpDistance/_frog.Profile.MovingData.Speed;
                dir = _frog.Profile.MovingData.MaxJumpDistance * (_jumpRotation * transform.forward);
                JumpToPos += dir;
            }
            else
            {
                var len = dir.magnitude;
                len = Mathf.Min(_frog.Profile.MovingData.MaxJumpDistance, len);
                dir = dir.normalized*len;
                _jumpRotation = Quaternion.FromToRotation(transform.forward, dir);
                JumpToPos = transform.position + dir;
                _timeToJump = len / _frog.Profile.MovingData.Speed;
            }


            CalculateStartJumpVelocity(dir);

            return _timeToJump;
        }

        private void RotateToJumpDirection()
        {
            transform.rotation = _jumpRotation * transform.rotation;
        }

        private void CalculateStartJumpVelocity(Vector3 diff)
        {
            _jumpVelocity = Vector3.zero;

            if (_timeToJump <= 0f)
            {
                return;
            }

            _jumpVelocity = diff/_timeToJump;
            _jumpVelocity += Gravity * _timeToJump / 2f;
        }

        private void UpdateJumpValue()
        {
            var time = _timeToJump - Mathf.Clamp(_timer, 0f, _timeToJump);
            var position = _startJumpPosition;
            position += time*_jumpVelocity;
            position -= Gravity * time * time / 2f;

            transform.position = position;
        }

    }

}