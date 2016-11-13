using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapGen;

public enum e2state
{
    moving,
    findPoint,
    chase,
    chasePoint
}

public class Enemy2Behavior : MonoBehaviour
{
    int point;
    Vector3 temp;
    MapTile[,] map;

    int minF, count;
    int currPoint = 0;
    node q, start, goal, n1, n2, n3, n4;
    bool pathFound;
    List<node> path = new List<node>();

    [SerializeField]
    GameObject enemy1, player;

    GameObject playerSight;

    public e2state currentState;
    bool moveStarted, newStarted, chaseStarted, pointStarted;
    private List<KeyValuePair<e2state, e2state>> transitions
        = new List<KeyValuePair<e2state, e2state>>();

    void Awake()
    {
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.moving, e2state.findPoint));
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.findPoint, e2state.chasePoint));
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.findPoint, e2state.moving));
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.chasePoint, e2state.findPoint));
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.chase, e2state.chasePoint));
        transitions.Add(new KeyValuePair<e2state, e2state>(e2state.chasePoint, e2state.chase));
    }

    // Use this for initialization
    void Start()
    {
        playerSight = GameObject.FindGameObjectWithTag("Player");
        map = FindObjectOfType<MapGenerator>().getMap();
        temp = transform.position;
        currPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case e2state.moving:
                if (!moveStarted)
                    StartCoroutine(moveMe());
                break;

            case e2state.findPoint:
                if (!newStarted)
                    StartCoroutine(findNew());
                break;

            case e2state.chasePoint:
                if (!pointStarted)
                    StartCoroutine(chasePoint());
                break;

            case e2state.chase:
                if (!chaseStarted)
                {
                    StartCoroutine(chasing());
                }
                break;
        }
    }

    IEnumerator moveMe()
    {
        moveStarted = true;
        newStarted = false;
        chaseStarted = false;
        pointStarted = false;

        while (true)
        {
            float distance = Vector3.Distance(temp, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, temp, Time.deltaTime * 8.0f);
            if (distance == 0)
            {
                Transition(e2state.findPoint);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator findNew()
    {
        newStarted = true;
        moveStarted = false;
        chaseStarted = false;
        pointStarted = false;

        if (playerSight != null)
            if ((int)this.transform.position.x == (int)playerSight.transform.position.x || (int)this.transform.position.z == (int)playerSight.transform.position.z)
            {
                path = findPath();
                Transition(e2state.chasePoint);
                yield break;
            }


        while (true)
        {

            yield return null;
            point = findPt();

            if (point == 1)
            {
                temp = new Vector3(transform.position.x + 1, 1.0f, transform.position.z);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.1f);
                    Transition(e2state.moving);
                    yield break;
                }
            }

            if (point == 2)
            {
                temp = new Vector3(transform.position.x - 1, 1.0f, transform.position.z);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.1f);
                    Transition(e2state.moving);
                    yield break;
                }
            }

            if (point == 3)
            {
                temp = new Vector3(transform.position.x, 1.0f, transform.position.z + 1);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.1f);
                    Transition(e2state.moving);
                    yield break;
                }
            }

            if (point == 4)
            {
                temp = new Vector3(transform.position.x, 1.0f, transform.position.z - 1);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.1f);
                    Transition(e2state.moving);
                    yield break;
                }
            }
        }
    }

    IEnumerator chasing()
    {
        chaseStarted = true;
        moveStarted = false;
        newStarted = false;
        pointStarted = false;
        if (path.Count > 0)
        {
            while (true)
            {
                Vector3 temp = new Vector3(path[currPoint].x, 1.0f, path[currPoint].y); //Make point equal to next target
                float distance = Vector3.Distance(temp, this.transform.position);
                transform.position = Vector3.MoveTowards(transform.position, temp, Time.deltaTime * 8.0f);
                if (distance == 0)
                {
                    Transition(e2state.chasePoint);
                    yield break;
                }
                yield return null;
            }
        }
        else
        {
            Transition(e2state.chasePoint);
            yield break;
        }

    }

    IEnumerator chasePoint()
    {
        pointStarted = true;
        moveStarted = false;
        newStarted = false;
        chaseStarted = false;
        currPoint++;
        if (currPoint >= count)
        {
            currPoint = 0;
            yield return new WaitForSeconds(1.0f);
            Transition(e2state.findPoint);
            yield break;
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.2f);
        Transition(e2state.chase);
        yield break;
    }

    public void Transition(e2state nextState)
    {
        if (transitions.Contains(new KeyValuePair<e2state, e2state>(currentState, nextState)))
        {
            currentState = nextState;
        }
    }

    int findPt()
    {
        return Random.Range(1, 5);
    }

    List<node> findPath()
    {
        MapTile[,] map = FindObjectOfType<MapGenerator>().getMap();
        player = GameObject.FindGameObjectWithTag("Player");
        start = new node((int)this.transform.position.x, (int)this.transform.position.z);
        goal = new node((int)player.transform.position.x, (int)player.transform.position.z);

        List<node> open = new List<node>();
        List<node> closed = new List<node>();

        open.Add(start);

        while (open.Count > 0)
        {
            minF = 1000;

            foreach (node a in open)
            {
                if (a.f < minF)
                {
                    minF = a.f;
                    q = a;
                }
            }

            open.Remove(q);

            n1 = new node(q.x + 1, q.y, start, goal);   //create neighbors
            n2 = new node(q.x - 1, q.y, start, goal);
            n3 = new node(q.x, q.y + 1, start, goal);
            n4 = new node(q.x, q.y - 1, start, goal);

            n1.parent = q;
            if (map.GetLength(0) > n1.x && n1.x >= 0 && map.GetLength(1) > n1.y && n1.y >= 0 && map[n1.x, n1.y].Walkable == true)                //check if neighbors are null in map and if they are walkable
            {
                if (n1.x == goal.x && n1.y == goal.y)
                {
                    //if successor is goal, stop search
                    pathFound = true;
                    break;
                }

                n1.g = q.g + 1;
                n1.f = n1.g + n1.h;

                foreach (node a in open)
                {
                    if (a.x == n1.x && a.y == n1.y)
                    {
                        n1.open = true;
                        if (a.f > n1.f)
                        {
                            a.parent = n1.parent;
                            a.g = n1.g;
                            a.f = n1.f;
                        }
                    }
                }

                foreach (node a in closed)
                {
                    if (a.x == n1.x && a.y == n1.y && a.f < n1.f)
                    {
                        n1.closed = true;
                        if (a.f > n1.f)
                        {
                            a.parent = n1.parent;
                            a.g = n1.g;
                            a.f = n1.f;
                        }
                    }
                }

                if (n1.open == false && n1.closed == false)
                    open.Add(n1);
            }

            n2.parent = q;
            if (map.GetLength(0) > n2.x && n2.x >= 0 && map.GetLength(1) > n2.y && n2.y >= 0 && map[n2.x, n2.y].Walkable == true)
            {
                if (n2.x == goal.x && n2.y == goal.y)
                {
                    //if successor is goal, stop search
                    pathFound = true;
                    break;
                }

                n2.g = q.g + 1;
                n2.f = n2.g + n2.h;

                foreach (node a in open)
                {
                    if (a.x == n2.x && a.y == n2.y)
                    {
                        n2.open = true;
                        if (a.f > n2.f)
                        {
                            a.parent = n2.parent;
                            a.g = n2.g;
                            a.f = n2.f;
                        }
                    }
                }

                foreach (node a in closed)
                {
                    if (a.x == n2.x && a.y == n2.y && a.f < n2.f)
                    {
                        n2.closed = true;
                        if (a.f > n2.f)
                        {
                            a.parent = n2.parent;
                            a.g = n2.g;
                            a.f = n2.f;
                        }
                    }
                }

                if (n2.open == false && n2.closed == false)
                    open.Add(n2);
            }

            n3.parent = q;
            if (map.GetLength(0) > n3.x && n3.x >= 0 && map.GetLength(1) > n3.y && n3.y >= 0 && map[n3.x, n3.y].Walkable == true)
            {
                if (n3.x == goal.x && n3.y == goal.y)
                {
                    //if successor is goal, stop search
                    pathFound = true;
                    break;
                }

                n3.g = q.g + 1;
                n3.f = n3.g + n3.h;


                foreach (node a in open)
                {
                    if (a.x == n3.x && a.y == n3.y)
                    {
                        n3.open = true;
                        if (a.f > n3.f)
                        {
                            a.parent = n3.parent;
                            a.g = n3.g;
                            a.f = n3.f;
                        }
                    }
                }

                foreach (node a in closed)
                {
                    if (a.x == n3.x && a.y == n3.y && a.f < n3.f)
                    {
                        n3.closed = true;
                        if (a.f > n3.f)
                        {
                            a.parent = n3.parent;
                            a.g = n3.g;
                            a.f = n3.f;
                        }
                    }
                }


                if (n3.open == false && n3.closed == false)
                    open.Add(n3);
            }

            n4.parent = q;
            if (map.GetLength(0) > n4.x && n4.x >= 0 && map.GetLength(1) > n4.y && n4.y >= 0 && map[n4.x, n4.y].Walkable == true)
            {
                if (n4.x == goal.x && n4.y == goal.y)
                {
                    //if successor is goal, stop search
                    pathFound = true;
                    break;
                }

                n4.g = q.g + 1;
                n4.f = n4.g + n4.h;

                foreach (node a in open)
                {
                    if (a.x == n4.x && a.y == n4.y)
                    {
                        n4.open = true;
                        if (a.f > n4.f)
                        {
                            a.parent = n4.parent;
                            a.g = n4.g;
                            a.f = n4.f;
                        }
                    }
                }

                foreach (node a in closed)
                {
                    if (a.x == n4.x && a.y == n4.y && a.f < n4.f)
                    {
                        n4.closed = true;
                        if (a.f > n4.f)
                        {
                            a.parent = n4.parent;
                            a.g = n4.g;
                            a.f = n4.f;
                        }
                    }
                }


                if (n4.open == false && n4.closed == false)
                    open.Add(n4);
            }
            closed.Add(q);
        }

        List<node> path = new List<node>();
        if (pathFound == true)
        {
            path.Add(goal);
            path.Add(q);
            while (q != start)
            {
                q = q.parent;
                path.Add(q);
            }
            path.Reverse();
        }

        count = 0;
        for (int i = 0; i < path.Count; i++)
            count++;

        return path;
    }
}
