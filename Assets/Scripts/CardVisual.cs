using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardVisual : AbstractCardVisual
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text suitText;
    [SerializeField] private TMP_Text valueText;

    [Header("Faces")]
    [SerializeField] private Sprite frontFace;
    [SerializeField] private Sprite backFace;

    /// <summary> Only set true for the human player </summary>
    public bool IsInteractable { get; set; }

    private CardData[] cards;

    public void Initialize(CardData[] cards, int cardId) {
        this.cards = cards;

        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (!IsValidCardId()) return;

        CardData card = cards[cardId];
        
        if (card.facedown) {
            suitText.text = "";
            valueText.text = "Facedown";
        }
        else {
            suitText.text = card.suit.ToString();
            valueText.text = card.value.ToString();
        }
    }
}
