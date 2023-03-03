using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Characters")]
    public PlayerController activePlayer;
    public List<PlayerController> allCharacters = new List<PlayerController>();
    public List<PlayerController> playerTeam = new List<PlayerController>();
    public List<PlayerController> enemyTeam = new List<PlayerController>();
    [SerializeField] private int currentCharacter;

    [Header("TurnPoints")]
    [SerializeField, Range(1, 10)] private int totalTurnPoints = 2;
    [HideInInspector] public int turnPointsRemaining;
    [HideInInspector] public int currentActionCost = 1;

    [Header("SpawnPoints")]
    [SerializeField] private bool shouldSpawnAtRandomPoints;
    [SerializeField] private List<Transform> playerSpawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();

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

        if (shouldSpawnAtRandomPoints)
        {
            foreach (PlayerController character in playerTeam)
            {
                if (playerSpawnPoints.Count > 0)
                {
                    int position = Random.Range(0, playerSpawnPoints.Count);

                    character.transform.position = playerSpawnPoints[position].position;

                    playerSpawnPoints.RemoveAt(position);
                }
            }

            foreach (PlayerController character in enemyTeam)
            {
                if (enemySpawnPoints.Count > 0)
                {
                    int position = Random.Range(0, enemySpawnPoints.Count);

                    character.transform.position = enemySpawnPoints[position].position;

                    enemySpawnPoints.RemoveAt(position);
                }
            }
        }

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
                PlayerInputMenu.instance.ShowInputMenu();
            }
            else
            {
                PlayerInputMenu.instance.HideMenus();

                activePlayer.brain.SelectAction();
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

        if (activePlayer.isEnemy == false)
        {
            PlayerInputMenu.instance.ShowInputMenu();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(true);
        }
        else
        {
            PlayerInputMenu.instance.HideMenus();
            PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);

            activePlayer.brain.SelectAction();
        }

        currentActionCost = 1;

        PlayerInputMenu.instance.UpdateTurnPointText(turnPointsRemaining);

        activePlayer.SetDefending(false);
    }

    public IEnumerator AISkipCo()
    {
        yield return new WaitForSeconds(1f);
        EndTurn();
    }
}