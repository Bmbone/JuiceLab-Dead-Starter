using UnityEngine;

namespace JuiceStudy.StoneCourt
{
    /// <summary>Per-renderer stone styling. Never changes a transform, collider, or shared material.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class MedievalStoneSurface : MonoBehaviour
    {
        [SerializeField, Range(1, 32)] private int columns = 1;
        [SerializeField, Range(1, 32)] private int courses = 1;
        [SerializeField] private bool staggerCourses;
        [SerializeField] private bool followSpriteColor = true;
        [SerializeField] private Color stoneTint = new Color(0.38f, 0.43f, 0.46f, 1);
        [SerializeField] private bool carvedEmblem = true;
        [SerializeField] private bool crenellations;
        private SpriteRenderer surface;
        private MaterialPropertyBlock properties;
        private Sprite previousSprite;
        private Vector2 previousSize;
        private Color previousTint;
        private int previousStyle = -1;
        private float seed;
        private static readonly int TintId = Shader.PropertyToID("_StoneTint");
        private static readonly int RectId = Shader.PropertyToID("_SpriteRect");
        private static readonly int SizeId = Shader.PropertyToID("_PhysicalSize");
        private static readonly int TilesId = Shader.PropertyToID("_Tiles");
        private static readonly int EmblemId = Shader.PropertyToID("_Emblem");
        private static readonly int CrenelsId = Shader.PropertyToID("_Crenels");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        private void OnEnable()
        {
            surface = GetComponent<SpriteRenderer>();
            properties = new MaterialPropertyBlock();
            Vector3 position = transform.position;
            int key = unchecked(Mathf.RoundToInt(position.x * 100) * 73856093 ^ Mathf.RoundToInt(position.y * 100) * 19349663);
            seed = (key & 65535) / 65535f * 83f;
            previousStyle = -1;
            Refresh();
        }

        public void Configure(int newColumns, int newCourses, Color tint, bool stagger = false)
        {
            columns = Mathf.Clamp(newColumns, 1, 32);
            courses = Mathf.Clamp(newCourses, 1, 32);
            stoneTint = tint;
            followSpriteColor = false;
            staggerCourses = stagger;
            carvedEmblem = false;
            crenellations = false;
            previousStyle = -1;
            Refresh();
        }

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            if (surface == null || surface.sprite == null || properties == null) return;
            Bounds bounds = surface.sprite.bounds;
            Vector3 scale = transform.lossyScale;
            Vector2 size = new Vector2(Mathf.Abs(bounds.size.x * scale.x), Mathf.Abs(bounds.size.y * scale.y));
            Color tint = followSpriteColor ? surface.color : stoneTint;
            int style = columns + courses * 64 + (staggerCourses ? 4096 : 0) +
                (carvedEmblem ? 8192 : 0) + (crenellations ? 16384 : 0);
            if (previousStyle == style && previousSprite == surface.sprite && previousSize == size && previousTint == tint) return;
            previousStyle = style;
            previousSprite = surface.sprite;
            previousSize = size;
            previousTint = tint;
            surface.GetPropertyBlock(properties);
            properties.SetColor(TintId, tint);
            properties.SetVector(RectId, new Vector4(bounds.min.x, bounds.min.y,
                1f / Mathf.Max(bounds.size.x, 0.001f), 1f / Mathf.Max(bounds.size.y, 0.001f)));
            properties.SetVector(SizeId, new Vector4(size.x, size.y, 0, 0));
            properties.SetVector(TilesId, new Vector4(columns, courses, staggerCourses ? 0.5f : 0, 0));
            properties.SetFloat(EmblemId, carvedEmblem ? 1 : 0);
            properties.SetFloat(CrenelsId, crenellations ? 1 : 0);
            properties.SetFloat(SeedId, seed);
            surface.SetPropertyBlock(properties);
        }

        private void OnDisable()
        {
            if (surface != null) surface.SetPropertyBlock(null);
        }
    }
}
