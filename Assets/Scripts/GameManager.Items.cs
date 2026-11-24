using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class GameManager
{
    public const int COWBOY_HAT_BONUS_PER_ACTION = 5;
    public const int CIGAR_KING_MULTIPLIER = 2;
    public const int CLOVER_CLUB_BONUS = 4;
    public const int RABBIT_FOOT_PAIR_BONUS = 10;
    public const int SUNGLASSES_REVEALED_BONUS = 3;
    public const int GABAGOOL_MIN_VALUE = 8;
    public const int GOLDEN_TOOTH_CHIP_PERCENT = 25;
    public const int TUXEDO_CHIP_BONUS = 100;
    public const int WEIGHTED_DICE_EXTRA_HANDS = 1;
    public const int CIGARETTES_EXTRA_ACTIONS = 1;

    private const int CARDS_PER_SUIT = 13;
    private const int KING_RANK = 13;
    private const int SCORE_PHASE_COUNT = 4;

    private struct ScoredCard {
        public int cardId;
        public CardSuit suit;
        public int rank;
        public int baseValue;
        public int value;
    }

    private string chipBreakdownText = "";

    private List<ItemType> GetOwnedItemsSorted() {
        List<ItemType> items = ItemsManager.Instance != null
            ? ItemsManager.Instance.GetOwnedItems()
            : new List<ItemType>();
        items.Sort();
        return items;
    }

    private int GetStartingHands() {
        int hands = startingHands;
        foreach (ItemType item in GetOwnedItemsSorted())
            hands += GetItemExtraHands(item);
        return hands;
    }

    private int GetStartingActions() {
        int actions = startingActions;
        foreach (ItemType item in GetOwnedItemsSorted())
            actions += GetItemExtraActions(item);
        return actions;
    }

    private int GetItemExtraHands(ItemType item) {
        switch (item) {
            case ItemType.WeightedDice:
                return WEIGHTED_DICE_EXTRA_HANDS;

            default:
                return 0;
        }
    }

    private int GetItemExtraActions(ItemType item) {
        switch (item) {
            case ItemType.PackOfCigarettes:
                return CIGARETTES_EXTRA_ACTIONS;

            default:
                return 0;
        }
    }

    private int GetItemChipBonus(ItemType item, int baseReward) {
        switch (item) {
            case ItemType.GoldenTooth:
                return Mathf.RoundToInt(baseReward * GOLDEN_TOOTH_CHIP_PERCENT / 100f);

            case ItemType.Tuxedo:
                return TUXEDO_CHIP_BONUS;

            default:
                return 0;
        }
    }

    private int CalculateChipReward(out string breakdown) {
        int baseReward = TransitionData.GetChipReward();
        int total = baseReward;
        StringBuilder itemLines = new StringBuilder();

        foreach (ItemType item in GetOwnedItemsSorted()) {
            int bonus = GetItemChipBonus(item, baseReward);
            if (bonus == 0) continue;

            total += bonus;
            itemLines.AppendLine($"{item.GetName()}: +{bonus}");
        }

        breakdown = itemLines.Length == 0 ? "" : "\n" + itemLines.ToString().TrimEnd();
        return total;
    }

    private int GetItemScorePhase(ItemType item) {
        switch (item) {
            case ItemType.Cigar:
                return 1;

            case ItemType.FourLeafClover:
                return 2;

            case ItemType.Gabagool:
            case ItemType.BrokenChip:
                return 3;

            case ItemType.RabbitFoot:
            case ItemType.Sunglasses:
            case ItemType.CowboyHat:
                return 4;

            default:
                return 0;
        }
    }

    private ScoredCard[] BuildScoredCards(CardZone zone) {
        CardZoneData zoneData = state.zones[(int)zone];
        ScoredCard[] cards = new ScoredCard[zoneData.count];

        for (int i = 0; i < zoneData.count; i++) {
            int cardId = zoneData.cardIds[i];
            CardData card = state.cards[cardId];
            cards[i] = new ScoredCard {
                cardId = cardId,
                suit = card.suit,
                rank = cardId % CARDS_PER_SUIT + 1,
                baseValue = card.value,
                value = card.value
            };
        }

        return cards;
    }

    private static int SumCardValues(ScoredCard[] cards) {
        int sum = 0;
        for (int i = 0; i < cards.Length; i++)
            sum += cards[i].value;
        return sum;
    }

    private static int FindLowestCard(ScoredCard[] cards) {
        int index = -1;
        for (int i = 0; i < cards.Length; i++) {
            if (index < 0 || cards[i].value < cards[index].value)
                index = i;
        }

        return index;
    }

    private static int FindHighestCard(ScoredCard[] cards, int skipIndex) {
        int index = -1;
        for (int i = 0; i < cards.Length; i++) {
            if (i == skipIndex) continue;
            if (index < 0 || cards[i].value > cards[index].value)
                index = i;
        }

        return index;
    }

    private static int CountRepeatedValues(ScoredCard[] cards) {
        int groups = 0;

        for (int i = 0; i < cards.Length; i++) {
            bool seenBefore = false;
            int matches = 1;

            for (int j = 0; j < cards.Length; j++) {
                if (j == i || cards[j].baseValue != cards[i].baseValue) continue;

                if (j < i) {
                    seenBefore = true;
                    break;
                }

                matches++;
            }

            if (!seenBefore && matches >= 2)
                groups++;
        }

        return groups;
    }

    private int CountRevealedCardsInHand(ScoredCard[] cards) {
        int[] revealed = state.handStart.playerSelectedCards;
        if (revealed == null) return 0;

        int count = 0;
        for (int i = 0; i < state.handStart.playerSelectedCount && i < revealed.Length; i++) {
            for (int j = 0; j < cards.Length; j++) {
                if (cards[j].cardId != revealed[i]) continue;

                count++;
                break;
            }
        }

        return count;
    }

    private int ApplyScoreItem(ItemType item, ScoredCard[] cards) {
        switch (item) {
            case ItemType.CowboyHat:
                return Mathf.Max(0, state.game.actionsRemaining) * COWBOY_HAT_BONUS_PER_ACTION;

            case ItemType.Cigar:
                for (int i = 0; i < cards.Length; i++) {
                    if (cards[i].rank == KING_RANK)
                        cards[i].value *= CIGAR_KING_MULTIPLIER;
                }

                return 0;

            case ItemType.FourLeafClover:
                for (int i = 0; i < cards.Length; i++) {
                    if (cards[i].suit == CardSuit.Clubs)
                        cards[i].value += CLOVER_CLUB_BONUS;
                }

                return 0;

            case ItemType.RabbitFoot:
                return CountRepeatedValues(cards) * RABBIT_FOOT_PAIR_BONUS;

            case ItemType.Sunglasses:
                return CountRevealedCardsInHand(cards) * SUNGLASSES_REVEALED_BONUS;

            case ItemType.Gabagool: {
                int lowest = FindLowestCard(cards);
                if (lowest >= 0 && cards[lowest].value < GABAGOOL_MIN_VALUE)
                    cards[lowest].value = GABAGOOL_MIN_VALUE;

                return 0;
            }

            case ItemType.BrokenChip: {
                if (cards.Length < 2) return 0;

                int lowest = FindLowestCard(cards);
                int highest = FindHighestCard(cards, lowest);
                cards[lowest].value = 0;
                cards[highest].value *= 2;

                return 0;
            }

            default:
                return 0;
        }
    }

    private static string FormatSigned(int amount) {
        return amount >= 0 ? $"+{amount}" : amount.ToString();
    }

    private int CalculateHandScore(out string breakdown) {
        int cardPoints = CalculateZonePoints(CardZone.PLAYER);
        int total = cardPoints;
        ScoredCard[] cards = BuildScoredCards(CardZone.PLAYER);
        List<ItemType> ownedItems = GetOwnedItemsSorted();
        StringBuilder itemLines = new StringBuilder();

        for (int phase = 1; phase <= SCORE_PHASE_COUNT; phase++) {
            foreach (ItemType item in ownedItems) {
                if (GetItemScorePhase(item) != phase) continue;

                int valuesBefore = SumCardValues(cards);
                int flatBonus = ApplyScoreItem(item, cards);
                int change = SumCardValues(cards) - valuesBefore + flatBonus;
                if (change == 0) continue;

                total += change;
                itemLines.AppendLine($"{item.GetName()}: {FormatSigned(change)}");
            }
        }

        breakdown = itemLines.Length == 0
            ? $"Hand Points: +{cardPoints}\n"
            : $"Hand Points: +{cardPoints}\n{itemLines}Total: {FormatSigned(total)}";

        return total;
    }
}
