using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public int healAmount = 50;
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        // Don't consume the pickup if already full health
        if (player.IsFullHealth())
            return;

        player.Heal(healAmount);

        if (pickupSound != null)
            PlaySound(pickupSound);

        Destroy(gameObject);
    }
    // 🎧 CREATE TEMP AUDIO OBJECT
    void PlaySound(AudioClip clip)
    {
        GameObject audioObj = new GameObject("Temp_Audio");

        // Set position
        audioObj.transform.position = transform.position;

        // Add AudioSource
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.playOnAwake = false;
        source.spatialBlend = 1f; // 3D sound (set 0 for UI sound)
        source.volume = 1f;

        source.Play();

        // Destroy after clip ends
        Destroy(audioObj, clip.length);
    }
}