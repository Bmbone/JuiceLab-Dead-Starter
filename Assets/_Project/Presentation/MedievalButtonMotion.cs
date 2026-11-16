using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JuiceStudy.Presentation
{
    /// <summary>Small visual response for existing buttons; click actions remain on their original Button.</summary>
    [DisallowMultipleComponent]
    public sealed class MedievalButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Button button;
        private Vector3 restScale;
        private bool hovered;
        private bool selected;
        private bool pressed;

        private void Awake()
        {
            button = GetComponent<Button>();
            restScale = transform.localScale;
        }

        private void OnEnable()
        {
            selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        }

        private void LateUpdate()
        {
            bool available = button != null && button.IsInteractable();
            float scale = !available ? 1f : pressed ? 0.98f : hovered || selected ? 1.025f : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, restScale * scale,
                1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
        }

        private void OnDisable()
        {
            hovered = selected = pressed = false;
            transform.localScale = restScale;
        }

        public void OnPointerEnter(PointerEventData data) => hovered = true;
        public void OnPointerExit(PointerEventData data) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) pressed = true;
        }
        public void OnPointerUp(PointerEventData data) => pressed = false;
        public void OnSelect(BaseEventData data) => selected = true;
        public void OnDeselect(BaseEventData data) { selected = false; pressed = false; }
    }
}
