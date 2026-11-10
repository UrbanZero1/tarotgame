using UnityEngine;
using UnityEngine.EventSystems;

// Put this on any UI element that has a raycast-target Graphic (an Image, for example) to show
// a tooltip while the pointer is over it. The tooltip follows the cursor by itself.
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UITooltip tooltip;
    [SerializeField, TextArea] private string text;

    private bool hovered;

    // Change the text from code (e.g. a live value); updates the tooltip if it is showing now.
    public void SetText(string newText) {
        text = newText;
        if (hovered && tooltip != null)
            tooltip.Show(text);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (tooltip == null || string.IsNullOrEmpty(text)) return;

        hovered = true;
        tooltip.Show(text);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    // The element can be hidden while hovered (e.g. a button disappearing), which sends no exit event.
    private void OnDisable() {
        Hide();
    }

    private void Hide() {
        if (!hovered) return;

        hovered = false;
        if (tooltip != null)
            tooltip.Hide();
    }
}
