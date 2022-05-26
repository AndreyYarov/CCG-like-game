namespace Game.Events
{
    public abstract class BaseEvent
    {
        public readonly object Sender;
        public readonly object Data;

        public BaseEvent(object sender, object data)
        {
            Sender = sender;
            Data = data;
        }
    }
}
