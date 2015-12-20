using UnityEngine;

namespace Gameplay
{

    public abstract class SceneObjectBase : MonoBehaviour, ISceneObject
    {

        protected virtual void Awake()
        {
            SceneObjectController.Instance.RegisterSceneObject(this);
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void Update()
        {
        }

        public virtual void CallDestroy()
        {
            SceneObjectController.Instance.UnregisterSceneObject(this);

            Destroy(gameObject);
        }

    }

}