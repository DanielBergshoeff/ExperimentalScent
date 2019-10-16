using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
public class ScentManager : MonoBehaviour
{
    public static ScentManager Instance;

    public List<GameObject> ScentObjects;
    public List<List<GridNode>> Paths;
    public List<VisualEffect> Scents;

    [SerializeField] private float timePerUpdate = 1.0f;
    [SerializeField] private int maxDistanceScent = 8;
    [SerializeField] private GameObject scentPrefab;

    private float timeTilUpdate = 0f;

    private bool smelling = false;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    private void InstantiateScentLines() {
        Paths = new List<List<GridNode>>();
        Scents = new List<VisualEffect>();

        foreach (GameObject go in ScentObjects) {
            Paths.Add(null);
            Scents.Add(Instantiate(scentPrefab).GetComponent<VisualEffect>());
        }

        UpdateScentLines();
    }

    private void DestroyScentLines() {
        foreach(VisualEffect vfx in Scents) {
            Destroy(vfx.gameObject);
        }
    }

    private void StopSmelling() {
        DestroyScentLines();
        smelling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !smelling) {
            InstantiateScentLines();
            smelling = true;
            timeTilUpdate = timePerUpdate;
            Invoke("StopSmelling", timePerUpdate);
        }

        if (!smelling)
            return;

        DisplayScentLines();

        timeTilUpdate -= Time.deltaTime;

        /*if (timeTilUpdate > 0f)
            return;
        
        timeTilUpdate = timePerUpdate;
        UpdateScentLines();*/
    }

    public void RemoveScentObject(GameObject scentObject)
    {
        if (!ScentObjects.Contains(scentObject))
            return;

        int index = ScentObjects.IndexOf(scentObject);

        VisualEffect vfx = Scents[index];
        Scents.Remove(vfx);

        GameObject go = ScentObjects[index];
        ScentObjects.Remove(go);

        List<GridNode> gridNodes = Paths[index];
        Paths.Remove(gridNodes);

        Destroy(vfx.gameObject);
        Destroy(go);
    }

    private void UpdateScentLines()
    {
        for (int i = 0; i < ScentObjects.Count; i++) //Go through all the scent objects
        {
            Paths[i] = RoomManager.Instance.pathfinding.FindPath(transform.position, ScentObjects[i].transform.position); //Calculate the path from the object to this position
            if (Paths[i].Count <= maxDistanceScent)
                continue;

            Paths[i].RemoveRange(maxDistanceScent, Paths[i].Count - maxDistanceScent);
        }
    }

    private void DisplayScentLines()
    {
        float timer = 1f - (timeTilUpdate / timePerUpdate);

        for (int i = 0; i < ScentObjects.Count; i++) //Go through all the scent objects
        {
            if (ScentObjects[i] == null) //If the scent object is null
                continue;

            if (!ScentObjects[i].activeSelf) //If the scent object is not active, continue
                continue;

            if (Paths[i] == null) //If there is no path
                continue;

            int intNode = (int)(Paths[i].Count * timer) - 1;
            Vector3 currentNode = Vector3.zero;
            Vector3 targetNode = Vector3.zero;

            if (intNode < 0)
                currentNode = transform.position;
            else
                currentNode = Paths[i][intNode].vPosition;

            if (intNode + 1 > Paths[i].Count - 1)
                targetNode = ScentObjects[i].transform.position;
            else
                targetNode = Paths[i][intNode + 1].vPosition;
            
            Vector3 newPosition = currentNode + (targetNode - currentNode).normalized * ((Paths[i].Count * timer) % 1);
            Scents[i].SetVector3("Position", newPosition);
        }
    }
}
