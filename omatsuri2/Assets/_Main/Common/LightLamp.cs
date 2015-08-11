using HutongGames.PlayMaker;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightLamp : MonoBehaviour {

    static LightLamp _instance;
    static public LightLamp Instance {get{return _instance;}}

    public GameObject right;
    public GameObject left;

    private bool LightFlg;
    private int Counter = 0;

	void Start () {
        _instance = this;
        LightFlg = false;
        Counter = 0;

        OFF();
	}
	
	void Update () {
        if (!LightFlg) return;

        Counter++;

        var r = (float)Counter * 3.14f / 60f * 10f;
        var v = Mathf.Sin(r);
        var v2 = v * -1f;

        right.GetComponent<UISprite>().alpha = v;
        left.GetComponent<UISprite>().alpha = v2;

	}

    public void ON()
    {
        LightFlg = true;
        Counter = 0;
    }

    public void OFF()
    {
        LightFlg = false;
        Counter = 0;
        right.GetComponent<UISprite>().alpha = 0;
        left.GetComponent<UISprite>().alpha = 0;
    }

    [ActionCategory("Ginpara")]
    public class DataLampOn : FsmStateAction
    {
        public override void OnEnter()
        {
            LightLamp.Instance.ON();
            Finish();
        }
    }

    [ActionCategory("Ginpara")]
    public class DataLampOff : FsmStateAction
    {
        public FsmFloat delayTime;

        public override void OnEnter()
        {
            LightLamp.Instance.OFF();
            Finish();
        }
    }
}
