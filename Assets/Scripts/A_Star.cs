using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public enum Direction { Up, Down, Left, Right };

public class A_Star : MonoBehaviour {
    private GameObject prefab_tile;
    private List<Tile> tiles;
    private int width;
    private int height;
    private bool started = false;

    public Tile originalStart = null;
    public int tileWidth = 30;
    public int tileHeight = 30;
    public int minimumCost = 10;
    public List<Tile> openList;
    public List<Tile> closedList;
    public bool dragging = false;
    public bool hasStart, hasGoal;
    public Button startButton, resetButton;

    void Start () {
        resetButton.interactable = false;

        tiles = new List<Tile>();
        
        // Initialize tile
        prefab_tile = Resources.Load<GameObject>("Prefabs/Tile");

        // Create grid
        GameObject grid = GameObject.Find("Grid");
        width = Convert.ToInt32(grid.GetComponent<RectTransform>().rect.width);
        height = Convert.ToInt32(grid.GetComponent<RectTransform>().rect.height);
        int index = 0;

        for (int x = tileWidth/2; x < width; x+= tileWidth)
        {
            for (int y = tileHeight/2; y < height; y+= tileHeight)
            {
                GameObject @object = Instantiate(prefab_tile, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                @object.transform.SetParent(grid.transform, false);
                @object.GetComponent<Tile>().index = index;
                tiles.Add(@object.GetComponent<Tile>());
                index++;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (openList.Count > 0 && closedList.Count > 0)
        {
            foreach (Tile node in tiles)
            {
                if ((openList.Contains(node) || closedList.Contains(node)))
                {
                    if (node.currentState != State.Goal && node.currentState != State.Start && node.currentState != State.Linked)
                    {
                        node.currentState = State.Highlighted;
                    }
                }
            } 
        }

        // Enable start button if start and end tile are placed
        if ((hasGoal && hasStart) && !started)
            startButton.interactable = true;
        else
            startButton.interactable = false;
    }

    public void Calculate()
    {
        started = true;

        Tile start = null, goal = null;

        // Disable button
        startButton.interactable = false;
        resetButton.interactable = false;

        // Get the start and goal tile
        foreach (Tile tile in tiles)
        {
            if (tile.currentState == State.Start)
            {
                start = tile;
            }
            else if (tile.currentState == State.Goal)
            {
                goal = tile;
            }
        }

        // Calculate heuristics
        foreach (Tile tile in tiles)
        {
            if (tile.currentState == State.Neutral)
            {
                tile.CalculateHeuristic(goal);
            }
        }

        // Put the start node in de closed list
        closedList.Add(start);

        // Start the F and G calculation
        StartCoroutine(CalculateSurroundingNodes(start));
    }

    private IEnumerator CalculateSurroundingNodes(Tile middle_node)
    {
        openList.Remove(middle_node);
        closedList.Add(middle_node);

        // Get all surrounding tiles
        List<Tile> surrounding_nodes = new List<Tile>();

        // Directly adjecent
        if (GetTile(middle_node.transform.localPosition, Direction.Up) != null) // up
            surrounding_nodes.Add(GetTile(middle_node.transform.localPosition, Direction.Up));
        if (GetTile(middle_node.transform.localPosition, Direction.Left) != null) // left
            surrounding_nodes.Add(GetTile(middle_node.transform.localPosition, Direction.Left));
        if (GetTile(middle_node.transform.localPosition, Direction.Right) != null) // right
            surrounding_nodes.Add(GetTile(middle_node.transform.localPosition, Direction.Right));
        if (GetTile(middle_node.transform.localPosition, Direction.Down) != null) // down
            surrounding_nodes.Add(GetTile(middle_node.transform.localPosition, Direction.Down));

        // Loop trough all surrounding tiles
        foreach (Tile node in surrounding_nodes)
        {
            // Check if node is not a wall
            if (node.currentState != State.Wall)
            {
                // Check if we reached the goal
                if (node.currentState == State.Goal)
                {
                    StopAllCoroutines();
                    Debug.Log("Found Goal!");
                    middle_node.LinkUp();
                    resetButton.interactable = true;
                    //started = false;
                    break;
                }

                // Special check
                if (openList.Contains(node))
                {
                    if ((middle_node.movementCost + minimumCost) < node.movementCost)
                    {
                        node.parent = middle_node;
                        node.movementCost = middle_node.movementCost + minimumCost;
                        node.totalCost = node.heuristic + node.movementCost;
                        //openList.Remove(node);
                        //closedList.Add(node);
                    }
                }

                if (!closedList.Contains(node) && !openList.Contains(node))
                {
                    // Put the node in open list
                    openList.Add(node);

                    // Set the node parent to start
                    node.parent = middle_node;

                    // Set the node g cost
                    node.movementCost = middle_node.movementCost + minimumCost;
  
                    // Set the f value
                    node.totalCost = node.heuristic + node.movementCost;
                }
            }
        }

        // Find the next best node
        int lowestTotalCost = 0;
        List<Tile> nextNodes = new List<Tile>();
        foreach (Tile node in openList)
        {
            if (!closedList.Contains(node) && node.currentState != State.Wall)
            {
                // Check which node has the lowest total cost (that is considered the best node)
                if (lowestTotalCost == 0 || node.totalCost <= lowestTotalCost)
                {
                    lowestTotalCost = node.totalCost;
                    nextNodes.Add(node);
                }
            }
        }

        int lowestHeuristic = 0;
        Tile nextNode = null;
        foreach (Tile node in nextNodes)
        {
            if (lowestHeuristic == 0 || node.heuristic < lowestHeuristic)
            {
                nextNode = node;
            }
        }

        yield return null;

        StartCoroutine(CalculateSurroundingNodes(nextNode));
    }

    private Tile GetTile(Vector3 position)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.transform.localPosition == position)
            {
                return tile;
            }
        }

        return null;
    }

