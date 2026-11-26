using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TableSelectController : MonoBehaviour
{
    private const string CARD_GAME_SCENE = "Cardgame";

    private static readonly Color clearedColor = new Color(0.6f, 0.85f, 0.6f, 1f);
    private static readonly Color lockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private TMP_Text chipText;

    private void Start()
    {
        if (!RunData.IsActive)
            RunData.StartNewRun();

        SetText("Title", "Choose Your Table");

        for (int i = 0; i < RunData.TABLE_COUNT; i++)
            SetupTable(i);

        GameObject chipObject = GameObject.Find("ChipText");
        if (chipObject != null)
            chipText = chipObject.GetComponent<TMP_Text>();

        if (ChipManager.Instance != null)
            ChipManager.Instance.ChipsUpdate += OnChipsChanged;

        OnChipsChanged(ChipManager.Instance != null ? ChipManager.Instance.CurrentChips : 0);
    }

    private void OnDestroy()
    {
        if (ChipManager.Instance != null)
            ChipManager.Instance.ChipsUpdate -= OnChipsChanged;
    }

    private void OnChipsChanged(int amount)
    {
        if (chipText != null)
            chipText.text = $"Chips: {amount}";
    }

    private static void SetText(string objectName, string text)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null) return;

        TMP_Text label = target.GetComponent<TMP_Text>();
        if (label != null)
            label.text = text;
    }

    private void SetupTable(int index)
    {
        GameObject table = GameObject.Find($"Table {index + 1}");
        GameObject playObject = GameObject.Find($"Play Button {index + 1}");
        if (table == null || playObject == null)
        {
            Debug.LogWarning($"Table {index + 1} UI not found!");
            return;
        }

        TableInfo info = RunData.GetTable(index);
        bool cleared = index < RunData.CurrentTable;
        bool current = index == RunData.CurrentTable;

        Button tableButton = table.GetComponent<Button>();
        if (tableButton != null)
            tableButton.transition = Selectable.Transition.None;

        Color tint = cleared ? clearedColor : current ? Color.white : lockedColor;
        foreach (Image image in table.GetComponentsInChildren<Image>())
            image.color = tint;

        TMP_Text tableLabel = table.GetComponentInChildren<TMP_Text>();
        if (tableLabel != null)
            tableLabel.text = info.name;

        Button playButton = playObject.GetComponent<Button>();
        TMP_Text playLabel = playObject.GetComponentInChildren<TMP_Text>();
        playButton.interactable = current;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(PlayTable);

        if (playLabel != null)
            playLabel.text = cleared ? "Cleared" : current ? "Play Table" : "Locked";

        SetText(
            $"Info {index + 1}",
            $"Goal: {info.goal} points\nReward: {TransitionData.GetChipReward(info.difficulty)} chips");
    }

    private void PlayTable()
    {
        RunData.PrepareTable();
        SceneManager.LoadScene(CARD_GAME_SCENE);
    }
}
