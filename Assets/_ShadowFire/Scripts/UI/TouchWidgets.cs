using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace ShadowFire
{
    /// Right-thumb drag = look. Raw pixel deltas go to GameInput; SensitivityModel converts to degrees.
    public class TouchLook : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData e) { GameInput.LookPx += e.delta; }
    }

    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform bg, knob; public float radius = 110f;
        public void OnPointerDown(PointerEventData e) { OnDrag(e); }
        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bg, e.position, e.pressEventCamera, out var p);
            Vector2 v = Vector2.ClampMagnitude((p - bg.rect.center) / radius, 1f);
            knob.anchoredPosition = v * radius; GameInput.Move = v;
        }
        public void OnPointerUp(PointerEventData e) { knob.anchoredPosition = Vector2.zero; GameInput.Move = Vector2.zero; }
    }

    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action<bool> OnHold;
        public void OnPointerDown(PointerEventData e) { OnHold?.Invoke(true); }
        public void OnPointerUp(PointerEventData e) { OnHold?.Invoke(false); }
    }
}
