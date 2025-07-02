using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    public enum EnemyState { Patrol, Attack, Die, Chase, Stunned }
    public EnemyState currentState = EnemyState.Chase;

    [Header("Patrol Settings")]
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 patrolCenter;

    [Header("Attack Settings")]
    public float moveSpeed = 3f;
    public float minDistance = 2.5f;
    public int damageValue = 10;
    public float attackCooldown = 2f;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 5f;
    public float dashChance = 0.3f; // 30% chance to dash when chasing
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpCooldown = 3f;
    public float heightDifferenceThreshold = 1.5f; // Minimum height difference to trigger jump
    public LayerMask groundLayer = 1; // Ground layer for jump detection
    private float lastJumpTime = -Mathf.Infinity;
    private bool isJumping = false;
    private bool isGrounded = true;
    private Rigidbody rb;

    [Header("Health & Death Settings")]
    public int health = 100;
    public int maxHealth = 100;
    public GameObject cheese;
    public GameObject apple;

    [Header("General Settings")]
    public int XPValue = 10;
    private Transform playerTransform;
    private GameObject playerObject;
    private Animator animator;
    private NavMeshAgent agent;
    private int animState;

    private bool isDying = false;
    private bool deathProcessed = false;
    private bool isStunned = false;
    private EnemyState stateBeforeStun;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
            playerTransform = playerObject.transform;

        patrolCenter = transform.position;
        health = maxHealth;
    }

    void Update()
    {
        if (isDying || deathProcessed) return;

        if (transform.position.y < -15f)
        {
            DestroyEnemy();
            return;
        }

        CheckGrounded();

        if (playerTransform && !isJumping)
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Stunned:
                break;
            case EnemyState.Die:
                if (!isDying && !deathProcessed)
                {
                    isDying = true;
                    deathProcessed = true;
                    Die();
                }
                break;
        }
    }

    void CheckGrounded()
    {
        // Ground detection using raycast slightly above enemy position 
        RaycastHit hit;
        float rayDistance = 0.1f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        // This prevents false positives from ground collision while maintaining accurate detection for jump state management
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance + 0.1f, groundLayer);

        if (isJumping && isGrounded)
        {
            isJumping = false;
        }
    }

    void SetupPatrolPoints()
    {
        patrolPoints = new Vector3[4];
        patrolPoints[0] = patrolCenter + new Vector3(-5, 0, -5);
        patrolPoints[1] = patrolCenter + new Vector3(5, 0, -5);
        patrolPoints[2] = patrolCenter + new Vector3(5, 0, 5);
        patrolPoints[3] = patrolCenter + new Vector3(-5, 0, 5);
    }

    void Patrol()
    {
        if (patrolPoints == null)
            SetupPatrolPoints();

        Vector3 targetPoint = patrolPoints[currentPatrolIndex];
        SetAnimationState(0);

        if (Vector3.Distance(transform.position, targetPoint) > 0.5f)
        {
            float step = moveSpeed * 0.5f * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);
            transform.LookAt(targetPoint);
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        if (!playerTransform) return;

        SetAnimationState(1);
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float heightDifference = playerTransform.position.y - transform.position.y;

        /* Jump behavior: Trigger when player is significantly higher and enemy is grounded
         * Includes cooldown and state checks to prevent conflicts with dash or other actions */
        if (heightDifference > heightDifferenceThreshold && isGrounded &&
            Time.time - lastJumpTime >= jumpCooldown && !isJumping && !isDashing)
        {
            PerformJump();
        }

        // Dash behavior: Random chance per frame scaled by deltaTime for consistent probability
        if (Time.time - lastDashTime >= dashCooldown && !isDashing && !isJumping &&
            Random.Range(0f, 1f) < dashChance * Time.deltaTime && distance > minDistance)
        {
            PerformDash();
        }
        // Only triggers when not performing other actions and player is beyond attack range
        if (distance > minDistance && !isDashing)
        {
            Vector3 direction = (transform.position - playerTransform.position).normalized;
            Vector3 stopPosition = playerTransform.position + direction * minDistance;
            agent.SetDestination(stopPosition);
        }
        else if (!isDashing)
        {
            agent.ResetPath();
            bool playerAlive = PlayerHealth.IsAlive;
            currentState = playerAlive ? EnemyState.Attack : EnemyState.Die;
        }
    }

    void PerformJump()
    {
        if (!isGrounded || isJumping) return;

        lastJumpTime = Time.time;
        isJumping = true;

        // Temporarily disable NavMeshAgent during jump to allow physics-based movement
        if (agent && agent.enabled)
        {
            agent.enabled = false;
        }
        // This prevents conflicts between AI pathfinding and Rigidbody physics
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity += new Vector3(directionToPlayer.x * moveSpeed, 0, directionToPlayer.z * moveSpeed);

        StartCoroutine(JumpCoroutine());
    }

    IEnumerator JumpCoroutine()
    {
        yield return new WaitForSeconds(0.1f);

        // Wait for enemy to land before re-enabling NavMeshAgent
        // This ensures smooth transition back to AI-controlled movement
        while (!isGrounded)
        {
            yield return new WaitForFixedUpdate();
        }

        if (agent && !agent.enabled && !isDying && !deathProcessed)
        {
            agent.enabled = true;
        }

        isJumping = false;
    }

    void PerformDash()
    {
        if (isDashing || !playerTransform) return;

        lastDashTime = Time.time;
        isDashing = true;

        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        float originalSpeed = agent.speed;
        agent.speed = dashSpeed;

        Vector3 dashTarget = playerTransform.position;
        agent.SetDestination(dashTarget);
        SetAnimationState(1);

        float dashTimer = 0f;
        while (dashTimer < dashDuration)
        {
            dashTimer += Time.deltaTime;
            yield return null;
        }

        agent.speed = originalSpeed;
        isDashing = false;
    }

    void Attack()
    {
        if (isAttacking || !playerTransform) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > minDistance)
        {
            currentState = EnemyState.Chase;
            return;
        }

        SetAnimationState(2);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;

            PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth)
                playerHealth.TakeDamage(damageValue);

            StartCoroutine(AttackCooldownCoroutine());
        }
    }

    void Die()
    {
        try
        {
            // Disable AI movement
            if (agent)
                agent.enabled = false;

            // Disable all colliders so enemy can't be hit anymore
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Update game score
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager)
                levelManager.UpdateScore(XPValue);

            // Give XP to player
            if (playerObject)
            {
                PlayerXP playerXP = playerObject.GetComponent<PlayerXP>();
                if (playerXP)
                    playerXP.TakeXP(XPValue);
            }
            int cheeseOrApple = Random.Range(1, 4);
            if (cheeseOrApple == 1)
            {
                Instantiate(cheese, transform.position, transform.rotation);
            }
            else if (cheeseOrApple == 2)
            {
                Instantiate(apple, transform.position, transform.rotation);
            }

            // Destroy enemy after brief delay
            Invoke(nameof(DestroyEnemy), 0.1f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in Die() function: {e.Message}");
            Invoke(nameof(DestroyEnemy), 0.1f);
        }
    }

    void DestroyEnemy()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (isDying || deathProcessed) return;

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            currentState = EnemyState.Die;
        }
    }

    public void Stun(float duration)
    {
        if (isDying || deathProcessed) return;

        // Stop any existing stun coroutine
        if (isStunned)
            StopCoroutine(nameof(StunCoroutine));

        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        if (!isStunned)
            stateBeforeStun = currentState;

        isStunned = true;
        isDashing = false;
        isJumping = false;
        currentState = EnemyState.Stunned;

        // Completely disable NavMeshAgent during stun to prevent movement
        // Reset path and velocity to ensure immediate stop
        if (agent && agent.enabled)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        isAttacking = false;
        SetAnimationState(0);

        yield return new WaitForSeconds(duration);

        if (!isDying && !deathProcessed)
        {
            if (agent && !agent.enabled)
                agent.enabled = true;

            isStunned = false;
            currentState = stateBeforeStun;
        }
    }

    private void SetAnimationState(int state)
    {
        if (animState != state && animator && !isDying)
        {
            animState = state;
            animator.SetInteger("animState", animState);
        }
    }

    IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(1.1f);

        if (isDying || deathProcessed) yield break;

        isAttacking = false;
        SetAnimationState(0);

        if (!playerTransform) yield break;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        currentState = distance > minDistance ? EnemyState.Chase : EnemyState.Attack;
    }
}