using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    public static class Monitor
    {
        public static readonly HashSet<Condition> conditions = new();
        public static readonly Dictionary<ObjectQuery, List<Object>> queries = new();
    }
}
