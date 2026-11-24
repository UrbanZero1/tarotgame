public enum ItemType {
    CowboyHat,
    Cigar,
    FourLeafClover,
    RabbitFoot,
    Sunglasses,
    Gabagool,
    BrokenChip,
    GoldenTooth,
    Tuxedo,
    WeightedDice,
    PackOfCigarettes
}

public static class ItemTypeExtensions {
    public static string GetName(this ItemType itemType) {
        switch (itemType) {
            case ItemType.CowboyHat:
                return "Cowboy Hat";

            case ItemType.Cigar:
                return "Cigar";

            case ItemType.FourLeafClover:
                return "Four Leaf Clover";

            case ItemType.RabbitFoot:
                return "Rabbit Foot";

            case ItemType.Sunglasses:
                return "Sunglasses";

            case ItemType.Gabagool:
                return "Gabagool";

            case ItemType.BrokenChip:
                return "Broken Chip";

            case ItemType.GoldenTooth:
                return "Golden Tooth";

            case ItemType.Tuxedo:
                return "Tuxedo";

            case ItemType.WeightedDice:
                return "Weighted Dice";

            case ItemType.PackOfCigarettes:
                return "Pack of Cigarettes";

            default:
                return "Unknown Item";
        }
    }

    public static string GetDescription(this ItemType itemType) {
        switch (itemType) {
            case ItemType.CowboyHat:
                return $"Quick Draw: when you pass with actions left, score +{GameManager.COWBOY_HAT_BONUS_PER_ACTION} for each unused action.";

            case ItemType.Cigar:
                return $"Big Shot: Kings in your hand score x{GameManager.CIGAR_KING_MULTIPLIER}.";

            case ItemType.FourLeafClover:
                return $"Lucky Clubs: every Club in your hand scores +{GameManager.CLOVER_CLUB_BONUS}.";

            case ItemType.RabbitFoot:
                return $"Lucky Pair: +{GameManager.RABBIT_FOOT_PAIR_BONUS} for each value that appears at least twice in your hand.";

            case ItemType.Sunglasses:
                return $"Poker Face: the cards you revealed at the start of the hand score +{GameManager.SUNGLASSES_REVEALED_BONUS} each while they are still in your hand.";

            case ItemType.Gabagool:
                return $"Fuhgeddaboutit: your lowest card is worth at least {GameManager.GABAGOOL_MIN_VALUE}.";

            case ItemType.BrokenChip:
                return "Cracked: your lowest card scores 0, but your highest card scores double.";

            case ItemType.GoldenTooth:
                return $"Gold Grin: winning pays {GameManager.GOLDEN_TOOTH_CHIP_PERCENT}% more chips.";

            case ItemType.Tuxedo:
                return $"High Roller: winning pays {GameManager.TUXEDO_CHIP_BONUS} extra chips.";

            case ItemType.WeightedDice:
                return $"Another Roll: +{GameManager.WEIGHTED_DICE_EXTRA_HANDS} hand.";

            case ItemType.PackOfCigarettes:
                return $"Smoke Break: +{GameManager.CIGARETTES_EXTRA_ACTIONS} action on every hand.";

            default:
                return "No description available.";
        }
    }

    public static int GetPrice(this ItemType itemType) {
        switch (itemType) {
            case ItemType.CowboyHat:
                return 300;

            case ItemType.Cigar:
                return 350;

            case ItemType.FourLeafClover:
                return 250;

            case ItemType.RabbitFoot:
                return 350;

            case ItemType.Sunglasses:
                return 400;

            case ItemType.Gabagool:
                return 450;

            case ItemType.BrokenChip:
                return 500;

            case ItemType.GoldenTooth:
                return 350;

            case ItemType.Tuxedo:
                return 350;

            case ItemType.WeightedDice:
                return 600;

            case ItemType.PackOfCigarettes:
                return 450;

            default:
                return -1;
        }
    }
}
