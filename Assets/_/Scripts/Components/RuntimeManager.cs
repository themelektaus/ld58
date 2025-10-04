using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_COMMON + "Runtime Manager")]
    public class RuntimeManager : MonoBehaviour
    {
        Scene scene;

        void Update()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (scene == activeScene)
                return;

            if (!activeScene.isLoaded)
                return;

            scene = activeScene;

            foreach (var type in RuntimeInstance.dataTypes)
            {
                var list = Database.findAllMethod
                    .MakeGenericMethod(type)
                    .Invoke(null, null) as IEnumerable;

                foreach (RuntimeInstance.Data data in list)
                {
                    if (data.scene != scene.name)
                        continue;

                    var resource = Utils.GetResource<GameObject>(data.resource);
                    resource.transform.GetChild(1).gameObject.Instantiate(x =>
                    {
                        if (!x.TryGetComponent(out RuntimeUnique unique))
                            x.Kill();

                        unique.id = data.uniqueId;
                    }).SetActive(true);
                }
            }
        }
    }
}
