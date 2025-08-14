using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
// Followed the tutorial by Ketra Games: "How to Procedurally Generate a Perfect Maze"
public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private AgentNavigation agent;
    [SerializeField] private MazeCell mazeCellPrefab;
    [SerializeField] private int mazeWidth;
    [SerializeField] private int mazeDepth;
    [SerializeField] private int seed;

    private MazeCell[,] mazeGrid;

    [SerializeField] private GameObject prizePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Random.InitState(seed);

        mazeGrid = new MazeCell[mazeWidth, mazeDepth];

        for(int i = 0; i < mazeWidth; i++)
        {
            for(int j = 0; j < mazeDepth; j++)
            {
                mazeGrid[i,j] = Instantiate(mazeCellPrefab, new Vector3(i, 0, j), Quaternion.identity, transform);
                mazeGrid[i,j].transform.localPosition = new Vector3 (i, 0, j);
            }
        }

        GenerateMaze(null, mazeGrid[0,0]);

        // open the start and end walls
        mazeGrid[0, 0].ClearBackWall(); // entry
        mazeGrid[mazeWidth - 1, mazeDepth - 1].ClearFrontWall(); // exit

        Vector3 localExitPos = new Vector3(mazeWidth - 1, 0, mazeDepth - 1);
        Vector3 worldExitPos = transform.TransformPoint(localExitPos);

        GetComponent<NavMeshSurface>().BuildNavMesh();

        if(prizePrefab != null)
        {
            // place trophy to the right of the agent and slightly above ground
            Instantiate(prizePrefab, worldExitPos + Vector3.right * 1f + Vector3.up * 1f, Quaternion.identity);
        }

        if (agent != null) 
        {
            agent.SetDestination(worldExitPos);
        }

    }

    private void GenerateMaze(MazeCell prevCell, MazeCell currCell)
    {
        currCell.Visit();

        ClearWalls(prevCell, currCell);

        new WaitForSeconds(0.05f);

        MazeCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currCell);

            if (nextCell != null)
            {
                GenerateMaze(currCell, nextCell);
            }
        }while(nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currCell)
    {
        var unvisitedCells = GetUnvisitedCells(currCell);

        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currCell)
    {
        int x = (int)currCell.transform.localPosition.x;
        int z = (int)currCell.transform.localPosition.z;

        //check for unvisited cell to the right
        if (x + 1 < mazeWidth)
        {
            var cellToRight = mazeGrid[x + 1,z];
            if (!cellToRight.isVisited)
            {
                yield return cellToRight;
            }
        }

        //check for unvisited cell to the left
        if(x - 1 >= 0)
        {
            var cellToLeft = mazeGrid[x - 1, z];
            if (!cellToLeft.isVisited)
            {
                yield return cellToLeft;
            }
        }

        //check for unvisited cell to the front
        if (z + 1 < mazeDepth)
        {
            var cellToFront = mazeGrid[x, z + 1];
            if (!cellToFront.isVisited)
            {
                yield return cellToFront;
            }
        }

        //check for unvisited cell to the back
        if (z - 1 >= 0)
        {
            var cellToBack = mazeGrid[x, z - 1];
            if (!cellToBack.isVisited)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeCell prevCell, MazeCell currCell)
    {
        // stopping condition
        if(prevCell == null)
        {
            return;
        }

        // if previous cell is to the left of the current cell
        // algorithm has gone from left to right
        // so clear right wall of previous cell and left wall of current cell
        if(prevCell.transform.localPosition.x < currCell.transform.localPosition.x)
        {
            prevCell.ClearRightWall();
            currCell.ClearLeftWall();
            return;
        }
        
        // right to left
        if (prevCell.transform.localPosition.x > currCell.transform.localPosition.x)
        {
            prevCell.ClearLeftWall();
            currCell.ClearRightWall();
        }

        // back to front
        if (prevCell.transform.localPosition.z < currCell.transform.localPosition.z)
        {
            prevCell.ClearFrontWall();
            currCell.ClearBackWall();
            return;
        }

        // front to back
        if (prevCell.transform.localPosition.z > currCell.transform.localPosition.z)
        {
            prevCell.ClearBackWall();
            currCell.ClearFrontWall();
            return;
        }
    }
}