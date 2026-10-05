using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public const int MAX_CARDS = 52;
    private const int MAX_CARDS_PER_ZONE = MAX_CARDS;
    public const int INVALID_CARD_ID = -1;
    const int PLAYERCOUNT = 2;
    [SerializeField] private float holdDuration = 0.5f; // Duration in seconds to trigger a hold event


    [Header("Zone Views")]
    [SerializeField] private ZoneView[] zoneViews;

    [SerializeField] private GameObject cardPrefab;


    private CardVisual[] cardVisuals = new CardVisual[MAX_CARDS];
    private CardData[] cards = new CardData[MAX_CARDS];
    private CardInteractionData[] cardInteractions = new CardInteractionData[MAX_CARDS];
    private CardZoneData[] zones;

    private bool started = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDefaultDeck(ref cards);
        InitZones();
        PopulateDeckZone();
        ShuffleZone(CardZone.DECK);

        GameStart();
    }

    private void InitZones() {
        zones = new CardZoneData[(int)CardZone.COUNT];
        for (int i = 0; i < (int)CardZone.COUNT; i++) {

            int[] cardIds = new int[MAX_CARDS_PER_ZONE];
            for (int j = 0; j < cardIds.Length; j++)
            {
                cardIds[j] = INVALID_CARD_ID;
            }

            zones[i] = new CardZoneData {
                zone = (CardZone)i,
                cardIds = cardIds,
                count = 0, 
                dirty = true,
            };
        }

    }

    private void SpawnCard(int cardId) {
        GameObject newCard = Instantiate(cardPrefab);
        CardVisual visual = newCard.GetComponent<CardVisual>();
        
        visual.Initialize(cards, cardId);
        
        visual.PointerEntered += OnCardPointerEntered;
        visual.PointerExited += OnCardPointerExited;
        visual.PointerPressed += OnCardPointerPressed;
        visual.PointerReleased += OnCardPointerReleased;
        
        cardVisuals[cardId] = visual;

        CardZone zone = cards[cardId].zone;

        if (!FindZoneView(zone, out ZoneView view))
        {
            Debug.LogError($"No ZoneView configured for zone {zone}.");
            return;
        }

        if (view.anchor == null)
        {
            Debug.LogError($"ZoneView for {zone} has no anchor assigned.");
            return;
        }

        visual.transform.SetParent(view.anchor, false);
        Debug.Log($"Spawned {cards[cardId]} in {zone}");
        
        MarkZoneDirty(zone);
    }

    private void MarkZoneDirty(CardZone zone) {
        zones[(int)zone].dirty = true;
    }

    public bool IsValidCardId(int cardId) {
        if (cardId < 0 || cardId >= cards.Length) {
            Debug.LogError($"Invalid card ID: {cardId}");
            return false;
        }

        return true;
    }

    private void OnCardPointerEntered(int cardId) {
        if (!IsValidCardId(cardId)) return;
        
        CardInteractionData interaction = cardInteractions[cardId];
        interaction.hovered = true;
        cardInteractions[cardId] = interaction;
        
        // TODO: Implement hover highlight
    }

    private void OnCardPointerExited(int cardId) {
        if (!IsValidCardId(cardId)) return;
        
        CardInteractionData interaction = cardInteractions[cardId];
        interaction.hovered = false;
        cardInteractions[cardId] = interaction;
    }

    private void OnCardPointerPressed(int cardId) {
        if (!IsValidCardId(cardId)) return;
        
        if (cards[cardId].zone != CardZone.PLAYER) return;
        
        CardInteractionData interaction = cardInteractions[cardId];
        interaction.pressed = true;
        interaction.holdTriggered = false;
        interaction.pressedTime = Time.time;
        cardInteractions[cardId] = interaction;
    }

    private void OnCardPointerReleased(int cardId) {
        if (!IsValidCardId(cardId)) return;

        CardInteractionData interaction = cardInteractions[cardId];
        if (!interaction.pressed) return;

        interaction.pressed = false;
        cardInteractions[cardId] = interaction;

        if (!interaction.holdTriggered) {
            OnCardClicked(cardId);
        }
    }
    
    private void OnCardClicked(int cardId) {
        CardData card = cards[cardId];
        Debug.Log($"Card clicked: {card}");

        cards[cardId].facedown = !cards[cardId].facedown;
        cards[cardId].visualDirty = true;
    }

    private void ResetCardInteraction(int cardId) {
        cardInteractions[cardId] = default;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnCardHeld(int cardId) {
        CardData card = cards[cardId];
        Debug.Log($"Card held: {card}");
    }

    private void MoveCard(int cardId, CardZone dest) {
        CardData card = cards[cardId];
        CardZone source = card.zone;

        RemoveCardFromZone(cardId, source);
        AddCardToZone(cardId, dest);

        cards[cardId].zone = dest;
        // Implement if we care where a card is in a specific zone
        // card.zoneIndex = zones[(int)dest].count - 1;
        
        ResetCardInteraction(cardId);
        
        MarkZoneDirty(source);
        MarkZoneDirty(dest);
    }

    private void AddCardToZone(int cardId, CardZone zone) {
        CardZoneData zoneData = zones[(int)zone];

        zoneData.cardIds[zoneData.count] = cardId;
        zoneData.count++;

        zones[(int)zone] = zoneData;
    }

    private void RemoveCardFromZone(int cardId, CardZone zone) {
        CardZoneData zoneData = zones[(int)zone];

        int removeIndex = -1;

        for (int i = 0; i < zoneData.count; i++) {
            if (zoneData.cardIds[i] == cardId) {
                removeIndex = i;
                break;
            }
        }

        if (removeIndex < 0) return;

        for (int i = removeIndex; i < zoneData.count - 1; i++) {
            zoneData.cardIds[i] = zoneData.cardIds[i + 1];
        }
        
        zoneData.count--;
        zones[(int)zone] = zoneData;
    }

    private void PopulateDeckZone() {
        for (int cardId = 0; cardId < cards.Length; cardId++) {
            AddCardToZone(cardId, CardZone.DECK);
        }
    }

    private bool CreateDefaultDeck(ref CardData[] deck){
        if (deck == null || deck.Length != MAX_CARDS) return false;

        int cardId = 0;

        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) {
            for (int value = 1; value <= 13; value++) {
                deck[cardId] = new CardData{
                    id = cardId,
                    value = value,
                    suit = suit,
                    zone = CardZone.DECK,
                    facedown = false,
                    visualDirty = true
                };
                cardId++;
            }
        }
        return true;
    }
    /*
     * RULES OF THE GAME
     * Both players draw 5 cards
     */
    private void GameStart() {
        DrawCards(CardZone.PLAYER, 5);
        //DrawCards(CardZone.ENEMY, 5);
        started = true;
    }

    private void DrawCards(CardZone destination, int amount, bool facedown = true) {
        for (int i = 0; i < amount; i++){
            int cardId = GetFirstCardInZone(CardZone.DECK);

            if (cardId < 0) return;
            
            MoveCard(cardId, destination);

            CardData card = cards[cardId];
            
            card.facedown = facedown;
            card.visualDirty = true;

            cards[cardId] = card;

            SpawnCard(cardId);
        }
    }

    private int GetFirstCardInZone(CardZone zone) {
        CardZoneData zoneData = zones[(int)zone];

        if (zoneData.count == 0) return -1;

        return zoneData.cardIds[0];
    }

    private void ShuffleZone(CardZone zone) {
        CardZoneData zoneData = zones[(int)zone];

        System.Random rand = new System.Random();


        for (int i = 0; i < zoneData.count; i++)
        {
            int randIdx = rand.Next(i + 1);   

            int temp = zoneData.cardIds[i];
            zoneData.cardIds[i] = zoneData.cardIds[randIdx];
            zoneData.cardIds[randIdx] = temp;
        }

        zoneData.dirty = true;
        zones[(int)zone] = zoneData;
    }


    private void UpdateDirtyZones() {
        for (int i = 0; i < zones.Length; i++) {
            if (!zones[i].dirty)
                continue;

            LayoutZone((CardZone)i);
            zones[i].dirty = false;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void LayoutZone(CardZone zone) {
        if (!FindZoneView(zone, out ZoneView view)) {
            Debug.Log($"Couldn't find ZoneView {zone}");
            return;
        }
        CardZoneData zoneData = zones[(int)zone];

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            CardVisual visual = cardVisuals[cardId];
            if (!visual) continue;

            visual.transform.SetParent(view.anchor, false);
        }

        if (view.layout is not null) {
            view.layout.Layout(
                    zoneData.cardIds,
                    zoneData.count,
                    cardVisuals);
        }
    }

    private bool FindZoneView(CardZone zone, out ZoneView result) {
        for (int i = 0; i < zoneViews.Length; i++) {
            if (zoneViews[i].zone == zone) {
                result = zoneViews[i];
                return true;
            }
        }

        result = default;
        return false;
    }

    private void UpdateCardVisuals() {
        for (int i = 0; i < cards.Length; i++) {
            if (!cards[i].visualDirty)
                continue;


            CardVisual visual = cardVisuals[i];
            if (visual != null) {
                visual.UpdateVisuals();
            }

            cards[i].visualDirty = false;
        }
    }

    private void UpdateCardInteractions() {
        float currentTime = Time.time;
        
        for (int cardId = 0; cardId < cardInteractions.Length; cardId++) {
            CardInteractionData interaction = cardInteractions[cardId];
            
            if (!interaction.pressed) continue; 
            if (interaction.holdTriggered) continue;
            
            float heldDuration = currentTime - interaction.pressedTime;
            
            if (heldDuration < holdDuration) continue;
            
            interaction.holdTriggered = true;
            cardInteractions[cardId] = interaction;
            
            OnCardHeld(cardId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!started) return;
        UpdateDirtyZones();
        UpdateCardVisuals();
        UpdateCardInteractions();
    }

}

public struct CardData {
    public int id;
    public int value;
    public CardSuit suit;
    public CardZone zone;
    public bool facedown;
    public bool visualDirty;

    public readonly override string ToString() {
        //if (facedown) return "Facedown";
        return $"{suit}: {value}";
    }
}

public struct CardZoneData {
    public CardZone zone;
    public int[] cardIds;
    public int count;
    public bool dirty;
}

[Serializable]
public struct ZoneView {
    public CardZone zone;
    public Transform anchor;
    public CardLayout layout;
}

public enum CardSuit {
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}

public enum CardZone {
    PLAYER = 0,
    ENEMY = 1,
    DECK = 2,
    COUNT
}
