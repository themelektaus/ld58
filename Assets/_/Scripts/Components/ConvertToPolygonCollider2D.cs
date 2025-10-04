using UnityEngine;

namespace Prototype
{
    [DefaultExecutionOrder(55)]
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Convert to Polygon Collider (2D)")]
    public class ConvertToPolygonCollider2D : MonoBehaviour
    {
        void Awake()
        {
            var a = GetComponent<BoxCollider2D>();
            a.size = new(
                Mathf.Max(a.size.x, 20.0001f),
                a.size.y < 11.5001f ? 11.255f : a.size.y
            );
            var b = gameObject.AddComponent<PolygonCollider2D>();
            b.isTrigger = a.isTrigger;
            var bounds = a.bounds;
            var min = bounds.min;
            var max = bounds.max;
            var offset = (Vector2) transform.position;
            b.SetPath(0, new[]
            {
                new Vector2(min.x, min.y) - offset,
                new Vector2(max.x, min.y) - offset,
                new Vector2(max.x, max.y) - offset,
                new Vector2(min.x, max.y) - offset
            });
            Destroy(a);
            Destroy(this);
        }
    }
}
