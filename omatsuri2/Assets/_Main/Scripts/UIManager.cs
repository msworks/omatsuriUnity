using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour {
    public static UIManager Instance {
        get { return _instance; }
    }
    private static UIManager _instance;

    public Text debugText;
    public Text errorText;
    public Text rateText;
    public Toggle autoPlayToggle;
    public Toggle semiAutoPlayToggle;
    public Toggle forceAutoPlayToggle;
    public GameObject PlayUI;
    public GameObject MenuUI;
    public GameObject WebUI;

    void Awake() {
        _instance = this;
    }

    public void OpenMenu() {
        PlayUI.SetActive(false);
        MenuUI.SetActive(true);
    }

    public void CloseMenu() {
        PlayUI.SetActive(true);
        MenuUI.SetActive(false);
    }

    public void Reset()
    {
        GameManager.Instance.PushBlankPlayData();
        Application.LoadLevel("Main");
    }

    /// <summary>
    /// 次に当たる役を強制的に設定する。
    /// </summary>
    public void SetForceYakuFlag(int flag) {
        GameManager.forceYakuValue = (Defines.ForceYakuFlag)flag;
    }
}
