using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text suitText;
    [SerializeField] private TMP_Text valueText;

    private CardData cardData;
    public CardData CardData => cardData;

    public void Initialize(CardData data) {
        cardData = data;
        UpdateVisuals();
    }

    public void UpdateVisuals(){
        if (cardData == null)
            return;

        suitText.text = cardData.suit.ToString();
        valueText.text = cardData.value.ToString();
    }
}
