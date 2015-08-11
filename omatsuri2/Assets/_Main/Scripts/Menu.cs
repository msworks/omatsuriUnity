using UnityEngine;
using UnityEngine.UI;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Menu : MonoBehaviour {

    static Menu _instance;
    static public Menu Instance {get{return _instance;}}

    [SerializeField]
    Image image;

    [SerializeField]
    Sprite second;

    void Start()
    {
        _instance = this;
    }

    public void OnTap()
    {
        GetComponent<PlayMakerFSM>().SendEvent("TAP");
    }

    public Menu DisplayStartPopup()
    {
        return this;
    }

    public Menu Next()
    {
        image.sprite = second;
        return this;
    }

    [ActionCategory("Ginpara")]
    public class LoadResource : FsmStateAction
    {
	    public override void OnEnter()
	    {
		    Finish();
	    }
    }

    [ActionCategory("Ginpara")]
    public class DisplayNext : FsmStateAction
    {
        public override void OnEnter()
        {
            Menu.Instance.Next();
            Finish();
        }
    }
}
