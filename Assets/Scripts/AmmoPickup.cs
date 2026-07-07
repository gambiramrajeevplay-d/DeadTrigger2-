using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 10;
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Works even if the collider is on a child object
        PlayerAutoMove player = other.GetComponentInParent<PlayerAutoMove>();

        if (player != null)
        {
            player.AddAmmo(ammoAmount);

            // Play pickup sound before destroying the pickup
            if (pickupSound != null)
                PlaySound(pickupSound);

            Destroy(gameObject);
        }
    }

    void PlaySound(AudioClip clip)
    {
        GameObject audioObj = new GameObject("Temp_Audio");

        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.playOnAwake = false;
        source.spatialBlend = 1f;   // 3D sound (set to 0 for UI sound)
        source.volume = 1f;

        source.Play();

        Destroy(audioObj, clip.length);
    }
}