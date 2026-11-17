
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardUI : MonoBehaviour
{
    private TarotCardType cardType;

    private TMP_Text priceText;
    private TMP_Text descriptionText;
    private Button buyButton;
    private Image cardImage;

    public void Setup(TarotCardType type, int price)
    {
        cardType = type;

        // Find the prefab's UI components automatically.
        priceText = FindChild<TMP_Text>("PriceText");
        descriptionText = FindChild<TMP_Text>("DescriptionText");
        buyButton = FindChild<Button>("BuyButton");
        cardImage = FindChild<Image>("CardImage");

        if (priceText != null)
        {
            priceText.text = price + " Chips";
        }

        if (descriptionText != null)
        {
            descriptionText.text = cardType.GetDescription();
        }

        // Load the correct artwork if available.
        Sprite artwork = Resources.Load<Sprite>(
            "TarotCards/" + cardType.ToString()
        );

        if (cardImage != null && artwork != null)
        {
            cardImage.sprite = artwork;
            cardImage.preserveAspect = true;
        }
        else if (artwork == null)
        {
            Debug.LogWarning(
                "Artwork not found for " + cardType);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyCard);
        }
        else
        {
            Debug.LogWarning("BuyButton not found!");
        }
    }

    // Search child objects, including inactive children.
    private T FindChild<T>(string objectName)
        where T : Component
    {
        foreach (Transform child in
            GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child.GetComponent<T>();
            }
        }

        return null;
    }

    private void BuyCard()
    {
        if (ItemsManager.Instance == null)
        {
            Debug.LogWarning("ItemsManager not found!");
            return;
        }

        bool success =
            ItemsManager.Instance.BuyCard(cardType);

        if (success)
        {
            Debug.Log(
                "Purchased: " + cardType.GetName());
        }
        else
        {
            Debug.LogWarning(
                "Purchase failed: " + cardType.GetName());
        }
    }
}
