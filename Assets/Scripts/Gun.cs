using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Gun : MonoBehaviour
{
    public Camera gunCamera;
    public float range = 100f;

    public ParticleSystem muzzleFlesh;
    public GameObject bulletImpact;

    public Animator animator;

    [SerializeField] int maxBullets = 30;

    private int bulletNumber;
    GameObject BImpact;

    public bool canShoot = true;
    public bool isReloading = false;

    public int particlesPerShot = 15;

    public enum FireMode { Semi, Auto }
    public FireMode fireMode = FireMode.Semi;
    
    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.AK47; // Set this in Inspector for each weapon

    public Image digit1;
    public Image digit2;
    public Image digit3;
    public Image digit4;

    public Sprite[] numberSprites;


    void Start()
    {
        bulletNumber = maxBullets;
        canShoot = true;

        animator = GetComponentInParent<Animator>();
        UpdateNumber2(maxBullets);
        UpdateNumber(bulletNumber);
    }

    void Update()
    {
        // SHOOT
        if (!isReloading)
        {
            if (fireMode == FireMode.Semi && Input.GetButtonDown("Fire1"))
                Shoot();

            if (fireMode == FireMode.Auto && Input.GetButton("Fire1"))
                Shoot();
        }

        // MANUAL RELOAD (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isReloading && bulletNumber < maxBullets)
                StartCoroutine(ReloadRoutine());
        }
    }

    void UpdateNumber(int value)
    {
        if (digit1 == null || digit2 == null || numberSprites == null || numberSprites.Length == 0) return;

        int d1 = value / 10;
        int d2 = value % 10;

        if (d1 < numberSprites.Length && d2 < numberSprites.Length)
        {
            digit1.sprite = numberSprites[d1];
            digit2.sprite = numberSprites[d2];
        }
    }

    void UpdateNumber2(int value)
    {
        if (digit3 == null || digit4 == null || numberSprites == null || numberSprites.Length == 0) return;

        int d1 = value / 10;
        int d2 = value % 10;

        if (d1 < numberSprites.Length && d2 < numberSprites.Length)
        {
            digit3.sprite = numberSprites[d1];
            digit4.sprite = numberSprites[d2];
        }
    }

    void Shoot()
    {
        if (!canShoot || isReloading) return;

        if (bulletNumber <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }
        
        // PLAY WEAPON SHOOT SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponShoot(weaponType);
        }

        // MUZZLE FLASH
        if (muzzleFlesh != null)
            muzzleFlesh.Emit(particlesPerShot);

        // RAYCAST
        RaycastHit hit;
        if (Physics.Raycast(gunCamera.transform.position, gunCamera.transform.forward, out hit, range))
        {

            // Check if we hit an enemy or any of its child objects
            Transform enemyRoot = GetEnemyRoot(hit.transform);
            if (enemyRoot != null)
            {
                
                // Try to get animator from the enemy root
                Animator hitAnimator = enemyRoot.GetComponent<Animator>();
                if (hitAnimator != null)
                {
                    hitAnimator.SetTrigger("hit");
                }

                StartCoroutine(DestroyEnemyAfterDelay(enemyRoot.gameObject, 5f));
                // Check for health component first
                //EnemyHealth enemyHealth = enemyRoot.GetComponent<EnemyHealth>();
                //if (enemyHealth != null)
                //{
                //    enemyHealth.TakeDamage(25); // Deal damage instead of instant kill
                //}
                //else
                //{
                //    // No health system, just destroy after animation delay
                    
                //}
            }

            // Bullet impact
            BImpact = GameObject.Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
            StartCoroutine(BulletImpactRoutine(0.3f));
        }

        bulletNumber--;
        UpdateNumber(bulletNumber);
    }

    Transform GetEnemyRoot(Transform hitTransform)
    {
        // Check if the hit object itself has the Enemy tag
        if (hitTransform.CompareTag("Enemy"))
            return hitTransform;
        
        // Check if any parent has the Enemy tag
        Transform current = hitTransform;
        while (current.parent != null)
        {
            if (current.parent.CompareTag("Enemy"))
                return current.parent;
            current = current.parent;
        }
        
        // Check if the name contains "Enemy" (fallback)
        current = hitTransform;
        while (current != null)
        {
            if (current.name.Contains("Enemy") || current.name.Contains("enemy"))
                return current;
            current = current.parent;
        }
        
        return null;
    }
    
    IEnumerator DestroyEnemyAfterDelay(GameObject enemy, float delay)
    {
        // Call the enemy's Die() method to properly handle death
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.Die(); // This stops all AI behavior and lets death animation play
        }
        else
        {
            // Fallback if EnemyAI script is missing
            // Disable NavMeshAgent to prevent floating during death animation
            UnityEngine.AI.NavMeshAgent navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
                navAgent.enabled = false;
            }
            
            // Get the Animator and ensure death animation plays
            Animator enemyAnimator = enemy.GetComponent<Animator>();
            if (enemyAnimator != null)
            {
                // Stop the animator from being controlled by the AI
                enemyAnimator.SetBool("walk", false);
                enemyAnimator.SetBool("run", false);
                enemyAnimator.SetBool("attack", false);
                enemyAnimator.SetFloat("speed", 0f);
            }
            
            // Disable enemy guns
            EnemyGunController[] enemyGuns = enemy.GetComponentsInChildren<EnemyGunController>();
            foreach (EnemyGunController gun in enemyGuns)
            {
                gun.canShoot = false;
                gun.enabled = false;
            }
        }
        
        // Disable collider so player can't interact with dead enemy
        Collider enemyCollider = enemy.GetComponent<Collider>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;
        
        yield return new WaitForSeconds(delay);
        
        Destroy(enemy);
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        canShoot = false;

        if (animator != null)
            animator.SetTrigger("reload");
        
        // PLAY RELOAD SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponReload(weaponType);
        }

        yield return new WaitForSeconds(1.3f); // match animation length

        bulletNumber = maxBullets;
        UpdateNumber(bulletNumber);

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