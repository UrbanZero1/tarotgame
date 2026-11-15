using System;
using UnityEngine;

public class ChipManager : MonoBehaviour
{
    public static ChipManager Instance { get; private set; }

    public event Action<int> ChipsUpdate;
    //  storing player chips
    public int CurrentChips { get; private set; }

    private const string ChipsSAVE = "PlayerChips";

    private void Awake()
    {
        Destroy(gameObject);
            return;

    Instance = this;
    // keeping the value alive between scenes
    DontDestroyOnLoad(gameObject);

    CurrentChips = PlayerPrefs.GetInt(ChipsSAVE, 0);

    
}
// Reward for winning, specifying the ammount per level of difficulty
public void RewardChips(int amount)
{
    if (amount <= 0) 
        return;

    CurrentChips = checked(CurrentChips + amount);
    ChipTotalSave();
    // plug for UI for chip balance
    ChipsUpdate?.Invoke(CurrentChips);
}
// Logic to deduct chips when purchasing
public bool SpendChips(int amount)
{
    if (amount <= 0 || CurrentChips < amount)
         return false;


     CurrentChips -= amount;
     ChipTotalSave();

     ChipsUpdate?.Invoke(CurrentChips);

        return true;
    }
// Function to save the balance between game sessions
public void ChipTotalSave()
    {
        PlayerPrefs.SetInt(ChipsSAVE, CurrentChips);
        PlayerPrefs.Save();
    }
}




