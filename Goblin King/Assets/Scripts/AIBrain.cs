using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public PlayerController playerController;

    public float waitBeforeAction = 1f, waitAfterActing = 1f, waitBeforeShoot = .75f;

    public float moveChance = 60f, defendChance = 25f, skipChance = 15f;

    [Range(0f, 100f)]
    public float ignoreShootChange, moveRandomChance;

    public void SelectAction()
    {
        StartCoroutine(SelectCo());
    }

    public IEnumerator SelectCo()
    {
        Debug.Log(name + " Is Selecting and Action");

        yield return new WaitForSeconds(waitBeforeAction);

        bool actionTaken = false;

        playerController.GetMeleeTargets();
        if (playerController.meleeTargets.Count > 0)
        {
            Debug.Log("Is doing Melee");

            playerController.currentMeleeTarget = Random.Range(0, playerController.meleeTargets.Count);

            GameManager.instance.currentActionCost = 1;

            StartCoroutine(WaitToEndAction(waitAfterActing));

            playerController.DoMelee();

            actionTaken = true;
        }

        playerController.GetRangeTargets();
        if (actionTaken == false && playerController.rangeTargets.Count > 0)
        {
            if (Random.Range(0f, 100f) > ignoreShootChange)
            {
                List<float> hitChances = new List<float>();

                for (int i = 0; i < playerController.rangeTargets.Count; i++)
                {
                    playerController.currentRangeTarget = i;
                    playerController.LookAtTarget(playerController.rangeTargets[i].transform);
                    hitChances.Add(playerController.CheckRangeChance());
                }

                float highestChange = 0f;
                for (int i = 0; i < hitChances.Count; i++)
                {
                    if (hitChances[i] > highestChange)
                    {
                        highestChange = hitChances[i];
                        playerController.currentRangeTarget = i;
                    }
                    else if (hitChances[i] == highestChange)
                    {
                        if (Random.value > .5f)
                        {
                            playerController.currentRangeTarget = i;
                        }
                    }
                }

                if (highestChange > 0f)
                {
                    playerController.LookAtTarget(playerController.rangeTargets[playerController.currentRangeTarget].transform);
                    CameraController.instance.SetRangeView();
                    actionTaken = true;

                    StartCoroutine(WaitToShoot());

                    Debug.Log(name + " Shot At: " + playerController.rangeTargets[playerController.currentRangeTarget].name);
                }
            }
        }

        if (actionTaken == false)
        {
            float actionDecision = Random.Range(0f, moveChance + defendChance + skipChance);

            if (actionDecision < moveChance)
            {
                float moveRandom = Random.Range(0f, 100f);

                List<MovePoint> potentialMovePoints = new List<MovePoint>();
                int selectedMovePoint = 0;

                if (moveRandom > moveRandomChance)
                {
                    int closestPlayer = 0;

                    for (int i = 1; i < GameManager.instance.playerTeam.Count; i++)
                    {
                        if (Vector3.Distance(transform.position, GameManager.instance.playerTeam[closestPlayer].transform.position)
                            > Vector3.Distance(transform.position, GameManager.instance.playerTeam[i].transform.position))
                        {
                            closestPlayer = i;
                        }
                    }

                    if (Vector3.Distance(transform.position, GameManager.instance.playerTeam[closestPlayer].transform.position) > playerController.moveRange && GameManager.instance.turnPointsRemaining >= 2)
                    {
                        potentialMovePoints = MoveGrid.instance.GetMovePointInRange(playerController.runRange, transform.position);

                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[closestPlayer].transform.position, potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[closestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedMovePoint = i;
                            }
                        }

                        GameManager.instance.currentActionCost = 2;

                        Debug.Log(name + " Is Running Towards " + GameManager.instance.playerTeam[closestPlayer].name);
                    }
                    else
                    {
                        potentialMovePoints = MoveGrid.instance.GetMovePointInRange(playerController.moveRange, transform.position);

                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[closestPlayer].transform.position, potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[closestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedMovePoint = i;
                            }
                        }

                        GameManager.instance.currentActionCost = 1;

                        Debug.Log(name + " Is Walking Towards " + GameManager.instance.playerTeam[closestPlayer].name);
                    }
                }
                else
                {
                    potentialMovePoints = MoveGrid.instance.GetMovePointInRange(playerController.moveRange, transform.position);

                    selectedMovePoint = Random.Range(0, potentialMovePoints.Count);

                    GameManager.instance.currentActionCost = 1;

                    Debug.Log(name + " Is Walking To A Random Spot!");
                }

                playerController.MoveToPoint(potentialMovePoints[selectedMovePoint].transform.position);
            }
            else if (actionDecision < moveChance + defendChance)
            {
                Debug.Log(name + " Is Defending!");

                playerController.SetDefending(true);

                GameManager.instance.currentActionCost = GameManager.instance.turnPointsRemaining;

                StartCoroutine(WaitToEndAction(waitAfterActing));




            }
            else
            {
                GameManager.instance.EndTurn();

                Debug.Log(name + " Skipped the Turn!");
            }
        }
    }

    IEnumerator WaitToEndAction(float timeToWait)
    {
        Debug.Log("Waiting to End Action");
        yield return new WaitForSeconds(timeToWait);
        GameManager.instance.SpendTurnPoints();
    }

    IEnumerator WaitToShoot()
    {
        yield return new WaitForSeconds(waitBeforeAction);

        playerController.Shoot();

        GameManager.instance.currentActionCost = 1;

        StartCoroutine(WaitToEndAction(waitAfterActing));
    }
}
