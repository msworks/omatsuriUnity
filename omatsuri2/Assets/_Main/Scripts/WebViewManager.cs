using UnityEngine;
using System.Collections;

public class WebViewManager : MonoBehaviour {

    public static WebViewManager Instance {
        get { return _instance; }
    }
    private static WebViewManager _instance;

    public string url = "http://web.ee-gaming.net/game/popup1.html";
    public WebViewObject notWinOSWebViewObject;
    public GameObject winOSWebViewObject;
    public int margin = 60;

    void Awake() {
        _instance = this;        
    }

    // Use this for initialization
    public void Open() {
        UIManager.Instance.WebUI.SetActive(true);
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer) {
            OpenOnWinOS();
        } else {
            OpenOnNotWinOS();
        }
    }

    void OpenOnNotWinOS() {
        notWinOSWebViewObject.gameObject.SetActive(true);
        notWinOSWebViewObject.Init((msg) => {
            Debug.Log(msg);
        });
        notWinOSWebViewObject.LoadURL(url);
        notWinOSWebViewObject.SetMargins(margin, margin, margin, margin);
        notWinOSWebViewObject.SetVisibility(true);
    }

    void OpenOnWinOS() {
        winOSWebViewObject.gameObject.SetActive(true);
    }

    public void Close() {
        UIManager.Instance.WebUI.SetActive(true);
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer) {
            CloseOnWinOS();
        } else {
            CloseOnNotWinOS();
        }
    }

    void CloseOnNotWinOS() {
        notWinOSWebViewObject.SetVisibility(false);
        UIManager.Instance.WebUI.SetActive(false);
    }

    void CloseOnWinOS() {
        winOSWebViewObject.gameObject.SetActive(false);
        UIManager.Instance.WebUI.SetActive(false);
    }
}
