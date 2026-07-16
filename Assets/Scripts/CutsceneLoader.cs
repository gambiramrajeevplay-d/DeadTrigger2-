using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneLoader : MonoBehaviour
{
    [Header("Cutscene")]
    [Tooltip("Seconds before loading the gameplay scene")]
    public float cutsceneDuration = 7f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(cutsceneDuration);

        string nextScene = PlayerPrefs.GetString("NextLevelScene", "");

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogError("NextLevelScene was not found.");
        }
    }
}