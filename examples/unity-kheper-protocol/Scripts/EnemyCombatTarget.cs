using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCombatTarget : MonoBehaviour
{
    public int maxHealth = 100;
    public float flashDuration = 0.05f;
    public float hitStunDuration = 0.2f;

    [Header("Animation")]
    public string hitTriggerParam = "Hit";
    public string dieTriggerParam = "Die";
    public bool destroyAfterDeathAnimation = true;
    public float destroyDelaySeconds = 1f;

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(int damage, Vector2 knockback)
    {
        currentHealth -= damage;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        TriggerHitAnimation();
        StartCoroutine(HitStun(hitStunDuration));
        StartCoroutine(FlashOnHit());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void TriggerHitAnimation()
    {
        if (anim == null || string.IsNullOrEmpty(hitTriggerParam))
        {
            return;
        }

        anim.SetTrigger(hitTriggerParam);
    }

    private IEnumerator HitStun(float duration)
    {
        if (rb == null)
        {
            yield break;
        }

        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        yield return new WaitForSeconds(duration);

        if (rb != null)
        {
            rb.constraints = originalConstraints;
        }
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
        if (anim != null && !string.IsNullOrEmpty(dieTriggerParam))
        {
            anim.SetTrigger(dieTriggerParam);
        }

        if (destroyAfterDeathAnimation)
        {
            Destroy(gameObject, destroyDelaySeconds);
            return;
        }

        Destroy(gameObject);
    }
}
