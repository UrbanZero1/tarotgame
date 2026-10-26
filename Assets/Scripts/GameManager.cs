using UnityEngine;
using System;
using System.Collections.Generic;
//using UnityEditor.U2D;
using UnityEngine.PlayerLoop;

public class GameManager : MonoBehaviour
{
    public const int MAX_CARDS = 52;
    private const int MAX_CARDS_PER_ZONE = MAX_CARDS;
    public const int INVALID_CARD_ID = -1;
    public const int STARTING_TAROT_CARDS = 3;
    
    [SerializeField] private float holdDuration = 0.5f; // Duration in seconds to trigger a hold event
    [SerializeField] private int startingHands = 3;
    [SerializeField] private int startingActions = 3;
    [SerializeField] private int pointsGoal = 100;


    [Header("Zone Views")]
    [SerializeField] private ZoneView[] zoneViews;
    [SerializeField] private ZoneView tarotZoneView;

    [SerializeField] private GameObject cardPrefab;

    [Header("UI Object")] 
    [SerializeField] GameObject uiObject;


    private CardVisual[] cardVisuals = new CardVisual[MAX_CARDS];
    private CardData[] cards = new CardData[MAX_CARDS];
    private CardInteractionData[] cardInteractions = new CardInteractionData[MAX_CARDS];
    private CardZoneData[] zones;
    private List<TarotCardData> availableTarotCards = new List<TarotCardData>();
    private HandStartData _handStartData;
    public UIData uiData;

    private bool started = false;
    
