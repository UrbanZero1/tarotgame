using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string TABLE_SELECT_SCENE = "TableSelect";
    private const string VOLUME_KEY = "MasterVolume";

    private const string HOW_TO_PLAY_TEXT =
        "Reach the table's point goal before you run out of hands.\n\n" +
        "Each hand you get 5 face-down cards. Pick 2 to reveal and memorise, then confirm.\n\n" +
        "Then spend your actions: draw a card and discard it or swap it for one of your hidden cards, " +
        "play a Tarot card for a special effect, or pass to score your hand early.\n\n" +
        "Your hand scores the value of its cards: Ace 1, number cards their number, Jack 11, Queen 12, King 13.\n\n" +
        "Win a table to earn chips. Spend them in the shop on items that change your scoring, " +
        "or on the slot machine for Tarot cards. Beat all three tables to finish the run. Lose once and the run is over.";

    private GameObject howToPlayPanel;
    private GameObject optionsPanel;

    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);

        Canvas canvas = RuntimeUI.CreateCanvas("MenuPanelsCanvas", 100);
        canvas.transform.SetParent(transform, false);
        howToPlayPanel = BuildHowToPlayPanel(canvas.transform);
        optionsPanel = BuildOptionsPanel(canvas.transform);

        HookButton("start", StartRun);
        HookButton("How To Play", () => howToPlayPanel.SetActive(true));
        HookButton("Options", () => optionsPanel.SetActive(true));
        HookButton("Quit", Quit);
    }

    private static void HookButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            Debug.LogWarning($"Menu button '{objectName}' not found!");
            return;
        }

        Button button = target.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void StartRun()
    {
        RunData.StartNewRun();
        SceneManager.LoadScene(TABLE_SELECT_SCENE);
    }

    private void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private GameObject BuildHowToPlayPanel(Transform parent)
    {
        GameObject root = BuildPanelRoot(parent, "HowToPlayPanel", "How to Play");

        RuntimeUI.CreateText(root.transform, "Body", HOW_TO_PLAY_TEXT, 30f, new Vector2(0f, 20f), new Vector2(1000f, 640f), TextAlignmentOptions.TopLeft);

        Button close = RuntimeUI.CreateButton(root.transform, "Close", "Close", new Vector2(0f, -430f), new Vector2(300f, 80f), new Color(0.2f, 0.45f, 0.75f, 1f), 34f);
        close.onClick.AddListener(() => root.SetActive(false));

        root.SetActive(false);
        return root;
    }

    private GameObject BuildOptionsPanel(Transform parent)
    {
        GameObject root = BuildPanelRoot(parent, "OptionsPanel", "Options");

        RuntimeUI.CreateText(root.transform, "VolumeLabel", "Master Volume", 36f, new Vector2(0f, 120f), new Vector2(600f, 60f));

        Slider slider = RuntimeUI.CreateSlider(root.transform, "VolumeSlider", new Vector2(0f, 20f), 600f);
        TMP_Text valueText = RuntimeUI.CreateText(root.transform, "VolumeValue", "", 32f, new Vector2(0f, -60f), new Vector2(300f, 50f));

        slider.value = AudioListener.volume;
        valueText.text = $"{Mathf.RoundToInt(slider.value * 100f)}%";
        slider.onValueChanged.AddListener(value =>
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VOLUME_KEY, value);
            valueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        });

        Button back = RuntimeUI.CreateButton(root.transform, "Back", "Back", new Vector2(0f, -430f), new Vector2(300f, 80f), new Color(0.2f, 0.45f, 0.75f, 1f), 34f);
        back.onClick.AddListener(() =>
        {
            PlayerPrefs.Save();
            root.SetActive(false);
        });

        root.SetActive(false);
        return root;
    }

    private static GameObject BuildPanelRoot(Transform parent, string name, string title)
    {
        Image dim = RuntimeUI.CreatePanel(parent, name, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.8f));
        RuntimeUI.Stretch(dim.rectTransform);

        Image panel = RuntimeUI.CreatePanel(dim.transform, "Panel", Vector2.zero, new Vector2(1200f, 1000f), new Color(0.1f, 0.05f, 0.12f, 0.98f));
        RuntimeUI.CreateText(panel.transform, "Title", title, 64f, new Vector2(0f, 430f), new Vector2(800f, 90f));

        return dim.gameObject;
    }
}
