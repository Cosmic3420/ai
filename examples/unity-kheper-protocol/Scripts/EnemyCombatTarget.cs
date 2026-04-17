using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCombatTarget : MonoBehaviour
{
    public int maxHealth = 100;
    public float flashDuration = 0.05f;
    public float hitStunDuration = 0.2f;
    public float destroyDelay = 1f;

    [Header("Animator (Optional)")]
    public Animator anim;
    public string hitTriggerParam = "Hit";
    public string dieTriggerParam = "Die";

    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 knockback)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        if (anim != null)
        {
            anim.SetTrigger(hitTriggerParam);
        }

        StartCoroutine(HitReactionRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitReactionRoutine()
    {
        StartCoroutine(FlashOnHit());
        yield return new WaitForSeconds(hitStunDuration);
    }

    private IEnumerator FlashOnHit()
    {
        if (sr == null)
        {
            yield break;
        }

        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger(dieTriggerParam);
            Destroy(gameObject, destroyDelay);
            return;
        }

        Destroy(gameObject);
    }
}
