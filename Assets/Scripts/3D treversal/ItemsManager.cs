
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using TMPro;

public class ItemsManager : MonoBehaviour
{
    public static ItemsManager Instance { get; private set; }

    // ============================
    // INVENTORY VARIABLES
    // ============================

    private List<TarotCardType> ownedCards = new List<TarotCardType>();

    [Header("Card Artwork")]
    [SerializeField] private CardArtTable cardArtTable;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryCardPrefab;

    private Transform inventoryCardContainer;

    private RectTransform inventoryPanel;
    private RectTransform inventoryButton;
    private Button inventoryButtonComponent;
    private CanvasGroup inventoryCanvasGroup;

    private Vector2 inventoryOpenPosition;
    private Vector2 inventoryClosedPosition;
    private Vector2 inventoryButtonClosedPosition;
    private Vector2 inventoryButtonOpenPosition;

    private bool isInventoryOpen = false;
    private bool isInventoryAnimating = false;

    // ============================
    // SHOP VARIABLES
    // ============================

    private Transform shelfCardContainer;

    private Dictionary<TarotCardType, int> cardPrices =
        new Dictionary<TarotCardType, int>()
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

    private GameObject shopCardPrefab;
    private Transform cardContainer;

    private RectTransform shopPanel;
    private RectTransform fortuneTeller;
    private Button fortuneTellerButton;
    private CanvasGroup shopCanvasGroup;
    private GameObject clickBlocker;

    private Vector2 shopOpenPosition;
    private Vector2 shopClosedPosition;
    private Vector2 tellerClosedPosition;
    private Vector2 tellerOpenPosition;

    private bool shopInitialized = false;
    private bool isShopOpen = false;
    private bool isAnimating = false;

    private const float SLIDE_DURATION = 0.5f;
    private const float TELLER_EXTRA_HEIGHT = -50f;

    // ============================
    // UNITY METHODS
    // ============================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        AddCard(TarotCardType.WheelOfFortune);
        AddCard(TarotCardType.TheMagician);
        AddCard(TarotCardType.TheHangedMan);
        AddCard(TarotCardType.WheelOfFortune);

        // Also support starting directly in the inventory scene.
        SetupInventory();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        shelfCardContainer = null;

        SetupInventory();

        if (scene.name != "Shop")
            return;

