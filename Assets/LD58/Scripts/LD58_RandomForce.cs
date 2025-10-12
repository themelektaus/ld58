using UnityEngine;

namespace Prototype.LD58
{
    public class LD58_RandomForce : MonoBehaviour
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] float power = 10;
        [SerializeField] float delay = .2f;

        float time;

        void FixedUpdate()
        {
            if (time < delay)
            {
                time += Time.fixedDeltaTime;
                return;
            }

            if (!Mathf.Approximately(power, 0))
            {
                body.AddForce(Random.insideUnitCircle.normalized * power, ForceMode2D.Impulse);
            }
            this.Destroy();
        }
    }
}
