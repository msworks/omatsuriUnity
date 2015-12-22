using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class SlotMachineState : MonoBehaviour
{
    PlayMakerFSM fsm;

    [SerializeField]
    int Bet { get; set; }

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
        Bet++;
        if (Bet == 3)
        {
            fsm.SendEvent("BetEnd");
        }
    }

    public void PlayEnd()
    {
        fsm.SendEvent("PlayEnd");
    }

    [ActionCategory("Ginpara")]
    public class ClearBet : FsmStateAction
    {
        public SlotMachineState SlotMachineState;

        public override void OnEnter()
        {
            SlotMachineState.Bet = 0;
            Finish();
        }
    }

    [ActionCategory("Ginpara")]
    public class GetBetCount : FsmStateAction
    {
        public SlotMachineState SlotMachineState;
        public FsmInt BetCount;

        public override void OnEnter()
        {
            BetCount.Value = SlotMachineState.Bet;
            Finish();
        }
    }
}
