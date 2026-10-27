using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CasinoManager : MonoBehaviour
{
    
    [SerializeField] private Transform floorsBtnContainer;
    [SerializeField] private GameObject elevatorMenu;
    [SerializeField] private GameObject floorButtonPrefab;

    [SerializeField] private GameObject StartMenu;
    [SerializeField] private GameObject LevelMenu;
    [SerializeField] private GameObject How_To_Play;

    //[SerializeField] private GameObject closeBtn;

    private int currentFloor = -1;


    private string[] floorScenes =
    {
        "Lobby",
        "Floor_1",
        "Floor_2",
        //"Floor_3"
    };

    void Start()
    {
      StartMenu = GameObject.Find("A2 StartMenu");
        LevelMenu = GameObject.Find("A2 LevelMenu");
        How_To_Play = GameObject.Find("A2 HowToPlay");

        string currentScene = SceneManager.GetActiveScene().name;

        // Find which floor we're currently on
        for (int i = 0; i < floorScenes.Length; i++)
        {
            if (floorScenes[i] == currentScene)
            {
                currentFloor = i;
                break;
            }
        }

        for (int i = 0; i < floorScenes.Length; i++)
        {
            GameObject newBtn = Instantiate(floorButtonPrefab, floorsBtnContainer);

            TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = floorScenes[i];

            Button btn = newBtn.GetComponent<Button>();
            int floorIndex = i; // Capture the current value of i
            btn.onClick.AddListener(() => GoToFloor(floorIndex));

            if(i == currentFloor)
            {
                btn.interactable = false; // Disable the button for the current floor
                btnText.color = Color.gray; // Change the text color to indicate it's disabled
            }
        }

        // Make sure this scene actually exists in our floor list
        if (currentFloor == -1)
        {
            Debug.LogError("Current scene is not in the floorScenes array.");
            return;
        }
        
    }
    public void GoToFloor(int floor)
    {
        SceneManager.LoadScene(floorScenes[floor]);
    }

    public void UseElevator()
    {
       elevatorMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void closeMenu()
    {
        elevatorMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public void A2Start()
    {
        StartMenu.SetActive(false);
    }

    public void HowToPlay()
    {
        
        How_To_Play.SetActive(true);
    }

}