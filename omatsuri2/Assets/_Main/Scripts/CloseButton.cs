using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public GameObject WebButton;

    [SerializeField]
    GameObject MenuPopup;

    public void OnClick()
    {
        // ポーズを解除する
        GameManager.Instance.pauseState = GameManager.PauseStatus.Play;
    }
}
