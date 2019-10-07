using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public bool Locked = false;
    public string SceneToLoad;
    public int doorId;
    public static int doorIdOrigin;
    public static bool comeFromDoor = false;

    // Start is called before the first frame update
    void Start()
    {
        RoomManager.AddDoor(this);
    }

    public void LoadScene()
    {
        doorIdOrigin = doorId;
        comeFromDoor = true;
        SceneManager.LoadScene(SceneToLoad);
    }
}
