using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapGen;

public enum e1State
{
    moving,
    findPoint,
    seekE2
}

public class Enemy1Behavior : MonoBehaviour
{
    int point;
    Vector3 temp;
    MapTile[,] map;

    Vector3 veloc, targetVeloc, steer;

    [SerializeField]
    GameObject enemy2, player;

    public e1State currentState;
    bool moveStarted, newStarted;
    [SerializeField]
    bool seekStarted, foundE2, newEnemy;
    private List<KeyValuePair<e1State, e1State>> transitions
        = new List<KeyValuePair<e1State, e1State>>();
    GameObject playerSight, enemyFind;

    void Awake()
    {
        transitions.Add(new KeyValuePair<e1State, e1State>(e1State.moving, e1State.findPoint));
        transitions.Add(new KeyValuePair<e1State, e1State>(e1State.findPoint, e1State.seekE2));
        transitions.Add(new KeyValuePair<e1State, e1State>(e1State.findPoint, e1State.moving));
        transitions.Add(new KeyValuePair<e1State, e1State>(e1State.seekE2, e1State.findPoint));
    }

    // Use this for initialization
    void Start()
    {
        playerSight = GameObject.FindGameObjectWithTag("Player");
        enemyFind = GameObject.FindGameObjectWithTag("Enemy2");
        map = FindObjectOfType<MapGenerator>().getMap();
        temp = transform.position;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy2" && seekStarted)
        {
            foundE2 = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case e1State.moving:
                if (!moveStarted)
                    StartCoroutine(moveMe());
                break;

            case e1State.findPoint:
                if (!newStarted)
                    StartCoroutine(findNew());
                break;

            case e1State.seekE2:
                if (!seekStarted)
                    StartCoroutine(seek());
                break;
        }
    }

    IEnumerator moveMe()
    {
        moveStarted = true;
        newStarted = false;
        seekStarted = false;
        //if see player, seek E2


        while (true)
        {
            float distance = Vector3.Distance(temp, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, temp, Time.deltaTime * 8.0f);
            if (distance == 0)
            {
                Transition(e1State.findPoint);
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator findNew()
    {
        newStarted = true;
        moveStarted = false;
        seekStarted = false;

        if (playerSight != null)
            if ((int)this.transform.position.x == (int)playerSight.transform.position.x || (int)this.transform.position.z == (int)playerSight.transform.position.z)
            {
                newEnemy = false;
                Transition(e1State.seekE2);
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
                    yield return new WaitForSeconds(0.2f);
                    Transition(e1State.moving);
                    yield break;
                }
            }

            if (point == 2)
            {
                temp = new Vector3(transform.position.x - 1, 1.0f, transform.position.z);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.2f);
                    Transition(e1State.moving);
                    yield break;
                }
            }

            if (point == 3)
            {
                temp = new Vector3(transform.position.x, 1.0f, transform.position.z + 1);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.2f);
                    Transition(e1State.moving);
                    yield break;
                }
            }

            if (point == 4)
            {
                temp = new Vector3(transform.position.x, 1.0f, transform.position.z - 1);
                if (temp.x >= 0 && temp.x < map.GetLength(0) && temp.z >= 0 && temp.z < map.GetLength(1)
                    && map[(int)temp.x, (int)temp.z].Walkable == true)
                {
                    yield return new WaitForSeconds(0.2f);
                    Transition(e1State.moving);
                    yield break;
                }
            }
        }
    }

    IEnumerator seek()
    {
        newStarted = false;
        moveStarted = false;
        seekStarted = true;
        while (true)
        {
            if (foundE2)
            {
                while (!newEnemy)
                {
                    int randomX = Random.Range(0, map.GetLength(0));
                    int randomY = Random.Range(0, map.GetLength(1));
                    if (map[randomX, randomY].Walkable == true)
                    {
                        newEnemy = true;
                        transform.position = new Vector3(randomX, 0.0f, randomY);
                    }
                }
                foundE2 = false;
                Transition(e1State.findPoint);
                yield break;
            }
            targetVeloc = Vector3.Normalize(new Vector3(enemyFind.transform.position.x - transform.position.x, 0.0f, enemyFind.transform.position.z - transform.position.z)) * .1f;
            steer = targetVeloc - veloc;
            veloc += steer;
            transform.position += veloc;
            yield return null;
        }

    }

    public void Transition(e1State nextState)
    {
        if (transitions.Contains(new KeyValuePair<e1State, e1State>(currentState, nextState)))
        {
            currentState = nextState;
        }
    }

    int findPt()
    {
        return Random.Range(1, 5);
    }
}
