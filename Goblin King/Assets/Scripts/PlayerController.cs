using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private Vector3 moveTarget;
    [Range(0f, 100f)] public float moveRange = 3.5f;
    [Range(0f, 100f)] public float runRange = 8f;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private bool isMoving;

    [Header("Combat")]
    public AIBrain brain;
    public bool isEnemy;

    [Header("Health")]
    [SerializeField][Range(0f, 100f)] private float maxHealth = 10f;
    [SerializeField][Range(0f, 100f)] private float currentHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthSlider;

    [Header("Melee")]
    [SerializeField][Range(0f, 100f)] private float meleeDamage = 5f;
    [SerializeField][Range(0f, 100f)] private float meleeRange = 1.5f;
    public List<PlayerController> meleeTargets = new List<PlayerController>();
    public int currentMeleeTarget;

    [Header("Range")]
    [SerializeField][Range(0f, 100f)] private float range;
    [SerializeField][Range(0f, 100f)] private float rangeDamage;
    public List<PlayerController> rangeTargets = new List<PlayerController>();
    public int currentRangeTarget;
    [SerializeField] private Transform rangePoint;
    [SerializeField] private Vector3 missRange;
    [SerializeField] private LineRenderer shootLine;
    [SerializeField][Range(0f, 100f)] private float shotRemainTime = .5f;
    [SerializeField] private float shotRemainCounter;

    [Header("Defend")]
    [SerializeField] private GameObject defendObject;
    [SerializeField] private bool isDefending;

    [Header("Effects")]
    [SerializeField] private GameObject redBloodHitEffect;
    [SerializeField] private GameObject wallMissEffect;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    private void Start()
    {
        moveTarget = transform.position;

        navMeshAgent.speed = moveSpeed;

        currentHealth = maxHealth;

        UpdateHealthDisplay();

        shootLine.transform.position = Vector3.zero;
        shootLine.transform.rotation = Quaternion.identity;
        shootLine.transform.SetParent(null);
    }
    private void Update()
    {
        if (isMoving == true)
        {
            if (GameManager.instance.activePlayer == this)
            {
                CameraController.instance.SetMoveTarget(transform.position);

                if (Vector3.Distance(transform.position, moveTarget) < .2f)
                {
                    isMoving = false;

                    GameManager.instance.FinishedMovement();

                    animator.SetBool("isWalking", false);
                }
            }
        }

        if (shotRemainCounter > 0)
        {
            shotRemainCounter -= Time.deltaTime;

            if (shotRemainCounter <= 0)
            {
                shootLine.gameObject.SetActive(false);
            }
        }
    }

    public void MoveToPoint(Vector3 pointToMoveTo)
    {
        moveTarget = pointToMoveTo;

        navMeshAgent.SetDestination(moveTarget);
        isMoving = true;

        animator.SetBool("isWalking", true);
    }

    public void GetMeleeTargets()
    {
        meleeTargets.Clear();

        if (isEnemy == false)
        {
            foreach (PlayerController character in GameManager.instance.enemyTeam)
            {
                if (Vector3.Distance(transform.position, character.transform.position) < meleeRange)
                {
                    meleeTargets.Add(character);
                }
            }
        }
        else
        {
            foreach (PlayerController character in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, character.transform.position) < meleeRange)
                {
                    meleeTargets.Add(character);
                }
            }
        }

        if (currentMeleeTarget >= meleeTargets.Count)
        {
            currentMeleeTarget = 0;
        }
    }

    public void DoMelee()
    {
        meleeTargets[currentMeleeTarget].TakeDamage(meleeDamage);

        animator.SetTrigger("doMelee");
    }

    public void TakeDamage(float damageToTake)
    {
        if (isDefending == true)
        {
            damageToTake += .5f;
        }

        currentHealth -= damageToTake;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            navMeshAgent.enabled = false;

            GameManager.instance.allCharacters.Remove(this);
            if (GameManager.instance.playerTeam.Contains(this))
            {
                GameManager.instance.playerTeam.Remove(this);
            }
            else if (GameManager.instance.enemyTeam.Contains(this))
            {
                GameManager.instance.enemyTeam.Remove(this);
            }

            animator.SetTrigger("die");
        }
        else
        {
            // Insert take-damage animation here
            //animator.SetTrigger("takeDamage");
        }

        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        healthText.text = currentHealth + " / " + maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    public void GetRangeTargets()
    {
        rangeTargets.Clear();

        if (isEnemy == false)
        {
            foreach (PlayerController character in GameManager.instance.enemyTeam)
            {
                if (Vector3.Distance(transform.position, character.transform.position) < range)
                {
                    rangeTargets.Add(character);
                }
            }
        }
        else
        {
            foreach (PlayerController character in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, character.transform.position) < range)
                {
                    rangeTargets.Add(character);
                }
            }
        }

        if (currentRangeTarget >= rangeTargets.Count)
        {
            currentRangeTarget = 0;
        }
    }

    public void Shoot()
    {
        Vector3 targetPoint = new Vector3(rangeTargets[currentRangeTarget].transform.position.x,
            rangeTargets[currentRangeTarget].rangePoint.position.y,
            rangeTargets[currentRangeTarget].transform.position.z);
        targetPoint.y = Random.Range(targetPoint.y, rangeTargets[currentRangeTarget].transform.position.y + .25f);

        Vector3 targetOffset = new Vector3(Random.Range(-missRange.x, missRange.x),
            Random.Range(-missRange.y, missRange.y),
            Random.Range(-missRange.z, missRange.z));
        targetOffset = targetOffset * (Vector3.Distance(rangeTargets[currentRangeTarget].transform.position, transform.position) / range);
        targetPoint += targetOffset;

        Vector3 rangeDirection = (targetPoint - rangePoint.position).normalized;

        Debug.DrawRay(rangePoint.position, rangeDirection * range, Color.red, 5f);

        RaycastHit hit;
        if (Physics.Raycast(rangePoint.position, rangeDirection, out hit, range))
        {
            if (hit.collider.gameObject == rangeTargets[currentRangeTarget].gameObject)
            {
                Debug.Log(name + "shot Target " + rangeTargets[currentRangeTarget].name);

                rangeTargets[currentRangeTarget].TakeDamage(rangeDamage);

                Instantiate(redBloodHitEffect, hit.point, Quaternion.identity);
            }
            else
            {
                Debug.Log(name + "missed Target " + rangeTargets[currentRangeTarget].name + "!");

                PlayerInputMenu.instance.ShowErrorText("Shot missed!");

                Instantiate(wallMissEffect, hit.point, Quaternion.identity);
            }

            shootLine.SetPosition(0, rangePoint.position);
            shootLine.SetPosition(1, hit.point);
        }
        else
        {
            Debug.Log(name + "missed Target " + rangeTargets[currentRangeTarget].name + "!");
            PlayerInputMenu.instance.ShowErrorText("Shot missed!");

            shootLine.SetPosition(0, rangePoint.position);
            shootLine.SetPosition(1, rangePoint.position + (rangeDirection * range));
        }

        shootLine.gameObject.SetActive(true);
        shotRemainCounter = shotRemainTime;
    }

    public float CheckRangeChance()
    {
        float rangeChance = 0f;

        RaycastHit hit;

        Vector3 targetPoint = new Vector3(rangeTargets[currentRangeTarget].transform.position.x,
            rangeTargets[currentRangeTarget].rangePoint.position.y,
            rangeTargets[currentRangeTarget].transform.position.z);

        Vector3 rangeDirection = (targetPoint - rangePoint.position).normalized;
        Debug.DrawRay(rangePoint.position, rangeDirection * range, Color.red, 1f);
        if (Physics.Raycast(rangePoint.position, rangeDirection, out hit, range))
        {
            if (hit.collider.gameObject == rangeTargets[currentRangeTarget].gameObject)
            {
                rangeChance += 50f;
            }
        }

        targetPoint.y = rangeTargets[currentRangeTarget].transform.position.y + .25f;
        rangeDirection = (targetPoint - rangePoint.position).normalized;
        Debug.DrawRay(rangePoint.position, rangeDirection * range, Color.red, 1f);
        if (Physics.Raycast(rangePoint.position, rangeDirection, out hit, range))
        {
            if (hit.collider.gameObject == rangeTargets[currentRangeTarget].gameObject)
            {
                rangeChance += 50f;
            }
        }

        rangeChance = rangeChance * .95f;
        rangeChance *= 1f - (Vector3.Distance(rangeTargets[currentRangeTarget].transform.position, transform.position) / range);

        return rangeChance;
    }

    public void LookAtTarget(Transform target)
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z), Vector3.up);
    }

    public void SetDefending(bool shouldDefend)
    {
        isDefending = shouldDefend;

        defendObject.SetActive(isDefending);
    }
}