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

    private GameStateData state;

    public void Initialize(GameStateData state, int cardId)
    {
        this.state = state;

        base.Initialize(cardId);
    }

    public override void UpdateVisuals() {
        if (!IsValidCardId())
            return;


        CardData card = state.cards[cardId];

        nameText.text = card.tarotType.GetName();
    }
}
