using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuButton : MonoBehaviour {

    public UISprite lightButton;

    [SerializeField]
    GameObject MenuPopup;

    [SerializeField]
    List<GameObject> DisplayPanels;

    [SerializeField]
    List<GameObject> HidePanels;

    /// <summary>
    /// タップ時処理
    /// </summary>
    public void OnClick()
    {
        // アプリをポーズ状態にする
        GameManager.Instance.PauseState = GameManager.PAUSE_STATE.PAUSE;

        StartCoroutine(light());

        foreach(var panel in DisplayPanels)
        {
            panel.SetActive(true);
        }

        foreach (var panel in HidePanels)
        {
            panel.SetActive(false);
        }

        MenuPopup.SetActive(true);
    }

    IEnumerator light()
    {
        var totalTime = 0.5f;
        var count = 0.0f;
        lightButton.alpha = 1.0f;

        while (count < totalTime)
        {
            count += Time.deltaTime;
            lightButton.alpha = 1.0f - count / totalTime;
            yield return null;
        }

        lightButton.alpha = 0.0f;

        yield return null;
    }

}
