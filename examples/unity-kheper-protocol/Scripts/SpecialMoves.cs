using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Special abilities resource loop:
/// - K: Wolf Summon (AoE strike)
/// - L: Dash Strike (mobility + hit)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SpecialMoves : MonoBehaviour
{
    [Header("Kheper Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 10f;

    [Header("Wolf Summon")]
    public float wolfCost = 25f;
    public float wolfCooldown = 3f;
    public float wolfRadius = 1.2f;
    public int wolfDamage = 18;

    [Header("Dash Strike")]
    public float dashCost = 15f;
    public float dashCooldown = 2f;
    public float dashForce = 14f;
    public float dashHitRadius = 0.8f;
    public int dashDamage = 14;

    [Header("Animation")]
    public string specialTriggerParam = "Special";

    [Header("Targeting")]
    public LayerMask enemyLayer;

    private float lastWolfTime = -999f;
    private float lastDashTime = -999f;
    private bool isUsingAbility;

    private Rigidbody2D rb;
    private Animator anim;

    public bool IsUsingAbility => isUsingAbility;

    public event Action<float, float> EnergyChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentEnergy = maxEnergy;
        NotifyEnergyChanged();
    }

    private void Update()
    {
        RegenerateEnergy();

        if (Input.GetKeyDown(KeyCode.K))
        {
            TryWolfSummon();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TryDashStrike();
        }
    }

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegenRate * Time.deltaTime);
            NotifyEnergyChanged();
        }
    }

    private void TryWolfSummon()
    {
        if (isUsingAbility || Time.time < lastWolfTime + wolfCooldown || currentEnergy < wolfCost)
        {
            return;
        }

        SpendEnergy(wolfCost);
        lastWolfTime = Time.time;
        StartCoroutine(WolfSummonRoutine());
    }

    private IEnumerator WolfSummonRoutine()
    {
        isUsingAbility = true;
        TriggerSpecialAnimation();

        // brief wind-up for readability / VFX sync
        yield return new WaitForSeconds(0.2f);

        Vector2 center = (Vector2)transform.position + Vector2.right * transform.localScale.x * 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, wolfRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyCombatTarget enemy = hit.GetComponent<EnemyCombatTarget>();
            if (enemy == null)
            {
                continue;
            }

            Vector2 knockback = new Vector2(transform.localScale.x * 6f, 2.5f);
            enemy.TakeDamage(wolfDamage, knockback);
        }

        isUsingAbility = false;
    }

    private void TryDashStrike()
    {
        if (isUsingAbility || Time.time < lastDashTime + dashCooldown || currentEnergy < dashCost)
        {
            return;
        }

        SpendEnergy(dashCost);
        lastDashTime = Time.time;
        StartCoroutine(DashStrikeRoutine());
    }

    private IEnumerator DashStrikeRoutine()
    {
        isUsingAbility = true;
        TriggerSpecialAnimation();

        rb.velocity = new Vector2(transform.localScale.x * dashForce, rb.velocity.y);
        yield return new WaitForSeconds(0.1f);

        Vector2 center = (Vector2)transform.position + Vector2.right * transform.localScale.x;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, dashHitRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyCombatTarget enemy = hit.GetComponent<EnemyCombatTarget>();
            if (enemy == null)
            {
                continue;
            }

            Vector2 knockback = new Vector2(transform.localScale.x * 7f, 1.5f);
            enemy.TakeDamage(dashDamage, knockback);
        }

        isUsingAbility = false;
    }

    private void TriggerSpecialAnimation()
    {
        if (anim == null || string.IsNullOrEmpty(specialTriggerParam))
        {
            return;
        }

        anim.SetTrigger(specialTriggerParam);
    }

    private void SpendEnergy(float cost)
    {
        currentEnergy = Mathf.Max(0f, currentEnergy - cost);
        NotifyEnergyChanged();
    }

    private void NotifyEnergyChanged()
    {
        EnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.84f, 0f);
        Vector3 wolfCenter = transform.position + Vector3.right * transform.localScale.x * 1.5f;
        Gizmos.DrawWireSphere(wolfCenter, wolfRadius);

        Gizmos.color = Color.blue;
        Vector3 dashCenter = transform.position + Vector3.right * transform.localScale.x;
        Gizmos.DrawWireSphere(dashCenter, dashHitRadius);
    }
}
