using UnityEngine;
namespace ShadowFire
{
    /// Greybox test bot: strafes left/right (so you can test drag-to-head), respawns after death.
    public class TargetDummy : MonoBehaviour
    {
        public bool strafe = true; public float amplitude = 6f, speed = 1.2f, respawn = 3f;
        public GameObject body; Health h; Vector3 origin; float phase;
        void Start()
        {
            h = GetComponent<Health>(); origin = transform.position; phase = Random.value * 6f;
            h.OnDied += _ => { body.SetActive(false); Invoke(nameof(Respawn), respawn); };
        }
        void Respawn() { h.Dead = false; h.hp = h.maxHp; body.SetActive(true); }
        void Update()
        {
            if (strafe && !h.Dead) transform.position = origin + Vector3.right * Mathf.Sin(Time.time * speed + phase) * amplitude;
        }
    }
}
