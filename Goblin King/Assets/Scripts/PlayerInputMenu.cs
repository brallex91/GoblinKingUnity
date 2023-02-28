using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerInputMenu : MonoBehaviour
{
    public static PlayerInputMenu instance;

    public GameObject inputMenu, moveMenu, meleeMenu, rangeMenu;

    public TMP_Text turnPointText;
    public TMP_Text errorText;

    public float errorDisplayTime;
    private float errorCounter;

    public TMP_Text hitChanceText;

    private void Awake()
    {
        instance = this;
    }

    public void HideMenus()
    {
        inputMenu.SetActive(false);
        moveMenu.SetActive(false);
        meleeMenu.SetActive(false);
        rangeMenu.SetActive(false);
    }

    public void ShowInputMenu()
    {
        inputMenu.SetActive(true);
    }

    public void ShowMoveMenu()
    {
        HideMenus();
        moveMenu.SetActive(true);

        ShowMove();
    }

    public void HideMoveMenu()
    {
        HideMenus();
        MoveGrid.instance.HideMovePoints();
        ShowInputMenu();
    }

    public void ShowMove()
    {
        if (GameManager.instance.turnPointsRemaining >= 1)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.moveRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 1;
        }
    }

    public void ShowRun()
    {
        if (GameManager.instance.turnPointsRemaining >= 2)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;
        }
    }

    public void UpdateTurnPointText(int turnPoints)
    {
        turnPointText.text = "Turn Points Remaining : " + turnPoints;
    }

    public void SkipTurn()
    {
        GameManager.instance.EndTurn();
    }

    public void ShowMeleeMenu()
    {
        HideMenus();
        meleeMenu.SetActive(true);
    }

    public void HideMeleeMenu()
    {
        HideMenus();
        ShowInputMenu();

        GameManager.instance.targetPoint.SetActive(false);
    }

    public void CheckMelee()
    {
        GameManager.instance.activePlayer.GetMeleeTargets();

        if (GameManager.instance.activePlayer.meleeTargets.Count > 0)
        {
            ShowMeleeMenu();

            GameManager.instance.targetPoint.SetActive(true);
            GameManager.instance.targetPoint.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
        }
        else
        {
            ShowErrorText("No enemies in melee range");
        }
    }

    public void MeleeHit()
    {
        GameManager.instance.activePlayer.DoMelee();
        GameManager.instance.currentActionCost = 1;

        HideMenus();

        GameManager.instance.targetPoint.SetActive(false);

        StartCoroutine(WaitToEndActionCo(1f));
    }

    public IEnumerator WaitToEndActionCo(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();
    }

    public void NextMeleeTarget()
    {
        GameManager.instance.activePlayer.currentMeleeTarget++;
        if (GameManager.instance.activePlayer.currentMeleeTarget >= GameManager.instance.activePlayer.meleeTargets.Count)
        {
            GameManager.instance.activePlayer.currentMeleeTarget = 0;
        }

        GameManager.instance.targetPoint.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
    }

    public void ShowErrorText(string messageToShow)
    {
        errorText.text = messageToShow;
        errorText.gameObject.SetActive(true);

        errorCounter = errorDisplayTime;
    }

    private void Update()
    {
        if (errorCounter > 0)
        {
            errorCounter -= Time.deltaTime;

            if (errorCounter <= 0)
            {
                errorText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowRangeMenu()
    {
        HideMenus();
        rangeMenu.SetActive(true);
        UpdateHitChance();
    }

    public void HideRangeMenu()
    {
        HideMenus();
        ShowInputMenu();

        GameManager.instance.targetPoint.SetActive(false);
    }

    public void CheckRange()
    {
        GameManager.instance.activePlayer.GetRangeTargets();

        if (GameManager.instance.activePlayer.rangeTargets.Count > 0)
        {
            ShowRangeMenu();

            GameManager.instance.targetPoint.SetActive(true);
            GameManager.instance.targetPoint.transform.position = GameManager.instance.activePlayer.rangeTargets[GameManager.instance.activePlayer.currentRangeTarget].transform.position;
        }
        else
        {
            ShowErrorText("No enemies in range");
        }
    }

    public void NextRangeTarget()
    {
        GameManager.instance.activePlayer.currentRangeTarget++;
        if (GameManager.instance.activePlayer.currentRangeTarget >= GameManager.instance.activePlayer.rangeTargets.Count)
        {
            GameManager.instance.activePlayer.currentRangeTarget = 0;
        }

        GameManager.instance.targetPoint.transform.position = GameManager.instance.activePlayer.rangeTargets[GameManager.instance.activePlayer.currentRangeTarget].transform.position;

        UpdateHitChance();
    }

    public void Shoot()
    {
        GameManager.instance.activePlayer.Shoot();
        GameManager.instance.currentActionCost = 1;

        HideMenus();

        GameManager.instance.targetPoint.SetActive(false);
        StartCoroutine(WaitToEndActionCo(1f));
    }

    public void UpdateHitChance()
    {
        float hitChance = Random.Range(50f, 95f);

        hitChanceText.text = "Change To Hit : " + GameManager.instance.activePlayer.CheckRangeChance().ToString("F1") + "%";
    }
}

