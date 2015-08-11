using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextMirror : MonoBehaviour {

    private Text text;
    public Text reference;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        text.text = reference.text;
	}
}
