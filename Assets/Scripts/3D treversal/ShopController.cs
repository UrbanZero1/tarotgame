using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    private const string TABLE_SELECT_SCENE = "TableSelect";

    private void Start()
    {
        RuntimeUI.EnsureEventSystem();

        Canvas canvas = RuntimeUI.CreateCanvas("ShopCanvas", 100);
        canvas.transform.SetParent(transform, false);

        Button nextTableButton = RuntimeUI.CreateButton(canvas.transform, "NextTable", "Next Table", new Vector2(0f, -470f), new Vector2(420f, 90f), new Color(0.2f, 0.45f, 0.75f, 1f), 38f);
        nextTableButton.onClick.AddListener(() => SceneManager.LoadScene(TABLE_SELECT_SCENE));
    }
}
