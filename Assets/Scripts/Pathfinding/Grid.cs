using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public LayerMask WallMask;//This is the mask that the program will look for when trying to find obstructions to the path.
    public Vector2 vGridWorldSize;//A vector2 to store the width and height of the graph in world units.
    public float fNodeRadius;//This stores how big each square on the graph will be
    public float fDistanceBetweenNodes;//The distance that the squares will spawn from eachother.

    public GridNode[,] NodeArray;//The array of nodes that the A Star algorithm uses.
    public List<GridNode> FinalPath;//The completed path that the red line will be drawn along


    public float fNodeDiameter;//Twice the amount of the radius (Set in the start function)
    int iGridSizeX, iGridSizeY;//Size of the Grid in Array units.


    private void Start()//Ran once the program starts
    {
        fNodeDiameter = fNodeRadius * 2;//Double the radius to get diameter
        iGridSizeX = Mathf.RoundToInt(vGridWorldSize.x / fNodeDiameter);//Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        iGridSizeY = Mathf.RoundToInt(vGridWorldSize.y / fNodeDiameter);//Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        CreateGrid();//Draw the grid
    }

    private void Update()
    {
        foreach (GridNode n in NodeArray)
        {
            n.timeNotSeen += Time.deltaTime;
        }
    }

    public List<List<GridNode>> CreateGridPortions(int amtOfPortions)
    {
        int nodesAdded = 0;

        int availableNodes = 0;
        foreach (GridNode n in NodeArray)
        {
            if (n.bIsWall)
                availableNodes++;
        }


        List<List<GridNode>> nodeLists = new List<List<GridNode>>(amtOfPortions);
        for (int i = 0; i < nodeLists.Capacity; i++)
        {
            nodeLists.Add(new List<GridNode>());
        }

        foreach (GridNode n in NodeArray)
        {
            if (n.bIsWall == true && !InList(n, nodeLists))
            {
                float portion = (float)availableNodes / (float)amtOfPortions;

                nodeLists[(int)(nodesAdded / (portion))].Add(n);
                nodesAdded++;
                AddNodesFromNeighbour(n, ref nodeLists, ref nodesAdded, amtOfPortions, availableNodes);
            }
        }

        return nodeLists;
    }

    private void AddNodesFromNeighbour(GridNode n, ref List<List<GridNode>> nodeLists, ref int nodesAdded, int amtOfPortions, int availableNodes)
    {
        List<GridNode> neighbours = GetNeighboringNodes(n);
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (neighbours[i].bIsWall == true && !InList(neighbours[i], nodeLists))
            {
                float portion = (float)availableNodes / (float)amtOfPortions;
                nodeLists[(int)(nodesAdded / (portion))].Add(neighbours[i]);
                nodesAdded++;
                AddNodesFromNeighbour(neighbours[i], ref nodeLists, ref nodesAdded, amtOfPortions, availableNodes);
            }
        }
    }

    private bool InList(GridNode n, List<List<GridNode>> nodeLists)
    {
        foreach (List<GridNode> list in nodeLists)
        {
            foreach (GridNode node in list)
            {
                if (node == n)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void CreateGrid()
    {
        NodeArray = new GridNode[iGridSizeX, iGridSizeY];//Declare the array of nodes.
        Vector3 bottomLeft = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;//Get the real world position of the bottom left of the grid.
        for (int x = 0; x < iGridSizeX; x++)//Loop through the array of nodes.
        {
            for (int y = 0; y < iGridSizeY; y++)//Loop through the array of nodes
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) + Vector3.forward * (y * fNodeDiameter + fNodeRadius);//Get the world co ordinates of the bottom left of the graph
                bool Wall = true;//Make the node a wall


                //If the node is not being obstructed
                //Quick collision check against the current node and anything in the world at its position. If it is colliding with an object with a WallMask,
                //The if statement will return false.
                if (Physics.CheckSphere(worldPoint, fNodeRadius, WallMask))
                {
                    Wall = false;//Object is not a wall
                }

                NodeArray[x, y] = new GridNode(Wall, worldPoint, x, y);//Create a new node in the array.
            }
        }
    }

    //Function that gets the neighboring nodes of the given node.
    public List<GridNode> GetNeighboringNodes(GridNode a_NeighborNode)
    {
        List<GridNode> NeighborList = new List<GridNode>();//Make a new list of all available neighbors.
        int icheckX;//Variable to check if the XPosition is within range of the node array to avoid out of range errors.
        int icheckY;//Variable to check if the YPosition is within range of the node array to avoid out of range errors.

        int[] gridXNeighbours = new int[4] { 1, -1, 0, 0 };
        int[] gridYNeighbours = new int[4] { 0, 0, 1, -1 };

        for (int i = 0; i < gridXNeighbours.Length; i++)
        {
            icheckX = a_NeighborNode.iGridX + gridXNeighbours[i];
            icheckY = a_NeighborNode.iGridY + gridYNeighbours[i];
            if (icheckX >= 0 && icheckX < iGridSizeX && icheckY >= 0 && icheckY < iGridSizeY)
            {
                NeighborList.Add(NodeArray[icheckX, icheckY]);
            }
        }

        return NeighborList;//Return the neighbors list.
    }

    //Gets the closest node to the given world position.
    public GridNode NodeFromWorldPoint(Vector3 a_vWorldPos)
    {

        float ixPos = ((a_vWorldPos.x - this.transform.position.x + vGridWorldSize.x / 2) / vGridWorldSize.x);
        float iyPos = ((a_vWorldPos.z - this.transform.position.z + vGridWorldSize.y / 2) / vGridWorldSize.y);

        ixPos = Mathf.Clamp01(ixPos);
        iyPos = Mathf.Clamp01(iyPos);

        int ix = Mathf.RoundToInt((iGridSizeX - 1) * ixPos);
        int iy = Mathf.RoundToInt((iGridSizeY - 1) * iyPos);

        return NodeArray[ix, iy];
    }


    //Function that draws the wireframe
    private void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(transform.position, new Vector3(vGridWorldSize.x, 1, vGridWorldSize.y));//Draw a wire cube with the given dimensions from the Unity inspector
    }

    public List<GridNode> GetAvailableNodes()
    {
        List<GridNode> availableNodes = new List<GridNode>();
        for (int i = 0; i < NodeArray.GetLength(0); i++)
        {
            for (int j = 0; j < NodeArray.GetLength(1); j++)
            {
                if (NodeArray[i, j].bIsWall == true)
                {
                    availableNodes.Add(NodeArray[i, j]);
                }
            }
        }

        return availableNodes;
    }
}