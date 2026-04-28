using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public float range = 100f;
    public ParticleSystem muzzleFlesh;
    public GameObject bulletImpact;
    public int particlesPerShot = 15;
    public Transform gunMuzzle; // Where bullets come from (assign gun barrel tip)
    
    [Header("Accuracy Settings")]
    [Range(0f, 45f)]
    public float bulletSpreadAngle = 5f; // How much bullets can deviate (degrees)
    public bool perfectAccuracy = false; // Debug: make every shot hit

    [Header("Ammo Settings")]
    [SerializeField] int maxBullets = 30;
    private int bulletNumber;

    [Header("Enemy Settings")]
    public Transform shootTarget; // For enemies to aim at
    public float enemyFireRate = 1f; // Time between enemy shots
    private float nextEnemyFireTime = 0f;
    public Animator enemyAnim;

    [Header("State")]
    public bool canShoot = true;
    public bool isReloading = false;
    private bool isDead = false; // Track if enemy is dead

    private GameObject BImpact;

    void Start()
    {
        bulletNumber = maxBullets;
        // Enemy guns start disabled until EnemyBehaviour enables them
        canShoot = false;

        // Try to find the player as target if not set
        if (shootTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                shootTarget = player.transform;
            }
        }
        
        // Auto-find gun muzzle if not assigned
        if (gunMuzzle == null)
        {
            // Try to find a child named "Muzzle" or "GunTip"
            Transform muzzleChild = transform.Find("Muzzle");
            if (muzzleChild == null)
                muzzleChild = transform.Find("GunTip");
            if (muzzleChild == null)
                muzzleChild = transform.Find("Barrel");
            
            if (muzzleChild != null)
            {
                gunMuzzle = muzzleChild;
            }
            else
            {
                // Use this transform as fallback
                gunMuzzle = transform;
            }
        }
    }

    void Update()
    {
        // SAFETY CHECK: Don't shoot if script is disabled, canShoot is false, or enemy is dead
        if (!enabled || !canShoot || isReloading || isDead)
            return;
        
        // Enemy auto-shoots at target with fire rate
        if (Time.time >= nextEnemyFireTime)
        {
            if (shootTarget != null)
            {
                // Check if target is in range and line of sight
                float distanceToTarget = Vector3.Distance(transform.position, shootTarget.position);
                if (distanceToTarget <= range)
                {
                    EnemyShoot();
                    nextEnemyFireTime = Time.time + enemyFireRate;
                }
            }
        }

        // Auto-reload when out of bullets
        if (bulletNumber <= 0 && !isReloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void EnemyShoot()
    {
        // CRITICAL SAFETY CHECK: Don't shoot if disabled, canShoot is false, or enemy is dead
        if (!enabled || !canShoot || isReloading || isDead) return;

        if (bulletNumber <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }
        
        // PLAY ENEMY SHOOT SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }

        // MUZZLE FLASH
        if (muzzleFlesh != null)
            muzzleFlesh.Emit(particlesPerShot);

        // Calculate shoot origin (use gun muzzle position)
        Vector3 shootOrigin = gunMuzzle != null ? gunMuzzle.position : transform.position;
        
        // Calculate base direction to target
        Vector3 baseDirection = (shootTarget.position - shootOrigin).normalized;
        
        // Add bullet spread for realism (unless perfect accuracy)
        Vector3 shootDirection = baseDirection;
        if (!perfectAccuracy && bulletSpreadAngle > 0f)
        {
            // Add random spread
            float spreadX = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
            float spreadY = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
            
            // Apply spread using quaternion rotation
            Quaternion spread = Quaternion.Euler(spreadY, spreadX, 0);
            shootDirection = spread * baseDirection;
        }
        
        // RAYCAST from gun muzzle
        RaycastHit hit;
        if (Physics.Raycast(shootOrigin, shootDirection, out hit, range))
        {
            // Check if we hit the player or any parent with player tag
            Transform hitTransform = hit.transform;
            Transform playerRoot = GetPlayerRoot(hitTransform);
            
            if (playerRoot != null)
            {
                // Try to damage player using SendMessage (avoids compile-time dependency)
                playerRoot.SendMessage("TakeDamage", 25, SendMessageOptions.DontRequireReceiver);
            }

            // Bullet impact
            if (bulletImpact != null)
            {
                BImpact = GameObject.Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
                StartCoroutine(BulletImpactRoutine(0.3f));
            }
        }

        bulletNumber--;
    }
    
    // Public method to mark gun as dead (called by EnemyAI.Die())
    public void MarkAsDead()
    {
        isDead = true;
        canShoot = false;
    }
    
    Transform GetPlayerRoot(Transform hitTransform)
    {
        // Check if the hit object itself has the Player tag
        if (hitTransform.CompareTag("Player"))
            return hitTransform;
        
        // Check if any parent has the Player tag
        Transform current = hitTransform;
        while (current.parent != null)
        {
            if (current.parent.CompareTag("Player"))
                return current.parent;
            current = current.parent;
        }
        
        // Check for CharacterController (typical player root)
        current = hitTransform;
        while (current != null)
        {
            if (current.GetComponent<CharacterController>() != null)
                return current;
            current = current.parent;
        }
        
        return null;
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        canShoot = false;

        yield return new WaitForSeconds(1.3f); // match animation length

        bulletNumber = maxBullets;

        isReloading = false;
        canShoot = true;
    }

    IEnumerator BulletImpactRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (BImpact != null)
            Destroy(BImpact);
    }
}
