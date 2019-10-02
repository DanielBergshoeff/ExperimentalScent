using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pig : MonoBehaviour
{
    public bool Free = false;
    public bool Nuzzled = false;

    private Door doorToExitThrough;

    private NavMeshAgent myNavMeshAgent;
    private Selector rootSelector;

    // Start is called before the first frame update
    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();

        ActionNode walkRandomly = new ActionNode(WalkRandomly);
        ActionNode moveToDoor =  new ActionNode(MoveToDoor);
        ActionNode exitDoor = new ActionNode(ExitDoor);

        rootSelector = new Selector(new List<Node>() { exitDoor, moveToDoor, walkRandomly});
    }

    // Update is called once per frame
    void Update()
    {
        if (Free)
        {
            rootSelector.Evaluate();
        }
    }

    private NodeStates WalkRandomly()
    {
        myNavMeshAgent.SetDestination(RandomNavmeshLocation(4f));
        return NodeStates.SUCCESS;
    }

    private NodeStates MoveToDoor() {
        if (!Nuzzled)
            return NodeStates.FAILURE;

        if(doorToExitThrough == null) {
            float dist = (RoomManager.Instance.Doors[0].transform.position - transform.position).sqrMagnitude;
            doorToExitThrough = RoomManager.Instance.Doors[0];
            foreach (Door door in RoomManager.Instance.Doors) {
                float tempDist = (door.transform.position - transform.position).sqrMagnitude;
                if(tempDist < dist) {
                    dist = tempDist;
                    doorToExitThrough = door;
                }
            }
        }

        if(doorToExitThrough != null) {
            myNavMeshAgent.SetDestination(doorToExitThrough.transform.position);
            return NodeStates.RUNNING;
        }

        return NodeStates.FAILURE;
    }

    private NodeStates ExitDoor() {
        if (!Nuzzled || doorToExitThrough == null)
            return NodeStates.FAILURE;
        
        if(Vector3.Distance(new Vector3(doorToExitThrough.transform.position.x, transform.position.y, doorToExitThrough.transform.position.z), transform.position) < 3.0f) {
            gameObject.SetActive(false);
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    private Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
}
