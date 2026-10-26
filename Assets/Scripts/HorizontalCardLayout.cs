using UnityEngine;

public class HorizontalCardLayout : CardLayout
{
    [Header("Spacing")]
    [SerializeField] private float cardSpacing = 1.2f;

    public override void Layout(int[] cardIds, int cardCount, AbstractCardVisual[] visuals) {
        float half = (cardCount - 1) * 0.5f;

        for (int i = 0; i < cardCount; i++) {
            int cardId = cardIds[i];
            float offset = i - half;

            visuals[cardId].transform.localPosition =
                new Vector3(offset * cardSpacing, 0f, 0f);
        }
    }
    
    public override void Enable(bool enable) {
        gameObject.SetActive(enable);
    }
}
