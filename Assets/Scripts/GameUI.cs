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
    [SerializeField] private Button playTarotButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button replaceButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    [Header("Magician Suit Buttons")]
    [SerializeField] private Button suitHeartsButton;
    [SerializeField] private Button suitDiamondsButton;
    [SerializeField] private Button suitClubsButton;
    [SerializeField] private Button suitSpadesButton;
    [SerializeField] public TarotCardTooltip tooltip;
    
    public event System.Action<ButtonCommands> Clicked;
    
    private void Awake() {
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
        playTarotButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.PLAY_TAROT));
        exitButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.EXIT));
        AddSuitListener(suitHeartsButton, ButtonCommands.SUIT_HEARTS);
        AddSuitListener(suitDiamondsButton, ButtonCommands.SUIT_DIAMONDS);
        AddSuitListener(suitClubsButton, ButtonCommands.SUIT_CLUBS);
        AddSuitListener(suitSpadesButton, ButtonCommands.SUIT_SPADES);
        EnableSuitButtons(false);
    }

    private void AddSuitListener(Button button, ButtonCommands command) {
        if (button == null) return;
        button.onClick.AddListener(() => OnButtonClicked(command));
    }

    public void EnableSuitButtons(bool enable) {
        SetButtonActive(suitHeartsButton, enable);
        SetButtonActive(suitDiamondsButton, enable);
        SetButtonActive(suitClubsButton, enable);
        SetButtonActive(suitSpadesButton, enable);
    }

    private static void SetButtonActive(Button button, bool active) {
        if (button != null)
            button.gameObject.SetActive(active);
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
    
    public void EnablePlayTarotButton(bool enable) {
        playTarotButton.gameObject.SetActive(enable);
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
        playTarotButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        EnableSuitButtons(false);
    }

    private void OnButtonClicked(ButtonCommands command) {
        Clicked?.Invoke(command);
        // Handle draw button click
    }
    
}
