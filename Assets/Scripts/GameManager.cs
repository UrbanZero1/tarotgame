using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public const int REGULARCARDS = 52;
    public const int TAROT_CARD_OFFSET = REGULARCARDS;
    public const int MAX_TOTAL_CARDS = REGULARCARDS + STARTING_TAROT_CARDS;
    private const int MAX_CARDS_PER_ZONE = REGULARCARDS;
    public const int INVALID_CARD_ID = -1;
    public const int STARTING_TAROT_CARDS = 3;
    
    [SerializeField] private float holdDuration = 0.5f; // Duration in seconds to trigger a hold event
    [SerializeField] private int startingHands = 3;
    [SerializeField] private int startingActions = 3;
    [SerializeField] private int pointsGoal = 100;
    [SerializeField] private float cardDrawAnimationDuration = 0.35f;
    [SerializeField] private float cardDrawAnimationStagger = 0.08f;
    [SerializeField] private float cardFlipAnimationDuration = 0.3f;
    [SerializeField] private float cardFlipAnimationStagger = 0.08f;


    [Header("Zone Views")]
    [SerializeField] private ZoneView[] zoneViews;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject tarotCardPrefab;

    [Header("UI Object")] 
    [SerializeField] GameObject uiObject;


    private AbstractCardVisual[] cardVisuals = new AbstractCardVisual[MAX_TOTAL_CARDS];
    private GameStateData state;
    private TooltipData tooltipData;
    public UIData uiData;

    private bool started = false;
    private bool cardAnimationInProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state.cards = new CardData[MAX_TOTAL_CARDS];
        state.cardInteractions = new CardInteractionData[MAX_TOTAL_CARDS];
        InitZones();
        CreateDefaultDeck(ref state.cards);
        PopulateDeckZone();
        ShuffleZone(CardZone.DECK);
        uiData.UI = uiObject.GetComponent<GameUI>();
        uiData.UI.Clicked += OnButtonClicked;
        GameStart();
    }
    
    void Restart() {
        state.cards = new CardData[MAX_TOTAL_CARDS];
        state.cardInteractions = new CardInteractionData[MAX_TOTAL_CARDS];
        CreateDefaultDeck(ref state.cards);
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
        for (int i = 0; i < state.zones.Length; i++) {
            state.zones[i].count = 0;
            state.zones[i].dirty = true;
            for (int j = 0; j < state.zones[i].cardIds.Length; j++) {
                state.zones[i].cardIds[j] = INVALID_CARD_ID;
            }
        }
    }

    /*
     * RULES OF THE GAME
     */
    private void GameStart() {
        CreateStartingTarotCards();
        state.game.state = GameState.START;
        state.game.currentPoints = 0;
        state.game.pointsGoal = pointsGoal;
        state.game.cardsSelected = new List<int>();
        state.game.actionsRemaining = startingActions;
        state.game.handsRemaining = startingHands;
        state.handStart = new HandStartData{};
        InitHandStartData(ref state.handStart);
        UpdateDirtyZones();
        DrawCards(CardZone.PLAYER, 5);
        uiData.dirty = true;
        started = true;
    }

    private void CreateStartingTarotCards() {
        TarotCardType[] allTarotCardTypes = 
            (TarotCardType[])Enum.GetValues(typeof(TarotCardType));
        
        List<TarotCardType> remainingCards = new List<TarotCardType>(allTarotCardTypes);
        
        
        CardData WheelOfFortune = new CardData {
            id = TAROT_CARD_OFFSET,
            kind = CardKind.Tarot,
            tarotType = TarotCardType.WheelOfFortune,
            zone = CardZone.TAROT,
            visualDirty = true,
        };
        
        state.cards[TAROT_CARD_OFFSET] = WheelOfFortune;
        SpawnTarotCard(WheelOfFortune);
        AddCardToZone(TAROT_CARD_OFFSET, CardZone.TAROT);
        
        for (int i = 1; i < STARTING_TAROT_CARDS; i++) {
            int randomIndex = UnityEngine.Random.Range(0, remainingCards.Count);
            TarotCardType selectedType = remainingCards[randomIndex];
            remainingCards.RemoveAt(randomIndex);

            int cardId = TAROT_CARD_OFFSET + i;
            CardData tarotCard = new CardData {
                id = cardId,
                kind = CardKind.Tarot,
                tarotType = selectedType,
                zone = CardZone.TAROT,
                visualDirty = true,
            };
            
            state.cards[cardId] = tarotCard;
            SpawnTarotCard(tarotCard);
            AddCardToZone(cardId, CardZone.TAROT);
        }
    }
    
    private void SpawnTarotCard(CardData tarotCard) {
        ZoneView zoneView = zoneViews[(int)CardZone.TAROT];
        
        GameObject newCard = Instantiate(tarotCardPrefab);
        TarotCardVisual visual = newCard.GetComponent<TarotCardVisual>();
        
        visual.Initialize(state, tarotCard.id);
        
        visual.PointerEntered += OnCardPointerEntered;
        visual.PointerExited += OnCardPointerExited;
        visual.PointerPressed += OnCardPointerPressed;
        visual.PointerReleased += OnCardPointerReleased;
        
        cardVisuals[tarotCard.id] = visual;
        
        if (zoneView.anchor == null) {
            Debug.LogError($"Tarot ZoneView has no anchor assigned.");
            return;
        }

        visual.transform.SetParent(zoneView.anchor, false);
        
        MarkZoneDirty(CardZone.TAROT);
    }

    private void InitHandStartData(ref HandStartData handStartData) {
        handStartData.playerSelectedCount = 0;
        handStartData.playerSelectedCards = new int[2];
        handStartData.state = HandStartState.WAITING_FOR_PLAYER_SELECTION;
    }

    private void InitZones() {
        state.zones = new CardZoneData[(int)CardZone.COUNT];
        for (int i = 0; i < (int)CardZone.COUNT; i++) {

            int[] cardIds = new int[MAX_CARDS_PER_ZONE];
            for (int j = 0; j < cardIds.Length; j++)
            {
                cardIds[j] = INVALID_CARD_ID;
            }

            state.zones[i] = new CardZoneData {
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
        
        visual.Initialize(state, cardId);
        
        visual.PointerEntered += OnCardPointerEntered;
        visual.PointerExited += OnCardPointerExited;
        visual.PointerPressed += OnCardPointerPressed;
        visual.PointerReleased += OnCardPointerReleased;
        
        cardVisuals[cardId] = visual;

        CardZone zone = state.cards[cardId].zone;
        Debug.Log($"Spawned {state.cards[cardId]} in {zone}");
        
        MarkZoneDirty(zone);
    }

    private void MarkZoneDirty(CardZone zone) {
        state.zones[(int)zone].dirty = true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public bool IsValidCardId(int cardId) {
        if (cardId < 0 || cardId >= state.cards.Length) {
            Debug.LogError($"Invalid card ID: {cardId}");
            return false;
        }

        return true;
    }

    private void OnCardPointerEntered(AbstractCardVisual visual, PointerEventData eventData) {
        // if (visualType == typeof(TarotCardVisual)) {;
        //     Debug.Log($"Tarot card hovered: {state.cards[cardId].tarotType}");
        // }
        // else if (visualType == typeof(CardVisual)) {
        //     Debug.Log($"Card hovered: {state.cards[cardId]}");
        // }
        // else {
        //     Debug.LogError($"Unknown visual type: {visualType}");
        // }
        int cardId = visual.cardId;
        if (visual is TarotCardVisual) {
            Debug.Log("Hovered over Tarot card: " + state.cards[cardId].tarotType);
            ShowToolTip(visual, eventData);
            return;
        }
        
        CardInteractionData interaction = state.cardInteractions[cardId];
        interaction.hovered = true;
        state.cardInteractions[cardId] = interaction;
        
        // TODO: Implement hover highlight
    }

    private void ShowToolTip(AbstractCardVisual visual, PointerEventData eventData) {
        tooltipData.visible = true;
        tooltipData.position = eventData.position;
        tooltipData.hoveredCardId = visual.cardId;
    }
    
    private void HideToolTip() {
        tooltipData.visible = false;
        tooltipData.hoveredCardId = INVALID_CARD_ID;
    }
    
    private void UpdateToolTip() {
        if (!tooltipData.visible) {
            uiData.UI.tooltip.Hide();
            return;
        }

        if (tooltipData.hoveredCardId == INVALID_CARD_ID) return;

        int cardId = tooltipData.hoveredCardId;
        uiData.UI.tooltip.Show(state.cards[cardId].tarotType.GetDescription(), tooltipData.position);
    }

    private void OnCardPointerExited(AbstractCardVisual visual, PointerEventData eventData) {
        int cardId = visual.cardId;
        if (visual is TarotCardVisual) {
            Debug.Log("Stopped hovering over Tarot card: " + state.cards[cardId].tarotType);
            HideToolTip();
            return;
        }
        
        if (!IsValidCardId(cardId)) return;
        
        CardInteractionData interaction = state.cardInteractions[cardId];
        interaction.hovered = false;
        state.cardInteractions[cardId] = interaction;
    }

    private void OnCardPointerPressed(AbstractCardVisual visual, PointerEventData eventData) {
        if (cardAnimationInProgress) return;

        int cardId = visual.cardId;
        if (!IsValidCardId(cardId)) return;
        
        CardInteractionData interaction = state.cardInteractions[cardId];
        interaction.pressed = true;
        interaction.holdTriggered = false;
        interaction.pressedTime = Time.time;
        state.cardInteractions[cardId] = interaction;
    }

    private void OnCardPointerReleased(AbstractCardVisual visual, PointerEventData eventData) {
        if (cardAnimationInProgress) return;

        int cardId = visual.cardId;
        if (!IsValidCardId(cardId)) return;

        CardInteractionData interaction = state.cardInteractions[cardId];
        if (!interaction.pressed) return;

        interaction.pressed = false;
        state.cardInteractions[cardId] = interaction;

        if (!interaction.holdTriggered) {
            OnCardClicked(cardId);
        }
    }
    
    private void OnCardClicked(int cardId) {
        if (cardAnimationInProgress) return;

        CardData card = state.cards[cardId];
        Debug.Log($"Card clicked: {card}");

        switch (state.game.state) {
            case GameState.START:
                switch (state.handStart.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        if (!state.cards[cardId].facedown
                            || state.cards[cardId].zone != CardZone.PLAYER) {
                            break;
                        }

                        int selectedIndex = FindSelectedCardIndex(cardId);
                        if (selectedIndex >= 0) {
                            RemoveSelectedCard(selectedIndex);
                        }
                        else if (state.handStart.playerSelectedCount
                            < state.handStart.playerSelectedCards.Length) {
                            state.handStart.playerSelectedCards[state.handStart.playerSelectedCount++] = cardId;
                            cardVisuals[cardId]?.SetSelected(true);
                        }
                        break;
                }

                break;
            case GameState.PLAY:
                switch (state.game.turnState) {
                    case TurnStates.PLAY_TAROT:
                        if (IsTarotCardSelected(cardId)) {
                            if (state.game.cardsSelected.Count < 1) {
                                state.game.cardsSelected.Add(cardId);
                            }
                            state.game.cardsSelected[0] = cardId;
                        }
                        break;
                    case TurnStates.REPLACE:
                        if (state.cards[cardId].zone == CardZone.PLAYER) {
                            if (state.game.cardsSelected.Count < 1) {
                                state.game.cardsSelected.Add(cardId);
                            }
                            state.game.cardsSelected[0] = cardId;
                        }
                        break;
                }
                break;
        }
    }

    private int FindSelectedCardIndex(int cardId)
    {
        for (int i = 0; i < state.handStart.playerSelectedCount; i++) {
            if (state.handStart.playerSelectedCards[i] == cardId)
                return i;
        }

        return -1;
    }

    private void RemoveSelectedCard(int selectedIndex)
    {
        int cardId = state.handStart.playerSelectedCards[selectedIndex];
        cardVisuals[cardId]?.SetSelected(false);

        for (int i = selectedIndex; i < state.handStart.playerSelectedCount - 1; i++) {
            state.handStart.playerSelectedCards[i] =
                state.handStart.playerSelectedCards[i + 1];
        }

        state.handStart.playerSelectedCount--;
        state.handStart.playerSelectedCards[state.handStart.playerSelectedCount] =
            INVALID_CARD_ID;
    }
    
    private void OnButtonClicked(ButtonCommands command) {
        if (cardAnimationInProgress && command != ButtonCommands.RESTART) return;

        switch (command) {
            case ButtonCommands.DRAW:
                DrawCards(CardZone.PLAYER_DRAW, 1, false);
                state.game.actionsRemaining--;
                state.game.turnState = TurnStates.DRAW;
                uiData.dirty = true;
                break;
            case ButtonCommands.PLAY_TAROT:
                state.game.actionsRemaining--;
                state.game.turnState = TurnStates.PLAY_TAROT;
                state.game.cardsSelected.Clear();
                uiData.dirty = true;
                break;
            case ButtonCommands.DISCARD:
                state.game.turnState = TurnStates.DISCARD;
                uiData.dirty = true;
                break;
            case ButtonCommands.PASS:
                state.game.hasPassed = true;
                state.game.turnState = TurnStates.END;
                break;
            case ButtonCommands.REPLACE:
                state.game.cardsSelected.Clear();
                state.game.turnState = TurnStates.REPLACE;
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
        state.cardInteractions[cardId] = default;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnCardHeld(int cardId) {
        CardData card = state.cards[cardId];
        Debug.Log($"Card held: {card}");
    }

    private void MoveCard(int cardId, CardZone dest) {
        CardData card = state.cards[cardId];
        CardZone source = card.zone;

        RemoveCardFromZone(cardId, source);
        AddCardToZone(cardId, dest);

        state.cards[cardId].zone = dest;
        // Implement if we care where a card is in a specific zone
        // card.zoneIndex = state.zones[(int)dest].count - 1;
        
        ResetCardInteraction(cardId);
        
        MarkZoneDirty(source);
        MarkZoneDirty(dest);
    }

    private void MoveCardsWithAnimation(int[] cardIds, CardZone destination) {
        if (cardIds == null || cardIds.Length == 0)
            return;

        List<CardMoveAnimation> moves = new List<CardMoveAnimation>();
        for (int i = 0; i < cardIds.Length; i++) {
            int cardId = cardIds[i];
            if (!IsValidCardId(cardId))
                continue;

            AbstractCardVisual visual = cardVisuals[cardId];
            moves.Add(new CardMoveAnimation {
                visual = visual,
                startPosition = visual != null ? visual.transform.position : Vector3.zero,
                targetPosition = Vector3.zero
            });
            MoveCard(cardId, destination);
        }

        UpdateDirtyZones();

        for (int i = 0; i < moves.Count; i++) {
            CardMoveAnimation move = moves[i];
            if (move.visual != null)
                move.targetPosition = move.visual.transform.position;
            moves[i] = move;
        }

        StartCoroutine(AnimateCardMoves(moves));
    }

    private void AddCardToZone(int cardId, CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];

        zoneData.cardIds[zoneData.count] = cardId;
        zoneData.count++;

        state.zones[(int)zone] = zoneData;
    }

    private void RemoveCardFromZone(int cardId, CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];

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
        state.zones[(int)zone] = zoneData;
    }

    private void PopulateDeckZone() {
        for (int cardId = 0; cardId < REGULARCARDS; cardId++) {
            AddCardToZone(cardId, CardZone.DECK);
            SpawnCard(cardId);
        }
    }

    private bool CreateDefaultDeck(ref CardData[] deck){
        if (deck == null || deck.Length < REGULARCARDS) return false;

        int cardId = 0;

        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) {
            for (int value = 1; value <= 13; value++) {
                deck[cardId] = new CardData{
                    id = cardId,
                    kind = CardKind.Standard,
                    value = value,
                    suit = suit,
                    zone = CardZone.DECK,
                    facedown = true,
                    visualDirty = true
                };
                cardId++;
            }
        }
        
        return true;
    }
    private void DrawCards(CardZone destination, int amount, bool facedown = true) {
        List<CardMoveAnimation> moves = new List<CardMoveAnimation>();

        for (int i = 0; i < amount; i++){
            int cardId = GetFirstCardInZone(CardZone.DECK);

            if (cardId < 0) break;

            AbstractCardVisual visual = cardVisuals[cardId];
            Vector3 startPosition = visual != null ? visual.transform.position : Vector3.zero;
            
            MoveCard(cardId, destination);

            CardData card = state.cards[cardId];
            
            card.facedown = facedown;
            card.visualDirty = true;

            state.cards[cardId] = card;

            moves.Add(new CardMoveAnimation {
                visual = visual,
                startPosition = startPosition,
                targetPosition = startPosition,
            });
        }

        UpdateDirtyZones();

        for (int i = 0; i < moves.Count; i++) {
            CardMoveAnimation move = moves[i];
            if (move.visual != null)
                move.targetPosition = move.visual.transform.position;
            moves[i] = move;
        }

        if (moves.Count > 0)
            StartCoroutine(AnimateCardMoves(moves));
    }

    private IEnumerator PlaySelectionReveal() {
        int selectedCount = state.handStart.playerSelectedCount;
        int[] selectedCards = new int[selectedCount];
        Array.Copy(state.handStart.playerSelectedCards, selectedCards, selectedCount);

        for (int i = 0; i < selectedCards.Length; i++)
            cardVisuals[selectedCards[i]]?.SetSelected(false);

        yield return StartCoroutine(PlayFlipBatch(selectedCards, false));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(PlayFlipBatch(selectedCards, true));

        state.game.state = GameState.PLAY;
        state.game.turnState = TurnStates.BEGIN;
        uiData.dirty = true;
    }

    private IEnumerator PlayFlipBatch(int[] cardIds, bool facedown) {
        if (cardIds == null || cardIds.Length == 0)
            yield break;

        cardAnimationInProgress = true;
        int completedCount = 0;

        for (int i = 0; i < cardIds.Length; i++) {
            int cardId = cardIds[i];
            if (!IsValidCardId(cardId)) {
                completedCount++;
                continue;
            }

            state.cards[cardId].facedown = facedown;
            state.cards[cardId].visualDirty = false;
            StartCoroutine(StartFlipAfterDelay(
                cardId,
                facedown,
                cardFlipAnimationStagger * i,
                () => completedCount++));
        }

        while (completedCount < cardIds.Length)
            yield return null;

        cardAnimationInProgress = false;
    }

    private IEnumerator StartFlipAfterDelay(
        int cardId,
        bool facedown,
        float delay,
        Action onComplete) {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        AbstractCardVisual visual = cardVisuals[cardId];
        if (visual == null) {
            onComplete?.Invoke();
            yield break;
        }

        visual.PlayFlip(facedown, cardFlipAnimationDuration, onComplete);
    }

    private IEnumerator AnimateCardMoves(List<CardMoveAnimation> moves) {
        cardAnimationInProgress = true;

        float totalDuration = cardDrawAnimationStagger * Mathf.Max(0, moves.Count - 1)
            + Mathf.Max(0.01f, cardDrawAnimationDuration);
        float duration = Mathf.Max(0.01f, cardDrawAnimationDuration);
        float elapsed = 0f;

        for (int i = 0; i < moves.Count; i++) {
            if (moves[i].visual != null)
                moves[i].visual.transform.position = moves[i].startPosition;
        }

        while (elapsed < totalDuration) {
            elapsed += Time.deltaTime;

            for (int i = 0; i < moves.Count; i++) {
                CardMoveAnimation move = moves[i];
                if (move.visual == null) continue;

                float moveElapsed = elapsed - cardDrawAnimationStagger * i;
                if (moveElapsed <= 0f) {
                    move.visual.transform.position = move.startPosition;
                    continue;
                }

                float progress = Mathf.Clamp01(moveElapsed / duration);
                move.visual.transform.position = Vector3.Lerp(
                    move.startPosition,
                    move.targetPosition,
                    Mathf.SmoothStep(0f, 1f, progress));
            }

            yield return null;
        }

        for (int i = 0; i < moves.Count; i++) {
            if (moves[i].visual != null)
                moves[i].visual.transform.position = moves[i].targetPosition;
        }

        cardAnimationInProgress = false;
        uiData.dirty = true;
    }

    private int GetFirstCardInZone(CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];

        if (zoneData.count == 0) {
            Debug.Log("No cards in zone " + zone + " to draw.");
            return -1;
        }

        return zoneData.cardIds[0];
    }

    private void ShuffleZone(CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];

        System.Random rand = new System.Random();


        for (int i = 0; i < zoneData.count; i++)
        {
            int randIdx = rand.Next(i + 1);   

            int temp = zoneData.cardIds[i];
            zoneData.cardIds[i] = zoneData.cardIds[randIdx];
            zoneData.cardIds[randIdx] = temp;
        }

        zoneData.dirty = true;
        state.zones[(int)zone] = zoneData;
    }


    private void UpdateDirtyZones() {
        for (int i = 0; i < state.zones.Length; i++) {
            if (!state.zones[i].dirty)
                continue;

            LayoutZone((CardZone)i);
            state.zones[i].dirty = false;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void LayoutZone(CardZone zone) {
        FindZoneView(zone, out ZoneView view);
        CardZoneData zoneData = state.zones[(int)zone];
        view.layout.Layout(
            zoneData.cardIds,
            zoneData.count,
            cardVisuals,
            view.anchor);
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
        for (int i = 0; i < state.cards.Length; i++) {
            if (!state.cards[i].visualDirty)
                continue;


            AbstractCardVisual visual = cardVisuals[i];
            if (visual) {
                visual.UpdateVisuals();
            }

            state.cards[i].visualDirty = false;
        }
    }

    private void UpdateCardInteractions() {
        if (cardAnimationInProgress) return;

        float currentTime = Time.time;
        
        for (int cardId = 0; cardId < state.cardInteractions.Length; cardId++) {
            CardInteractionData interaction = state.cardInteractions[cardId];
            
            if (!interaction.pressed) continue; 
            if (interaction.holdTriggered) continue;
            
            float heldDuration = currentTime - interaction.pressedTime;
            
            if (heldDuration < holdDuration) continue;
            
            interaction.holdTriggered = true;
            state.cardInteractions[cardId] = interaction;
            
            OnCardHeld(cardId);
        }
    }
    
    private int CalculateZonePoints(CardZone zone) {
        int points = 0;
        CardZoneData zoneData = state.zones[(int)zone];

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            points += state.cards[cardId].value;
        }

        return points;
    }

    private void UpdateUI() {
        if (!uiData.dirty) return;
        uiData.dirty = false;
        switch (state.game.state) {
            case GameState.START:
                switch (state.handStart.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateHandsRemainingText($"Hands Remaining: {state.game.handsRemaining}");
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {state.game.actionsRemaining}");
                        uiData.UI.UpdatePointsGoalText($"Goal: {state.game.pointsGoal}");
                        uiData.UI.UpdatePlayerPointsText($"Current Points: {state.game.currentPoints}");
                        uiData.UI.UpdateMiddleText("Select 2 cards to reveal");
                        break;
                    case HandStartState.PLAYER_SELECTION_DONE:
                        break;
                    case HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION:
                        break;
                }
                break;
            case GameState.PLAY:
                switch (state.game.turnState) {
                    case TurnStates.BEGIN:
                        uiData.UI.UpdatePlayerPointsText($"Current Points: {state.game.currentPoints}");
                        uiData.UI.UpdateMiddleText("Select an action");
                        uiData.UI.EnableDrawButton(true);
                        uiData.UI.EnablePassButton(true);
                        uiData.UI.EnablePlayTarotButton(true);
                        break;
                    case TurnStates.PLAY_TAROT:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {state.game.actionsRemaining}");
                        uiData.UI.UpdateMiddleText("Select a Tarot card to play");
                        break;
                    case TurnStates.DRAW:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {state.game.actionsRemaining}");
                        uiData.UI.UpdateMiddleText("Discard or Replace");
                        uiData.UI.EnableDiscardButton(true);
                        uiData.UI.EnableReplaceButton(true);
                        break;
                    case TurnStates.REPLACE:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateActionsRemainingText($"Actions Remaining: {state.game.actionsRemaining}");
                        uiData.UI.UpdateMiddleText("Replace a card");
                        break;
                    case TurnStates.DISCARD:
                        uiData.UI.DisableAllButtons();
                        break;
                    case TurnStates.CALCULATE_HAND:
                        uiData.UI.DisableAllButtons();
                        uiData.UI.UpdateHandsRemainingText($"Hands Remaining: {state.game.handsRemaining}");
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
                    uiData.UI.UpdatePlayerPointsText($"Current Points: {state.game.currentPoints}");
                    uiData.UI.UpdateMiddleText(state.game.currentPoints >= pointsGoal
                        ? $"You Win! Final Score: {state.game.currentPoints}"
                        : $"Game Over! Final Score: {state.game.currentPoints}");
                    uiData.UI.EnableRestartButton(true);
                    uiData.UI.EnableExitButton(true);
                    break;
        }
    }
    
    private void SetAllCardsInZoneFaceDown(CardZone zone, bool facedown) {
        CardZoneData zoneData = state.zones[(int)zone];
        int[] cardIds = new int[zoneData.count];

        for (int i = 0; i < zoneData.count; i++) {
            cardIds[i] = zoneData.cardIds[i];
        }

        StartCoroutine(PlayFlipBatch(cardIds, facedown));
    }
    
    private void SetCardFaceDown(int cardId, bool facedown) {
        if (!IsValidCardId(cardId)) return;
        
        StartCoroutine(PlayFlipBatch(new[] { cardId }, facedown));
    }
    
    private void ReplaceCardFromDraw(int drawCardId, int replaceCardId) {
        CardZoneData zoneData = state.zones[(int)CardZone.PLAYER];
        AbstractCardVisual drawVisual = IsValidCardId(drawCardId)
            ? cardVisuals[drawCardId]
            : null;
        AbstractCardVisual replacedVisual = IsValidCardId(replaceCardId)
            ? cardVisuals[replaceCardId]
            : null;
        Vector3 drawStart = drawVisual != null ? drawVisual.transform.position : Vector3.zero;
        Vector3 replacedStart = replacedVisual != null
            ? replacedVisual.transform.position
            : Vector3.zero;

        for (int i = 0; i < zoneData.count; i++) {
            if (zoneData.cardIds[i] == replaceCardId) {
                zoneData.cardIds[i] = drawCardId;
                RemoveCardFromZone(drawCardId, CardZone.PLAYER_DRAW);
                AddCardToZone(replaceCardId, CardZone.DISCARD);       
                state.cards[drawCardId].zone = CardZone.PLAYER;
                state.cards[replaceCardId].zone = CardZone.DISCARD;
                MarkZoneDirty(CardZone.PLAYER_DRAW);
                MarkZoneDirty(CardZone.PLAYER);
                MarkZoneDirty(CardZone.DISCARD);
                UpdateDirtyZones();

                List<CardMoveAnimation> moves = new List<CardMoveAnimation> {
                    new CardMoveAnimation {
                        visual = drawVisual,
                        startPosition = drawStart,
                        targetPosition = drawVisual != null
                            ? drawVisual.transform.position
                            : Vector3.zero
                    },
                    new CardMoveAnimation {
                        visual = replacedVisual,
                        startPosition = replacedStart,
                        targetPosition = replacedVisual != null
                            ? replacedVisual.transform.position
                            : Vector3.zero
                    }
                };
                StartCoroutine(AnimateCardMoves(moves));
                break;
            }
        }

    }
    
    private bool IsTarotCardSelected(int cardId) {
        if (!IsValidCardId(cardId)) return false;
        return state.cards[cardId].kind == CardKind.Tarot;
    }
    
    private void RemoveTarotCard(int cardId) {
        if (!IsValidCardId(cardId)) return;
        if (state.cards[cardId].kind != CardKind.Tarot) return;

        RemoveCardFromZone(cardId, CardZone.TAROT);
        state.cards[cardId] = default;
        Destroy(cardVisuals[cardId].gameObject);
        cardVisuals[cardId] = null;
        ResetCardInteraction(cardId);
        HideToolTip();
    }
    
    private void DiscardAllCardsInZone(CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];
        Debug.Log($"Discarding all cards in zone {zone} ({zoneData.count} cards)");

        int[] cardIds = new int[zoneData.count];
        for (int i = 0; i < zoneData.count; i++) {
            cardIds[i] = zoneData.cardIds[i];
        }
        
        MoveCardsWithAnimation(cardIds, CardZone.DISCARD);
    }

    // Update is called once per frame
    void Update()
    {
        if (!started) return;
        
        switch (state.game.state) {
            // Player selects 2 cards and reveals them.
            case GameState.START:
                switch (state.handStart.state) {
                    case HandStartState.WAITING_FOR_PLAYER_SELECTION:
                        if (state.handStart.playerSelectedCount >= 2) {
                            state.handStart.state = HandStartState.PLAYER_SELECTION_DONE;
                        }
                        break;
                    case HandStartState.PLAYER_SELECTION_DONE:
                        state.handStart.state = HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION;
                        StartCoroutine(PlaySelectionReveal());
                        break;
                    
                    case HandStartState.WAITING_TO_HIDE_PLAYER_SELECTION:
                        break;
                }
                break;
            case GameState.PLAY:
                switch (state.game.turnState) {
                    case TurnStates.REVEAL:
                        break;
                    case TurnStates.BEGIN:
                        break;
                    case TurnStates.DRAW:
                        break;
                    case TurnStates.PLAY_TAROT:
                        if (state.game.cardsSelected.Count > 0) {
                            int cardId = state.game.cardsSelected[0];
                            if (IsTarotCardSelected(cardId)) {
                                state.cards[cardId].tarotType.PlayCardEffect(ref state);
                                RemoveTarotCard(cardId);
                                state.game.cardsSelected.Clear();
                                state.game.turnState = TurnStates.END;
                            }
                        }
                        break;
                    case TurnStates.REPLACE:
                        if (state.game.cardsSelected.Count > 0) {
                            int drawCardId = GetFirstCardInZone(CardZone.PLAYER_DRAW);
                            int removeCardId = state.game.cardsSelected[0];
                            ReplaceCardFromDraw(drawCardId, removeCardId);
                            SetCardFaceDown(drawCardId, true);
                            state.game.cardsSelected.Clear();
                            state.game.turnState = TurnStates.END;
                            uiData.dirty = true;
                        }
                        break;
                    case TurnStates.DISCARD: {
                        int cardId = GetFirstCardInZone(CardZone.PLAYER_DRAW);
                        MoveCardsWithAnimation(new[] { cardId }, CardZone.DISCARD);
                        state.game.turnState = TurnStates.END;
                    }
                        break;
                    case TurnStates.END:
                        uiData.dirty = true;
                        if (state.game.actionsRemaining < 1 || state.game.hasPassed) {
                            state.game.turnState = TurnStates.CALCULATE_HAND;
                            state.game.waitUntil = Time.time + 2f;
                            state.game.hasPassed = false;
                            break;
                        }
                        
                        state.game.turnState = TurnStates.BEGIN;
                        break;
                    case TurnStates.CALCULATE_HAND:
                        if (Time.time >= state.game.waitUntil) {
                            state.game.handsRemaining--;
                            state.game.currentPoints += CalculateZonePoints(CardZone.PLAYER);
                            uiData.dirty = true;
                            if (state.game.currentPoints >= state.game.pointsGoal) {
                                state.game.state = GameState.END;
                            }
                            else {
                                if (state.game.handsRemaining > 0) {
                                    SetAllCardsInZoneFaceDown(CardZone.PLAYER, true);
                                    DiscardAllCardsInZone(CardZone.PLAYER);
                                    DrawCards(CardZone.PLAYER, 5);
                                    state.game.state = GameState.START;
                                    state.game.actionsRemaining = startingActions;
                                    InitHandStartData(ref state.handStart);
                                    uiData.dirty = true;
                                }
                                else {
                                    state.game.state = GameState.END;
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
        UpdateToolTip();
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
    public float waitUntil;
    public int handsRemaining, actionsRemaining;
    public int currentPoints, pointsGoal;
    public bool hasPassed;
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
    CALCULATE_HAND,
    PLAY_TAROT
}

public struct CardData {
    public int id;
    public CardKind kind;
    public int value;
    public CardSuit suit;
    public TarotCardType tarotType;
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

public struct CardMoveAnimation {
    public AbstractCardVisual visual;
    public Vector3 startPosition;
    public Vector3 targetPosition;
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

struct TooltipData {
    public int hoveredCardId;
    public bool visible;
    public Vector2 position;
}

public enum CardZone {
    PLAYER,
    DECK,
    PLAYER_DRAW,
    DISCARD,
    TAROT,
    COUNT
}

public struct HandStartData {
    public HandStartState state;
    
    public int playerSelectedCount;
    public int[] playerSelectedCards;
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