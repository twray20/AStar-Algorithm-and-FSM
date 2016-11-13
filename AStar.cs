using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapGen;

public enum playState
{
    moving,
    newTile,
    stop,
    run
}

public class node
{
    public node parent;    //Parent of node
    public int x, y;       //Coordinates
    public int f, g, h;    //costs
    public bool closed, open;


    public node()
    {
        f = 0;
        g = 0;
        h = 0;
    }

    public node(int a, int b) //Constructor for start and goal points
    {
        f = 0;
        g = 0;
        h = 0;
        x = a;
        y = b;
        closed = false;
        open = false;
    }

    public node(int a, int b, node start, node goal) //Constructor for any new node
    {
        x = a;
        y = b;
        g = 0;//(Mathf.Abs(this.x - start.x) + Mathf.Abs(this.y - start.y));
        h = (Mathf.Abs(goal.x - this.x) + Mathf.Abs(goal.y - this.y));
        f = g + h;
        closed = false;
        open = false;
    }
}

public class AStar : MonoBehaviour
{
    float speed = 2.0f;

    [SerializeField]
    GameObject bread;

    [SerializeField]
    GameObject crumbs;

    [SerializeField]
    GameObject enemy1, enemy2;

    int minF, count, currPoint;
    node q, start, goal, n1, n2, n3, n4;
    List<node> path = new List<node>();
    bool pathFound = false;

    GameObject eS1, eS2;

    public playState currentState;

    private List<KeyValuePair<playState, playState>> transitions
        = new List<KeyValuePair<playState, playState>>();
    bool moveStarted;
    bool newStarted;
    bool stopped;

    void Awake()
    {
        transitions.Add(new KeyValuePair<playState, playState>(playState.moving, playState.newTile));
        transitions.Add(new KeyValuePair<playState, playState>(playState.moving, playState.stop));
        transitions.Add(new KeyValuePair<playState, playState>(playState.newTile, playState.moving));
        transitions.Add(new KeyValuePair<playState, playState>(playState.newTile, playState.run));
        transitions.Add(new KeyValuePair<playState, playState>(playState.run, playState.moving));
    }

    // Use this for initialization
    void Start()
    {
        eS1 = GameObject.FindGameObjectWithTag("Enemy1");
        eS2 = GameObject.FindGameObjectWithTag("Enemy2");
        path = findPath();
        count = 0;
        for (int i = 0; i < path.Count; i++)
            count++;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case playState.moving:
                if (!moveStarted)
                    StartCoroutine(move());
                break;

            case playState.newTile:
                if (!newStarted)
                    StartCoroutine(findNew());
                break;

            case playState.run:
                StartCoroutine(running());
                break;

            case playState.stop:
                if (!stopped)
                    StartCoroutine(finished());
                break;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy1" || col.gameObject.tag == "Enemy2")
            Destroy(this.gameObject);
    }

    List<node> findPath()
    {
        MapTile[,] map = FindObjectOfType<MapGenerator>().getMap();
        foreach (MapTile a in map)
        {
            if (a.IsGoal == true)
                goal = new node(a.X, a.Y);
            if (a.IsStart == true)
            {
                start = new node(a.X, a.Y);
            }
        }

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

        return path;
    }

    void drawPath()
    {
        foreach (node a in path)  //Test to visualize path and closed nodes
        {
            Instantiate(bread, new Vector3(a.x, 1, a.y), new Quaternion());
        }

        //foreach (node a in closed)
        //{
        //    Instantiate(crumbs, new Vector3(a.x, 0, a.y), new Quaternion());
        //}
    }

    IEnumerator move()
    {
        moveStarted = true;
        newStarted = false;

        if (currPoint >= count)
        {
            Transition(playState.stop);
            yield break;
        }
        while (true)
        {
            Vector3 temp = new Vector3(path[currPoint].x, 1.0f, path[currPoint].y); //Make point equal to next target
            float distance = Vector3.Distance(temp, this.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, temp, Time.deltaTime * speed);
            if (distance == 0)
            {
                Transition(playState.newTile);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator finished()
    {
        stopped = true;
        yield break;
    }

    IEnumerator running()
    {
        speed = 15.0f;
        Transition(playState.moving);
        yield break;
    }

    IEnumerator findNew()
    {
        newStarted = true;
        moveStarted = false;
        currPoint++;
        if ((int)this.transform.position.x == (int)eS1.transform.position.x || (int)this.transform.position.z == (int)eS1.transform.position.z 
            || (int)this.transform.position.x == (int)eS2.transform.position.x || (int)this.transform.position.z == (int)eS2.transform.position.z)
        {
            Transition(playState.run);
            yield break;
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.2f);
        Transition(playState.moving);
        yield break;
    }

    public void Transition(playState nextState)
    {
        if (transitions.Contains(new KeyValuePair<playState, playState>(currentState, nextState)))
        {
            currentState = nextState;
        }
    }
}