using UnityEngine;
using System;
using System.Collections;
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
    public bool IsSelected { get; private set; }

    public event Action<AbstractCardVisual, PointerEventData> PointerEntered;
    public event Action<AbstractCardVisual, PointerEventData> PointerExited;
    public event Action<AbstractCardVisual, PointerEventData> PointerPressed;
    public event Action<AbstractCardVisual, PointerEventData> PointerReleased;

    [Header("Selection (float + shadow)")]
    [Tooltip("How far a selected card rises. Local -Z points up off the table.")]
    [SerializeField] private float liftHeight = 0.15f;
    [SerializeField] private float liftDuration = 0.12f;
    [SerializeField] private float shadowMaxAlpha = 0.5f;
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.02f, -0.05f);
    [Tooltip("How much bigger the shadow gets as the card rises (1 = same size).")]
    [SerializeField] private float shadowSpread = 1.12f;
    [Tooltip("The shadow is sized from this.")]
    [SerializeField] private Vector2 cardSize = new Vector2(1f, 1.4f);

    [Header("Hover glow")]
    [SerializeField] private Color glowColor = new Color(0.35f, 1f, 0.3f, 1f);
    [SerializeField] private float glowMaxAlpha = 1f;
    [SerializeField] private float glowFadeDuration = 0.12f;
    [Tooltip("How much the glow dims at the bottom of its pulse (0 = no pulse).")]
    [SerializeField] private float glowPulse = 0.15f;
    [SerializeField] private float glowPulseSpeed = 3f;

    private Transform liftRoot;
    private Vector3 liftRestPosition;
    private SpriteRenderer shadow;
    private float lift;
    private Coroutine liftRoutine;
    private SpriteRenderer glow;
    private float glowAmount;
    private bool glowTarget;
    private Coroutine glowRoutine;

    private const float SHADOW_SHAPE_EXTENT = 0.75f;
    private const float GLOW_MARGIN = 0.3f;
    private const float GLOW_CORNER_RADIUS = 0.07f;
    private const float GLOW_SIGMA = 0.09f;
    private const float GLOW_PIXELS_PER_UNIT = 128f;
    private static Sprite shadowSprite;
    private static Sprite glowSprite;

    protected virtual void Awake()
    {
        SpriteRenderer art = GetComponentInChildren<SpriteRenderer>();
        if (art == null) return;

        liftRoot = art.transform;
        liftRestPosition = liftRoot.localPosition;

        GameObject shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(transform, false);
        shadow = shadowObject.AddComponent<SpriteRenderer>();
        shadow.sprite = GetShadowSprite();
        shadow.sharedMaterial = art.sharedMaterial; 
        shadow.sortingLayerID = art.sortingLayerID;
        shadow.sortingOrder = art.sortingOrder - 2;
        shadow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shadow.receiveShadows = false;
        ApplyLift();

        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(liftRoot, false);
        Vector3 artScale = liftRoot.localScale;
        glowObject.transform.localScale = new Vector3(1f / artScale.x, 1f / artScale.y, 1f / artScale.z);
        glow = glowObject.AddComponent<SpriteRenderer>();
        glow.sprite = GetGlowSprite(cardSize);
        glow.sharedMaterial = art.sharedMaterial;
        glow.sortingLayerID = art.sortingLayerID;
        glow.sortingOrder = art.sortingOrder - 1;
        glow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        glow.receiveShadows = false;
        ApplyGlow(1f);
    }

    private void OnDisable()
    {
        liftRoutine = null;
        lift = IsSelected ? 1f : 0f;
        ApplyLift();

        glowRoutine = null;
        glowTarget = false;
        glowAmount = 0f;
        ApplyGlow(1f);
    }

    public virtual void Initialize(int cardId)
    {
        this.cardId = cardId;
        UpdateVisuals();
    }

    public abstract void UpdateVisuals();

    // A selected card floats a little above the table and casts a shadow.
    public virtual void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (liftRoot == null) return;

        // Coroutines can't run on an inactive object so jump straight to the end state.
        if (!isActiveAndEnabled)
        {
            lift = selected ? 1f : 0f;
            ApplyLift();
            return;
        }

        if (liftRoutine != null)
            StopCoroutine(liftRoutine);
        liftRoutine = StartCoroutine(LiftRoutine(selected ? 1f : 0f));
    }

    private IEnumerator LiftRoutine(float target)
    {
        float speed = 1f / Mathf.Max(0.01f, liftDuration);

        while (!Mathf.Approximately(lift, target))
        {
            lift = Mathf.MoveTowards(lift, target, speed * Time.deltaTime);
            ApplyLift();
            yield return null;
        }

        lift = target;
        ApplyLift();
        liftRoutine = null;
    }

    private void ApplyLift()
    {
        if (liftRoot == null || shadow == null) return;

        float eased = 1f - Mathf.Pow(1f - lift, 3f);

        liftRoot.localPosition = liftRestPosition + new Vector3(0f, 0f, -liftHeight * eased);

        // The shadow stays on the table at the card's resting spot and grows/fades in with the lift.
        shadow.enabled = eased > 0.001f;
        Color color = Color.black;
        color.a = shadowMaxAlpha * eased;
        shadow.color = color;

        float spread = Mathf.Lerp(1f, shadowSpread, eased);
        shadow.transform.localPosition = new Vector3(
            shadowOffset.x * eased,
            shadowOffset.y * eased,
            liftRestPosition.z);
        shadow.transform.localScale = new Vector3(
            cardSize.x * spread / SHADOW_SHAPE_EXTENT,
            cardSize.y * spread / SHADOW_SHAPE_EXTENT,
            1f);
    }

    public void SetHovered(bool hovered)
    {
        if (glow == null || hovered == glowTarget) return;

        glowTarget = hovered;

        if (!isActiveAndEnabled)
        {
            glowAmount = hovered ? 1f : 0f;
            ApplyGlow(1f);
            return;
        }

        if (glowRoutine != null)
            StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        float speed = 1f / Mathf.Max(0.01f, glowFadeDuration);

        while (glowTarget || glowAmount > 0f)
        {
            glowAmount = Mathf.MoveTowards(glowAmount, glowTarget ? 1f : 0f, speed * Time.deltaTime);
            float pulse = glowTarget
                ? 1f - glowPulse * 0.5f * (1f + Mathf.Sin(Time.time * glowPulseSpeed))
                : 1f;
            ApplyGlow(pulse);
            yield return null;
        }

        glowRoutine = null;
    }

    private void ApplyGlow(float pulse)
    {
        if (glow == null) return;

        glow.enabled = glowAmount > 0.001f;
        Color color = glowColor;
        color.a = glowColor.a * glowMaxAlpha * glowAmount * pulse;
        glow.color = color;
    }

    // A soft rounded rectangle, generated once so no art asset is needed.
    private static Sprite GetShadowSprite()
    {
        if (shadowSprite != null) return shadowSprite;

        const int size = 64;
        const float radius = 0.25f;
        const float softness = 0.2f;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "CardShadow",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;

                // Signed distance to a rounded rectangle (negative inside).
                float qx = Mathf.Abs(nx) - (SHADOW_SHAPE_EXTENT - radius);
                float qy = Mathf.Abs(ny) - (SHADOW_SHAPE_EXTENT - radius);
                float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                float distance = outside + inside - radius;

                float alpha = 1f - Mathf.SmoothStep(-softness, softness, distance);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        // One pixel per 1/size unit, so the sprite is exactly 1 unit wide.
        shadowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return shadowSprite;
    }

    private static Sprite GetGlowSprite(Vector2 size)
    {
        if (glowSprite != null) return glowSprite;

        float totalWidth = size.x + 2f * GLOW_MARGIN;
        float totalHeight = size.y + 2f * GLOW_MARGIN;
        int width = Mathf.CeilToInt(totalWidth * GLOW_PIXELS_PER_UNIT);
        int height = Mathf.CeilToInt(totalHeight * GLOW_PIXELS_PER_UNIT);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "CardGlow",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 halfSize = size * 0.5f;
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float px = (x + 0.5f) / GLOW_PIXELS_PER_UNIT - totalWidth * 0.5f;
                float py = (y + 0.5f) / GLOW_PIXELS_PER_UNIT - totalHeight * 0.5f;

                float qx = Mathf.Abs(px) - (halfSize.x - GLOW_CORNER_RADIUS);
                float qy = Mathf.Abs(py) - (halfSize.y - GLOW_CORNER_RADIUS);
                float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                float distance = outside + inside - GLOW_CORNER_RADIUS;

                float alpha = 1f;
                if (distance > 0f)
                {
                    float t = distance / GLOW_SIGMA;
                    float fadeOut = 1f - Mathf.SmoothStep(GLOW_MARGIN * 0.6f, GLOW_MARGIN, distance);
                    alpha = Mathf.Exp(-t * t) * fadeOut;
                }

                pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        glowSprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            GLOW_PIXELS_PER_UNIT);
        return glowSprite;
    }

    public virtual void PlayFlip(bool facedown, float duration, Action onComplete)
    {
        UpdateVisuals();
        onComplete?.Invoke();
    }

    protected bool IsValidCardId()
    {
        return cardId >= 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered?.Invoke(this, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke(this, eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerPressed?.Invoke(this, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerReleased?.Invoke(this, eventData);
    } 
}
