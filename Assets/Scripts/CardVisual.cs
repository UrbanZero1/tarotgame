using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text suitText;
    [SerializeField] private TMP_Text valueText;

    [Header("Faces")]
    [SerializeField] private Sprite frontFace;
    [SerializeField] private Sprite backFace;

    /// <summary> Only set true for the human player </summary>
    public bool IsInteractable { get; set; }

    public event Action<int> PointerEntered;
    public event Action<int> PointerExited;
    public event Action<int> PointerPressed;
    public event Action<int> PointerReleased;

    private CardData[] cards;
    private int cardId = -1;

    public int CardId => cardId;

    public void Initialize(CardData[] cards, int id) {
        this.cards = cards;
        cardId = id;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        if (!IsValid()) return;

        CardData card = cards[cardId];

        IsInteractable = card.zone == CardZone.PLAYER;
        
        if (card.facedown) {
            suitText.text = "";
            valueText.text = "Facedown";
        }
        else {
            suitText.text = card.suit.ToString();
            valueText.text = card.value.ToString();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        PointerEntered?.Invoke(cardId);
    }
    
    public void OnPointerExit(PointerEventData eventData) {
        PointerExited?.Invoke(cardId);
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        if (!IsInteractable) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerPressed?.Invoke(cardId);
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (!IsInteractable) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerReleased?.Invoke(cardId);
    }

    private bool IsValid() {
        return cards != null && cardId >= 0 && cardId < cards.Length;
    }
}
