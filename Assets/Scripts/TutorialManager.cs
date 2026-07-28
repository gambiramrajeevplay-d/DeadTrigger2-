using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial UI")]
    public GameObject mobileUI;
    public GameObject tvUI;

    private void Awake()
    {
        Instance = this;

        if (mobileUI != null)
            mobileUI.SetActive(false);

        if (tvUI != null)
            tvUI.SetActive(false);
    }

    private void Start()
    {
        ShowTutorial();
    }

    public void ShowTutorial()
    {
        Pauser.LockPause();

        if (PlatformManager.Instance.IsTV())
        {
            if (tvUI != null)
                tvUI.SetActive(true);

            if (mobileUI != null)
                mobileUI.SetActive(false);
        }
        else
        {
            if (mobileUI != null)
                mobileUI.SetActive(true);

            if (tvUI != null)
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