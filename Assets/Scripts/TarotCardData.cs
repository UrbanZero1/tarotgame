using System;
using UnityEngine;

[Serializable]
public struct TarotCardData {
    public int cardId;
    public TarotCardType type;
}

public enum TarotCardType {
    WheelOfFortune,
    TheMagician,
    Death,
    Justice,
    TheChariot,
    TheHangedMan,
    TheHermit,
    TheMoon,
    TheStar,
    TheFool,
    TheWorld,
    TheDevil,
    TheHighPriestess,
    Judgement,
    TheSun
}

public static class TarotCardTypeExtensions {
    public static string GetName(this TarotCardType cardType) {
        switch (cardType) {
            case TarotCardType.WheelOfFortune:
                return "Wheel of Fortune";

            case TarotCardType.TheMagician:
                return "The Magician";

            case TarotCardType.Death:
                return "Death";

            case TarotCardType.Justice:
                return "Justice";

            case TarotCardType.TheChariot:
                return "The Chariot";

            case TarotCardType.TheHangedMan:
                return "The Hanged Man";

            case TarotCardType.TheHermit:
                return "The Hermit";

            case TarotCardType.TheMoon:
                return "The Moon";

            case TarotCardType.TheStar:
                return "The Star";

            case TarotCardType.TheFool:
                return "The Fool";

            case TarotCardType.TheWorld:
                return "The World";

            case TarotCardType.TheDevil:
                return "The Devil";

            case TarotCardType.TheHighPriestess:
                return "The High Priestess";

            case TarotCardType.Judgement:
                return "Judgement";

            case TarotCardType.TheSun:
                return "The Sun";

            default:
                return "Unknown Card";
        }
    }

    public static string GetDescription(this TarotCardType cardType) {
        switch (cardType) {
            case TarotCardType.WheelOfFortune:
                return "Generate a random number from 1-13. All face-down cards' values become that number.";

            case TarotCardType.TheMagician:
                return "Declare a suit, then reveal the top card of your deck. If its suit matches the declared suit, discard it and add its value to your total score. If its suit is different, subtract its value from your total score.";

            case TarotCardType.Death:
                return "Discard 2 face-down cards. For the rest of the game, you must play with only 3 face-down cards, then double your total score for the final round. This card can only be played on round 1.";

            case TarotCardType.Justice:
                return "Choose 1 face-down card. Its score value increases by the current action number multiplied by 3. For example, on your 2nd action, the card gains +6.";

            case TarotCardType.TheChariot:
                return "Take the top card of the discard pile and swap it with any card in your hand.";

            case TarotCardType.TheHangedMan:
                return "Look at your entire hand, then immediately discard 1 card and draw a new one.";

            case TarotCardType.TheHermit:
                return "Flip 1 of your cards face-up. For the rest of the game, any cards placed in that zone remain face-up.";

            case TarotCardType.TheMoon:
                return "Discard 2 face-down cards. Then draw 2 new cards and put them in your hand without looking at them.";

            case TarotCardType.TheStar:
                return "Look at the top 3 cards of the deck, then put them back in any order you choose.";

            case TarotCardType.TheFool:
                return "Discard your entire 5-card lineup. Draw 5 brand-new cards and immediately reveal 2 to memorise. This restarts steps 1 and 2.";

            case TarotCardType.TheWorld:
                return "When you reveal your hand at the end of a round, if you have a sequence of 3 cards in numerical order, such as 4, 5, and 6, add +20 points to your total score.";

            case TarotCardType.TheDevil:
                return "Draw 3 cards from the deck. You must swap 2 of them into your hand, even if they have terrible values. Discard the remaining card.";

            case TarotCardType.TheHighPriestess:
                return "Look at any 2 face-down cards in your hand.";

            case TarotCardType.Judgement:
                return "Reveal all cards in your hand. If none of them are picture cards, the card with the lowest value doubles its value. If you have a picture card, your picture card with the lowest value loses an amount equal to the lowest-value card in your hand.";

            case TarotCardType.TheSun:
                return "If each pair of scores at the end of consecutive rounds differs by at least 20 points, and you are within 20 points of the threshold at the end of the game, you win anyway.";

            default:
                return "No description available.";
        }
    }
}