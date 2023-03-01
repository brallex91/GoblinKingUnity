using System.Collections.Generic;
using UnityEngine;

public class MoveGrid : MonoBehaviour
{
    public static MoveGrid instance;

    public Vector2Int spawnRange;
    public MovePoint startPoint;

    public LayerMask whatIsGround, whatIsObstacle;
    public float obstacleCheckRange;

    public List<MovePoint> allMovePoints = new List<MovePoint>();

    private void Awake()
    {
        instance = this;

        GenerateMoveGrid();
        HideMovePoints();
    }

    public void GenerateMoveGrid()
    {
        for (int x = -spawnRange.x; x <= spawnRange.x; x++)
        {
            for (int y = -spawnRange.y; y <= spawnRange.y; y++)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position + new Vector3(x, 10f, y), Vector3.down, out hit, 20f, whatIsGround))
                {
                    if (Physics.OverlapSphere(hit.point, obstacleCheckRange, whatIsObstacle).Length == 0)
                    {
                        MovePoint newPoint = Instantiate(startPoint, hit.point, transform.rotation);
                        newPoint.transform.SetParent(transform);

                        allMovePoints.Add(newPoint);
                    }
                }
            }
        }

        startPoint.gameObject.SetActive(false);
    }

    public void HideMovePoints()
    {
        foreach (MovePoint movePoint in allMovePoints)
        {
            movePoint.gameObject.SetActive(false);
        }
    }

    public void ShowPointsInRange(float moveRange, Vector3 centerPoint)
    {
        HideMovePoints();

        foreach (MovePoint movePoint in allMovePoints)
        {
            if (Vector3.Distance(centerPoint, movePoint.transform.position) <= moveRange)
            {
                movePoint.gameObject.SetActive(true);

                foreach (PlayerController character in GameManager.instance.allCharacters)
                {
                    if (Vector3.Distance(character.transform.position, movePoint.transform.position) < .5f)
                    {
                        movePoint.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public List<MovePoint> GetMovePointInRange(float moveRange, Vector3 centerPoint)
    {
        List<MovePoint> foundMovePoints = new List<MovePoint>();

        foreach (MovePoint movePoint in allMovePoints)
        {
            if (Vector3.Distance(centerPoint, movePoint.transform.position) <= moveRange)
            {
                bool shouldAdd = true;

                foreach (PlayerController character in GameManager.instance.allCharacters)
                {
                    if (Vector3.Distance(character.transform.position, movePoint.transform.position) < .5f)
                    {
                        shouldAdd = false;
                    }
                }
                if (shouldAdd == true)
                {
                    foundMovePoints.Add(movePoint);
                }
            }
        }

        return foundMovePoints;
    }
}
