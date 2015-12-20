using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class FrogVision : MonoBehaviour
    {
        private Frog _frog;
        private readonly HashSet<Frog> _visibleFrogs = new HashSet<Frog>();
        private float _visionAngleCos;

        public int VisibleFrogsCount { get { return _visibleFrogs.Count; } }

        public void Init()
        {
            _frog = gameObject.GetComponent<Frog>();
            _visionAngleCos = Mathf.Cos(Mathf.Deg2Rad * _frog.Profile.VisionData.VisionAngle);
        }

        public void UpdateVision()
        {
            _visibleFrogs.Clear();

            var anotherFrogs = FrogController.Instance.GetFrogs(x => x != _frog && x.IsVisibleForMe(_frog));
            var sqrRad = _frog.Profile.VisionData.FeelEnemyRadius;
            sqrRad *= sqrRad;

            var visionRangeSqr = _frog.Profile.VisionData.VisionRange;
            visionRangeSqr *= visionRangeSqr;

            foreach (var anotherFrog in anotherFrogs)
            {
                if(anotherFrog == null) continue;

                var dir = anotherFrog.transform.position - transform.position;
                var dirSqr = dir.sqrMagnitude;
                if (dirSqr <= sqrRad)
                {
                    _visibleFrogs.Add(anotherFrog);
                    continue;
                };

                if(dirSqr > visionRangeSqr) continue;

                var dotProduct = Vector3.Dot(transform.forward, dir.normalized);
                if(dotProduct < _visionAngleCos) continue;

                _visibleFrogs.Add(anotherFrog);

            }

        }

    }

}