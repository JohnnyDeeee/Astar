using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;

public enum State { Start, Goal, Wall, Neutral, Highlighted, Linked };

public class Tile : MonoBehaviour {
    private A_Star aStar;

    public State currentState = State.Neutral;
    public Tile parent;
    public int index = 0;
    public int movementCost = 0;
    public int heuristic = 0;
    public int totalCost = 0;

	void Start () {
        aStar = GameObject.FindObjectOfType<A_Star>();
        transform.FindChild("Index").gameObject.SetActive(false);
        transform.FindChild("MovementCost").gameObject.SetActive(false);
        transform.FindChild("Heuristic").gameObject.SetActive(false);
        transform.FindChild("TotalCost").gameObject.SetActive(false);
    }
	
	void Update () {
        switch (currentState)
        {
            case State.Start:
                GetComponent<Image>().color = Color.blue;
                break;
            case State.Goal:
                GetComponent<Image>().color = Color.green;
                break;
            case State.Wall:
                GetComponent<Image>().color = Color.red;
                break;
            case State.Neutral:
                GetComponent<Image>().color = Color.white;
                break;
            case State.Highlighted:
                GetComponent<Image>().color = Color.yellow;
                break;
            case State.Linked:
                GetComponent<Image>().color = Color.cyan;
                break;
            default:
                break;
        }

        transform.FindChild("Index").GetComponent<Text>().text = ("" + index);
        transform.FindChild("MovementCost").GetComponent<Text>().text = ("" + movementCost);
        transform.FindChild("Heuristic").GetComponent<Text>().text = ("" + heuristic);
        transform.FindChild("TotalCost").GetComponent<Text>().text = ("" + totalCost);
    }

    public void Click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case State.Start:
                    if (!aStar.hasGoal)
                    {
                        currentState = State.Goal;
                        aStar.hasGoal = true;
                        aStar.hasStart = false;
                    }
                    else
                    {
                        currentState = State.Wall;
                        aStar.hasStart = false;
                    }
                    break;
                case State.Goal:
                    currentState = State.Wall;
                    aStar.hasGoal = false;
                    break;
                case State.Wall:
                    currentState = State.Neutral;
                    break;
                case State.Neutral:
                    if (!aStar.hasStart)
                    {
                        currentState = State.Start;
                        aStar.hasStart = true;
                        aStar.originalStart = this;
                    }
                    else if (!aStar.hasGoal)
                    {
                        currentState = State.Goal;
                        aStar.hasGoal = true;
                    }
                    else
                    {
                        currentState = State.Wall;
                    }
                    break;
                case State.Highlighted:
                    currentState = State.Wall;
                    break;
                default:
                    break;
            } 
        }
    }

    public void Enter()
    {
        if (aStar.dragging)
        {
            switch (currentState)
            {
                case State.Wall:
                    currentState = State.Neutral;
                    break;
                case State.Neutral:
                    currentState = State.Wall;
                    break;
                case State.Highlighted:
                    currentState = State.Wall;
                    break;
                default:
                    break;
            }
        }
    }

    public void CalculateHeuristic(Tile goal)
    {
        Vector3 goal_pos = goal.transform.localPosition;
        Vector3 my_pos = transform.localPosition;

        int x_diff = ((int)my_pos.x - (int)goal_pos.x) / aStar.tileWidth;
        int y_diff = ((int)my_pos.y - (int)goal_pos.y) / aStar.tileHeight;

        heuristic = (Mathf.Abs(x_diff) + Mathf.Abs(y_diff)) * aStar.minimumCost;

        transform.FindChild("Index").gameObject.SetActive(true);
        transform.FindChild("MovementCost").gameObject.SetActive(true);
        transform.FindChild("Heuristic").gameObject.SetActive(true);
        transform.FindChild("TotalCost").gameObject.SetActive(true);
    }

    public void LinkUp()
    {
        if (parent != null)
        {
            if (parent.currentState != State.Start)
            {
                currentState = State.Linked;
                parent.LinkUp();
            }
            else
            {
                this.currentState = State.Linked;
                //this.currentState = State.Start;
                //parent.currentState = State.Stepped;
                //aStar.Restart();
                //aStar.Calculate();
            }
        }
    }
}
