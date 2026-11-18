
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ItemsManager : MonoBehaviour
{
    public static ItemsManager Instance { get; private set; }

    // ============================
    // INVENTORY VARIABLES
    // ============================

    private List<TarotCardType> ownedCards = new List<TarotCardType>();

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

        if (scene.name != "Shop")
            return;

        StartCoroutine(InitializeShelf());
    }

    private IEnumerator InitializeShelf()
    {
        yield return null; // Wait one frame

        FindShelfContainer(SceneManager.GetActiveScene());
        SetupShelfCards();
    }

    // ============================
    // INVENTORY METHODS
    // ============================

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

        int index = 0;
        const float spacing = 270f;

        foreach (TarotCardType cardType in cardPrices.Keys)
        {
            if (!IsCardAvailable(cardType))
                continue;

            GameObject card = Instantiate(
                shopCardPrefab,
                shelfCardContainer,
                false
            );

            ShopCardUI cardUI =
                card.GetComponent<ShopCardUI>();

            RectTransform rect =
                card.GetComponent<RectTransform>();

            if (cardUI == null || rect == null)
            {
                Debug.LogWarning(
                    "ShopCardUI prefab needs ShopCardUI " +
                    "and RectTransform components."
                );

                Destroy(card);
                continue;
            }

            cardUI.Setup(cardType, GetCardPrice(cardType));

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(index * spacing, 0f);

            rect.localScale = Vector3.one;

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
