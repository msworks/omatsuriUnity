using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Post : MonoBehaviour
{
    private string URI = "http://web.ee-gaming.net/game/";

    public void Quit()
    {
        Application.Quit();
    }

    public Post StartCommand(FsmEvent success, FsmEvent failed)
    {

        var postURI = URI + "Start.json";
        var fsm = GetComponent<PlayMakerFSM>();

        POST(postURI,
             new Dictionary<string, string>(){
                { "sv", "ohana" },
                { "ap", "1" } },
             www =>
             {
                 Debug.Log(www.text);
                 fsm.SendEvent(success.Name);
             },
             www =>
             {
                 Debug.Log(www.error);
                 fsm.SendEvent(failed.Name);
             }
        );

        return this;
    }

    public Post UpdateCommand(FsmEvent success, FsmEvent failed)
    {
        var postURI = URI + "Update.json";
        var fsm = GetComponent<PlayMakerFSM>();

        POST(postURI,
             new Dictionary<string, string>(){
                { "sv", "ohana" },
                { "ap", "1" },
                { "id", "000001"},
                { "cval", "255" },
                { "stat", "0" },
                { "count", "99" },
                { "dat", "0,0,0,0,1,1,1,1,1,1" },
             },
             www =>
             {
                 Debug.Log(www.text);
                 fsm.SendEvent(success.Name);
             },
             www =>
             {
                 Debug.Log(www.error);
                 fsm.SendEvent(failed.Name);
             }
        );

        return this;
    }

    public Post EndCommand(FsmEvent success, FsmEvent failed)
    {
        var postURI = URI + "End.json";
        var fsm = GetComponent<PlayMakerFSM>();

        POST(postURI,
             new Dictionary<string, string>(){
                { "sv", "ohana" },
                { "ap", "1" },
                { "id", "000001"},
                { "cval", "255" },
                { "stat", "0" },
                { "count", "99" },
                { "hall", "1" },
                { "dai", "255" },
                { "cd", "123456789" },
                { "dat", "0,0,0,0,1,1,1,1,1,1" },
             },
             www =>
             {
                 Debug.Log(www.text);
                 fsm.SendEvent(success.Name);
             },
             www =>
             {
                 Debug.Log(www.error);
                 fsm.SendEvent(failed.Name);
             }
        );

        return this;
    }

    /// <summary>
    /// ログイン（テスト）
    /// ログインの目的はtokenを得ること。
    /// Web版ではResponseでtokenが得られるので、
    /// 本番では使わない。
    /// </summary>
    public void TestLogin()
    {
        // Wallet Api 7 POST / authenticate.html
        var WalletApi = "http://web.ee-gaming.net/apis/wallet1_1/";
        var authenticate = WalletApi + "login.html";

        POST(authenticate,
            HashCalculation(new Dictionary<string, string>()
            {
                { "gameId", "1" },
                //{ "key1", "value1" },
                //{ "key2", "value2SECRET" },
                { "login", "ttakekawa@manasoft.co.jp" },
                { "password", "L18mmTR3" },
                { "providerId", "33" },
                { "siteId", "1" }
            }),
            www => { Debug.Log(www.text); },
            www => { Debug.LogError(www.text); }
        );
    }

    /// <summary>
    /// WEBページからのレスポンス
    /// </summary>
    public void Response(string msg)
    {
        // デバッグ用にアラートを出す
        Application.ExternalCall("AlertByUnity", msg);

        // レスポンスからtokenを取り出す
        var token = msg.Split('=')[1];

        // Wallet Api 7 POST / authenticate.html
        var WalletApi = "http://web.finnplay.com/apis/wallet/";
        var authenticate = WalletApi + "authenticate.html";

        POST(authenticate,
            HashCalculation( new Dictionary<string, string>()
            {
                { "token", token },
                { "providerId", "10" }
            }),
            www => { Application.ExternalCall("AlertByUnity", www.text); },
            www => { Application.ExternalCall("AlertByUnity", www.text); }
        );
    }

    Dictionary<string, string> HashCalculation(Dictionary<string, string> i)
    {
        Func<string> f = () =>
        {
            var list = new List<string>();
            foreach (var l in i)
            {
                list.Add(l.Key + "=" + l.Value);
            }
            return String.Join("&", list.ToArray());
        };

        var s = f();

        Debug.Log("PRE HASH:" + s);

        var data = System.Text.Encoding.UTF8.GetBytes(s);
        var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        var bs = md5.ComputeHash(data);
        md5.Clear();
        var result = new System.Text.StringBuilder();
        foreach (byte b in bs)
        {
            result.Append(b.ToString("x2"));
        }

        //結果を表示
        Debug.Log("POST HASH:"+result.ToString());

        i.Add("hash", result.ToString());

        return i;
    }

    private void POST(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
        StartCoroutine(PostCore(url, post, success, failed));
    }

    private IEnumerator PostCore(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
        Debug.Log("POST:url=" + url);

        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> post_arg in post)
        {
            form.AddField(post_arg.Key, post_arg.Value);

            Debug.Log("POST:" + post_arg.Key + "=" + post_arg.Value);
        }
        WWW www = new WWW(url, form);

        yield return www;

        if (www.error == null)
        {
            success(www);
        }
        else
        {
            failed(www);
        }
    }

    [ActionCategory("Ginpara")]
    public class PostStart : FsmStateAction
    {
        public Post post;
        public FsmEvent success;
        public FsmEvent failed;

        public override void OnEnter()
        {
            post.StartCommand(success, failed);
        }
    }

    [ActionCategory("Ginpara")]
    public class PostUpdate : FsmStateAction
    {
        public Post post;
        public FsmEvent success;
        public FsmEvent failed;

        public override void OnEnter()
        {
            post.UpdateCommand(success, failed);
        }
    }

    [ActionCategory("Ginpara")]
    public class PostEnd : FsmStateAction
    {
        public Post post;
        public FsmEvent success;
        public FsmEvent failed;

        public override void OnEnter()
        {
            post.EndCommand(success, failed);
        }
    }

    [ActionCategory("Ginpara")]
    public class GetParameter : FsmStateAction
    {
        public override void OnEnter()
        {
            // Webページに対してパラメータ送信要求
            Application.ExternalCall("GetParameter");
        }
    }

    [ActionCategory("Ginpara")]
    public class Login : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            post.TestLogin();
        }
    }
}
