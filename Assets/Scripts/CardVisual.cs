using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardVisual : AbstractCardVisual
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text suitText;
    [SerializeField] private TMP_Text valueText;

    [Header("Faces")]
    [SerializeField] private SpriteRenderer cardRenderer;
    [SerializeField] private CardArtTable cardArtTable;
    [SerializeField] private Vector2 cardRendererSize = Vector2.one;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.35f);

    private GameStateData state;
    private bool flipInProgress;
    private Color normalColor = Color.white;

    public void Initialize(GameStateData state, int cardId) {
        this.state = state;

        if (cardRenderer == null)
            cardRenderer = GetComponentInChildren<SpriteRenderer>();

        if (cardRenderer != null)
            normalColor = cardRenderer.color;
        
        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (flipInProgress) return;
        if (!IsValidCardId()) return;

        CardData card = state.cards[cardId];

        if (cardArtTable == null)
        {
            Debug.LogError("CardVisual is missing its CardArtTable.", this);
            return;
        }

        if (card.facedown) {
            suitText.text = "";
            valueText.text = "Facedown";
            ApplyCardSprite(cardArtTable.GetCardArt("Card_Back"));
        }
        else {
            suitText.text = card.suit.ToString();
            valueText.text = GetRankName(card.value);
            ApplyCardSprite(cardArtTable.GetCardArt(
                $"{card.suit}_{GetRankName(card.value)}"));
        }

        ApplySelectionColor();
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        ApplySelectionColor();
    }

    private void ApplySelectionColor()
    {
        if (cardRenderer != null)
            cardRenderer.color = IsSelected ? selectedColor : normalColor;
    }

    public override void PlayFlip(bool facedown, float duration, Action onComplete)
    {
        if (flipInProgress) return;
        StartCoroutine(FlipRoutine(facedown, duration, onComplete));
    }

    private IEnumerator FlipRoutine(bool facedown, float duration, Action onComplete)
    {
        flipInProgress = true;
        float safeDuration = Mathf.Max(0.01f, duration);
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        bool faceApplied = false;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / safeDuration);

            if (!faceApplied && progress >= 0.5f)
            {
                ApplyFace(facedown);
                faceApplied = true;
            }

            float width = Mathf.Abs(Mathf.Cos(progress * Mathf.PI));
            transform.localScale = new Vector3(
                originalScale.x * Mathf.Max(width, 0.01f),
                originalScale.y,
                originalScale.z);
            yield return null;
        }

        ApplyFace(facedown);
        transform.localScale = originalScale;
        flipInProgress = false;
        onComplete?.Invoke();
    }

    private void ApplyFace(bool facedown)
    {
        if (!IsValidCardId() || cardArtTable == null) return;
        CardData card = state.cards[cardId];

        if (facedown)
        {
            suitText.text = "";
            valueText.text = "Facedown";
            ApplyCardSprite(cardArtTable.GetCardArt("Card_Back"));
        }
        else
        {
            suitText.text = card.suit.ToString();
            valueText.text = GetRankName(card.value);
            ApplyCardSprite(cardArtTable.GetCardArt(
                $"{card.suit}_{GetRankName(card.value)}"));
        }
    }

    private void ApplyCardSprite(Sprite sprite) {
        if (sprite == null) {
            Debug.LogWarning($"Missing Card Sprite for card ID {cardId}.", this);
            return;
        }
        
        cardRenderer.drawMode = SpriteDrawMode.Sliced;
        cardRenderer.size = cardRendererSize;
        cardRenderer.sprite = sprite;
    }

    private static string GetRankName(int value)
    {
        switch (value)
        {
            case 1:
                return "A";
            case 11:
                return "Jack";
            case 12:
                return "Queen";
            case 13:
                return "King";
            default:
                return value.ToString();
        }
    }
}
