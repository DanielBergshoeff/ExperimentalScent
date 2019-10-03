﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
public class ScentManager : MonoBehaviour
{
    public List<GameObject> ScentObjects;
    public List<List<GridNode>> Paths;
    public List<VisualEffect> Scents;

    [SerializeField] private float timePerUpdate = 1.0f;
    [SerializeField] private int maxDistanceScent = 8;
    [SerializeField] private GameObject scentPrefab;

    private float timeTilUpdate = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Paths = new List<List<GridNode>>();

        foreach (GameObject go in ScentObjects)
        {
            Paths.Add(null);
            Scents.Add(Instantiate(scentPrefab).GetComponent<VisualEffect>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        DisplayScentLines();

        timeTilUpdate -= Time.deltaTime;

        if (timeTilUpdate > 0f)
            return;

        timeTilUpdate = timePerUpdate;
        UpdateScentLines();
    }

    private void UpdateScentLines()
    {
        for (int i = 0; i < ScentObjects.Count; i++) //Go through all the scent objects
        {
            Paths[i] = RoomManager.Instance.pathfinding.FindPath(ScentObjects[i].transform.position, transform.position); //Calculate the path from the object to this position
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
            if (!ScentObjects[i].activeSelf) //If the scent object is not active, continue
                continue;

            if (Paths[i] == null)
                continue;

            int intNode = (int)(Paths[i].Count * timer) - 1;
            Vector3 currentNode = Vector3.zero;
            Vector3 targetNode = Vector3.zero;

            if (intNode < 0)
                currentNode = ScentObjects[i].transform.position;
            else
                currentNode = Paths[i][intNode].vPosition;

            if (intNode + 1 > Paths[i].Count - 1)
                targetNode = transform.position;
            else
                targetNode = Paths[i][intNode + 1].vPosition;
            
            Vector3 newPosition = currentNode + (targetNode - currentNode).normalized * ((Paths[i].Count * timer) % 1);
            Scents[i].SetVector3("Position", newPosition);
        }
    }
}
