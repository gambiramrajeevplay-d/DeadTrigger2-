using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public float speed = 80f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    Transform target;
    Vector3 shootDirection;

    [Header("Damage")]
    public int damage = 20;

    [Header("VFX")]
    public ParticleSystem hitEffect;


    
    public static List<Bullet> activeBullets = new List<Bullet>();

    // 🔥 prevent instant hit
    private float spawnTime;
    public float minHitDelay = 0.1f;

    private HashSet<Collider> hitColliders = new HashSet<Collider>();
    Vector3 previousPos;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        spawnTime = Time.time;
        activeBullets.Add(this);

        if (target != null)
            shootDirection = (target.position - transform.position).normalized;
        else
            shootDirection = transform.forward;

        Destroy(gameObject, lifeTime);

        IgnoreOwnerCollision();

        previousPos = transform.position;
    }

    void IgnoreOwnerCollision()
    {
        Collider myCol = GetComponent<Collider>();
        Collider[] parentCols = GetComponentsInParent<Collider>();

        foreach (var col in parentCols)
        {
            Physics.IgnoreCollision(myCol, col);
        }
    }

    void FixedUpdate()
    {
        Vector3 nextPos = transform.position + shootDirection * speed * Time.fixedDeltaTime;

        if (Physics.Linecast(previousPos, nextPos, out RaycastHit hit))
        {
            HitBox hb = hit.collider.GetComponent<HitBox>();

            if (hb == null)
                hb = hit.collider.GetComponentInParent<HitBox>();

            if (hb != null)
            {
                hb.Hit(damage);
                PlayHitEffect(hit.point, hb.transform);
                DisableBullet();
                return;
            }
        }

        transform.position = nextPos;
        previousPos = nextPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < minHitDelay)
            return;

        if (hitColliders.Contains(other))
            return;

        hitColliders.Add(other);

        HitBox hitBox = other.GetComponent<HitBox>();

        if (hitBox == null)
            hitBox = other.GetComponentInParent<HitBox>();

        if (hitBox == null)
            return;

        hitBox.Hit(damage);

        PlayHitEffect(transform.position, hitBox.transform);

        DisableBullet();
    }



    void PlayHitEffect(Vector3 position, Transform parent)
    {
        if (hitEffect == null) return;

        ParticleSystem fx = Instantiate(hitEffect, position, Quaternion.identity);
        fx.transform.SetParent(parent);

        Destroy(fx.gameObject, 2f);
    }



    void DisableBullet()
    {
        // stop movement immediately
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // disable collider instantly
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // stop homing/tracking
        enabled = false;

        // destroy instantly
        Destroy(gameObject);
    }
    public static void DestroyAllEnemyBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (activeBullets[i] == null) continue;

            
        }
    }
    public void SetTarget(Transform t)
    {
        target = t;
    }

    void OnDestroy()
    {
        activeBullets.Remove(this);
    }
    public void ForceDestroy()
    {
        DisableBullet();
    }
}
