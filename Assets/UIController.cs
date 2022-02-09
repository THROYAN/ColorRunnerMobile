using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Player player;
    public Text levelText;
    public string levelFormat = "Level: {0}";

    void OnValidate()
    {
        if (player == null) {
            player = FindObjectOfType<Player>();
        }
    }

    void Update()
    {
        if (levelText == null) {
            return;
        }

        levelText.text = System.String.Format(levelFormat, player.CurrentLevel);
    }
}
