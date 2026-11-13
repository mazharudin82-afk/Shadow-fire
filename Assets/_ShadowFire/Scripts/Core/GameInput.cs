using UnityEngine;
namespace ShadowFire
{
    /// Single input bus. Touch UI writes here; DesktopInput writes here in Editor/PC for testing.
    public static class GameInput
    {
        public static Vector2 Move, LookPx;
        public static bool Fire;
        public static bool Jump, Reload, AdsToggle, CrouchToggle, ProneToggle, Cover, Ability, Heal;
        public static void ClearFrame()
        {
            LookPx = Vector2.zero;
            Jump = Reload = AdsToggle = CrouchToggle = ProneToggle = Cover = Ability = Heal = false;
        }
    }

    [DefaultExecutionOrder(-100)]
    public class DesktopInput : MonoBehaviour
    {
        public float mouseToPixels = 6f;
        void Start() { if (Application.isMobilePlatform) { enabled = false; return; } Cursor.lockState = CursorLockMode.Locked; }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            if (Cursor.lockState != CursorLockMode.Locked) { GameInput.Fire = false; return; }
            GameInput.Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            GameInput.LookPx += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseToPixels;
            GameInput.Fire = Input.GetMouseButton(0);
            if (Input.GetMouseButtonDown(1)) GameInput.AdsToggle = true;
            if (Input.GetKeyDown(KeyCode.Space)) GameInput.Jump = true;
            if (Input.GetKeyDown(KeyCode.C)) GameInput.CrouchToggle = true;
            if (Input.GetKeyDown(KeyCode.Z)) GameInput.ProneToggle = true;
            if (Input.GetKeyDown(KeyCode.R)) GameInput.Reload = true;
            if (Input.GetKeyDown(KeyCode.F)) GameInput.Cover = true;
            if (Input.GetKeyDown(KeyCode.Q)) GameInput.Ability = true;
            if (Input.GetKeyDown(KeyCode.H)) GameInput.Heal = true;
        }
    }
}
