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
        // Ensure that the ItemsManager persists across scenes
        DontDestroyOnLoad(gameObject);
       if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("ItemsManager started");

        AddCard(TarotCardType.WheelOfFortune);
        AddCard(TarotCardType.WheelOfFortune);


        Debug.Log(ownedCards.Count);
    }

    public void AddCard(TarotCardType cardType)
    {
        if (!ownedCards.Contains(cardType))
        {
            ownedCards.Add(cardType);
        }
    }

    public bool HasCard(TarotCardType cardType)
    {
        return ownedCards.Contains(cardType);
    }

    public List<TarotCardType> GetOwnedCards()
    {
        return new List<TarotCardType>(ownedCards);
    }

    // ============================
    // INVENTORY SYSTEM METHODS 
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
            return -1; // Return -1 or some default value if the card type is not found
        }
    }
}
