using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerAutoMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Rigidbody rb;
    public Animator anim;

    [Header("Combat")]
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Aim UI")]
    public GameObject aimSprite;
    public float aimHeightOffset = 1.6f;
    public float aimScale = 1.1f;
    public float aimRotateSpeed = 200f;

    [Header("Audio")]
    public AudioClip footstepSound;
    public AudioClip shootSound;
    public AudioClip deathSound;

    private AudioSource footstepSource;
    private HitBox currentHitBox;

    [Header("Waypoints")]
    public List<Transform> points = new List<Transform>();
    public float stoppingDistance = 0.2f;

    private int currentIndex = 0;
    private bool isDead;
    private bool isShooting;
    private bool gameStarted = true;
    private bool aimInitialized;
    private Camera mainCamera;
    public float aimFollowSpeed = 12f;

    [Header("Weapon Recoil")]
    public Transform weaponHolder;
    public float recoilDistance = 0.35f;
    public float recoilSpeed = 25f;
    public float returnSpeed = 18f;

    private Vector3 weaponStartPos;
    private float recoilAmount = 0f;

    public bool IsWalking { get; private set; }

    [Header("Fire")]
    public float fireRate = 0.2f;

    private float nextFireTime;

    [Header("Ammo")]
    public int totalAmmo = 25;      // Maximum ammo
    public int currentAmmo = 25;    // Current ammo
    public TMP_Text ammoText;       // UI Text


    void Start()
    {
        if (weaponHolder != null)
        {
            weaponStartPos = weaponHolder.localPosition;
        }

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (anim == null)
            anim = GetComponent<Animator>();

        GameObject parent = GameObject.FindGameObjectWithTag("Points");

        if (parent == null)
        {
           
            return;
        }

        ammoText = GameObject.FindGameObjectWithTag("AmmoText").GetComponent<TMP_Text>();

        currentAmmo = totalAmmo;
        UpdateAmmoUI();

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");

        if (camObj != null)
        {
            mainCamera = camObj.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("MainCamera not found!");
        }

        points.Clear();

        foreach (Transform t in parent.transform)
        {
            points.Add(t);
          
        }

       

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepSound;
        footstepSource.loop = true;
        footstepSource.spatialBlend = 1f;

       

    }
    void Update()
    {
        DetectEnemy();

        bool fireInput =
            Input.GetKey(KeyCode.Space) ||
            Input.GetMouseButton(0) ||
            Input.GetKey(KeyCode.JoystickButton0);

        if (fireInput &&
          currentHitBox != null &&
          currentAmmo > 0 &&
          !isShooting &&
          Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            StartCoroutine(ShootRoutine());
        }

        if (currentHitBox != null)
        {
            LookAtEnemy();
        }

        UpdateWeaponRecoil();
        UpdateAim();
    }
    void FixedUpdate()
    {
        if (isDead || !gameStarted)
            return;

        if (!isShooting && currentHitBox == null)
        {
            HandleMovement();
        }
        else
        {
            IsWalking = false;
            rb.velocity = Vector3.zero;

          

            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
       
    }
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;

        if (currentAmmo > totalAmmo)
            currentAmmo = totalAmmo;

        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = currentAmmo + "/" + totalAmmo;
    }
    void UpdateWeaponRecoil()
    {
        if (weaponHolder == null)
            return;

        recoilAmount = Mathf.Lerp(recoilAmount, 0f, returnSpeed * Time.deltaTime);

        Vector3 targetPos = weaponStartPos + Vector3.back * recoilAmount;

        weaponHolder.localPosition = Vector3.Lerp(
            weaponHolder.localPosition,
            targetPos,
            recoilSpeed * Time.deltaTime);
    }
    void HandleMovement()
    {
       
        if (points == null || points.Count == 0)
            return;
       
        if (currentIndex >= points.Count)
        {
            rb.velocity = Vector3.zero;
          

            if (footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        Transform target = points[currentIndex];

        Vector3 dir = target.position - rb.position;
        dir.y = 0;

        float distance = dir.magnitude;
        if (distance <= stoppingDistance)
        {
            IsWalking = false;

            currentIndex++;

            rb.velocity = Vector3.zero;

            if (currentIndex >= points.Count && footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        Vector3 moveDir = dir.normalized;
        IsWalking = true;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        Quaternion targetRot = Quaternion.LookRotation(moveDir);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime));

        if (!footstepSource.isPlaying && footstepSound != null)
            footstepSource.Play();
    }

    void DetectEnemy()
    {
        if (currentHitBox != null)
        {
            Enemy enemy = currentHitBox.GetComponentInParent<Enemy>();

            if (enemy != null &&
                !enemy.HasFinishedDeath &&
                currentHitBox.gameObject.activeInHierarchy)
            {
                return;
            }

            currentHitBox = null;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        float closestDistance = Mathf.Infinity;
        HitBox closestHitBox = null;

        foreach (Collider c in hits)
        {
            HitBox hb = c.GetComponent<HitBox>();

            if (hb == null)
                continue;

            Enemy enemy = hb.GetComponentInParent<Enemy>();

            if (enemy == null || enemy.IsDead)
                continue;

            float dist = Vector3.Distance(transform.position, hb.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestHitBox = hb;
            }
        }

        currentHitBox = closestHitBox;
    }
    public void ClearTarget(HitBox hitBox)
    {
        if (currentHitBox == hitBox)
            currentHitBox = null;
    }
    void LookAtEnemy()
    {
        Vector3 dir = currentHitBox.transform.position - transform.position;
        dir.y = 0;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }

    IEnumerator ShootRoutine()
    {
        isShooting = true;
        IsWalking = false;

        anim.SetTrigger("Shoot");

        yield return new WaitForSeconds(0.2f);
        currentAmmo--;
        UpdateAmmoUI();

        recoilAmount = recoilDistance;

        if (shootSound != null)
            AudioSource.PlayClipAtPoint(shootSound, transform.position);

        if (bulletPrefab != null && firePoint != null && currentHitBox != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
                b.SetTarget(currentHitBox.transform);
        }

        isShooting = false;
    }

    void UpdateAim()
    {
        if (aimSprite == null || mainCamera == null)
            return;

        // Always keep the crosshair visible
        if (!aimSprite.activeSelf)
            aimSprite.SetActive(true);

        Image aimImage = aimSprite.GetComponent<Image>();

        if (currentHitBox == null || !currentHitBox.gameObject.activeInHierarchy)
        {
            // White when no enemy
            if (aimImage != null)
                aimImage.color = Color.white;

            // Keep it at the center of the screen
            RectTransform rect = aimSprite.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;

            aimInitialized = false;
            return;
        }

        // Red when aiming at an enemy
        if (aimImage != null)
            aimImage.color = Color.red;

        // Follow the enemy HitBox
        Vector3 screenPos = mainCamera.WorldToScreenPoint(currentHitBox.transform.position);

        if (screenPos.z > 0)
            aimSprite.transform.position = Vector3.Lerp(
      aimSprite.transform.position,
      screenPos,
      aimFollowSpeed * Time.deltaTime);

        if (!aimInitialized)
        {
            aimSprite.transform.localScale = Vector3.one * aimScale;
            aimInitialized = true;
        }
    }
    public void KillPlayer()
    {
        if (isDead) return;

        isDead = true;
        IsWalking = false;

        rb.velocity = Vector3.zero;

        if (footstepSource.isPlaying)
            footstepSource.Stop();

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(3f);

        //if (GameManager.Instance != null)
        //    GameManager.Instance.OnPlayerDied();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
