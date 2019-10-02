using UnityEngine;
using UnityEngine.UI;

public class PigController : MonoBehaviour
{
    public Camera FirstPersonCam;
    public float InteractDistance = 2.0f;
    public KeyCode InteractionKey;

    public GameObject DoorPrompt;
    public GameObject NuzzlePrompt;
    public GameObject GameOverText;

    private void Start() {
        GameOverText.SetActive(false);
    }

    private void Update()
    {
        TurnOffPrompts();
        RaycastForward();
    }

    private void TurnOffPrompts()
    {
        DoorPrompt.SetActive(false);
        NuzzlePrompt.SetActive(false);
    }

    private void RaycastForward()
    {
        RaycastHit hit;
        //Raycast forward with length InteractDistance
        Ray ray = new Ray(FirstPersonCam.transform.position, FirstPersonCam.transform.forward);
        if (Physics.Raycast(ray, out hit, InteractDistance)) 
        {
            //If a door is hit
            if (hit.transform.tag == "Door")
            {
                DoorPrompt.SetActive(true);

                //If the interaction key is pressed
                if (Input.GetKeyDown(InteractionKey))
                {
                    Door door = hit.transform.GetComponent<Door>();
                    //If the door is not locked
                    if (!door.Locked)
                    {
                        //Load new scene and add turn
                        GameManager.TurnsUsed++;
                        door.LoadScene();
                    }
                }
            }

            //If a nuzzleable object is hit
            else if(hit.transform.tag == "Nuzzleable")
            {
                NuzzlePrompt.SetActive(true);

                //If the interaction key is pressed
                if (Input.GetKeyDown(InteractionKey))
                {
                    //Trigger nuzzle animation
                    Nuzzleable nuzzleable = hit.transform.GetComponent<Nuzzleable>();
                    nuzzleable.TriggerAnimation();
                }
            }

            //If a pig is hit
            else if(hit.transform.tag == "Pig") {
                NuzzlePrompt.SetActive(true);

                //If the interaction key is pressed
                if (Input.GetKeyDown(InteractionKey)) {
                    Pig pig = hit.transform.GetComponent<Pig>();
                    pig.Nuzzled = true;
                }
            }
        }
    }

    public void GameOver() {
        GameOverText.SetActive(true);
    }
}
