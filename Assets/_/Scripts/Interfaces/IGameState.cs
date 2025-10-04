namespace Prototype
{
    public interface IGameState<T> where T : GameStateInstance
    {
        public T gameStateInstance { get; set; }
    }
}
