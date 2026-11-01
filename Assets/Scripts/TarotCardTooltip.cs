using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TarotCardTooltip : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Positioning")]
    [SerializeField] private Vector2 mouseOffset = new Vector2(10f, -10f);

    private void Awake() {
        DisableRaycasts();
        Hide();
    }

    // Call once when the hover starts; use SetPosition every frame to follow the cursor.
    public void Show(string text) {
        tooltipText.text = text;
        gameObject.SetActive(true);
        // The tooltip size depends on its text, so rebuild before SetPosition measures it.
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
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

    public void Hide() {
        gameObject.SetActive(false);
    }

    private void DisableRaycasts() {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics) {
            graphic.raycastTarget = false;
        }
    }
}
