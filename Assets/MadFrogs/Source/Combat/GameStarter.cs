
namespace Gameplay
{

    public class GameStarter : ControllerBase<GameStarter>
    {
        public CombatSceneProfile SceneProfile;

        protected override void Awake()
        {
            base.Awake();

            name = "[C] " + name;

            CombatController.CreateInstance();
            SceneObjectController.CreateInstance();
            FrogController.CreateInstance();
            MosquitoController.CreateInstance();
            CameraController.CreateInstance();
        }

        protected override void Start()
        {
            base.Start();

            FrogController.Instance.CreateTestFrog();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            FrogController.CallDestroy();
            MosquitoController.CallDestroy();
            CombatController.CallDestroy();
            SceneObjectController.CallDestroy();
            CameraController.CallDestroy();
        }

    }

}