        StartCoroutine(InitializeShelf());
    }

    private IEnumerator InitializeShelf()
    {
        yield return null;

        FindShelfContainer(SceneManager.GetActiveScene());
        SetupShelfCards();
    }

    // ============================
    // INVENTORY METHODS
    // ============================

    public void SetupInventory()
    {
        Transform root = FindSceneObject("inventoryRoot");

        if (root == null)
        {
            inventoryPanel = null;
            inventoryButton = null;
            inventoryButtonComponent = null;
            inventoryCanvasGroup = null;
            inventoryCardContainer = null;
            return;
        }

        Transform panel = root.Find("InventoryPanel");
        Transform button = root.Find("inventoryBtn");

        Transform container =
            panel != null ? panel.Find("CardContainer") : null;

        if (panel == null || button == null || container == null)
        {
            Debug.LogWarning("Inventory UI objects not found!");
            return;
        }

        inventoryPanel = panel.GetComponent<RectTransform>();
        inventoryButton = button.GetComponent<RectTransform>();
        inventoryButtonComponent = button.GetComponent<Button>();
        inventoryCanvasGroup = panel.GetComponent<CanvasGroup>();
        inventoryCardContainer = container;

        if (inventoryPanel == null ||
            inventoryButton == null ||
            inventoryButtonComponent == null ||
            inventoryCanvasGroup == null)
        {
            Debug.LogWarning(
                "Missing components: " +
                "Panel RectTransform = " + (inventoryPanel != null) +
                ", Button RectTransform = " + (inventoryButton != null) +
                ", Button = " + (inventoryButtonComponent != null) +
                ", CanvasGroup = " + (inventoryCanvasGroup != null)
            );

            return;
        }

        inventoryOpenPosition = inventoryPanel.anchoredPosition
            + new Vector2(0f, 260f);
        inventoryButtonClosedPosition = inventoryButton.anchoredPosition;

        float slideDistance = inventoryPanel.rect.height + 30f;

        inventoryClosedPosition =
            inventoryOpenPosition + Vector2.down * slideDistance;

        inventoryButtonOpenPosition =
            inventoryButtonClosedPosition +
            Vector2.up * (slideDistance + TELLER_EXTRA_HEIGHT);

        inventoryPanel.anchoredPosition = inventoryClosedPosition;
        inventoryButton.anchoredPosition = inventoryButtonClosedPosition;

        inventoryCanvasGroup.alpha = 0f;
        inventoryCanvasGroup.interactable = false;
        inventoryCanvasGroup.blocksRaycasts = false;

        isInventoryOpen = false;
        isInventoryAnimating = false;

        root.SetAsLastSibling();
        button.SetAsLastSibling();

        inventoryButtonComponent.onClick.RemoveListener(ToggleInventory);
        inventoryButtonComponent.onClick.AddListener(ToggleInventory);

        Debug.Log("Inventory UI found successfully!");
    }

    public void ToggleInventory()
    {
        if (isInventoryAnimating ||
            inventoryPanel == null ||
            inventoryButton == null ||
            inventoryButtonComponent == null ||
            inventoryCanvasGroup == null)
            return;

        if (!isInventoryOpen)
        {
            GenerateInventoryCards();
        }

        StartCoroutine(AnimateInventory(!isInventoryOpen));
    }

    private IEnumerator AnimateInventory(bool opening)
    {
        isInventoryAnimating = true;
        inventoryButtonComponent.interactable = false;

        if (opening)
            inventoryCanvasGroup.alpha = 1f;

        inventoryCanvasGroup.interactable = false;
        inventoryCanvasGroup.blocksRaycasts = false;

        Vector2 panelStart = inventoryPanel.anchoredPosition;
        Vector2 buttonStart = inventoryButton.anchoredPosition;

        Vector2 panelTarget =
            opening ? inventoryOpenPosition : inventoryClosedPosition;

        Vector2 buttonTarget =
            opening ? inventoryButtonOpenPosition : inventoryButtonClosedPosition;

        float elapsed = 0f;

        while (elapsed < SLIDE_DURATION)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / SLIDE_DURATION)
            );

            inventoryPanel.anchoredPosition =
                Vector2.Lerp(panelStart, panelTarget, t);

            inventoryButton.anchoredPosition =
                Vector2.Lerp(buttonStart, buttonTarget, t);

            yield return null;
        }

        inventoryPanel.anchoredPosition = panelTarget;
        inventoryButton.anchoredPosition = buttonTarget;

        isInventoryOpen = opening;

        inventoryCanvasGroup.alpha = opening ? 1f : 0f;
        inventoryCanvasGroup.interactable = opening;
        inventoryCanvasGroup.blocksRaycasts = opening;

        inventoryButtonComponent.interactable = true;
        isInventoryAnimating = false;
    }


    public void GenerateInventoryCards()
    {
        if (inventoryCardContainer == null ||
            inventoryCardPrefab == null ||
            cardArtTable == null)
            return;

        // Clear previously generated cards.
        for (int i = inventoryCardContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = inventoryCardContainer.GetChild(i);
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // Count copies of each card.
        Dictionary<TarotCardType, int> cardCounts =
            new Dictionary<TarotCardType, int>();

        foreach (TarotCardType cardType in ownedCards)
        {
            if (!cardCounts.ContainsKey(cardType))
                cardCounts[cardType] = 0;

            cardCounts[cardType]++;
        }

        // Create one UI card for each unique card type.
        foreach (var entry in cardCounts)
        {
            GameObject card = Instantiate(
                inventoryCardPrefab,
                inventoryCardContainer,
                false
            );

            Image cardImage = card.GetComponent<Image>();

            if (cardImage != null)
            {
                cardImage.sprite =
                    cardArtTable.GetCardArt(entry.Key.GetName());

                cardImage.preserveAspect = true;
            }

            // Update the quantity label.
            Transform quantityObject = card.transform.Find("QuantityText");

            if (quantityObject != null)
            {
                TMP_Text quantityText =
                    quantityObject.GetComponent<TMP_Text>();

                if (quantityText != null)
                {
                    quantityText.text = "×" + entry.Value;
                    quantityText.gameObject.SetActive(entry.Value > 1);
                }
            }

            RectTransform rect = card.GetComponent<RectTransform>();

            if (rect != null)
                rect.localScale = Vector3.one;
        }
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
        if (!ownedCards.Remove(cardType))
        {
            Debug.LogWarning("Player does not own " + cardType);
            return false;
        }

        Debug.Log("Removed one " + cardType);
        return true;
    }

    public List<TarotCardType> GetOwnedCards()
    {
        return new List<TarotCardType>(ownedCards);
    }

    // ============================
    // SHOP PURCHASE METHODS
    // ============================

    public int GetCardPrice(TarotCardType cardType)
    {
        if (cardPrices.TryGetValue(cardType, out int price))
        {
            return price;
        }

        Debug.LogWarning("Price not found for " + cardType);
        return -1;
    }

    public bool BuyCard(TarotCardType cardType)
    {
        int price = GetCardPrice(cardType);

        if (price <= 0)
        {
            Debug.LogWarning("Invalid card price.");
            return false;
        }

        if (ChipManager.Instance == null)
        {
            Debug.LogWarning("ChipManager not found!");
            return false;
        }

        if (!ChipManager.Instance.SpendChips(price))
        {
            Debug.LogWarning("Not enough chips!");
            return false;
        }

        AddCard(cardType);

        Debug.Log(
            "Purchased " + cardType.GetName()
            + " for " + price + " chips."
        );

        return true;
    }

    // ============================
    // AUTOMATIC UI LOOKUP
    // ============================

    private Transform FindSceneObject(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] children =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == objectName)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private void LoadCardsPrefab()
    {
        if (shopCardPrefab == null)
        {
            shopCardPrefab =
                Resources.Load<GameObject>("ShopCardUI");
        }

        if (shopCardPrefab == null)
        {
            Debug.LogError(
                "ShopCardUI prefab missing from Resources!"
            );
        }
    }

    // ============================
    // NEW 3D SHOP SCENE SETUP
    // ============================

    private void FindShelfContainer(Scene scene)
    {
        GameObject container = GameObject.Find("ShelfCardContainer");

        if (container != null)
        {
            shelfCardContainer = container.transform;
            Debug.Log("ShelfCardContainer found!");
        }
        else
        {
            Debug.LogWarning("ShelfCardContainer still not found!");
        }
    }

    private void SetupShelfCards()
    {
        if (shelfCardContainer == null)
            return;

        LoadCardsPrefab();

        if (shopCardPrefab == null)
            return;

        int maxCards = 3;
        int index = 0;

        float spacingX = 110f;
        float offsetY = 50f;

        foreach (TarotCardType cardType in cardPrices.Keys)
        {
            if (!IsCardAvailable(cardType))
                continue;

            if (index >= maxCards)
                break;

            GameObject card = Instantiate(
                shopCardPrefab,
                shelfCardContainer,
                false
            );

            ShopCardUI cardUI = card.GetComponent<ShopCardUI>();
            RectTransform rect = card.GetComponent<RectTransform>();

            if (cardUI == null || rect == null)
            {
                Destroy(card);
                continue;
            }

            cardUI.Setup(cardType, GetCardPrice(cardType));

            // Apply artwork from the shared CardArtTable.
            if (cardArtTable != null)
            {
                Sprite artwork = cardArtTable.GetCardArt(
                    cardType.GetName()
                );

                if (artwork != null)
                {
                    Image cardImage = card.transform
                        .Find("CardImage")?.GetComponent<Image>();

                    if (cardImage != null)
                    {
                        cardImage.sprite = artwork;
                        cardImage.preserveAspect = true;
                    }
                }
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            float x = (index - 1) * spacingX;

            rect.anchoredPosition = new Vector2(x, offsetY);
            rect.localScale = Vector3.one * 0.4f;

            index++;
        }

        Debug.Log("Generated " + index + " shelf cards.");
    }

    // ============================
    // AVAILABLE TEST CARDS
    // ============================

    private bool IsCardAvailable(TarotCardType cardType)
    {
        switch (cardType)
        {
            case TarotCardType.WheelOfFortune:
            case TarotCardType.TheMagician:
            case TarotCardType.TheHangedMan:
            case TarotCardType.TheHighPriestess:
            case TarotCardType.TheStar:
                return true;

            default:
                return false;
        }
    }

    // ============================
    // OLD SHOP FUNCTIONS
    // ============================

    // These functions are kept for reference.
    // They are not called by the new Shop scene.

    private void CreateClickBlocker(Transform shopRoot)
    {
        Transform existing =
            shopRoot.Find("ShopClickBlocker");

        if (existing != null)
        {
            clickBlocker = existing.gameObject;
        }
        else
        {
            clickBlocker = new GameObject(
                "ShopClickBlocker",
                typeof(RectTransform),
                typeof(Image)
            );

            clickBlocker.transform.SetParent(shopRoot, false);

            RectTransform rect =
                clickBlocker.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image =
                clickBlocker.GetComponent<Image>();

            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = true;
        }

        clickBlocker.transform.SetAsFirstSibling();
        clickBlocker.SetActive(false);
    }

    private void SetupShop()
    {
        if (shopInitialized)
            return;

        LoadCardsPrefab();

        if (shopCardPrefab == null || cardContainer == null)
        {
            Debug.LogWarning("Shop setup failed!");
            return;
        }

        foreach (TarotCardType cardType in cardPrices.Keys)
        {
            if (!IsCardAvailable(cardType))
                continue;

            GameObject cardUI =
                Instantiate(shopCardPrefab, cardContainer);

            ShopCardUI cardComponent =
                cardUI.GetComponent<ShopCardUI>();

            if (cardComponent != null)
            {
                cardComponent.Setup(
                    cardType,
                    GetCardPrice(cardType)
                );
            }
            else
            {
                Debug.LogWarning(
                    "ShopCardUI script missing from prefab!"
                );

                Destroy(cardUI);
            }
        }

        shopInitialized = true;
    }

    public void ToggleShop()
    {
        if (isAnimating || shopPanel == null)
            return;

        if (!isShopOpen)
        {
            SetupShop();

            if (!shopInitialized)
                return;
        }

        StartCoroutine(AnimateShop(!isShopOpen));
    }

    private IEnumerator AnimateShop(bool opening)
    {
        isAnimating = true;
        fortuneTellerButton.interactable = false;

        if (opening)
        {
            clickBlocker.SetActive(true);
            shopCanvasGroup.alpha = 1f;
        }

        shopCanvasGroup.interactable = false;
        shopCanvasGroup.blocksRaycasts = false;

        Vector2 panelStart = shopPanel.anchoredPosition;
        Vector2 tellerStart = fortuneTeller.anchoredPosition;

        Vector2 panelTarget =
            opening ? shopOpenPosition : shopClosedPosition;

        Vector2 tellerTarget =
            opening ? tellerOpenPosition : tellerClosedPosition;

        float elapsed = 0f;

        while (elapsed < SLIDE_DURATION)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / SLIDE_DURATION);
            t = Mathf.SmoothStep(0f, 1f, t);

            shopPanel.anchoredPosition =
                Vector2.Lerp(panelStart, panelTarget, t);

            fortuneTeller.anchoredPosition =
                Vector2.Lerp(tellerStart, tellerTarget, t);

            yield return null;
        }

        shopPanel.anchoredPosition = panelTarget;
        fortuneTeller.anchoredPosition = tellerTarget;

        isShopOpen = opening;

        shopCanvasGroup.alpha = opening ? 1f : 0f;
        shopCanvasGroup.interactable = opening;
        shopCanvasGroup.blocksRaycasts = opening;

        clickBlocker.SetActive(opening);

        fortuneTellerButton.interactable = true;
        isAnimating = false;
    }
}
