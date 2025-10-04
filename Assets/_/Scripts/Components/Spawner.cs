using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Spawner")]
    public class Spawner : MonoBehaviour
    {
        [SerializeField] List<SpawnerObject> objects;
        [SerializeField] Vector2 interval = Vector2.one;
        [SerializeField] Collider area;

        readonly Timer timer = new();

        void Update()
        {
            if (timer.Update(interval))
                Spawn();
        }

        void Spawn()
        {
            var @object = Utils.RandomPick(objects);
            var position = Utils.RandomPointInsideCollider(area);
            var poolObject = @object.gameObject.Instantiate(position);
            if (@object.lifetime >= 0)
                poolObject.GetOrAddComponent<Destroy>(x => x.delay = @object.lifetime);
            else
                poolObject.RemoveComponent<Destroy>();
        }
    }
}
