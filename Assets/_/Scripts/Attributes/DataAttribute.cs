using System;

namespace Prototype
{
    public class DataAttribute : Attribute
    {
        public Type type { get; private set; }
        public DataAttribute(Type type) => this.type = type;
    }
}
