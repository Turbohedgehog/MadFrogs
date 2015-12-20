

namespace Gameplay
{

    public class CombatController : ControllerBase<CombatController>
    {
        public enum BattleState
        {
            MainMenu,
            Battle,
            Pause,
            Results,

        }

        public BattleState CurrentState { get; private set; }
        
    }

}