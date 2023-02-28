using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public Vector3 moveTarget;
    public float moveRange = 3.5f, runRange = 8f;

    public NavMeshAgent navMeshAgent;
    private bool isMoving;
    public bool isEnemy;

    public float meleeRange = 1.5f;
    public List<PlayerController> meleeTargets = new List<PlayerController>();
    public int currentMeleeTarget;

    //Health
    public float maxHealth = 10f;
    public float currentHealth;
    public TMP_Text healthText;
    public Slider healthSlider;

    //Melee
    public float meleeDamage = 5f;

    //Range
    public float range, rangeDamage;
    public List<PlayerController> rangeTargets = new List<PlayerController>();
    public int currentRangeTarget;
    public Transform rangePoint;
    public Vector3 missRange;

    public LineRenderer shootLine;
    public float shotRemainTime = .5f;
    private float shotRemainCounter;

    public GameObject redBloodHitEffect, wallMissEffect;

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
    }

    public void TakeDamage(float damageToTake)
    {
        currentHealth -= damageToTake;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            navMeshAgent.enabled = false;

            transform.rotation = Quaternion.Euler(-70f, transform.rotation.eulerAngles.y, 0f);

            GameManager.instance.allCharacters.Remove(this);
            if (GameManager.instance.playerTeam.Contains(this))
            {
                GameManager.instance.playerTeam.Remove(this);
            }
            else if (GameManager.instance.enemyTeam.Contains(this))
            {
                GameManager.instance.enemyTeam.Remove(this);
            }
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

        Vector3 targetOffset = new Vector3(Random.Range(-missRange.x, missRange.x),
            Random.Range(-missRange.y, missRange.y),
            Random.Range(-missRange.z, missRange.z));
        targetOffset = targetOffset * (Vector3.Distance(targetPoint, rangePoint.position) / range);
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
}
