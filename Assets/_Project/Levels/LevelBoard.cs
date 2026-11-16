using JuiceStudy.Abilities;
using JuiceStudy.Arcade;
using UnityEngine;

namespace JuiceStudy.Levels
{
    public sealed class LevelBoard : MonoBehaviour
    {
        [SerializeField] private LevelBrick brickPrefab;
        [SerializeField] private Vector2 brickSize = new Vector2(1.8f, 0.7f);
        [SerializeField] private Vector2 spacing = new Vector2(0.15f, 0.15f);
        [SerializeField] private float topY = 4f;
        [SerializeField] private Color[] rowColors;

        public void Clear()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        public int Rebuild(LevelSession session, LevelDefinition layout)
        {
            Clear();
            float stepX = brickSize.x + spacing.x;
            float stepY = brickSize.y + spacing.y;
            float startX = -(layout.Columns * stepX - spacing.x) * 0.5f + brickSize.x * 0.5f;
            int count = 0;
            for (int row = 0; row < layout.Rows; row++)
            for (int col = 0; col < layout.Columns; col++)
            {
                if (!layout.HasBrick(row, col)) continue;
                var position = new Vector3(startX + col * stepX, topY - row * stepY, 0);
                LevelBrick brick = Instantiate(brickPrefab, position, Quaternion.identity, transform);
                brick.name = $"Brick_r{row}_c{col}";
                brick.transform.localScale = new Vector3(brickSize.x, brickSize.y, 1);
                brick.Initialize(session);
                if (rowColors != null && rowColors.Length > 0 && brick.TryGetComponent(out SpriteRenderer renderer))
                {
                    Color color = rowColors[row % rowColors.Length];
                    if (color.a <= 0.01f) color.a = 1;
                    renderer.color = color;
                }
                count++;
            }
            return count;
        }
    }
}
