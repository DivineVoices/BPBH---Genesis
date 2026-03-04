using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float HP;
    [SerializeField] private float MaxHP;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float Width, Height;
    [SerializeField] private Coroutine dodgeStunRoutine;
    [SerializeField] private float stunDuration;

    [SerializeField] private SphereDetection sphereDetectorRef;
    [SerializeField] private LayerMask attackLayerMask;
    [SerializeField] private float attackDetectionRange = 2f;

    [SerializeField] private Animator animator;
    [SerializeField] private float fadeDuration = 0.1f;
    [SerializeField] private float stayDuration = 0.5f;
    private Coroutine fadeRoutine;
    [SerializeField] private Image flash;
    [SerializeField] private AttackNotif attackNotif;

    [SerializeField] private EnemyBlink enemyBlink;

    [SerializeField] private ParticleSystem shockEffect;
    [SerializeField] private bool bigAttack;

    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private bool facePlayer = true;

    void Start()
    {
        HP = MaxHP;
        RefreshHPUI();
        shockEffect.Stop();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Update()
    {
        FacePlayer();
        int randomness = Random.Range(0, 100);
        if (randomness == 0)
            animator.SetTrigger("Attack");
        if (randomness == 1)
            animator.SetTrigger("AttackBig");
    }

    public void OnDamage(int modif)
    {
        
        HP += modif;

        if (HP == 0)
            die();
        else if (HP > MaxHP)
            HP = MaxHP;

        enemyBlink.StartBlinkRoutine(1f);
        shockEffect.Play();
        RefreshHPUI();
    }

    public void AnnounceAttack()
    {
        //Debug.Log("Announce Triggered");
        attackNotif.AnnounceAttack();
    }

    public void BigAttackTrue()
    {
        bigAttack = true;
    }
    public void BigAttackFalse()
    {
        bigAttack = false;
    }

    public void Attack()
    {
        List<GameObject> allDamageables;

        if (bigAttack)
        {
            allDamageables = sphereDetectorRef.SphereDetectAll(attackLayerMask, 200);
        }
        else
        {
            allDamageables = sphereDetectorRef.SphereCastAhead(attackLayerMask, attackDetectionRange, 2);
        }

        if (allDamageables == null)
            return;

        foreach (GameObject thing in allDamageables)
        {
            if (!thing.CompareTag("Player"))
                continue;

            IDamageable damageable = thing.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.OnDamage(-1);
            }
        }
    }

    private void RefreshHPUI()
    {
        float newWidth = (HP / MaxHP) * Width;

        healthBar.sizeDelta = new Vector2(newWidth, Height);
        string healthDisplayText = $"{HP} / {MaxHP}";
        healthText.text = healthDisplayText;
    }

    public void ShowHitmarker()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // Make visible instantly
        SetAlpha(1f);
        yield return new WaitForSeconds(stayDuration);

        // Fade out smoothly
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
        fadeRoutine = null;
    }

    private void SetAlpha(float a)
    {
        if (flash != null)
        {
            Color c = flash.color;
            c.a = a;
            flash.color = c;
        }
    }

    private void HandlePlayerDodge(PlayerController player)
    {
        // Example reactions
        Debug.Log("Player dodged! Enemy reacts.");
        if (dodgeStunRoutine != null)
            StopCoroutine(dodgeStunRoutine);

        dodgeStunRoutine = StartCoroutine(DodgeStunRoutine());
    }

    IEnumerator DodgeStunRoutine()
    {
        animator.SetBool("Dodged", true);
        Debug.Log("Stunned");
        yield return new WaitForSeconds(stunDuration);
        animator.SetBool("Dodged", false);
        Debug.Log("Unstunned");
    }

    private void die()
    {
        animator.SetTrigger("Die");
        Debug.Log("Gagner!");
    }

    private void FacePlayer()
    {
        if (!facePlayer || player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Prevent vertical tilting

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
            );
    }
}