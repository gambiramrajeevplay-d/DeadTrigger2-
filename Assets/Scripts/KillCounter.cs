using TMPro;
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance;

    private TMP_Text killText;

    private int totalEnemies;
    private int killedEnemies;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameObject textObj = GameObject.FindGameObjectWithTag("KillCountText");

        if (textObj != null)
        {
            killText = textObj.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("No GameObject with tag 'KillCountText' found!");
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        totalEnemies = enemies.Length;
        killedEnemies = 0;

        UpdateUI();
    }

    public void EnemyKilled()
    {
        killedEnemies++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (killText != null)
            killText.text = $"{killedEnemies}/{totalEnemies}";
    }
}