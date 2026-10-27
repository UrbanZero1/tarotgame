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

    private GameStateData state;

    public void Initialize(GameStateData state, int cardId) {
        this.state = state;

        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (!IsValidCardId()) return;

        CardData card = state.cards[cardId];
        
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
