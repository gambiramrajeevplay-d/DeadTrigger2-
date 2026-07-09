using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlashUI : MonoBehaviour
{
    public float fadeDuration = 2f;

    private Image[] damageImages;
    private Coroutine currentRoutine;

    void Start()
    {
        damageImages = GetComponentsInChildren<Image>(true);

        foreach (Image img in damageImages)
        {
            img.gameObject.SetActive(false);
        }
    }

    public void ShowDamage()
    {
        if (damageImages == null || damageImages.Length == 0)
            return;

        foreach (Image img in damageImages)
        {
            img.gameObject.SetActive(true);
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // Start invisible
        foreach (Image img in damageImages)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        float fadeInTime = 0.15f;
        float t = 0f;

        // Fade In
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);

            foreach (Image img in damageImages)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // Fade Out
        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            foreach (Image img in damageImages)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }

            yield return null;
        }

        foreach (Image img in damageImages)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
            img.gameObject.SetActive(false);
        }
    }
}