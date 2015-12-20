
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{

    public class FrogController : ControllerBase<FrogController>
    {
        private readonly List<Frog> _frogs = new List<Frog>();

        public Frog CreateFrog(FrogProfile profile, Vector3 pos, Quaternion rotation)
        {
            if (profile == null || profile.Prefab == null) return null;

            var frogGo = Instantiate(profile.Prefab, pos, rotation) as GameObject;
            if (frogGo == null) return null;

            var frog = frogGo.AddComponent<Frog>();
            frog.Init(profile);

            frog.transform.parent = transform;

            return frog;
        }

        public void RegisterFrog(Frog frog)
        {
            if (frog == null || _frogs.Contains(frog)) return;
            _frogs.Add(frog);
        }

        public void UnregisterFrog(Frog frog)
        {
            if (frog == null) return;
            _frogs.Remove(frog);
        }

        public IEnumerable<Frog> GetFrogs(Func<Frog, bool> predicate = null)
        {
            return predicate == null ? _frogs : _frogs.Where(predicate);
        }

        public void CreateTestFrog()
        {
            if (!GameStarter.Instance.SceneProfile.FrogProfiles.Any()) return;

            var profilesCount = GameStarter.Instance.SceneProfile.FrogProfiles.Count;
            var hWidth = GameStarter.Instance.SceneProfile.Width/4f;
            var hLenght = GameStarter.Instance.SceneProfile.Lenght / 4f;

            for (var i = 0; i < 2; ++i)
            {
                var index = UnityEngine.Random.Range(0, profilesCount*1000)%profilesCount;
                var profile = GameStarter.Instance.SceneProfile.FrogProfiles[index];
                var x = UnityEngine.Random.Range(0f, hWidth);
                var z = UnityEngine.Random.Range(0f, hLenght);
                var frog = CreateFrog(profile, new Vector3(x, 0f, z), Quaternion.identity);

                frog.transform.parent = transform;
            }

        }

        public void OnMosquitoRemoved(Mosquito mosquito)
        {

        }

        public Frog GetTargetFor(Frog frog)
        {
            var enemies = _frogs.Where(x => x != frog && x.IsVisibleForMe(frog));
            if(!enemies.Any()) return null;

            var attackers = enemies.Where(x => x.AttackTarget == frog);
            if (attackers.Any()) enemies = attackers;
            enemies.OrderBy(x => (x.transform.position - frog.transform.position).sqrMagnitude);

            return enemies.First();
        }
    }

}