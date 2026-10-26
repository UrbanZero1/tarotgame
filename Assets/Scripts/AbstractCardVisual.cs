using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class AbstractCardVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public int cardId { get; private set; } = -1;

    public event Action<int,Type> PointerEntered;
    public event Action<int, Type> PointerExited;
    public event Action<int, Type> PointerPressed;
    public event Action<int, Type> PointerReleased;

    public virtual void Initialize(int cardId)
    {
        this.cardId = cardId;
        UpdateVisuals();
    }

    public abstract void UpdateVisuals();

    protected bool IsValidCardId()
    {
        return cardId >= 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered?.Invoke(cardId, GetType());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke(cardId, GetType());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerPressed?.Invoke(cardId, GetType());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerReleased?.Invoke(cardId, GetType());
    } 
}
