using UnityEngine;
using UnityEngine.EventSystems;

namespace JuiceStudy.Abilities
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class AbilityPaddle : MonoBehaviour
    {
        [SerializeField] private float xLimit = 5.95f;
        [SerializeField] private float followSpeed = 30f;
        private Camera cam;
        private Vector3 restScale;
        private float baseWidth;
        private float widthFactor = 1;
        public float Width => baseWidth * widthFactor;
        private float TravelLimit => Mathf.Max(0, xLimit - (Width - baseWidth) * 0.5f);

        private void Awake()
        {
            cam = Camera.main;
            restScale = transform.localScale;
            baseWidth = GetComponent<BoxCollider2D>().size.x * Mathf.Abs(transform.lossyScale.x);
        }

        public void SetExpanded(bool expanded)
        {
            float factor = expanded ? 1.6f : 1;
            if (Mathf.Approximately(widthFactor, factor)) return;
            widthFactor = factor;
            transform.localScale = new Vector3(restScale.x * widthFactor, restScale.y, restScale.z);
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, -TravelLimit, TravelLimit);
            transform.position = position;
            Physics2D.SyncTransforms();
        }

        private void Update()
        {
            float keyboard = Input.GetAxisRaw("Horizontal");
            float target;
            if (Mathf.Abs(keyboard) > 0.01f) target = transform.position.x + keyboard * 100;
            else if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            else target = cam.ScreenToWorldPoint(Input.mousePosition).x;
            Vector3 position = transform.position;
            position.x = Mathf.MoveTowards(position.x, Mathf.Clamp(target, -TravelLimit, TravelLimit), followSpeed * Time.deltaTime);
            transform.position = position;
        }
    }
}
