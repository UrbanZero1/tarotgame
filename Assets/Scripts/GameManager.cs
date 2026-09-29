using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handLayoutGroup;
    private Transform tempTransform;

    private CardData[] deck = new CardData[52];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempTransform = new GameObject("temp").transform;
        CreateDefaultDeck(ref deck);
        SpawnCard(deck[0]);
    }

    private void SpawnCard(CardData data){
        GameObject newCard = Instantiate(cardPrefab, tempTransform);
        CardVisual visual = newCard.GetComponent<CardVisual>();
        
        visual.Initialize(data);
    }

    private bool CreateDefaultDeck(ref CardData[] newDeck){
        if (newDeck.Length != 52) return false;
        int cardnum = 0;
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) {
            for (int i = 1; i <= 13; i++) {
                newDeck[cardnum].suit = suit;
                newDeck[cardnum].value = i;
                cardnum++;
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct CardData {
    public int value;
    public CardSuit suit;
}

public enum CardSuit {
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}
