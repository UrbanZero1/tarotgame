using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TarotCardTooltip : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Positioning")]
    [SerializeField] private Vector2 mouseOffset = new Vector2(10f, -10f);

    private Canvas parentCanvas;
    private InputSystemUIInputModule uiModule;

    private void Awake() {
        parentCanvas = GetComponentInParent<Canvas>();
        DisableRaycasts();
        uiModule = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
    
        Hide();
    }
    
    public void Show(string text, Vector2 screenPosition) {
        tooltipText.text = text;
        tooltipRect.gameObject.SetActive(true);
        SetPosition(screenPosition);
    }

    public void SetPosition(Vector2 screenPosition) {
        tooltipRect.position = screenPosition + mouseOffset;
    }
    
    public void Hide() {
        tooltipRect.gameObject.SetActive(false);
    }
    

    private void DisableRaycasts() {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        
        foreach (Graphic graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
}
