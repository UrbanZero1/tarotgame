using UnityEngine;
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

    private GameStateData state;

    public void Initialize(GameStateData state, int cardId) {
        this.state = state;

        if (cardRenderer == null)
            cardRenderer = GetComponentInChildren<SpriteRenderer>();
        
        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
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
