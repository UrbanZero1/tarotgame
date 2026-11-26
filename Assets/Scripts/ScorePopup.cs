using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float riseDistance = 0.5f;

    private TMP_Text scoreText;
    private Vector3 startPosition;
    private Color startColor;
    private float timer;

    private void Start()
    {
        scoreText = GetComponent<TMP_Text>();
        startPosition = transform.position;
        startColor = scoreText.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / duration);

        transform.position = startPosition +
            Vector3.forward * riseDistance * progress;

        Color color = startColor;
        color.a *= 1f - progress;
        scoreText.color = color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}