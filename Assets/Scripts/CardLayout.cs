using UnityEngine;
using System.Collections.Generic;

public abstract class CardLayout : MonoBehaviour
{
    public abstract void Layout(
        int[] cardIds,
        int cardCount,
        AbstractCardVisual[] visuals,
        Transform anchor);

    public abstract void Enable(bool enable);
}
