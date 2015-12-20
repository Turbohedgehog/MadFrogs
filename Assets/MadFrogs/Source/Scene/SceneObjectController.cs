using System.Collections.Generic;

namespace Gameplay
{
    public class SceneObjectController : ControllerBase<SceneObjectController>
    {

        private readonly List<ISceneObject> _sceneObjects = new List<ISceneObject>();

        public void RegisterSceneObject(ISceneObject sceneObject)
        {
            if (sceneObject == null || _sceneObjects.Contains(sceneObject)) return;

            _sceneObjects.Add(sceneObject);
        }

        public void UnregisterSceneObject(ISceneObject sceneObject)
        {
            if (sceneObject == null) return;

            _sceneObjects.Remove(sceneObject);
        }

    }

}