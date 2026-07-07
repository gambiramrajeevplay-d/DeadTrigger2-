using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Temp : MonoBehaviour
{
    public static GameManager_Temp Instance;

    [Header("UI")]
    public GameObject finishPanel;

    private List<Enemy> aliveZombies = new List<Enemy>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (finishPanel != null)
            finishPanel.SetActive(false);

        aliveZombies.Clear();
        aliveZombies.AddRange(FindObjectsOfType<Enemy>());

        Debug.Log("Zombies Found: " + aliveZombies.Count);
    }

    public void ZombieDied(Enemy enemy)
    {
        Debug.Log(enemy.name + " died");

        if (aliveZombies.Contains(enemy))
            aliveZombies.Remove(enemy);

        Debug.Log("Remaining: " + aliveZombies.Count);

        if (aliveZombies.Count == 0)
        {
            Debug.Log("SHOW PANEL");

            if (finishPanel != null)
                finishPanel.SetActive(true);
        }
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ShowFinishPanel()
    {
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

}