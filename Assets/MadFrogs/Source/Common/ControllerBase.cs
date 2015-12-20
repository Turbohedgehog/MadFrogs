using UnityEngine;

namespace Gameplay
{

    public class ControllerBase<T> : MonoBehaviour, IController where T : MonoBehaviour, IController
    {
        public static T Instance { get; private set; }

        public static void CreateInstance()
        {
            if (Instance != null)
            {
                return;
            }

            var contoller = new GameObject("[C] " + typeof (T).Name);
            contoller.AddComponent<T>();
        }

        public static void CallDestroy()
        {
            if (Instance == null)
            {
                return;
            }

            Destroy(Instance);
        }

        protected virtual void Awake()
        {
            if (Instance == this)
            {
                Destroy(Instance);
            }

            Instance = this as T;
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

    }

}