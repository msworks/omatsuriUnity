using UnityEngine;
using System.Collections;

public class SlotMachineState : MonoBehaviour
{
    PlayMakerFSM fsm;

    void Start()
    {
        fsm = GetComponent<PlayMakerFSM>();
    }

    public void ConfigFinished()
    {
        fsm.SendEvent("ConfigFinished");
    }

    public void Lever()
    {
        fsm.SendEvent("Lever");
    }

    public void InsertCoin()
    {
        fsm.SendEvent("InsertCoin");
    }

    public void PlayEnd()
    {
        fsm.SendEvent("PlayEnd");
    }
}
