using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReelBuilder : MonoBehaviour {

    public GameObject facePrefab;
    public float distance = -125;
    public GameObject[] faces = new GameObject[21];

	// Use this for initialization
	void Awake () {
        for (int idx = 20; idx >= 0; idx--) {
            GameObject o = (GameObject)Instantiate(facePrefab);
            o.transform.SetParent(transform);
            o.transform.localScale = new Vector3(1f, 1f, 1f);
            o.transform.Rotate(transform.right, idx * 360f/21f, Space.World);
            o.transform.localPosition = o.transform.forward * distance;
            faces[idx] = o;
        }
	}
}
