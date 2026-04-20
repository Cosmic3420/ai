using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// MVP 2D fighter controller:
/// - Movement / jump / dash
/// - 3-step normal combo
/// - Ma'at balance meter with buffs / penalties
/// - Optional animator sync + impact feedback
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float baseGravity = 3f;
    public float fallGravity = 5f;
    public float dashForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Normal Attack")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int combo1Damage = 10;
    public int combo2Damage = 15;
    public int combo3Damage = 25;
    public float comboResetWindow = 0.8f;

    [Header("Animation")]
    public bool useAnimationEvents = false;

    [Tooltip("Animator parameter names are optional. Leave empty to disable parameter writes.")]
    public string speedParam = "Speed";

    public string isGroundedParam = "IsGrounded";
    public string attackTriggerParam = "AttackTrigger";
    public string comboStepParam = "ComboStep";
    public string isDashingParam = "IsDashing";

    [Header("Impact Feedback")]
    public bool enableHitStop = true;
    public float hitStopDuration = 0.05f;
    public bool enableCameraShake = false;
    public float cameraShakeAmount = 0.1f;

    [Header("Ma'at System")]
    public float maxMaat = 100f;
    public float maat = 50f;
    public float maatGainOnHit = 5f;
    public float maatLossOnWhiff = 10f;
    public float imbalanceThreshold = 20f;
    public float harmonyThreshold = 80f;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private bool isGrounded;

    private int comboStep;
    private float lastComboTime;
    private int currentAttackDamage;

    private bool isImbalanced;
    private float baseMoveSpeed;
    private SpriteRenderer cachedSpriteRenderer;

    public bool IsImbalanced => isImbalanced;

    public event Action<float, float> MaatChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        baseMoveSpeed = moveSpeed;
        cachedSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        NotifyMaatChanged();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            HandleCombo();
        }

        CheckMaatState();
        ApplyMaatVisuals();
        SyncAnimatorMovement();
    }

    private void FixedUpdate()
    {
        Move();
        CheckGround();
        ApplyGravityProfile();
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput > 0f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (moveInput < 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void Dash()
    {
        rb.velocity = new Vector2(transform.localScale.x * dashForce, rb.velocity.y);

        if (anim != null && !string.IsNullOrEmpty(isDashingParam))
        {
            anim.SetBool(isDashingParam, true);
            StartCoroutine(ClearDashFlagNextFrame());
        }
    }

    private IEnumerator ClearDashFlagNextFrame()
    {
        yield return null;

        if (anim != null && !string.IsNullOrEmpty(isDashingParam))
        {
            anim.SetBool(isDashingParam, false);
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void ApplyGravityProfile()
    {
        rb.gravityScale = rb.velocity.y < 0f ? fallGravity : baseGravity;
    }

    private void HandleCombo()
    {
        if (Time.time - lastComboTime > comboResetWindow)
        {
            comboStep = 0;
        }

        comboStep++;
        lastComboTime = Time.time;

        if (comboStep == 1)
        {
            currentAttackDamage = combo1Damage;
        }
        else if (comboStep == 2)
        {
            currentAttackDamage = combo2Damage;
        }
        else
        {
            currentAttackDamage = combo3Damage;
            comboStep = 0;
        }

        SyncAnimatorAttack();

        if (!useAnimationEvents)
        {
            DealDamage();
        }
    }

    private void SyncAnimatorMovement()
    {
        if (anim == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(speedParam))
        {
            anim.SetFloat(speedParam, Mathf.Abs(moveInput));
        }

        if (!string.IsNullOrEmpty(isGroundedParam))
        {
            anim.SetBool(isGroundedParam, isGrounded);
        }
    }

    private void SyncAnimatorAttack()
    {
        if (anim == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(attackTriggerParam))
        {
            anim.SetTrigger(attackTriggerParam);
        }

        if (!string.IsNullOrEmpty(comboStepParam))
        {
            int displayStep = comboStep == 0 ? 3 : comboStep;
            anim.SetInteger(comboStepParam, displayStep);
        }
    }

    // Can be called by animation events for frame-accurate hits.
    public void DealDamage()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint is missing on PlayerCombatController.", this);
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        if (hitEnemies.Length == 0)
        {
            UpdateMaat(-maatLossOnWhiff);
            return;
        }

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyCombatTarget enemy = enemyCollider.GetComponent<EnemyCombatTarget>();
            if (enemy == null)
            {
                continue;
            }

            float damageMultiplier = GetMaatDamageMultiplier();
            int finalDamage = Mathf.RoundToInt(currentAttackDamage * damageMultiplier);
            Vector2 knockback = new Vector2(transform.localScale.x * 5f, 2f);
            enemy.TakeDamage(finalDamage, knockback);
        }

        ApplyImpactFeedback();
        UpdateMaat(maatGainOnHit);
    }

    private void ApplyImpactFeedback()
    {
        if (enableHitStop)
        {
            StartCoroutine(HitStop(hitStopDuration));
        }

        if (enableCameraShake && Camera.main != null)
        {
            Camera.main.transform.position += (Vector3)(UnityEngine.Random.insideUnitCircle * cameraShakeAmount);
        }
    }

    private IEnumerator HitStop(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }

    private float GetMaatDamageMultiplier()
    {
        if (isImbalanced)
        {
            return 0.8f;
        }

        if (maat >= harmonyThreshold)
        {
            return 1.2f;
        }

        return 1f;
    }

    private void UpdateMaat(float delta)
    {
        maat = Mathf.Clamp(maat + delta, 0f, maxMaat);
        NotifyMaatChanged();
    }

    private void NotifyMaatChanged()
    {
        MaatChanged?.Invoke(maat, maxMaat);
    }

    private void CheckMaatState()
    {
        if (!isImbalanced && maat <= imbalanceThreshold)
        {
            EnterImbalance();
        }
        else if (isImbalanced && maat > imbalanceThreshold)
        {
            ExitImbalance();
        }
    }

    private void EnterImbalance()
    {
        isImbalanced = true;
        moveSpeed = baseMoveSpeed * 0.8f;
    }

    private void ExitImbalance()
    {
        isImbalanced = false;
        moveSpeed = baseMoveSpeed;
    }

    private void ApplyMaatVisuals()
    {
        if (cachedSpriteRenderer == null)
        {
            return;
        }

        if (isImbalanced)
        {
            cachedSpriteRenderer.color = new Color(1f, 0.45f, 0.45f);
        }
        else if (maat >= harmonyThreshold)
        {
            cachedSpriteRenderer.color = new Color(1f, 0.95f, 0.55f);
        }
        else
        {
            cachedSpriteRenderer.color = Color.white;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
