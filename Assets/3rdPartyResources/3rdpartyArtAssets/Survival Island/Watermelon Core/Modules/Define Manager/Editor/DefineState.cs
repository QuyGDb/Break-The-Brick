namespace Watermelon
{
    [System.Serializable]
    public class DefineState
    {
        private string define;
        public string Define => define;

        private bool state;
        public bool State => state;

        public DefineState(string define, bool state)
        {
            this.define = define;
            this.state = state;
        }
    }
}