using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    [Header("UI Components")] 
    [SerializeField] private TMP_Text middleText;
    [FormerlySerializedAs("turnText")] [SerializeField] private TMP_Text pointsGoalText;
    [SerializeField] private TMP_Text playerPointsText;
    [SerializeField] private TMP_Text handsRemainingText;
    [SerializeField] private TMP_Text actionsRemainingText;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button replaceButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    [Header("Card Tooltip")]
    [SerializeField] private float tooltipVerticalSpacing = 0.15f;
    [SerializeField] private Vector2 tooltipPadding = new Vector2(0.2f, 0.1f);
    [SerializeField] private Color tooltipBackgroundColor = new Color(0f, 0f, 0f, 0.85f);

    private RectTransform tooltipRoot;
    private TMP_Text tooltipText;
    
    public event System.Action<ButtonCommands> Clicked;
    
    private void Awake() {
        EnsureTooltip();
        HideCardTooltip();

        UpdateMiddleText("");
        UpdatePointsGoalText("");
        UpdatePlayerPointsText("");
        UpdateActionsRemainingText("");
        UpdateHandsRemainingText("");
        drawButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.DRAW));
        discardButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.DISCARD));
        replaceButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.REPLACE));
        passButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.PASS));
        restartButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.RESTART));
        exitButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.EXIT));
    }

    public void ShowCardTooltip(string text, RectTransform cardAnchor) {
        if (string.IsNullOrEmpty(text) || !cardAnchor) {
            HideCardTooltip();
            return;
        }

        EnsureTooltip();

        if (!tooltipRoot || !tooltipText) return;

        tooltipRoot.SetParent(cardAnchor, false);
        tooltipText.text = text;

        Vector2 preferred = tooltipText.GetPreferredValues(text);
        tooltipRoot.sizeDelta = new Vector2(
            preferred.x + tooltipPadding.x * 2f,
            preferred.y + tooltipPadding.y * 2f);

        tooltipRoot.anchoredPosition = new Vector2(0f, cardAnchor.rect.height * 0.5f + tooltipVerticalSpacing);
        tooltipRoot.gameObject.SetActive(true);
    }

    public void HideCardTooltip() {
        if (tooltipRoot) {
            tooltipRoot.gameObject.SetActive(false);
        }
    }

    public void UpdateMiddleText(string text) {
        middleText.text = text;
    }

    public void UpdatePointsGoalText(string text) {
        pointsGoalText.text = text;
    }
    
    public void UpdatePlayerPointsText(string text) {
        playerPointsText.text = text;
    }
    
    public void UpdateHandsRemainingText(string text) {
        handsRemainingText.text = text;
    }
    
    public void UpdateActionsRemainingText(string text) {
        actionsRemainingText.text = text;
    }
    
    public void EnableDrawButton(bool enable) {
        drawButton.gameObject.SetActive(enable);
    }
    
    public void EnableDiscardButton(bool enable) {
        discardButton.gameObject.SetActive(enable);
    }
    
    public void EnableReplaceButton(bool enable) {
        replaceButton.gameObject.SetActive(enable);
    }
    
    public void EnableRestartButton(bool enable) {
        restartButton.gameObject.SetActive(enable);
    }
    
    public void EnableExitButton(bool enable) {
        exitButton.gameObject.SetActive(enable);
    }
    
    public void EnablePassButton(bool enable) {
        passButton.gameObject.SetActive(enable);
    }

    public void DisableAllButtons() {
        drawButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }
    
    private void OnButtonClicked(ButtonCommands command) {
        Clicked?.Invoke(command);
        // Handle draw button click
    }

    private void EnsureTooltip() {
        if (tooltipRoot && tooltipText) return;

        GameObject tooltipObject = new GameObject("CardTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tooltipRoot = tooltipObject.GetComponent<RectTransform>();
        tooltipRoot.SetParent(transform, false);
        tooltipRoot.anchorMin = new Vector2(0.5f, 0f);
        tooltipRoot.anchorMax = new Vector2(0.5f, 0f);
        tooltipRoot.pivot = new Vector2(0.5f, 0f);

        Image background = tooltipObject.GetComponent<Image>();
        background.color = tooltipBackgroundColor;
        background.raycastTarget = false;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(tooltipRoot, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = tooltipPadding;
        textRect.offsetMax = -tooltipPadding;

        tooltipText = textObject.GetComponent<TextMeshProUGUI>();
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.enableWordWrapping = false;
        tooltipText.raycastTarget = false;
        tooltipText.text = "";
    }
    
}
