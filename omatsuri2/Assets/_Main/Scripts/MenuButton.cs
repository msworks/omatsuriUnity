using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButton : MonoBehaviour
{
    public UISprite lightButton;

    [SerializeField]
    GameObject MenuPopup;

    [SerializeField]
    MainPanel mainPanel;

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
        GameManager.Instance.pauseState = GameManager.PauseStatus.Pause;

        StartCoroutine(lighting());

        foreach(var panel in DisplayPanels)
        {
            panel.SetActive(true);
        }

        foreach (var panel in HidePanels)
        {
            panel.SetActive(false);
        }

        mainPanel.Display();

        MenuPopup.SetActive(true);
    }

    IEnumerator lighting()
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
