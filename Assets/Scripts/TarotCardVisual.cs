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

    private TarotCardData[] cards;

    public void Initialize(TarotCardData[] cards, int cardId)
    {
        this.cards = cards;

        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (!IsValidCardId())
            return;


        TarotCardData card = cards[cardId];

        nameText.text = card.type.GetName();
    }
}
