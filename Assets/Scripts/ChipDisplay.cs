using UnityEngine;
using TMPro;

public class ChipDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text chipText;

    private ChipManager chipManager;

    private void Start()
    {
        chipManager = ChipManager.Instance;

        if (chipManager == null)
        {
            Debug.LogWarning("ChipManager not found!");
            return;
        }

        chipManager.ChipsUpdate += UpdateChipText;

        UpdateChipText(chipManager.CurrentChips);
    }

    private void UpdateChipText(int amount)
    {
        chipText.text = $"Chips: {amount}";
    }

    private void OnDestroy()
    {
        if (chipManager != null)
        {
            chipManager.ChipsUpdate -= UpdateChipText;
        }
    }
}
