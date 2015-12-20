using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class MosquitoController : ControllerBase<MosquitoController>
    {
        private readonly List<Mosquito> _mosquitos = new List<Mosquito>();

        public void RegisterMosquito(Mosquito mosquito)
        {
            if (mosquito == null || _mosquitos.Contains(mosquito)) return;

            _mosquitos.Add(mosquito);
        }

        public void UnregisterMosquito(Mosquito mosquito)
        {
            if (mosquito == null) return;

            _mosquitos.Remove(mosquito);
        }

        public Mosquito CreateMosquito(MosquitoProfile profile, Vector3 pos)
        {
            if (profile == null || profile.Prefab == null) return null;

            var mosquitoGo = Instantiate(profile.Prefab, pos, Quaternion.identity) as GameObject;
            if (mosquitoGo == null) return null;

            var mosquito = mosquitoGo.AddComponent<Mosquito>();
            mosquito.Init(profile);

            mosquito.transform.parent = transform;

            return mosquito;
        }

        protected override void Update()
        {
            base.Update();

            SpawnMosquitos();
        }

        private void SpawnMosquitos()
        {
            if (GameStarter.Instance.SceneProfile.MosquitoProfiles.Count == 0) return;
            var maxMosquitos = GameStarter.Instance.SceneProfile.MaxMosquitos;
            if (_mosquitos.Count >= maxMosquitos) return;

            var needCount = maxMosquitos - _mosquitos.Count;
            var hWidth = GameStarter.Instance.SceneProfile.Width/4f;
            var hLenght = GameStarter.Instance.SceneProfile.Lenght/4f;

            for (var i = 0; i < needCount; ++i)
            {
                var pos = new Vector3(Random.Range(-hWidth, hWidth), 0f, Random.Range(-hLenght, hLenght));
                var profileCount = GameStarter.Instance.SceneProfile.MosquitoProfiles.Count;
                var index = Random.Range(0, 1000*profileCount)%profileCount;
                CreateMosquito(GameStarter.Instance.SceneProfile.MosquitoProfiles[index], pos);
            }
        }

        public Mosquito GetNearMosquito(Vector3 position)
        {
            if(!_mosquitos.Any()) return null;

            var mosquitos = _mosquitos.Where(x => !x.IsCatched);
            if(!mosquitos.Any()) return null;

            var mosquito = mosquitos.OrderBy(x => (x.transform.position - position).sqrMagnitude).First();
            return mosquito;
        }
    }

}