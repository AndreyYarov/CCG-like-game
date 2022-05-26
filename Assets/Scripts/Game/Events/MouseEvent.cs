using UnityEngine;

namespace Game.Events
{
    public class MouseEvent : BaseEvent
    {
        public enum EventType
        {
            MouseDown,
            MouseUp,
            MouseMove,
        }

        public readonly EventType Type;
        public readonly Transform Transform;

        public MouseEvent(object sender, EventType eventType, Transform transform, object data) : base(sender, data)
        {
            Type = eventType;
            Transform = transform;
        }
    }
}
