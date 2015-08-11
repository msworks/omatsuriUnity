using UnityEngine;
using System.Collections;

/// <summary>
/// GameObjectのアクティブ状態を同期する
/// </summary>
public class SyncActive : MonoBehaviour {

    public GameObject reference;
    public GameObject target;
    public bool isExclusive;

	void Update () {
        target.SetActive(reference.activeSelf ^ isExclusive);
	}
}