    private Tile GetTile(Vector3 position, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return GetTile(new Vector3(position.x, position.y + tileWidth, position.z));
            case Direction.Down:
                return GetTile(new Vector3(position.x, position.y - tileWidth, position.z));
            case Direction.Left:
                return GetTile(new Vector3(position.x - tileHeight, position.y, position.z));
            case Direction.Right:
                return GetTile(new Vector3(position.x + tileHeight, position.y, position.z));
            default:
                break;
        }

        return null;
    }

    public void Restart(bool resetEntireLevel = false)
    {
        started = false;
        resetButton.interactable = false;

        openList.Clear();
        closedList.Clear();

        foreach (Tile node in tiles)
        {
            node.heuristic = 0;
            node.movementCost = 0;
            node.totalCost = 0;
            node.parent = null;

            if (node.currentState == State.Highlighted || node.currentState == State.Linked)
                node.currentState = State.Neutral;

            node.transform.FindChild("Index").gameObject.SetActive(false);
            node.transform.FindChild("MovementCost").gameObject.SetActive(false);
            node.transform.FindChild("Heuristic").gameObject.SetActive(false);
            node.transform.FindChild("TotalCost").gameObject.SetActive(false);
        }

        if (resetEntireLevel)
        {
            foreach (Tile node in tiles)
            {
                if (node.currentState == State.Start)
                    node.currentState = State.Neutral;
            }

            originalStart.currentState = State.Start;
        }

        startButton.enabled = true;
    }

    public void FillGrid(string color)
    {
        if (color == "red")
        {
            foreach (Tile tile in tiles)
            {
                tile.currentState = State.Wall;
            }
        }
        else if (color == "white")
        {
            foreach (Tile tile in tiles)
            {
                tile.currentState = State.Neutral;
            }
        }

        hasGoal = false;
        hasStart = false;
    }
}
