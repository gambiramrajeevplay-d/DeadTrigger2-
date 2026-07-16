using Script;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Current Level Root")]
    public GameObject currentLevel;

    [Header("Level Settings")]
    public int currentLevelIndex = 1;

    [Header("Result Audio")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private bool gameEnded = false;

    [Header("Game Start")]
    public float startDelay = 2f;

    private List<Enemy> aliveZombies = new List<Enemy>();

    [Header("Kill Count UI")]
    public TMP_Text winKillText;
    public TMP_Text loseKillText;

    private int totalZombies;
    private int killedZombies;

    [Header("Result Delay")]
    public float resultDelay = 1f;

    [Header("Result Camera")]
    public Camera resultCamera;

    [Header("Reward UI")]
    public TMP_Text winRewardText;
    public TMP_Text loseRewardText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
  
    void Start()
    {
        if (resultCamera != null)
            resultCamera.gameObject.SetActive(false);

        // 🔥 AUTO FIND UI

        if (winPanel == null)
        {
            GameObject passObj =
                GameObject.FindGameObjectWithTag("Pass");

            if (passObj != null)
                winPanel = passObj;
        }

        if (losePanel == null)
        {
            GameObject failObj =
                GameObject.FindGameObjectWithTag("Fail");

            if (failObj != null)
                losePanel = failObj;
        }

        if (currentLevel == null)
        {
            GameObject levelObj =
                GameObject.FindGameObjectWithTag("Level");

            if (levelObj != null)
                currentLevel = levelObj;
        }

        // 🔥 RESET UI

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        AudioListener.pause = true;
        Time.timeScale = 0f;
        Pauser.LockPause();

       
        aliveZombies.Clear();
        aliveZombies.AddRange(FindObjectsOfType<Enemy>());

        totalZombies = aliveZombies.Count;
        killedZombies = 0;

        UpdateKillUI();

        Debug.Log("Zombies Found: " + totalZombies);

        StartCoroutine(StartGameRoutine());

    }
    public void StartGame()
    {
        Time.timeScale = 1f;

        AudioListener.pause = false;

        PlayerAutoMove player =
      FindObjectOfType<PlayerAutoMove>();

        if (player != null)
        {
            player.StartGameplay();
        }

        // 🔥 ENABLE PAUSE
        Pauser.UnlockPause();
    }
    // =========================
    // WIN
    // =========================
    public void OnWin()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        StartCoroutine(WinRoutine());
    }

    IEnumerator WinRoutine()
    {
        Pauser.LockPause();

        // Wait before showing result
        yield return new WaitForSeconds(resultDelay);

        // No reward in tutorial
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            const int reward = 100;

            CurrecnyManager.instance?.AddCurrency(reward);
            UpdateRewardUI(reward);

            Debug.Log(reward + " Coins Rewarded");
        }
        else
        {
            UpdateRewardUI(0);

            Debug.Log("Tutorial completed - no coins rewarded");
        }

        UnlockNextLevel();

        EnableResultCamera();

        StopAllGameAudio();
        PlayResultSound(winClip);

        if (winPanel != null)
            winPanel.SetActive(true);

        DisableLevel();

        Time.timeScale = 0f;
    }
    // =========================
    // LOSE
    // =========================
    public void OnPlayerDied()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        StartCoroutine(LoseRoutine());
    }

    IEnumerator LoseRoutine()
    {
        Pauser.LockPause();

        // Wait before showing result
        yield return new WaitForSeconds(resultDelay);

        StopAllGameAudio();
        PlayResultSound(loseClip);

        UpdateRewardUI(0);

        EnableResultCamera();


        if (losePanel != null)
            losePanel.SetActive(true);

        DisableLevel();

        Time.timeScale = 0f;
    }

    // =========================
    // STOP ALL GAME AUDIO
    // =========================
    void StopAllGameAudio()
    {
        AudioSource[] allAudio =
            FindObjectsOfType<AudioSource>();

        foreach (AudioSource audioSource in allAudio)
        {
            audioSource.Stop();
        }
    }

    // =========================
    // PLAY RESULT SOUND
    // =========================
    void PlayResultSound(AudioClip clip)
    {
        if (clip == null)
            return;

        GameObject audioObj =
            new GameObject("ResultAudio");

        AudioSource source =
            audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.playOnAwake = false;

        // important while paused
        source.ignoreListenerPause = true;

        source.Play();

        Destroy(audioObj, clip.length);
    }

    // =========================
    // UNLOCK NEXT LEVEL
    // =========================
    void UnlockNextLevel()
    {
        // ❌ DO NOT UNLOCK FROM TUTORIAL
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            Debug.Log("Tutorial completed - next level NOT unlocked");
            return;
        }

        int unlockedLevel =
            PlayerPrefs.GetInt(StringsData.playerLevel, 1);

        if (currentLevelIndex >= unlockedLevel)
        {
            PlayerPrefs.SetInt(
                StringsData.playerLevel,
                currentLevelIndex + 1
            );

            PlayerPrefs.Save();

            Debug.Log("Unlocked Level: " + (currentLevelIndex + 1));
        }
    }

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        StartGame();
    }
    // =========================
    // DISABLE CURRENT LEVEL
    // =========================
    void DisableLevel()
    {
        if (currentLevel != null)
        {
            currentLevel.SetActive(false);
        }
    }

    // =========================
    // RESTART
    // =========================
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================
    // HOME
    // =========================
    public void GoHome()
    {
        Time.timeScale = 1f;

        // 🔥 SHOW SUBSCRIPTION AFTER GAMEPLAY
        PlayerPrefs.SetInt("ShowSubscriptionPanel", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(0);
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void ZombieDied(Enemy enemy)
    {
        if (enemy == null)
            return;

        if (aliveZombies.Remove(enemy))
        {
            killedZombies++;
            UpdateKillUI();
        }

        Debug.Log("Remaining Zombies: " + aliveZombies.Count);

        if (aliveZombies.Count == 0)
        {
            Debug.Log("All Zombies Eliminated");
            OnWin();
        }
    }
    void UpdateKillUI()
    {
        string text = $"Kills : {killedZombies}/{totalZombies}";

        if (winKillText != null)
            winKillText.text = text;

        if (loseKillText != null)
            loseKillText.text = text;
    }
    void EnableResultCamera()
    {
        if (resultCamera != null)
            resultCamera.gameObject.SetActive(true);
    }
    void UpdateRewardUI(int reward)
    {
        string rewardText = reward.ToString();

        if (winRewardText != null)
            winRewardText.text = rewardText;

        if (loseRewardText != null)
            loseRewardText.text = rewardText;
    }
}