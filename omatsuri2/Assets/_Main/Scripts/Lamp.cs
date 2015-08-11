using UnityEngine;
using System.Collections;

public class Lamp : MonoBehaviour {

    public Texture2D lit;
    public Texture2D unLit;

    public void SetLamp(bool isLit) {
        if (isLit) {
            GetComponent<Renderer>().material.mainTexture = lit;
        } else {
            GetComponent<Renderer>().material.mainTexture = unLit;
        }
    }

}
