using UnityEngine;
namespace ShadowFire
{
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCamera : MonoBehaviour
    {
        public PlayerController target;
        public Vector3 hipOffset = new Vector3(0.6f, 0.1f, -2.8f);   // over-right-shoulder
        public Vector3 adsOffset = new Vector3(0.45f, 0.05f, -1.5f);
        public float hipFov = 70f, smooth = 14f;
        Camera cam; Vector3 cur;

        void Awake() { cam = GetComponent<Camera>(); cur = hipOffset; }
        void LateUpdate()
        {
            if (!target || !target.camPivot) return;
            bool ads = target.ads;
            float fov = ads ? (target.weapon && target.weapon.data ? target.weapon.AdsFov : 55f) : hipFov;
            cur = Vector3.Lerp(cur, ads ? adsOffset : hipOffset, Time.deltaTime * smooth);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, Time.deltaTime * smooth);
            Quaternion rot = Quaternion.Euler(target.pitch, target.yaw, 0f);
            Vector3 pivot = target.camPivot.position;
            Vector3 want = pivot + rot * cur;
            if (Physics.Linecast(pivot, want, out var hit, ~0, QueryTriggerInteraction.Ignore) && hit.transform.root != target.transform.root)
                want = hit.point + hit.normal * 0.1f;
            transform.SetPositionAndRotation(want, rot);
        }
    }
}
