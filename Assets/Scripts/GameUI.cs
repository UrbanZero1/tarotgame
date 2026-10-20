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
    
    public void EnablePassButton(bool enable) {
        passButton.gameObject.SetActive(enable);
    }

    public void DisableAllButtons() {
        drawButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);
    }
    
    private void OnButtonClicked(ButtonCommands command) {
        Clicked?.Invoke(command);
        // Handle draw button click
    }
    
}
