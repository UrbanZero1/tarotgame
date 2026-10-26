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

    public event Action<AbstractCardVisual> PointerEntered;
    public event Action<AbstractCardVisual> PointerExited;
    public event Action<AbstractCardVisual> PointerPressed;
    public event Action<AbstractCardVisual> PointerReleased;

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
        PointerEntered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerPressed?.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerReleased?.Invoke(this);
    } 
}
