using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TarotCardVisual : AbstractCardVisual
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text nameText;

    [Header("Faces")]
    [SerializeField] private GameObject frontFace;
    [SerializeField] private GameObject backFace;

    [Header("Art")]
    [SerializeField] private SpriteRenderer cardRenderer;
    [SerializeField] private CardArtTable cardArtTable;
    [SerializeField] private Vector2 cardRendererSize = Vector2.one;

    private GameStateData state;

    public void Initialize(GameStateData state, int cardId)
    {
        this.state = state;

        if (cardRenderer == null)
            cardRenderer = GetComponentInChildren<SpriteRenderer>();

        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (!IsValidCardId())
            return;


        CardData card = state.cards[cardId];
        string cardName = card.tarotType.GetName();

        Sprite art = cardArtTable != null ? cardArtTable.GetCardArt(cardName) : null;
        if (art != null && cardRenderer != null) {
            ApplyCardSprite(art);
            // The art already has the card's name printed on it.
            nameText.gameObject.SetActive(false);
        }
        else {
            nameText.gameObject.SetActive(true);
            nameText.text = cardName;
        }
    }

    private void ApplyCardSprite(Sprite sprite) {
        // Same as CardVisual: stretch the art to fill the renderer's local size.
        cardRenderer.drawMode = SpriteDrawMode.Sliced;
        cardRenderer.size = cardRendererSize;
        cardRenderer.sprite = sprite;
    }
}
