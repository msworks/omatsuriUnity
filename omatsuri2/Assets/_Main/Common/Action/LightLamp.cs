using UnityEngine;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ActionCategory("Ginpara")]
public class LightLampPM : FsmStateAction
{
    public GameObject Lamp;
	
	// Code that runs on entering the state.
	public override void OnEnter()
	{
        Lamp.GetComponent<LightLamp>().ON();
		Finish();
	}
}

[ActionCategory("Ginpara")]
public class LightLampOffPM : FsmStateAction
{
    public GameObject Lamp;

    // Code that runs on entering the state.
    public override void OnEnter()
    {
        Lamp.GetComponent<LightLamp>().OFF();

        Finish();
    }
}
