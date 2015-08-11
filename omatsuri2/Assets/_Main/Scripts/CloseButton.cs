using UnityEngine;
using System.Collections;

public class CloseButton : MonoBehaviour {

    public GameObject WebButton;

    [SerializeField]
    GameObject MenuPopup;

    public void OnClick()
    {
        // ポーズを解除する
        GameManager.Instance.PauseState = GameManager.PAUSE_STATE.PLAY;
    }
}
