using System;

public enum CardKind
{
    Standard,
    Tarot
}

[Serializable]
public struct GameStateData
{
    public CardData[] cards;
    public CardInteractionData[] cardInteractions;
    public CardZoneData[] zones;
    public SequenceState sequence;
    public GameData game;
    public HandStartData handStart;
}
