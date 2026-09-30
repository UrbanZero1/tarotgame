using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handLayoutGroup;
    private Transform tempTransform;

    private CardData[] baseDeck = new CardData[52];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempTransform = new GameObject("temp").transform;
        CreateDefaultDeck(ref baseDeck);
        System.Random rand = new System.Random();
        int cardNum = rand.Next(0,52);
    
        SpawnCard(baseDeck[cardNum]);

        ShuffleDeck(ref baseDeck);
    }

    private void SpawnCard(CardData data){
        GameObject newCard = Instantiate(cardPrefab, tempTransform);
        CardVisual visual = newCard.GetComponent<CardVisual>();
        visual.Initialize(ref data);
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

    private bool ShuffleDeck(ref CardData[] deck){
        // create temp deck of size of deck
        int deckSize = deck.Length;
        CardData[] tempDeck = new CardData[deckSize];

        Debug.Log(deck);

        System.Random rand = new System.Random();


        // select random card from input deck and put it into each slot sequentially       
        for (int i = 0; i < deckSize; i++)
        {
            int randomIndex = 0;
            bool isDuplicate = true;

            //check if the card is already in the temp deck, if so
           //pick another random card until a unique card is found
            while (isDuplicate)
            {
                randomIndex = rand.Next(0, deckSize);
                isDuplicate = false;


                for (int j = 0; j < i; j++)
                {
                    if (deck[randomIndex].value == tempDeck[j].value && deck[randomIndex].suit == tempDeck[j].suit)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }

          
                    tempDeck[i] = deck[randomIndex];

        }

        // replace the original deck with the shuffled temp deck
        deck = tempDeck;
        Debug.Log(deck);
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
    public Transform transform;
}

public enum CardSuit {
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}
