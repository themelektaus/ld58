using UnityEngine;

namespace Prototype
{
    public class DescriptionAttribute : PropertyAttribute
    {
        public MessageType type;
        public string text;
        public int height = 1;
    }
}
