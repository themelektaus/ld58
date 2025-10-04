using UnityEngine;

namespace Prototype
{
    public interface ICameraTarget
    {
        public Vector3 GetLookAtPosition();
        public Vector3 GetFollowPosition();
    }
}
