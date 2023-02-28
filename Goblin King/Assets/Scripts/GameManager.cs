using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerController activePlayer;

    public List<PlayerController> allCharacters = new List<PlayerController>();
    public List<PlayerController> playerTeam = new List<PlayerController>();
    public List<PlayerController> enemyTeam = new List<PlayerController>();

    private int currentCharacter;

    public int totalTurnPoints = 2;
    [HideInInspector] public int turnPointsRemaining;

    public int currentActionCost = 1;

    public GameObject targetPoint;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        List<PlayerController> tempList = new List<PlayerController>();
        tempList.AddRange(FindObjectsOfType<PlayerController>());

        int iterations = tempList.Count + 50;
        while (tempList.Count > 0 && iterations > 0)
        {
            int randomPick = Random.Range(0, tempList.Count);
            allCharacters.Add(tempList[randomPick]);

            tempList.RemoveAt(randomPick);

            iterations--;
        }

        foreach (PlayerController character in allCharacters)
        {
            if (character.isEnemy == false)
            {
                playerTeam.Add(character);
            }
            else if (character.isEnemy == true)
            {
                enemyTeam.Add(character);
            }
        }

        allCharacters.Clear();

        if (Random.value >= .5f)
        {
            allCharacters.AddRange(playerTeam);
            allCharacters.AddRange(enemyTeam);
        }
        else
        {
            allCharacters.AddRange(enemyTeam);
            allCharacters.AddRange(playerTeam);
        }

        activePlayer = allCharacters[0];
        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        currentCharacter = -1;
        EndTurn();
    }

    public void FinishedMovement()
    {
        SpendTurnPoints();
    }

    public void SpendTurnPoints()
    {
        turnPointsRemaining -= currentActionCost;

        if (turnPointsRemaining <= 0)
        {
            EndTurn();
        }
        else
        {
            if (activePlayer.isEnemy == false)
            {
                //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

                PlayerInputMenu.instance.ShowInputMenu();
            }
            else
            {
                PlayerInputMenu.instance.HideMenus();
            }
        }

        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }

    public void EndTurn()
    {
        currentCharacter++;
        if (currentCharacter >= allCharacters.Count)
        {
            currentCharacter = 0;
        }
        activePlayer = allCharacters[currentCharacter];

        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        turnPointsRemaining = totalTurnPoints;

        //SKIP ALL ENEMY-TURNS!
        if (activePlayer.isEnemy == false)
        {
            //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

            PlayerInputMenu.instance.ShowInputMenu();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(true);
        }
        else
        {
            PlayerInputMenu.instance.HideMenus();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);

            StartCoroutine(AISkipCo());
        }

        currentActionCost = 1;
        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);
    }

    public IEnumerator AISkipCo()
    {
        yield return new WaitForSeconds(1f);
        EndTurn();
    }
}
