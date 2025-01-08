namespace Watermelon
{
    public interface IWorldElement
    {
        public int InitialisationOrder { get; }
        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public void OnWorldLoaded();
        public void OnWorldUnloaded();
    }
}
