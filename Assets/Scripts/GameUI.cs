using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    [Header("UI Components")] 
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button replaceButton;
    [SerializeField] private Button swapButton;
    [SerializeField] private Button wordButton;
    
    public event System.Action<ButtonCommands> Clicked;
    
    private void Awake() {
        UpdateMiddleText("");
        UpdateTurnText("");
        drawButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.DRAW));
        discardButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.DISCARD));
        replaceButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.REPLACE));
        swapButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.SWAP));
        wordButton.onClick.AddListener(() => OnButtonClicked(ButtonCommands.WORD));
    }

    public void UpdateMiddleText(string text) {
        middleText.text = text;
    }

    public void UpdateTurnText(string text) {
        turnText.text = text;
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
    
    public void EnableSwapButton(bool enable) {
        swapButton.gameObject.SetActive(enable);
    }
    
    public void EnableWordButton(bool enable) {
        wordButton.gameObject.SetActive(enable);
    }

    public void DisableAllButtons() {
        drawButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);
        replaceButton.gameObject.SetActive(false);
        swapButton.gameObject.SetActive(false);
        wordButton.gameObject.SetActive(false);
    }
    
    private void OnButtonClicked(ButtonCommands command) {
        Clicked?.Invoke(command);
        // Handle draw button click
    }
    
}
