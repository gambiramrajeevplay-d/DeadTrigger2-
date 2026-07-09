using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 12f;
    public float attackRange = 1.5f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public int damage = 20;
    public float attackCooldown = 1.2f;

    [Header("Animation")]
    public Animator animator;

    [Header("Health UI")]
    public SpriteHealthBar healthBar;

    private PlayerAutoMove player;
    private PlayerHitBox playerHitBox;

    private bool isDead;
    private float nextAttackTime;

    [Header("Audio")]
    public AudioClip idleRunSound;
    public AudioClip attackSound;
    public AudioClip deathSound;

    private AudioSource audioSource;

    private AudioSource loopAudio;
    private AudioSource sfxAudio;

    private bool isAttacking;
    public bool IsDead => isDead;

    private bool isHit;
    public float hitDuration = 0.25f;

    public enum DropType
    {
        None,
        HealthPack,
        BulletSet
    }

    [Header("Drop")]
    public DropType dropType = DropType.None;

    public GameObject healthPackPrefab;
    public GameObject bulletSetPrefab;

    // Offset from the enemy where the item will spawn
    public Vector3 dropOffset = new Vector3(2f, 0f, 0f);

    [Header("Drop Animation")]
    public float dropJumpHeight = 1f;
    public float dropJumpDuration = 0.5f;

    private DamageFlashUI damageFlash;
    public bool HasFinishedDeath { get; private set; }

    void Start()
    {
        damageFlash = FindFirstObjectByType<DamageFlashUI>(FindObjectsInactive.Include);

        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.UpdateHealth(1f);

        playerHitBox = FindObjectOfType<PlayerHitBox>();

        if (playerHitBox == null)
            Debug.LogError("PlayerHitBox not found!");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 15f;
        audioSource.clip = idleRunSound;

        loopAudio = gameObject.AddComponent<AudioSource>();
        loopAudio.loop = true;
        loopAudio.playOnAwake = false;
        loopAudio.spatialBlend = 1f;
        loopAudio.clip = idleRunSound;

        sfxAudio = gameObject.AddComponent<AudioSource>();
        sfxAudio.loop = false;
        sfxAudio.playOnAwake = false;
        sfxAudio.spatialBlend = 1f;

    }

    void Update()
    {


        if (isDead || playerHitBox == null)
            return;


        float distance = Vector3.Distance(transform.position, playerHitBox.transform.position);

        if (distance > detectionRange)
        {
            animator.SetBool("isRunning", false);
            PlayIdleRunSound();
            return;
        }

        LookAtPlayer();

        if (distance > attackRange)
        {
            animator.SetBool("isRunning", true);
            PlayIdleRunSound();

            Vector3 dir = (playerHitBox.transform.position - transform.position).normalized;
            dir.y = 0;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                rotationSpeed * Time.deltaTime);

            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isRunning", false);
            StopIdleRunSound();
            if (!isAttacking && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                StartCoroutine(AttackRoutine());
            }
        }
    }

   public void DealDamage()
    {
        if (isDead || playerHitBox == null)
            return;

        if (Vector3.Distance(transform.position, playerHitBox.transform.position) <= attackRange + 0.2f)
        {
            playerHitBox.Hit(damage);

            if (damageFlash != null)
                damageFlash.ShowDamage();

            CameraWalkBob camBob = FindFirstObjectByType<CameraWalkBob>();

            if (camBob != null)
                camBob.ShakeOnHit();

        }
    }

    public void PlayAttackSound()
    {
        if (isDead || attackSound == null)
            return;


        sfxAudio.PlayOneShot(attackSound);

    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        animator.SetTrigger("isAttacking");

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Mutant Swiping"))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.35f)
            yield return null;

        DealDamage();

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        isAttacking = false;
    }
    void PlayIdleRunSound()
    {
        if (isDead || idleRunSound == null)
            return;

        if (!loopAudio.isPlaying)
            loopAudio.Play();
    }

    void StopIdleRunSound()
    {
        loopAudio.Stop();
    }

   
    void LookAtPlayer()
    {
        Vector3 dir = playerHitBox.transform.position - transform.position;
        dir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.UpdateHealth((float) currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (!isHit)
            StartCoroutine(HitRoutine());
    }
    IEnumerator HitRoutine()
{
    isHit = true;

    animator.ResetTrigger("isAttacking");
    animator.SetTrigger("Hit");

    yield return new WaitForSeconds(hitDuration);

    isHit = false;

    float distance = Vector3.Distance(transform.position, playerHitBox.transform.position);

    if (distance > attackRange)
        animator.SetBool("isRunning", true);
}
    void Die()
    {
        if (isDead)
            return;

        isDead = true;
      

       
        // Notify GameManager
        if (GameManager_Temp.Instance != null)
            GameManager_Temp.Instance.ZombieDied(this);

        loopAudio.Stop();

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        animator.SetBool("isRunning", false);
        animator.SetTrigger("Dead");

        SpawnDrop();

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        StartCoroutine(FlickerAndDisable(deathSound != null ? deathSound.length : 2f));
    }

    IEnumerator FlickerAndDisable(float delay)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        yield return new WaitForSeconds(delay);

        for (int i = 0; i < 6; i++)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = !r.enabled;
            }

            yield return new WaitForSeconds(0.15f);
        }

        HasFinishedDeath = true;
        gameObject.SetActive(false);

        // Tell player the enemy is finally gone
        PlayerAutoMove player = FindObjectOfType<PlayerAutoMove>();

        if (player != null)
        {
            HitBox hitBox = GetComponentInChildren<HitBox>();

            if (hitBox != null)
                player.ClearTarget(hitBox);
        }
    }

    void SpawnDrop()
    {
        GameObject prefab = null;

        switch (dropType)
        {
            case DropType.HealthPack:
                prefab = healthPackPrefab;
                break;

            case DropType.BulletSet:
                prefab = bulletSetPrefab;
                break;

            case DropType.None:
                return;
        }

        if (prefab == null)
            return;

        Vector3 targetPos = transform.position + transform.TransformDirection(dropOffset);

        GameObject drop = Instantiate(
            prefab,
            transform.position,
            prefab.transform.rotation);

        StartCoroutine(JumpDrop(drop.transform, targetPos));
    }
    IEnumerator JumpDrop(Transform drop, Vector3 targetPos)
    {
        Vector3 startPos = drop.position;

        float time = 0f;

        while (time < dropJumpDuration)
        {
            float t = time / dropJumpDuration;

            // Horizontal movement
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            // Jump arc
            pos.y += Mathf.Sin(t * Mathf.PI) * dropJumpHeight;

            drop.position = pos;

            time += Time.deltaTime;
            yield return null;
        }

        drop.position = targetPos;
    }
}