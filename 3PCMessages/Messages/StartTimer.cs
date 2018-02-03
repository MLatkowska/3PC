namespace _3PC.Shared.Messages
{
    public class StartTimer
    {
        public int Seconds { get; }

        public StartTimer(int seconds)
        {
            Seconds = seconds;
        }
    }
}