    GameData gameData; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDefaultDeck(ref cards);
        InitZones();
        PopulateDeckZone();
        ShuffleZone(CardZone.DECK);
        uiData.UI = uiObject.GetComponent<GameUI>();
        uiData.UI.Clicked += OnButtonClicked;
        GameStart();
    }
    
    void Restart() {
        cards = new CardData[MAX_CARDS];
        CreateDefaultDeck(ref cards);
        ResetZones();
        DestroyCards();
        PopulateDeckZone();
        ShuffleZone(CardZone.DECK);
        GameStart();
    }
    
    void DestroyCards() {
        for (int i = 0; i < cardVisuals.Length; i++) {
            if (cardVisuals[i] != null) {
                Destroy(cardVisuals[i].gameObject);
                cardVisuals[i] = null;
            }
        }
    }

    void ResetZones() {
        for (int i = 0; i < zones.Length; i++) {
            zones[i].count = 0;
            zones[i].dirty = true;
            for (int j = 0; j < zones[i].cardIds.Length; j++) {
                zones[i].cardIds[i] = INVALID_CARD_ID;
            }
        }
    }

    /*
     * RULES OF THE GAME
     * Both players draw 5 cards
     */
    private void GameStart() {
        CreateStartingTarotCards();
        DrawCards(CardZone.PLAYER, 5);
        gameData.state = GameState.START;
        gameData.currentPoints = 0;
        gameData.pointsGoal = pointsGoal;
        gameData.currentTurn = 0;
        gameData.cardsSelected = new List<int>();
        gameData.actionsRemaining = startingActions;
        gameData.handsRemaining = startingHands;
        _handStartData = new HandStartData{};
        InitHandStartData(ref _handStartData);
        uiData.dirty = true;
        started = true;
    }

    private void CreateStartingTarotCards() {
        TarotCardType[] allTarotCardTypes = 
            (TarotCardType[])Enum.GetValues(typeof(TarotCardType));
        
        List<TarotCardType> remainingCards = new List<TarotCardType>(allTarotCardTypes);

        availableTarotCards.Clear();
        
        for (int i = 0; i < STARTING_TAROT_CARDS && remainingCards.Count > 0; i++) {
            int randomIndex = UnityEngine.Random.Range(0, remainingCards.Count);
            TarotCardType selectedType = remainingCards[randomIndex];
            remainingCards.RemoveAt(randomIndex);

            TarotCardData tarotCard = new TarotCardData {
                type = selectedType,
            };

            availableTarotCards.Add(tarotCard);
        }
    }

    private void InitHandStartData(ref HandStartData handStartData) {
        handStartData.playerSelectedCount = 0;
        handStartData.playerSelectedCards = new int[2];
        handStartData.state = HandStartState.WAITING_FOR_PLAYER_SELECTION;
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

    // ReSharper disable Unity.PerformanceAnalysis
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

        switch (gameData.state) {
            case GameState.START:
                switch (_handStartData.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        if (cards[cardId].facedown && cards[cardId].zone == CardZone.PLAYER) {
                            _handStartData.playerSelectedCards[_handStartData.playerSelectedCount++] = cardId;
                            cards[cardId].facedown = !cards[cardId].facedown;
                            cards[cardId].visualDirty = true;
                        }
                        break;
                }
                break;
            case GameState.PLAY:
                switch (gameData.turnState) {
                    case TurnStates.REPLACE:
                        if (cards[cardId].zone == CardZone.PLAYER) {
                            if (gameData.cardsSelected.Count < 1) {
                                gameData.cardsSelected.Add(cardId);
                            }
                            gameData.cardsSelected[0] = cardId;
                        }
                        break;
                }
                break;
        }
    }
    
    private void OnButtonClicked(ButtonCommands command) {
        switch (command) {
            case ButtonCommands.DRAW:
                DrawCards(CardZone.PLAYER_DRAW, 1, false);
                gameData.actionsRemaining--;
                gameData.turnState = TurnStates.DRAW;
                uiData.dirty = true;
                break;
            case ButtonCommands.DISCARD:
                gameData.turnState = TurnStates.DISCARD;
                uiData.dirty = true;
                break;
            case ButtonCommands.PASS:
                gameData.hasPassed = true;
                gameData.turnState = TurnStates.END;
                break;
            case ButtonCommands.REPLACE:
                gameData.cardsSelected = new List<int>();
                gameData.turnState = TurnStates.REPLACE;
                uiData.dirty = true;
                break;
            case ButtonCommands.RESTART:
                Restart();
                break;
            case ButtonCommands.EXIT:
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
        }
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

        if (zoneData.count == 0) {
            Debug.Log("No cards in zone " + zone + " to draw.");
            return -1;
        }

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

        if (view.anchor == null || view.layout == null) {
            for (int i = 0; i < zoneData.count; i++) {
                int cardId = zoneData.cardIds[i];
                CardVisual visual = cardVisuals[cardId];
                if (!visual) continue;

                visual.transform.SetParent(null, false);
                visual.gameObject.SetActive(false);
            }
            return;
        }
        
        // TODO: Combine this with the above loop to avoid iterating twice

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            CardVisual visual = cardVisuals[cardId];
            if (!visual) continue;

            visual.transform.SetParent(view.anchor, false);
            visual.gameObject.SetActive(true);
        }

        if (view.layout) {
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
            if (visual) {
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
    
    private int CalculateZonePoints(CardZone zone) {
        int points = 0;
        CardZoneData zoneData = zones[(int)zone];

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            points += cards[cardId].value;
        }

        return points;
    }

    private void UpdateUI() {
        if (!uiData.dirty) return;
        uiData.dirty = false;
        switch (gameData.state) {
            case GameState.START:
                switch (_handStartData.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateHandsRemainingText($"Hands Remaining: {gameData.handsRemaining}");
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {gameData.actionsRemaining}");
                        uiData.UI.UpdatePointsGoalText($"Goal: {gameData.pointsGoal}");
                        uiData.UI.UpdatePlayerPointsText($"Current Points: {gameData.currentPoints}");
                        uiData.UI.UpdateMiddleText("Select 2 cards to reveal");
                        break;
                    case HandStartState.PLAYER_SELECTION_DONE:
                        break;
                    case HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION:
                        break;
                }
                break;
            case GameState.PLAY:
                switch (gameData.turnState) {
                    case TurnStates.BEGIN:
                        uiData.UI.UpdatePlayerPointsText($"Current Points: {gameData.currentPoints}");
                        uiData.UI.UpdateMiddleText("Select an action");
                        uiData.UI.EnableDrawButton(true);
                        uiData.UI.EnablePassButton(true);
                        break;
                    case TurnStates.DRAW:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {gameData.actionsRemaining}");
                        uiData.UI.UpdateMiddleText("Discard or Replace");
                        uiData.UI.EnableDiscardButton(true);
                        uiData.UI.EnableReplaceButton(true);
                        break;
                    case TurnStates.REPLACE:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {gameData.actionsRemaining}");
                        uiData.UI.UpdateMiddleText("Replace a card");
                        break;
                    case TurnStates.DISCARD:
                        uiData.UI.DisableAllButtons();
                        break;
                    case TurnStates.CALCULATE_HAND:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateHandsRemainingText($"Hands Remaining: {gameData.handsRemaining}");
                        int playerPoints = CalculateZonePoints(CardZone.PLAYER);
                        string resultText = $"Hand Points: +{playerPoints}\n";
                        uiData.UI.UpdateMiddleText(resultText);
                        SetAllCardsInZoneFaceDown(CardZone.PLAYER, false);
                        break;
                    case TurnStates.END:
                        uiData.UI.DisableAllButtons();
                        break;
                        
                }
                break;
            case GameState.END:
                    uiData.UI.UpdatePlayerPointsText($"Current Points: {gameData.currentPoints}");
                    uiData.UI.UpdateMiddleText(gameData.currentPoints >= pointsGoal
                        ? $"You Win! Final Score: {gameData.currentPoints}"
                        : $"Game Over! Final Score: {gameData.currentPoints}");
                    uiData.UI.EnableRestartButton(true);
                    uiData.UI.EnableExitButton(true);
                    break;
        }
    }
    
    private void SetAllCardsInZoneFaceDown(CardZone zone, bool facedown) {
        CardZoneData zoneData = zones[(int)zone];

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            SetCardFaceDown(cardId, facedown);
        }
    }
    
    private void SetCardFaceDown(int cardId, bool facedown) {
        if (!IsValidCardId(cardId)) return;
        
        cards[cardId].facedown = facedown;
        cards[cardId].visualDirty = true;
    }
    
    private void ReplaceCardFromDraw(int drawCardId, int replaceCardId) {
        CardZoneData zoneData = zones[(int)CardZone.PLAYER];

        for (int i = 0; i < zoneData.count; i++) {
            if (zoneData.cardIds[i] == replaceCardId) {
                zoneData.cardIds[i] = drawCardId;
                RemoveCardFromZone(drawCardId, CardZone.PLAYER_DRAW);
                AddCardToZone(replaceCardId, CardZone.DISCARD);       
                cards[drawCardId].zone = CardZone.PLAYER;
                cards[replaceCardId].zone = CardZone.DISCARD;
                MarkZoneDirty(CardZone.PLAYER_DRAW);
                MarkZoneDirty(CardZone.PLAYER);
                MarkZoneDirty(CardZone.DISCARD);
                break;
            }
        }

    }
    
    private void DiscardAllCardsInZone(CardZone zone) {
        CardZoneData zoneData = zones[(int)zone];
        Debug.Log($"Discarding all cards in zone {zone} ({zoneData.count} cards)");

        int[] cardIds = new int[zoneData.count];
        for (int i = 0; i < zoneData.count; i++) {
            cardIds[i] = zoneData.cardIds[i];
        }
        
        foreach (int cardId in cardIds) {
            MoveCard(cardId, CardZone.DISCARD);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!started) return;
        
        switch (gameData.state) {
            // player selects 2 cards and reveals them
            case GameState.START:
                switch (_handStartData.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        if (_handStartData.playerSelectedCount >= 2) {
                            _handStartData.state = HandStartState.PLAYER_SELECTION_DONE;
                        }
                        break;
                    case HandStartState.PLAYER_SELECTION_DONE:
                        // TODO: Wait 2 seconds then flip the cards back over and move to enemy selection
                        _handStartData.waitUntil = Time.time + 2f;
                        _handStartData.state = HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION;
                        break;
                    
                    case HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION:
                        if (Time.time < _handStartData.waitUntil) break;
                        
                        for (int i = 0; i < _handStartData.playerSelectedCount; i++) {
                            int cardId = _handStartData.playerSelectedCards[i];
                            cards[cardId].facedown = true;
                            cards[cardId].visualDirty = true;
                        }
                        gameData.state = GameState.PLAY;
                        gameData.turnState = TurnStates.BEGIN;
                        uiData.dirty = true;
                        gameData.currentTurn = 1;
                        break;
                }
                break;
            case GameState.PLAY:
                switch (gameData.turnState) {
                    case TurnStates.REVEAL:
                        break;
                    case TurnStates.BEGIN:
                        break;
                    case TurnStates.DRAW:
                        break;
                    case TurnStates.REPLACE:
                        if (gameData.cardsSelected.Count > 0) {
                            int drawCardId = GetFirstCardInZone(CardZone.PLAYER_DRAW);
                            int removeCardId = gameData.cardsSelected[0];
                            ReplaceCardFromDraw(drawCardId, removeCardId);
                            SetCardFaceDown(drawCardId, true);
                            gameData.cardsSelected.Clear();
                            gameData.turnState = TurnStates.END;
                            uiData.dirty = true;
                        }
                        break;
                    case TurnStates.DISCARD: {
                        int cardId = GetFirstCardInZone(CardZone.PLAYER_DRAW);
                        MoveCard(cardId, CardZone.DISCARD);
                        gameData.turnState = TurnStates.END;
                    }
                        break;
                    case TurnStates.END:
                        gameData.currentTurn++;
                        uiData.dirty = true;
                        if (gameData.actionsRemaining < 1 || gameData.hasPassed) {
                            gameData.turnState = TurnStates.CALCULATE_HAND;
                            gameData.waitUntil = Time.time + 2f;
                            gameData.hasPassed = false;
                            break;
                        }
                        
                        gameData.turnState = TurnStates.BEGIN;
                        break;
                    case TurnStates.CALCULATE_HAND:
                        if (Time.time >= gameData.waitUntil) {
                            gameData.handsRemaining--;
                            gameData.currentPoints += CalculateZonePoints(CardZone.PLAYER);
                            uiData.dirty = true;
                            if (gameData.currentPoints >= gameData.pointsGoal) {
                                gameData.state = GameState.END;
                            }
                            else {
                                if (gameData.handsRemaining > 0) {
                                    SetAllCardsInZoneFaceDown(CardZone.PLAYER, true);
                                    DiscardAllCardsInZone(CardZone.PLAYER);
                                    DrawCards(CardZone.PLAYER, 5);
                                    gameData.state = GameState.START;
                                    gameData.actionsRemaining = startingActions;
                                    InitHandStartData(ref _handStartData);
                                    uiData.dirty = true;
                                }
                                else {
                                    gameData.state = GameState.END;
                                }
                            }
                        }
                        break;
                }
                break;
            case GameState.END:
                break;
        }
        
        UpdateDirtyZones();
        UpdateCardVisuals();
        UpdateUI();
        UpdateCardInteractions();
    }

}

public struct UIData {
    public bool dirty;
    public GameUI UI;
}

public struct GameData {
    public GameState state;
    public TurnStates turnState;
    public int currentTurn;
    public float waitUntil;
    public int handsRemaining, actionsRemaining;
    public int currentPoints, pointsGoal;
    public bool hasPassed, isNewHand;
    public List<int> cardsSelected;
}

// public enum PlayerNames {
//     PLAYER,
//     ENEMY
// }

public enum TurnStates {
    BEGIN,
    REVEAL,
    CHOOSE_ACTION,
    DRAW,
    REPLACE,
    DISCARD,
    END,
    CALCULATE_HAND
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
    PLAYER,
    DECK,
    PLAYER_DRAW,
    DISCARD,
    TAROT,
    COUNT
}

struct HandStartData {
    public HandStartState state;
    
    public int playerSelectedCount;
    public int[] playerSelectedCards;

    public float waitUntil;
}

public enum HandStartState {
    WAITING_FOR_PLAYER_SELECTION,
    PLAYER_SELECTION_DONE,
    WAITING_TO_HIDE_PLAYER_SELECTION,
    READY_TO_START
}


public enum GameState {
    START,
    PLAY,
    END
}
