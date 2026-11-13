using UnityEngine;
using UnityEngine.SceneManagement;
namespace ShadowFire
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public enum Stance { Stand, Crouch, Prone }
        public Transform camPivot;
        public float walkSpeed = 5f, crouchSpeed = 2.4f, proneSpeed = 1.1f, downedSpeed = 0.8f, jumpSpeed = 6f, gravity = -22f;
        public Vector2 pitchLimits = new Vector2(-65f, 70f);
        [HideInInspector] public float yaw, pitch, speedMul = 1f;
        public Stance stance; public bool ads;
        [HideInInspector] public WeaponSystem weapon;
        [HideInInspector] public AimAssist assist;
        CharacterController cc; Health hp; float vy;
        static readonly float[] Heights = { 1.8f, 1.2f, 0.6f };

        void Awake() { cc = GetComponent<CharacterController>(); yaw = transform.eulerAngles.y; }
        void Start()
        {
            hp = GetComponent<Health>(); weapon = GetComponent<WeaponSystem>(); assist = GetComponent<AimAssist>();
            if (SensitivityModel.S.gyroEnabled && SystemInfo.supportsGyroscope) Input.gyro.enabled = true;
            if (hp) hp.OnDied += _ => Invoke(nameof(Restart), 3f);
        }
        void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

        public void AddLook(Vector2 deg) { yaw += deg.x; pitch = Mathf.Clamp(pitch - deg.y, pitchLimits.x, pitchLimits.y); }
        public void AddRecoil(Vector2 r) { AddLook(r); }     // r.y = climb up, r.x = yaw wander

        void Update()
        {
            if (hp && hp.Dead) return;
            float dt = Time.deltaTime;
            bool downed = hp && hp.Downed;

            if (GameInput.AdsToggle && !downed) ads = !ads;
            if (GameInput.CrouchToggle) stance = stance == Stance.Crouch ? Stance.Stand : Stance.Crouch;
            if (GameInput.ProneToggle) stance = stance == Stance.Prone ? Stance.Stand : Stance.Prone;
            if (downed) { ads = false; stance = Stance.Prone; }

            // ---- look: drag -> degrees through the sensitivity model ----
            var mode = ads ? (weapon && weapon.Scoped ? SensitivityModel.Mode.Scoped : SensitivityModel.Mode.Ads) : SensitivityModel.Mode.Hip;
            float gunScale = (weapon && weapon.data) ? weapon.SensScale : 1f;
            Vector2 deg = SensitivityModel.ToDegrees(GameInput.LookPx, dt, mode, gunScale);
            if (SensitivityModel.S.gyroEnabled && SystemInfo.supportsGyroscope)
            {
                Vector3 r = Input.gyro.rotationRateUnbiased * Mathf.Rad2Deg * dt * SensitivityModel.S.gyro;
                deg += new Vector2(-r.y, -r.x);   // landscape axes; flip signs per device if needed
            }
            AddLook(deg);
            if (assist) assist.Feed(dt > 0f ? deg / dt : Vector2.zero);

            // ---- move ----
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            float spd = downed ? downedSpeed : stance == Stance.Stand ? walkSpeed : stance == Stance.Crouch ? crouchSpeed : proneSpeed;
            spd *= speedMul * (ads ? 0.7f : 1f);
            Vector3 mv = transform.right * GameInput.Move.x + transform.forward * GameInput.Move.y;
            mv = Vector3.ClampMagnitude(mv, 1f) * spd;
            if (cc.isGrounded) { vy = -2f; if (GameInput.Jump && stance == Stance.Stand && !downed) vy = jumpSpeed; }
            else vy += gravity * dt;
            mv.y = vy;
            cc.Move(mv * dt);

            float h = Heights[(int)stance];
            cc.height = Mathf.Lerp(cc.height, h, dt * 12f);
            cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
            if (camPivot) camPivot.localPosition = new Vector3(0f, Mathf.Max(0.5f, cc.height - 0.2f), 0f);
        }

        void LateUpdate() { GameInput.ClearFrame(); }
    }
}
