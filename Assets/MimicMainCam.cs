using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicMainCam : MonoBehaviour
{
    private GameObject MainCamera;

    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = MainCamera.transform.rotation;
        transform.position = MainCamera.transform.position;
    }
}
