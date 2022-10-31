
    public abstract class GameState
    {
        public abstract void EnterState(GameManager game);

        public abstract void UpdateState(GameManager game);
        
        public abstract void LeaveState(GameManager game);
    }