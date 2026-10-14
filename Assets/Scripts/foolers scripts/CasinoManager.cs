using UnityEngine;
using UnityEngine.SceneManagement;

public class CasinoManager : MonoBehaviour
{
    private int currentFloor = -1;

    private bool canGoUp = false;
    private bool canGoDown = false;

    private string[] floorScenes =
    {
        "Lobby",
        "Floor_1",
        //"Floor_2",
        //"Floor_3"
    };

    void Start()
    {
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

        // Make sure this scene actually exists in our floor list
        if (currentFloor == -1)
        {
            Debug.LogError("Current scene is not in the floorScenes array.");
            return;
        }

        // Work out which directions are available
        canGoUp = currentFloor + 1 < floorScenes.Length;
        canGoDown = currentFloor - 1 >= 0;

        Debug.Log(
            "Current Floor: " + currentFloor +
            " | Can Go Up: " + canGoUp +
            " | Can Go Down: " + canGoDown
        );
    }

    public void UseElevator()
    {
        // Lobby
        if (canGoUp && !canGoDown)
        {
            GoUp();
        }

        // Highest floor
        else if (!canGoUp && canGoDown)
        {
            GoDown();
        }

        // Middle floor
        else if (canGoUp && canGoDown)
        {
            // TEMPORARY:
            // Go up until we create the elevator menu.
            GoUp();
        }
    }

    public void GoUp()
    {
        if (canGoUp)
        {
            SceneManager.LoadScene(floorScenes[currentFloor + 1]);
        }
    }

    public void GoDown()
    {
        if (canGoDown)
        {
            SceneManager.LoadScene(floorScenes[currentFloor - 1]);
        }
    }
}