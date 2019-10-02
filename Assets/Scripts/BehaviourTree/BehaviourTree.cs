using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class BehaviourTree : MonoBehaviour {

    private Animator myAnimator;

    public float ViewDistance = 20.0f;
    public float ViewAngle = 80f;

    public float WalkSpeed = 1.0f;
    public float RunSpeed = 3.0f;
    public float AttackRange = 1.0f;

    public PigPen toProd;
    public GameObject Target;
    public Door doorToLeaveThrough;

    private Selector RootSelector;
    
    private NavMeshAgent myNavMeshAgent;

    [Header("Scent")]
    public float ScentHeight = 2.0f;
    public float ScentUpdateTime = 1.0f;
    public float ScentSpeed = 10.0f;
    public Transform ScentPosition;
    private float scentUpdateTimer = 0f;
    private LineRenderer myLineRenderer;
    private List<GridNode> scentNodes;
    private int currentScentNode = 0;

    private void Start() {
        //Player actions
        ActionNode SetPlayerTargetAction = new ActionNode(SetPlayerTarget);
        ActionNode AttackPlayerAction = new ActionNode(AttackPlayer);
        ActionNode RunToPlayerAction = new ActionNode(RunToPlayer);

        //Pen actions
        ActionNode SetPenTargetAction = new ActionNode(SetPenTarget);
        ActionNode WalkToPenAction = new ActionNode(WalkToPen);
        ActionNode ProdPigAction = new ActionNode(ProdPig);

        //Leave room actions
        ActionNode SetDoorAction = new ActionNode(SetDoor);
        ActionNode WalkToDoorAction = new ActionNode(WalkToDoor);
        ActionNode LeaveRoomAction = new ActionNode(LeaveRoom);

        Selector RunTowardsOrAttackSelector = new Selector(new List<Node>() { AttackPlayerAction, RunToPlayerAction});
        Sequence PlayerSequence = new Sequence(new List<Node>() { SetPlayerTargetAction, RunTowardsOrAttackSelector });

        Selector WalkTowardsOrProdSelector = new Selector(new List<Node>() { ProdPigAction, WalkToPenAction});
        Sequence PenSequence = new Sequence(new List<Node>() { SetPenTargetAction, WalkTowardsOrProdSelector });

        Selector WalkTowardsOrLeaveSelector = new Selector(new List<Node>() { LeaveRoomAction, WalkToDoorAction });
        Sequence LeaveSequence = new Sequence(new List<Node>() { SetDoorAction, WalkTowardsOrLeaveSelector});

        RootSelector = new Selector(new List<Node>() { PlayerSequence,  PenSequence, LeaveSequence});

        myLineRenderer = GetComponent<LineRenderer>();
        myAnimator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        RootSelector.Evaluate();
        UpdateScentLine();
    }

    private void UpdateScentLine()
    {
        if (scentUpdateTimer <= 0f)
        {
            scentNodes = RoomManager.Instance.pathfinding.FindPath(transform.position, RoomManager.Instance.Player.transform.position);
            scentUpdateTimer = ScentUpdateTime;
            ScentPosition.position = transform.position + Vector3.up * ScentHeight;
            currentScentNode = 0;
        }

        if (scentNodes.Count < 1)
            return;

        scentUpdateTimer -= Time.deltaTime;
        float part = ScentUpdateTime - scentUpdateTimer;
        ScentPosition.position = Vector3.MoveTowards(ScentPosition.position, scentNodes[currentScentNode].vPosition, Time.deltaTime * ScentSpeed);

        if((ScentPosition.position - scentNodes[currentScentNode].vPosition).sqrMagnitude < 0.01f)
        {
            if(currentScentNode + 1 < scentNodes.Count)
                currentScentNode++;
        }
    }

    /// <summary>
    /// If the Target has not yet been set, try to set player as target
    /// </summary>
    /// <returns></returns>
    private NodeStates SetPlayerTarget()
    {
        if (Target == null)
        {
            float dist = ViewDistance;
            foreach(PigPen pen in RoomManager.Instance.Pens) {
                if(!pen.MamaPig.Free)
                    continue;
                float distPig = Vector3.Distance(transform.position, pen.MamaPig.transform.position);

                //If the pig is not within viewing range of the staff member
                if (distPig > ViewDistance)
                    continue;

                Debug.Log("Pig in viewing distance");

                Vector3 pigHeading = pen.MamaPig.transform.position - transform.position;
                bool pigInRangeView = Mathf.Abs(Vector3.Angle(transform.forward, pigHeading)) < ViewAngle;

                if (!pigInRangeView)
                    continue;

                Debug.Log("Pig in view angle");

                RaycastHit pigHit;
                bool raycastHitPig = Physics.Raycast(transform.position, pigHeading.normalized, out pigHit, ViewDistance);

                if (!raycastHitPig)
                    continue;

                Debug.Log("Pig hit by raycast");
                Debug.Log(pigHit.transform.gameObject);
                Debug.Log(distPig);

                if(pigHit.transform.gameObject == pen.MamaPig.gameObject && distPig < dist) {
                    Debug.Log("Set pig target");
                    Target = pen.MamaPig.gameObject;
                }
            }

            float distTarget = Vector3.Distance(transform.position, RoomManager.Instance.Player.transform.position);

            if (Target != null && distTarget > dist)
                return NodeStates.SUCCESS;

            //If the player is not within viewing range of the staff member
            if (distTarget > ViewDistance)
                return NodeStates.FAILURE;

            Vector3 heading = RoomManager.Instance.Player.transform.position - transform.position;
            bool inRangeView = Mathf.Abs(Vector3.Angle(transform.forward, heading)) < ViewAngle;

            //If the player is within the view angle of the staff member
            if (!inRangeView)
                return NodeStates.FAILURE;

            RaycastHit hit;
            bool raycastHit = Physics.Raycast(transform.position, heading.normalized, out hit, ViewDistance);

            if (!raycastHit)
                return NodeStates.FAILURE;

            if (hit.transform.gameObject == RoomManager.Instance.Player) { //If there's nothing in between the staff member and player
                Target = RoomManager.Instance.Player;
            }
        }

        if (Target != null)
            return NodeStates.SUCCESS;

        return NodeStates.FAILURE;
    }

    /// <summary>
    /// If the player is within attack range, attack player
    /// </summary>
    /// <returns></returns>
    private NodeStates AttackPlayer()
    {
        if(Vector3.Distance(transform.position, RoomManager.Instance.Player.transform.position) < AttackRange)
        {
            //Trigger or continue attack animation
            if (Target == RoomManager.Instance.Player)
                RoomManager.Instance.Player.GetComponent<PigController>().GameOver();
            else {
                Destroy(Target);
                Target = null;
            }
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    /// <summary>
    /// Run towards the player
    /// </summary>
    /// <returns></returns>
    private NodeStates RunToPlayer()
    {
        myAnimator.SetTrigger("Run");
        List<GridNode> nodes = RoomManager.Instance.pathfinding.FindPath(transform.position, Target.transform.position);
        if (nodes.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, nodes[0].vPosition, Time.deltaTime * RunSpeed);
            transform.LookAt(nodes[0].vPosition);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, VectorMinHeight(Target.transform.position), Time.deltaTime * RunSpeed);
            transform.LookAt(VectorMinHeight(Target.transform.position));
        }

        return NodeStates.SUCCESS;
    }

    /// <summary>
    /// If no target pen has been set, set a target pen
    /// </summary>
    /// <returns></returns>
    private NodeStates SetPenTarget()
    {
        if (toProd == null)
        {
            foreach (PigPen pen in RoomManager.Instance.Pens)
            {
                if (!pen.Prodded && !pen.BeingProdded)
                {
                    toProd = pen;
                    pen.BeingProdded = true;
                    break;
                }
            }
        }

        if (toProd != null)
            return NodeStates.SUCCESS;

        return NodeStates.FAILURE;
    }

    /// <summary>
    /// Walk towards the target pen
    /// </summary>
    /// <returns></returns>
    private NodeStates WalkToPen()
    {
        myAnimator.SetTrigger("Walk");
        Vector3 targetPosition = VectorMinHeight(toProd.transform.position) + toProd.transform.forward * 1.0f;
        List<GridNode> nodes = RoomManager.Instance.pathfinding.FindPath(transform.position, targetPosition);
        if (nodes.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, nodes[0].vPosition, Time.deltaTime * WalkSpeed);
            transform.LookAt(nodes[0].vPosition);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, VectorMinHeight(toProd.transform.position), Time.deltaTime * WalkSpeed);
            transform.LookAt(VectorMinHeight(toProd.transform.position));
        }

        return NodeStates.SUCCESS;
    }

    /// <summary>
    /// Prod the pig in the pen
    /// </summary>
    /// <returns></returns>
    private NodeStates ProdPig()
    {
        if (Vector3.Distance(transform.position, VectorMinHeight(toProd.transform.position) + toProd.transform.forward * 1.0f) < AttackRange)
        {
            //Trigger or continue prod animation
            toProd.BeingProdded = false;
            toProd.Prodded = true;
            toProd = null;
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    /// <summary>
    /// Find the closest door to leave through
    /// </summary>
    /// <returns></returns>
    private NodeStates SetDoor()
    {
        if (doorToLeaveThrough != null)
            return NodeStates.SUCCESS;

        doorToLeaveThrough = RoomManager.Instance.Doors[0];
        float distance = Vector3.Distance(transform.position, VectorMinHeight(RoomManager.Instance.Doors[0].transform.position));
        foreach (Door door in RoomManager.Instance.Doors)
        {
            float tempdist = Vector3.Distance(transform.position, VectorMinHeight(door.transform.position));
            if(tempdist < distance)
            {
                distance = tempdist;
                doorToLeaveThrough = door;
            }
        }

        return NodeStates.SUCCESS;
    }
    
    /// <summary>
    /// Walk to the door to leave through
    /// </summary>
    /// <returns></returns>
    private NodeStates WalkToDoor()
    {
        myAnimator.SetTrigger("Walk");
        Vector3 targetPosition = VectorMinHeight(doorToLeaveThrough.transform.position) + doorToLeaveThrough.transform.forward * 1.0f;
        List<GridNode> nodes = RoomManager.Instance.pathfinding.FindPath(transform.position, targetPosition);
        if (nodes.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, nodes[0].vPosition, Time.deltaTime * WalkSpeed);
            transform.LookAt(nodes[0].vPosition);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, VectorMinHeight(doorToLeaveThrough.transform.position), Time.deltaTime * WalkSpeed);
            transform.LookAt(VectorMinHeight(doorToLeaveThrough.transform.position));
        }

        return NodeStates.SUCCESS;
    }

    /// <summary>
    /// Leave room if close enough
    /// </summary>
    /// <returns></returns>
    private NodeStates LeaveRoom()
    {
        if (Vector3.Distance(transform.position, VectorMinHeight(doorToLeaveThrough.transform.position) + doorToLeaveThrough.transform.forward * 1.0f) < AttackRange)
        {
            //Leave room
            Destroy(this.gameObject);
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    /// <summary>
    /// Return a vector with the height removed
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    private Vector3 VectorMinHeight(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }
}
