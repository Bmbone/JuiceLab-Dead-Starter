using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceStudy.Abilities
{
    public sealed class AbilityHud
    {
        private readonly Button[] buttons = new Button[3];
        private readonly TMP_Text[] status = new TMP_Text[3];
        private readonly Image[] fills = new Image[3];
        private static readonly Color Gold = new Color(0.906f, 0.773f, 0.494f);
        private static readonly Color Active = new Color(0.4f, 1, 0.8f);

        public AbilityHud(Transform parent, TMP_FontAsset font, Action<Spell> cast)
        {
            var root = Rect("Ability hotbar", parent, 0, 8, 1008, 64);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0);
            root.pivot = new Vector2(0.5f, 0);
            string[] labels = { "[1 / SPACE] FIREBALL", "[2] GIANT'S GRIP", "[3] RUNE SHIELD" };
            for (int i = 0; i < buttons.Length; i++)
            {
                Spell spell = (Spell)i;
                var card = Rect(labels[i], root, (i - 1) * 344, 0, 320, 64);
                var image = card.gameObject.AddComponent<Image>();
                buttons[i] = card.gameObject.AddComponent<Button>();
                // Mouse-clickable, but Space must always mean fireball rather than UI Submit.
                buttons[i].navigation = new Navigation { mode = Navigation.Mode.None };
                buttons[i].targetGraphic = image;
                var colors = buttons[i].colors;
                colors.normalColor = new Color(0.12f, 0.13f, 0.17f, 0.96f);
                colors.highlightedColor = colors.selectedColor = new Color(0.3f, 0.25f, 0.15f);
                colors.pressedColor = new Color(0.45f, 0.33f, 0.12f);
                colors.disabledColor = new Color(0.07f, 0.08f, 0.11f, 0.94f);
                colors.fadeDuration = 0.08f;
                buttons[i].colors = colors;
                buttons[i].onClick.AddListener(() => cast(spell));
                Text("Spell", card, 14, 24, 20, font, Gold).text = labels[i];
                status[i] = Text("Status", card, -8, 22, 18, font, Gold);
                var track = Rect("Cooldown track", card, 0, -27, 280, 4);
                var trackImage = track.gameObject.AddComponent<Image>();
                trackImage.color = new Color(0.2f, 0.21f, 0.25f);
                trackImage.raycastTarget = false;
                var fill = Rect("Cooldown fill", track, 0, 0, 280, 0);
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = Vector2.up;
                fill.pivot = new Vector2(0, 0.5f);
                fills[i] = fill.gameObject.AddComponent<Image>();
                fills[i].raycastTarget = false;
            }
        }

        public void Refresh(AbilityState state)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var spell = (Spell)i;
                float cooldown = state.CooldownRemaining(spell);
                float active = state.ActiveRemaining(spell);
                bool ready = state.IsReady(spell);
                buttons[i].interactable = ready;
                status[i].text = active > 0 ? $"ACTIVE {Mathf.CeilToInt(active)}s" :
                    ready ? "READY" : $"{Mathf.CeilToInt(cooldown)}s COOLDOWN";
                status[i].color = active > 0 ? Active : Gold;
                fills[i].color = active > 0 ? Active : Gold;
                fills[i].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    280 * Mathf.Clamp01(1 - cooldown / AbilityState.CooldownLength(spell)));
            }
        }

        private static TMP_Text Text(string name, Transform parent, float y, float height, float size, TMP_FontAsset font, Color color)
        {
            var rect = Rect(name, parent, 0, y, 300, height);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
