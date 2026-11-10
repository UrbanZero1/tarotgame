using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// One generic tooltip for the whole UI. Anything (a tarot card, a HUD element, ...) calls
// Show(text) when a hover starts and Hide() when it ends. While it is visible the tooltip
// follows the cursor on its own, so callers never need to update its position.
public class UITooltip : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Positioning")]
    [SerializeField] private Vector2 mouseOffset = new Vector2(10f, -10f);

    // Show() can be the call that first activates (and so Awakes) this object.
    private bool isShowing;

    private void Awake() {
        DisableRaycasts();
        if (!isShowing)
            Hide();
    }

    public void Show(string text) {
        isShowing = true;
        tooltipText.text = text;
        // UI draws in hierarchy order, so move to the end of the siblings to draw above every other element.
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        // The tooltip size depends on its text, so rebuild before SetPosition measures it.
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        FollowCursor();
    }

    public void Hide() {
        isShowing = false;
        gameObject.SetActive(false);
    }

    // Only runs while the tooltip is active.
    private void Update() {
        FollowCursor();
    }

    private void FollowCursor() {
        Mouse mouse = Mouse.current;
        if (mouse != null)
            SetPosition(mouse.position.ReadValue());
    }

    public void SetPosition(Vector2 screenPosition) {
        // Works for a Screen Space - Overlay canvas, where lossyScale is the canvas scale factor.
        Vector2 size = Vector2.Scale(tooltipRect.rect.size, tooltipRect.lossyScale);
        Vector2 pivot = tooltipRect.pivot;

        // Bottom-left corner when the pivot sits at cursor + offset.
        Vector2 corner = screenPosition + mouseOffset - Vector2.Scale(size, pivot);

        // Flip to the other side of the cursor if the tooltip would leave the screen.
        if (corner.x + size.x > Screen.width)
            corner.x = screenPosition.x - mouseOffset.x - size.x;
        if (corner.y < 0f)
            corner.y = screenPosition.y - mouseOffset.y;

        // Final clamp for tooltips larger than the space on either side.
        corner.x = Mathf.Clamp(corner.x, 0f, Mathf.Max(0f, Screen.width - size.x));
        corner.y = Mathf.Clamp(corner.y, 0f, Mathf.Max(0f, Screen.height - size.y));

        tooltipRect.position = corner + Vector2.Scale(size, pivot);
    }

    private void DisableRaycasts() {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
}
