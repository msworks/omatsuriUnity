using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour {

    private Text text;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        text.text = "";
        text.text += ZZ.innerHitPattern;
        foreach (string s in ZZ.reelStopStatus) {
            if(s != null) text.text += s + System.Environment.NewLine;
        }
	}
}
