using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text suitText;
    [SerializeField] private TMP_Text valueText;

    public void Initialize(ref CardData data) {
        string suitString = data.suit.ToString();
        string valueString = data.value.ToString();

        suitText.text = suitString;
        valueText.text = valueString;
    }
}
