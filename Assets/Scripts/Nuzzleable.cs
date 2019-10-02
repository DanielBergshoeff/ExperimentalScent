using UnityEngine;
using UnityEngine.Events;

public class Nuzzleable : MonoBehaviour
{
    public UnityEvent NuzzleEvent;

    public Animator MyAnimator;

    public string TriggerName;

    private void Start() {

    }

    public void TriggerAnimation()
    {
        NuzzleEvent.Invoke();
        MyAnimator.SetTrigger(TriggerName);
    }
}
