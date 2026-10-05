using System;
using UnityEngine;

[Serializable]
public struct CardInteractionData {
    public bool hovered;
    public bool pressed;
    public bool holdTriggered;
    public float pressedTime;
}
