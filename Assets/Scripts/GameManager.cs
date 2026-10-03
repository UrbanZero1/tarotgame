using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    const int DECKSIZE = 52;
    const int PLAYERCOUNT = 4;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handLayoutGroup;

    [Header("Player Hand UI Groups")]
    // Element 0 = P1 (Bottom), Element 0 = P2 (Left), Element 0 = P3 (Top), Element 0 = P4 (Right),
    [SerializeField] private Transform[] playerHandTransforms = new Transform[4];

    private List<CardVisual> spawnedCardVisuals;
    private CardData[] masterDeck = new CardData[DECKSIZE];
    private int currentDeckIndex = 0;
    private Player[] players = new Player[PLAYERCOUNT];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnedCardVisuals = new List<CardVisual>();
        CreateDefaultDeck(masterDeck);
        // System.Random rand = new System.Random();
        // int cardNum = rand.Next(0,52);
    
        // SpawnCard(masterDeck[0]);

        GameStart();
    }

    private void SpawnCard(CardData data, Transform parentCardContainer){
        GameObject newCard = Instantiate(cardPrefab, parentCardContainer);
        CardVisual visual = newCard.GetComponent<CardVisual>();
        visual.Initialize(data);
        spawnedCardVisuals.Add(visual);
    }

    private bool CreateDefaultDeck(CardData[] newDeck){
        if (newDeck.Length != 52) return false;
        int cardnum = 0;
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) {
            for (int i = 1; i <= 13; i++) {
                newDeck[cardnum] = new CardData(i, suit);
                cardnum++;
            }
        }
        return true;
    }

    private void GameStart() {
        for (int i = 0; i < PLAYERCOUNT; i++) {
            players[i] = new Player();
            DrawCards(i, 5);
            // string text = string.Join(Environment.NewLine, players[i].hand);
            // Debug.Log(text);
        }
    }

    private void DrawCards(int playerIdx, int numToDraw) {
        if (currentDeckIndex + numToDraw >= DECKSIZE) {
            return;
        }
        for (int i = 0; i < numToDraw; i++){
            SpawnCard(masterDeck[currentDeckIndex], playerHandTransforms[playerIdx]);
            players[playerIdx].hand.Add(masterDeck[currentDeckIndex++]);
        }
    }

    private bool ShuffleDeck(CardData[] deck){
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
        //masterDeck[0].value += 1;
        spawnedCardVisuals.ForEach(cardObject => cardObject.UpdateVisuals());
    }
}

public class Player {
    public List<CardData> hand;

    public Player() {
        hand = new List<CardData>();
    }
}

public class CardData {
    public int value;
    public CardSuit suit;
    public Transform transform;

    public CardData(int value, CardSuit suit) {
        this.value = value;
        this.suit = suit;
    }

    public override string ToString() {
        return $"{suit}: {value}";
    }
}


public enum CardSuit {
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}
