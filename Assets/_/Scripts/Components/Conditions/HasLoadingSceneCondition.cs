using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    [AddComponentMenu(Const.PROTOTYPE_CONDITION + "Has Loading Scene")]
    public class HasLoadingSceneCondition : Condition
    {
        public override IEnumerable<bool> If()
        {
            yield return LoadingScene.current;
        }
    }
}
