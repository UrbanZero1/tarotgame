using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    // ============================
    // INVENTORY SYSTEM VARIABLES 
    // ============================
    private List <TarotCardType> ownedCards = new List <TarotCardType>();

    public static ItemsManager Instance;

    // ============================
    // SHOP SYSTEM VARIABLES 
    // ============================

    private Dictionary<TarotCardType, int> cardPrices = new Dictionary<TarotCardType, int>()
    {
        { TarotCardType.WheelOfFortune, 100 },
        { TarotCardType.TheMagician, 150 },
        { TarotCardType.Death, 200 },
        { TarotCardType.Justice, 250 },
        { TarotCardType.TheChariot, 300 },
        { TarotCardType.TheHangedMan, 350 },
        { TarotCardType.TheHermit, 400 },
        { TarotCardType.TheMoon, 450 },
        { TarotCardType.TheStar, 500 },
        { TarotCardType.TheFool, 550 },
        { TarotCardType.TheWorld, 600 },
        { TarotCardType.TheDevil, 650 },
        { TarotCardType.TheHighPriestess, 700 },
        { TarotCardType.Judgement, 750 },
        { TarotCardType.TheSun, 800 }
    };


    // ============================
    // INVENTORY SYSTEM METHODS 
    // ============================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("ItemsManager started");

        // Temporary test: player owns 2 Wheel of Fortune cards
        AddCard(TarotCardType.WheelOfFortune);
        AddCard(TarotCardType.WheelOfFortune);

        Debug.Log("Cards owned: " + ownedCards.Count);
    }

    public bool AddCard(TarotCardType cardType)
    {
        ownedCards.Add(cardType);
        return true;
    }

    public bool HasCard(TarotCardType cardType)
    {
        return ownedCards.Contains(cardType);
    }

    public bool RemoveCard(TarotCardType cardType)
    {
        if (!ownedCards.Contains(cardType))
        {
            Debug.LogWarning($"Player does not own {cardType}");
            return false;
        }

        ownedCards.Remove(cardType);
        Debug.Log($"Removed {cardType}. Cards remaining: {ownedCards.Count}");
        return true;
    }

    public List<TarotCardType> GetOwnedCards()
    {
        return new List<TarotCardType>(ownedCards);
    }


    // ============================
    // SHOP SYSTEM METHODS 
    // ============================

    public int GetCardPrice(TarotCardType cardType)
    {
        if (cardPrices.TryGetValue(cardType, out int price))
        {
            return price;
        }
        else
        {
            Debug.LogWarning($"Price for card type {cardType} not found.");
            return -1;
        }
    }

    public bool BuyCard(TarotCardType cardType)
    {
        int price = GetCardPrice(cardType);

        // Check if the card has a valid price
        if (price <= 0)
        {
            Debug.LogWarning($"Invalid price for {cardType}");
            return false;
        }

        // Check if the currency manager exists
        if (ChipManager.Instance == null)
        {
            Debug.LogWarning("ChipManager not found!");
            return false;
        }

        // Try to deduct the required chips
        if (!ChipManager.Instance.SpendChips(price))
        {
            Debug.LogWarning("Not enough chips to buy " + cardType);
            return false;
        }

        // Add the purchased card to the inventory
        AddCard(cardType);

        Debug.Log($"Purchased {cardType} for {price} chips!");

        return true;
    }

}
