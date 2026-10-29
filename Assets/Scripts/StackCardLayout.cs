using UnityEngine;

public class StackCardLayout : CardLayout
{
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 0.01f, 0f);

    public override void Layout(
        int[] cardIds,
        int cardCount,
        AbstractCardVisual[] visuals,
        Transform anchor)
    {
        for (int i = 0; i < cardCount; i++)
        {
            AbstractCardVisual visual = visuals[cardIds[i]];
            if (visual == null)
                continue;

            visual.transform.SetParent(anchor, false);
            visual.transform.localPosition = cardOffset * i;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.SetSiblingIndex(i);
            visual.gameObject.SetActive(true);
        }
    }

    public override void Enable(bool enable)
    {
        gameObject.SetActive(enable);
    }
}
