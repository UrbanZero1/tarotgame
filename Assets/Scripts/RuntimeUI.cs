using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class RuntimeUI
{
    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public static RectTransform CreateRect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject rectObject = new GameObject(name, typeof(RectTransform));

        RectTransform rect = rectObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return rect;
    }

    public static Image CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = CreateRect(parent, name, position, size);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        return image;
    }

    public static TMP_Text CreateText(
        Transform parent,
        string name,
        string text,
        float fontSize,
        Vector2 position,
        Vector2 size,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        RectTransform rect = CreateRect(parent, name, position, size);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;

        return label;
    }

    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 size,
        Color color,
        float fontSize = 36f)
    {
        Image image = CreatePanel(parent, name, position, size, color);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        TMP_Text text = CreateText(image.transform, "Label", label, fontSize, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform);

        return button;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static TMP_Text GetButtonLabel(Button button)
    {
        return button.GetComponentInChildren<TMP_Text>();
    }

    public static Slider CreateSlider(Transform parent, string name, Vector2 position, float width)
    {
        RectTransform root = CreateRect(parent, name, position, new Vector2(width, 50f));
        Slider slider = root.gameObject.AddComponent<Slider>();

        Image background = CreatePanel(root, "Background", Vector2.zero, new Vector2(width, 20f), new Color(0.2f, 0.2f, 0.2f, 1f));

        RectTransform fillArea = CreateRect(root, "Fill Area", Vector2.zero, new Vector2(width - 30f, 20f));
        Image fill = CreatePanel(fillArea, "Fill", Vector2.zero, Vector2.zero, new Color(0.3f, 0.8f, 0.4f, 1f));
        Stretch(fill.rectTransform);

        RectTransform handleArea = CreateRect(root, "Handle Slide Area", Vector2.zero, new Vector2(width - 30f, 50f));
        Image handle = CreatePanel(handleArea, "Handle", Vector2.zero, new Vector2(30f, 0f), Color.white);
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);

        slider.targetGraphic = handle;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        return slider;
    }
}
