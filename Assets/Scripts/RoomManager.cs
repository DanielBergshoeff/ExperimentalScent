using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<Door> Doors;
    public List<PigPen> Pens;
    public static RoomManager Instance;

    public GameObject WorkerPrefab;
    public List<Worker> Workers;

    public GameObject Player;
    public Pathfinding pathfinding;

    public bool SpawnWorkers = false;
    public float MinTimeSpawn = 60.0f;
    public float MaxTimeSpawn = 300.0f;
    public float TimeTillSpawn = 0f;

    private bool setPlayerPosition = true;

    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;
        Doors = new List<Door>();
        TimeTillSpawn = Random.Range(MinTimeSpawn, MaxTimeSpawn);
        Pens = new List<PigPen>();
        Workers = new List<Worker>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (setPlayerPosition)
        {
            if(Door.comeFromDoor)
            {
                if(Pens.Count > 0) {

                }

                //Set player position based on door id
                foreach(Door d in Doors)
                {
                    if(d.doorId == Door.doorIdOrigin)
                    {
                        Player.GetComponent<CharacterController>().enabled = false;
                        Player.transform.position = d.transform.position + d.transform.forward * 1f;
                        Player.GetComponent<CharacterController>().enabled = true;
                    }
                }
            }
            setPlayerPosition = false;
        }

        if (!SpawnWorkers)
            return;

        TimeTillSpawn -= Time.deltaTime;
        if(TimeTillSpawn <= 0f)
        {
            TimeTillSpawn = Random.Range(MinTimeSpawn, MaxTimeSpawn);
            SpawnWorker();
        }
    }

    public static void AddDoor(Door door)
    {
        Instance.Doors.Add(door);
    }

    public static void AddPigPen(PigPen pigPen)
    {
        Instance.Pens.Add(pigPen);
    }

    private void SpawnWorker()
    {
        BehaviourTree worker = Instantiate(WorkerPrefab).GetComponent<BehaviourTree>();
        Door door = Doors[Random.Range(0, Doors.Count - 1)];
        worker.transform.position = door.transform.position;
    }
}
