using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PigPen : MonoBehaviour
{
    public static Dictionary<CycleStage, int> TurnsPerStage = new Dictionary<CycleStage, int>() {
        { CycleStage.NewSow, 3 },
        { CycleStage.Insemination, 3 },
        { CycleStage.Piglets, 3 },
        { CycleStage.PigletRemoval, 3},
        { CycleStage.Empty, 3 }
    };

    public bool Prodded;
    public bool BeingProdded;
    public Pig MamaPig;
    public GameObject Piglets;

    public GameObject inseminationCross1;
    public GameObject inseminationCross2;
    public GameObject inseminationCross3;

    public CycleStage CurrentStage = CycleStage.NewSow;
    public int StageDifference = 0;

    public Nuzzleable nuzzleable;

    void Start() {
        RoomManager.AddPigPen(this);
        if (nuzzleable.NuzzleEvent == null)
            nuzzleable.NuzzleEvent = new UnityEvent();
        nuzzleable.NuzzleEvent.AddListener(FreePig);

        int turn = GameManager.TurnsUsed - StageDifference;
        int inseminationCycle = 0;
        CycleStage stage = CycleStage.NewSow;

        while (turn > 0) {
            if (turn - TurnsPerStage[stage] >= 0) {
                turn -= TurnsPerStage[stage];
                if ((int)stage + 2 >= TurnsPerStage.Count && inseminationCycle < 3) {
                    stage = 0;
                }
                else if ((int)stage + 1 >= TurnsPerStage.Count) {
                    stage = 0;
                    inseminationCycle = 0;
                }
                else
                    stage += 1;
                if (stage == CycleStage.Insemination)
                    inseminationCycle++;
            }
        }

        CurrentStage = stage;

        if (CurrentStage == CycleStage.NewSow || CurrentStage == CycleStage.Insemination || CurrentStage == CycleStage.PigletRemoval) {
            MamaPig.gameObject.SetActive(true);
            Piglets.SetActive(false);
        }
        else if(CurrentStage == CycleStage.Piglets) {
            MamaPig.gameObject.SetActive(true);
            Piglets.SetActive(true);
        }
        else if(CurrentStage == CycleStage.Empty) {
            MamaPig.gameObject.SetActive(false);
            Piglets.SetActive(false);
        }

        inseminationCross1.SetActive(false);
        inseminationCross2.SetActive(false);
        inseminationCross3.SetActive(false);

        if (inseminationCycle == 0)
            return;

        if (inseminationCycle > 0) 
            inseminationCross1.SetActive(true);
        if (inseminationCycle > 1)
            inseminationCross2.SetActive(true);
        if (inseminationCycle > 2)
            inseminationCross3.SetActive(true);
    }

    public void FreePig() {
        MamaPig.Free = true;
    }
}


public enum CycleStage {
    NewSow,
    Insemination,
    Piglets,
    PigletRemoval,
    Empty
}
