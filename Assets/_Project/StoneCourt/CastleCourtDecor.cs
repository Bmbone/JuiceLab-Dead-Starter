using UnityEngine;

namespace JuiceStudy.StoneCourt
{
    /// <summary>One decorative hierarchy outside the playable wall faces. Contains no physics components.</summary>
    [DisallowMultipleComponent]
    public sealed class CastleCourtDecor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer leftWall;
        [SerializeField] private SpriteRenderer rightWall;
        [SerializeField] private Material stoneMaterial;
        [SerializeField] private Material flatMaterial;
        private Transform decorations;
        private Sprite square;
        private int nextOrder;
        private static readonly Color Stone = new Color(0.46f, 0.49f, 0.48f);
        private static readonly Color Dark = new Color(0.07f, 0.09f, 0.10f);
        private static readonly Color Brass = new Color(0.64f, 0.48f, 0.25f);

        private void Awake()
        {
            if (leftWall == null || rightWall == null || stoneMaterial == null || flatMaterial == null)
            {
                Debug.LogError("Assign wall renderers and the two materials to CastleCourtDecor.", this);
                enabled = false;
                return;
            }
            square = leftWall.sprite;
            if (square == null) { enabled = false; return; }
            decorations = new GameObject("Castle pillar decorations - visual only").transform;
            decorations.SetParent(transform, false);
            BuildPillar(leftWall, -1);
            BuildPillar(rightWall, 1);
        }

        private void BuildPillar(SpriteRenderer wall, int side)
        {
            Bounds b = wall.bounds;
            float inner = side < 0 ? b.max.x : b.min.x;
            float width = b.size.x;
            float center = b.center.x;
            // Every piece ends at or outside the existing inner collider face.
            float capWidth = width * 1.14f;
            float capCenter = inner + side * capWidth * 0.5f;
            StonePiece("Pillar foot", capCenter, b.min.y + 0.34f, capWidth, 0.58f, Stone * 0.86f);
            float[] bands = { -5.55f, -1.85f, 2.45f, 4.75f, 5.16f };
            foreach (float y in bands)
            {
                StonePiece("Pillar moulding", capCenter, b.center.y + y, capWidth, 0.20f, Stone);
                Flat("Moulding lower shadow", capCenter, b.center.y + y - 0.11f, capWidth, 0.035f, Dark);
                Flat("Moulding highlight", capCenter, b.center.y + y + 0.085f, capWidth, 0.025f,
                    new Color(0.65f, 0.65f, 0.58f));
            }
            Flat("Inner edge shadow", inner + side * 0.055f, b.center.y, 0.065f, b.size.y, Dark);
            Flat("Inner edge highlight", inner + side * 0.105f, b.center.y, 0.025f, b.size.y,
                new Color(0.52f, 0.57f, 0.55f));
            float slitY = b.center.y - 3.3f;
            Flat("Arrow slit recess", center, slitY, 0.17f, 0.95f, Dark);
            Flat("Arrow slit cross", center, slitY + 0.1f, 0.48f, 0.10f, Dark);
            StonePiece("Arrow slit lintel", center, slitY + 0.54f, 0.60f, 0.15f, Stone);
            BuildBanner(center, b.center.y + 0.3f, side < 0 ? new Color(0.23f, 0.42f, 0.35f) : new Color(0.46f, 0.22f, 0.20f));
        }

        private void BuildBanner(float x, float y, Color cloth)
        {
            Flat("Banner shadow", x + 0.045f, y - 0.045f, 0.68f, 1.70f, Dark);
            Flat("Banner cloth", x, y, 0.64f, 1.66f, cloth);
            Flat("Banner edge left", x - 0.29f, y, 0.025f, 1.66f, Brass);
            Flat("Banner edge right", x + 0.29f, y, 0.025f, 1.66f, Brass);
            Flat("Banner hem", x, y - 0.78f, 0.60f, 0.035f, Brass);
            Flat("Banner rod", x, y + 0.88f, 0.86f, 0.065f, Brass);
            var crest = Flat("Banner diamond crest", x, y + 0.09f, 0.23f, 0.23f, Brass);
            crest.transform.localRotation = Quaternion.Euler(0, 0, 45);
            var inset = Flat("Banner crest inset", x, y + 0.09f, 0.12f, 0.12f, cloth * 0.65f);
            inset.transform.localRotation = Quaternion.Euler(0, 0, 45);
        }

        private void StonePiece(string name, float x, float y, float width, float height, Color tint)
        {
            tint.a = 1;
            SpriteRenderer renderer = Piece(name, x, y, width, height, tint, stoneMaterial);
            renderer.gameObject.AddComponent<MedievalStoneSurface>().Configure(1, 1, tint);
        }

        private SpriteRenderer Flat(string name, float x, float y, float width, float height, Color tint)
        {
            tint.a = 1;
            return Piece(name, x, y, width, height, tint, flatMaterial);
        }

        private SpriteRenderer Piece(string name, float x, float y, float width, float height, Color tint, Material material)
        {
            var piece = new GameObject(name);
            piece.transform.SetParent(decorations, false);
            Bounds spriteBounds = square.bounds;
            Vector3 parentScale = decorations.lossyScale;
            piece.transform.localScale = new Vector3(width / spriteBounds.size.x / parentScale.x,
                height / spriteBounds.size.y / parentScale.y, 1);
            piece.transform.position = new Vector3(x - spriteBounds.center.x * width / spriteBounds.size.x,
                y - spriteBounds.center.y * height / spriteBounds.size.y, 0);
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.sharedMaterial = material;
            renderer.color = tint;
            renderer.sortingLayerID = leftWall.sortingLayerID;
            renderer.sortingOrder = leftWall.sortingOrder + 1 + nextOrder++;
            return renderer;
        }

        private void OnDestroy()
        {
            if (decorations != null) Destroy(decorations.gameObject);
        }
    }
}
