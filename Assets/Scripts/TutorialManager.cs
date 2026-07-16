using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial UI")]
    public GameObject mobileUI;
    public GameObject tvUI;

    [SerializeField] private float tutorialDuration = 5f;

    private void Awake()
    {
        Instance = this;

        // Make sure both are hidden initially
        if (mobileUI != null)
            mobileUI.SetActive(false);

        if (tvUI != null)
            tvUI.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(ShowTutorialRoutine());
    }

    IEnumerator ShowTutorialRoutine()
    {
        Pauser.LockPause();

        if (PlatformManager.Instance.IsTV())
        {
            tvUI.SetActive(true);
            mobileUI.SetActive(false);
        }
        else
        {
            mobileUI.SetActive(true);
            tvUI.SetActive(false);
        }

        yield return new WaitForSecondsRealtime(tutorialDuration);

        HideTutorial();
    }

    public void ShowTutorial()
    {
        Pauser.LockPause();

        if (PlatformManager.Instance.IsTV())
        {
            tvUI.SetActive(true);
            mobileUI.SetActive(false);
        }
        else
        {
            mobileUI.SetActive(true);
            tvUI.SetActive(false);
        }
    }

    public void HideTutorial()
    {
        if (mobileUI != null)
            mobileUI.SetActive(false);

        if (tvUI != null)
            tvUI.SetActive(false);

        Pauser.UnlockPause();
    }

    public bool IsTutorialOpen()
    {
        return (mobileUI != null && mobileUI.activeSelf) ||
               (tvUI != null && tvUI.activeSelf);
    }
}