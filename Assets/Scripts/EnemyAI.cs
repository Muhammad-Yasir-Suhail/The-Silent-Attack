using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] wayPoints;
    public int wayPointIndex = 0;
    public float waypointReachDistance = 1f; // How close to get to waypoint before "reaching" it
    
    [Header("Wait Settings")]
    public float waitTimeAtWaypoint = 2f; // How long to wait at each waypoint
    public bool shouldLookBack = true; // Should enemy look back when waiting?
    
    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    
    [Header("Animation Settings")]
    public float animationSpeedThreshold = 0.1f; // Speed below which we consider enemy "stopped"
    public bool useSpeedParameter = true; // Use "speed" float parameter for smoother animation
    public bool debugAnimatorState = true; // Enable to see animator state info in console
    public bool debugAnimationClip = false; // Enable to see which clip is playing every second
    public float runSpeedMultiplier = 1.5f; // How much faster to move when chasing (multiply agent.speed)
    
    [Header("Player Detection & Combat")]
    public float detectionRange = 15f; // How far the enemy can detect the player
    public float attackRange = 10f; // How close enemy needs to be to start shooting
    public float losePlayerRange = 20f; // How far before enemy loses interest and returns to patrol
    public LayerMask detectionLayerMask; // Layers to check for line of sight (set to Default + Player)
    public bool requireLineOfSight = true; // Does enemy need line of sight to detect player?
    public float combatRotationSpeed = 50f; // How fast enemy rotates to face player in combat (50+ = instant)
    public bool lookAtCamera = true; // Look at player's camera instead of player position
    public bool debugCombatAiming = true; // Show debug lines for aiming
    public float modelRotationOffset = 90f; // If enemy model faces wrong direction, adjust this (try 90, -90, 180)
    
    // Private variables
    private Vector3 target;
    private bool isWaiting = false; // Is the enemy currently waiting at a waypoint?
    private bool isCurrentlyWalking = false; // Track current walking state to avoid setting bool every frame
    private bool hasSpeedParameter = false; // Does animator have "speed" parameter?
    private bool hasWalkParameter = false; // Does animator have "walk" parameter?
    private bool hasAttackParameter = false; // Does animator have "attack" parameter?
    private bool hasRunParameter = false; // Does animator have "run" parameter?
    private float debugTimer = 0f;
    private float normalSpeed = 0f; // Store normal patrol speed
    
    // Player detection & combat variables
    private Transform player;
    private EnemyGunController gunController;
    private bool isChasing = false;
    private bool isInCombat = false;
    private float playerCheckInterval = 0.2f; // Check for player every 0.2 seconds
    private float nextPlayerCheckTime = 0f;
    
    // AI States
    private enum AIState { Patrol, Chase, Combat, Dead }
    private AIState currentState = AIState.Patrol;
    
    void Start()
    {
        wayPointIndex = 0;
        
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        
        // Get the Animator component
        animator = GetComponent<Animator>();
        
        // Check if NavMeshAgent exists
        if (agent == null)
        {
            return;
        }
        
        // IMPORTANT: Disable NavMeshAgent's rotation control so we can manually control it
        agent.updateRotation = false;
        
        // Check if waypoints are set
        if (wayPoints == null || wayPoints.Length == 0)
        {
            return;
        }
        
        // Check if animator exists and parameters
        if (animator != null)
        {
            // Check what parameters exist
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "walk" && param.type == AnimatorControllerParameterType.Bool)
                    hasWalkParameter = true;
                
                if (param.name == "speed" && param.type == AnimatorControllerParameterType.Float)
                    hasSpeedParameter = true;
                
                if (param.name == "attack" && param.type == AnimatorControllerParameterType.Bool)
                    hasAttackParameter = true;
                
                if (param.name == "run" && param.type == AnimatorControllerParameterType.Bool)
                    hasRunParameter = true;
            }
        }
        
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        
        // Get the EnemyGunController component (check children first, then self)
        gunController = GetComponentInChildren<EnemyGunController>();
        if (gunController == null)
        {
            gunController = GetComponent<EnemyGunController>();
        }
        
        if (gunController != null)
        {
            gunController.canShoot = false; // Start with gun disabled
        }
        
        // Store normal movement speed
        if (agent != null)
        {
            normalSpeed = agent.speed;
        }
        
        // Start moving to the first waypoint
        MoveToNextWaypoint();
    }

    void Update()
    {
        // Don't do anything if components are missing or enemy is dead
        if (agent == null || wayPoints == null || wayPoints.Length == 0 || currentState == AIState.Dead) return;
        
        // Update walking animation based on actual velocity
        UpdateWalkingAnimation();
        
        // Check for player periodically
        if (player != null && Time.time >= nextPlayerCheckTime)
        {
            nextPlayerCheckTime = Time.time + playerCheckInterval;
            CheckForPlayer();
        }
        
        // Handle different AI states
        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrolState();
                // Rotate toward movement direction when patrolling
                if (agent.velocity.sqrMagnitude > 0.1f)
                {
                    RotateTowardsMovement();
                }
                break;
            case AIState.Chase:
                HandleChaseState();
                // Rotate toward movement direction when chasing
                if (agent.velocity.sqrMagnitude > 0.1f)
                {
                    RotateTowardsMovement();
                }
                break;
            case AIState.Combat:
                HandleCombatState();
                // Combat state handles its own rotation
                break;
            case AIState.Dead:
                // Do nothing, enemy is dead
                break;
        }
    }
    
    
    
    
    // CENTRALIZED ANIMATION CONTROL - Manages animation parameters
    // walk = movement (true when moving, false when stopped)
    // run = sprint overlay (true when chasing)
    // attack = combat overlay (true when aggressive/shooting)
    void SetAnimationState(bool walk, bool run, bool attack, float speed = -1f)
    {
        if (animator == null) return;
        
        // Set all animation parameters
        // walk reflects actual movement state
        if (hasWalkParameter)
        {
            animator.SetBool("walk", walk);
            isCurrentlyWalking = walk;
        }
        
        // run is an overlay for chase state
        if (hasRunParameter)
        {
            animator.SetBool("run", run);
        }
        
        // attack is an overlay for combat state
        if (hasAttackParameter)
        {
            animator.SetBool("attack", attack);
        }
        
        // Set speed parameter if provided
        if (speed >= 0f && hasSpeedParameter)
        {
            animator.SetFloat("speed", speed);
        }
    }
    
    // Update walking animation based on actual agent velocity
    void UpdateWalkingAnimation()
    {
        if (animator == null || agent == null) return;
        
        // ONLY update walk animation during Patrol state
        // Chase and Combat states have their own specific animations
        if (currentState != AIState.Patrol) return;
        
        // Get the agent's current speed
        float currentSpeed = agent.velocity.magnitude;
        bool isMoving = currentSpeed > animationSpeedThreshold;
        
        // Update walk based on actual movement (only during patrol)
        if (hasWalkParameter)
        {
            // Only update if state changed to avoid unnecessary calls
            if (isMoving != isCurrentlyWalking)
            {
                animator.SetBool("walk", isMoving);
                isCurrentlyWalking = isMoving;
                
                if (debugAnimatorState)
                    Debug.Log($"[PATROL] Walk animation: {(isMoving ? "ON" : "OFF")} (speed: {currentSpeed:F2})");
            }
        }
        
        // Update speed parameter if available
        if (hasSpeedParameter && useSpeedParameter)
        {
            animator.SetFloat("speed", currentSpeed);
        }
    }
    
    // Rotate the enemy toward the direction it's moving
    void RotateTowardsMovement()
    {
        // Get the direction the agent is moving
        Vector3 direction = agent.velocity.normalized;
        
        // Only rotate if we have a valid direction
        if (direction.magnitude > 0.1f)
        {
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Apply model rotation offset if enemy model faces wrong direction
            if (Mathf.Abs(modelRotationOffset) > 0.1f)
            {
                targetRotation *= Quaternion.Euler(0f, modelRotationOffset, 0f);
            }
            
            // Smoothly rotate toward the target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    // Called when enemy reaches a waypoint
    void ReachedWaypoint()
    {
        // Start waiting at this waypoint
        StartCoroutine(WaitAtWaypoint());
    }
    
    // Wait at the waypoint, then move to the next one
    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        
        // Stop the agent by setting speed to 0 (better than isStopped)
        float originalSpeed = agent.speed;
        agent.speed = 0f;
        
        // Wait a moment for agent to fully stop
        yield return new WaitForSeconds(0.2f);
        
        // Look back if enabled
        if (shouldLookBack)
        {
            yield return StartCoroutine(RotateToLookBack());
        }
        
        // Wait for the specified time
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        
        // Look forward again if we looked back
        if (shouldLookBack)
        {
            // Update to next waypoint index first
            UpdateWaypointIndex();
            
            // Look toward next waypoint
            yield return StartCoroutine(RotateToLookForward());
        }
        else
        {
            UpdateWaypointIndex();
        }
        
        // Restore agent speed
        agent.speed = originalSpeed;
        
        isWaiting = false;
        
        // Move to the next waypoint
        MoveToNextWaypoint();
    }
    
    // Rotate the enemy to look back
    IEnumerator RotateToLookBack()
    {
        // Calculate the target rotation (180 degrees from current rotation)
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 180, 0);
        
        // Smoothly rotate over 0.5 seconds
        float rotationTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Make sure we end at exactly the target rotation
        transform.rotation = targetRotation;
    }
    
    // Rotate the enemy to look forward (toward the next waypoint)
    IEnumerator RotateToLookForward()
    {
        // Make sure the waypoint exists
        if (wayPoints[wayPointIndex] == null)
        {
            yield break;
        }
        
        // Calculate direction to the next waypoint
        Vector3 directionToWaypoint = (wayPoints[wayPointIndex].position - transform.position);
        directionToWaypoint.y = 0; // Keep rotation on horizontal plane only
        
        if (directionToWaypoint.magnitude < 0.1f)
        {
            yield break; // Too close to calculate direction
        }
        
        directionToWaypoint.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        
        Quaternion startRotation = transform.rotation;
        
        // Smoothly rotate over 0.5 seconds
        float rotationTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Make sure we end at exactly the target rotation
        transform.rotation = targetRotation;
    }
    
    // Move to the next waypoint
    void MoveToNextWaypoint()
    {
        // Make sure the waypoint exists
        if (wayPoints[wayPointIndex] == null)
        {
            return;
        }
        
        // Get the position of the next waypoint
        target = wayPoints[wayPointIndex].position;
        
        // Tell the NavMeshAgent to move to that position
        agent.SetDestination(target);
    }
    
    // Update the waypoint index to point to the next waypoint
    void UpdateWaypointIndex()
    {
        wayPointIndex++;
        
        // If we've gone past the last waypoint, loop back to the first one
        if (wayPointIndex >= wayPoints.Length)
        {
            wayPointIndex = 0;
        }
    }
    
    // ========== PLAYER DETECTION & COMBAT METHODS ==========
    
    // Check if player is in range and switch states accordingly
    void CheckForPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if we can see the player
        bool canSeePlayer = false;
        if (requireLineOfSight)
        {
            canSeePlayer = HasLineOfSightToPlayer();
        }
        else
        {
            canSeePlayer = true; // Don't require line of sight
        }
        
        // State transitions based on distance and line of sight
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            // Player detected!
            if (currentState == AIState.Patrol)
            {
                StartChasing();
            }
            
            // Check if we're close enough to attack
            if (distanceToPlayer <= attackRange)
            {
                if (currentState != AIState.Combat)
                {
                    StartCombat();
                }
            }
            else if (currentState == AIState.Combat && distanceToPlayer > attackRange)
            {
                // Too far to shoot, chase instead
                if (currentState != AIState.Chase)
                {
                    StartChasing();
                }
            }
        }
        else if (distanceToPlayer > losePlayerRange)
        {
            // Player is too far, return to patrol
            if (currentState != AIState.Patrol)
            {
                ReturnToPatrol();
            }
        }
    }
    
    // Check if enemy has line of sight to player
    bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer, out hit, distanceToPlayer, detectionLayerMask))
        {
            // Check if we hit the player
            if (hit.transform == player || hit.transform.root == player.root)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Handle patrol state (normal waypoint following)
    void HandlePatrolState()
    {
        // If we're waiting, don't check for waypoint reaching
        if (isWaiting) return;
        
        // Check if the enemy has reached the current waypoint
        float distanceToTarget = Vector3.Distance(transform.position, target);
        if (distanceToTarget < waypointReachDistance && !agent.pathPending)
        {
            // We reached the waypoint!
            ReachedWaypoint();
        }
    }
    
    // Handle chase state (following the player)
    void HandleChaseState()
    {
        if (player == null)
        {
            ReturnToPatrol();
            return;
        }
        
        // Resume movement if stopped
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }
        
        // Keep moving toward the player
        agent.SetDestination(player.position);
        
        // IMPORTANT: Ensure run animation stays active during chase
        // This prevents idle animation from playing
        if (animator != null && hasRunParameter)
        {
            if (!animator.GetBool("run"))
            {
                animator.SetBool("run", true);
                if (debugAnimatorState)
                    Debug.LogWarning("[CHASE] Run animation was OFF! Re-enabling...");
            }
        }
        
        // Ensure walk and attack stay OFF
        if (animator != null)
        {
            if (hasWalkParameter && animator.GetBool("walk"))
            {
                animator.SetBool("walk", false);
                isCurrentlyWalking = false;
                if (debugAnimatorState)
                    Debug.LogWarning("[CHASE] Walk was ON! Disabling...");
            }
            
            if (hasAttackParameter && animator.GetBool("attack"))
            {
                animator.SetBool("attack", false);
                if (debugAnimatorState)
                    Debug.LogWarning("[CHASE] Attack was ON! Disabling...");
            }
        }
        
        // Rotation is handled by RotateTowardsMovement() in Update()
    }
    
    // Handle combat state (shooting at player)
    void HandleCombatState()
    {
        if (player == null)
        {
            ReturnToPatrol();
            return;
        }
        
        // Stop moving completely
        if (agent.isStopped == false)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        
        // Get the camera (Main Camera)
        Camera playerCamera = Camera.main;
        Vector3 lookAtTarget;
        
        if (playerCamera != null && lookAtCamera)
        {
            // Look DIRECTLY at the camera transform position
            lookAtTarget = playerCamera.transform.position;
        }
        else
        {
            // Fallback: look at player position at eye level
            lookAtTarget = player.position + Vector3.up * 1.6f;
        }
        
        // Calculate direction vector from enemy to look target
        Vector3 directionToTarget = lookAtTarget - (transform.position + Vector3.up * 1.5f);
        
        // Project direction onto horizontal plane (ignore Y axis)
        directionToTarget.y = 0f;
        
        // Check if we have a valid direction (not too close)
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            // Normalize the direction
            directionToTarget.Normalize();
            
            // Create target rotation from the direction
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Apply model rotation offset if enemy model faces wrong direction
            if (Mathf.Abs(modelRotationOffset) > 0.1f)
            {
                targetRotation *= Quaternion.Euler(0f, modelRotationOffset, 0f);
            }
            
            // Apply rotation (smooth or instant based on speed)
            if (combatRotationSpeed >= 50f)
            {
                // Instant rotation for very high speeds
                transform.rotation = targetRotation;
            }
            else
            {
                // Smooth rotation
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * combatRotationSpeed
                );
            }
            
            // Debug visualization
            if (debugCombatAiming)
            {
                Vector3 enemyEyePosition = transform.position + Vector3.up * 1.5f;
                Debug.DrawRay(enemyEyePosition, transform.forward * 10f, Color.red);      // Where enemy is looking (RED)
                Debug.DrawLine(enemyEyePosition, lookAtTarget, Color.yellow);              // Line to target (YELLOW)
                Debug.DrawRay(enemyEyePosition, directionToTarget * 10f, Color.green);    // Target direction (GREEN)
            }
        }
    }
    
    // Start chasing the player
    void StartChasing()
    {
        currentState = AIState.Chase;
        isChasing = true;
        
        // Stop any waiting coroutines
        StopAllCoroutines();
        isWaiting = false;
        
        // Resume movement if stopped
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = normalSpeed * runSpeedMultiplier;
        }
        
        // IMPORTANT: Set animations IMMEDIATELY and in correct order
        // Turn OFF all other animations FIRST, then enable run
        if (animator != null)
        {
            // Step 1: Turn OFF all animations first
            if (hasWalkParameter)
            {
                animator.SetBool("walk", false);
                isCurrentlyWalking = false;
            }
            
            if (hasAttackParameter)
                animator.SetBool("attack", false);
            
            // Step 2: Set speed to running speed
            if (hasSpeedParameter)
                animator.SetFloat("speed", normalSpeed * runSpeedMultiplier);
            
            // Step 3: Enable run animation LAST
            if (hasRunParameter)
                animator.SetBool("run", true);
            
            // Force animator update to prevent transition delay
            animator.Update(0f);
        }
        
        // Disable gun while chasing
        if (gunController != null)
        {
            gunController.canShoot = false;
        }
        
        // Debug log to verify state
        if (debugAnimatorState)
        {
            Debug.Log($"[CHASE START] walk={animator.GetBool("walk")}, run={animator.GetBool("run")}, attack={animator.GetBool("attack")}, speed={animator.GetFloat("speed")}");
        }
    }
    
    // Start combat (shooting at player)
    void StartCombat()
    {
        currentState = AIState.Combat;
        isInCombat = true;
        
        // Stop the agent immediately
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.speed = normalSpeed;
        }
        
        // Set animations: standing still (no walk/run) + attack (shooting)
        if (animator != null)
        {
            if (hasWalkParameter)
            {
                animator.SetBool("walk", false);
                isCurrentlyWalking = false;
            }
            
            if (hasRunParameter)
                animator.SetBool("run", false);
            
            if (hasAttackParameter)
                animator.SetBool("attack", true);
            
            if (hasSpeedParameter)
                animator.SetFloat("speed", 0f);
        }
        
        // Enable gun controller
        if (gunController != null)
        {
            gunController.canShoot = true;
            gunController.shootTarget = player;
        }
    }
    
    // Return to normal patrol behavior
    void ReturnToPatrol()
    {
        currentState = AIState.Patrol;
        isChasing = false;
        isInCombat = false;
        
        // Resume movement
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = normalSpeed;
        }
        
        // Clear combat overlays (run and attack)
        // walk will be managed by UpdateWalkingAnimation based on actual movement
        if (animator != null)
        {
            if (hasAttackParameter)
                animator.SetBool("attack", false);
            
            if (hasRunParameter)
                animator.SetBool("run", false);
            
            if (hasSpeedParameter)
                animator.SetFloat("speed", normalSpeed);
        }
        
        // Disable gun
        if (gunController != null)
        {
            gunController.canShoot = false;
        }
        
        // Find nearest waypoint and continue patrol
        FindNearestWaypoint();
        MoveToNextWaypoint();
    }
    
    // Find the nearest waypoint to continue patrol from
    void FindNearestWaypoint()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;
        
        float nearestDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for (int i = 0; i < wayPoints.Length; i++)
        {
            if (wayPoints[i] == null) continue;
            
            float distance = Vector3.Distance(transform.position, wayPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        wayPointIndex = nearestIndex;
    }
    
    // Call this when enemy dies (from Gun.cs or other damage sources)
    public void Die()
    {
        // ABSOLUTE FIRST PRIORITY: Disable gun IMMEDIATELY with multiple safety checks
        if (gunController != null)
        {
            // Mark gun as permanently dead
            gunController.MarkAsDead();
            
            // Triple safety: disable shooting, disable script, disable GameObject
            gunController.canShoot = false;
            gunController.enabled = false;
            gunController.gameObject.SetActive(false);
        }
        
        // Set state to dead
        currentState = AIState.Dead;
        
        // Stop all AI behavior
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }
        
        // Disable all animation parameters to let death animation play
        if (animator != null)
        {
            if (hasWalkParameter)
            {
                animator.SetBool("walk", false);
                isCurrentlyWalking = false;
            }
            
            if (hasRunParameter)
                animator.SetBool("run", false);
            
            if (hasAttackParameter)
                animator.SetBool("attack", false);
            
            if (hasSpeedParameter)
                animator.SetFloat("speed", 0f);
        }
        
        // Additional cleanup for gun and its children
        if (gunController != null)
        {
            // Stop any ongoing coroutines
            gunController.StopAllCoroutines();
            
            GameObject gunObject = gunController.gameObject;
            
            // Deactivate all child objects
            Transform[] allChildren = gunObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.gameObject != gunObject)
                {
                    child.gameObject.SetActive(false);
                }
            }
            
            // Stop and clear all particle systems
            ParticleSystem[] particles = gunObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }
        
        // Disable this script's Update loop
        this.enabled = false;
        
        Debug.Log("[ENEMY DIED] All systems disabled, gun deactivated");
    }
}
