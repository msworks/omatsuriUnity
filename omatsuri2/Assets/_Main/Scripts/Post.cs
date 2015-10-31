using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Post : MonoBehaviour
{
    string head = "http://web.ee-gaming.net/ps/";

    enum MODE
    {
        WEB,
        DESKTOP,
    }

    MODE mode = MODE.DESKTOP;

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
    public class Open : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            post._Open();
        }
    }

    [ActionCategory("Ginpara")]
    public class ConnectionFailed : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            Application.ExternalCall("AlertByUnity", "ConnectionFailed");
        }
    }

    [ActionCategory("Ginpara")]
    public class Config : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
        }
    }

    public void PostConfig()
    {
        var url = head + "login.html";
        var fsm = GetComponent<PlayMakerFSM>();

        PostWWW(url,
            HashCalculation(new Dictionary<string, string>()
            {
                { "gameId", "1" },
                { "login", "ttakekawa@manasoft.co.jp" },
                { "password", "L18mmTR3" },
                { "providerId", "33" },
                { "siteId", "1" }
            }, "test123"),
            www => { fsm.SendEvent("Succeed"); },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    /// <summary>
    /// ログイン
    /// </summary>
    public void _Open()
    {
        var fsm = GetComponent<PlayMakerFSM>();

        if (mode == MODE.DESKTOP)
        {
            // TEST CODE
            var WalletApi = "http://web.ee-gaming.net/apis/wallet1_1/";
            var authenticate = WalletApi + "login.html";

            PostWWW(authenticate,
                HashCalculation(new Dictionary<string, string>()
                {
                    { "gameId", "2" },
                    { "login", "ttakekawa@manasoft.co.jp" },
                    { "password", "L18mmTR3" },
                    { "providerId", "33" },
                    { "siteId", "1" }
                }, "test123"),
                www => { 
                    var text = www.text;

                    // {"token":"HQWQQ5D9IUQC70KKXK5ZFFHFPPG36TQ9"}
                    var json = new JSONObject(text);
                    var token = json.GetField("token").str;

                    //gameId=2&token=aaa&language=ja&operatorId=1&mode=1

                    var msg = string.Format("gameId=2&token={0}&language=ja&operatorId=1&mode=1", token);

                    var kvs = msg.Split('&')
                               .Select(query => query.Split('='))
                               .Select(strings => new KeyValuePair<string, string>(strings[0], strings[1]));

                    var param = new Dictionary<string, string>();
                    foreach (var kv in kvs)
                    {
                        param.Add(kv.Key, kv.Value);
                    }

                    PostOpen(param);
                },
                www => { fsm.SendEvent("Failed"); }
            );
        }
        else if (mode == MODE.WEB)
        {
            // Webページに対してパラメータ送信要求
            Application.ExternalCall("GetParameter");
        }
        else
        {
            fsm.SendEvent("Failed");
        }
    }

    public void PostOpen(Dictionary<string, string> param)
    {
        // http://web.ee-gaming.net/ps/open.html?gameId=1&operator=1&token=abc&language=en&mode=1

        var fsm = GetComponent<PlayMakerFSM>();
        var url = head + "open.html";

        PostWWW(url, param,
            www => { fsm.SendEvent("Succeed"); },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    /// <summary>
    /// WEBページからのレスポンス
    /// </summary>
    public void Response(string msg)
    {
        //[msg]
        //gameId=2&token=aaa&language=ja&operatorId=1&mode=1

        // デバッグ用にアラートを出す
        //Application.ExternalCall("AlertByUnity", msg);

        var param = new Dictionary<string, string>();

        var kvs = msg.Split('&')
           .Select(query => query.Split('='))
           .Select(strings => new KeyValuePair<string, string>(strings[0], strings[1]));

        foreach(var kv in kvs)
        {
            param.Add(kv.Key, kv.Value);
        }

        // OpenをPOST
        PostOpen(param);
    }

    Dictionary<string, string> HashCalculation(Dictionary<string, string> i, string himitsu)
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

        var s = f() + himitsu;

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

    void PostWWW(
        string url,
        Dictionary<string, string> post,
        Action<WWW> success,
        Action<WWW> failed
    )
    {
        StartCoroutine(PostWWWCore(url, post, success, failed));
    }

    IEnumerator PostWWWCore(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
        Debug.Log("POST:url=" + url);

        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> post_arg in post)
        {
            form.AddField(post_arg.Key, post_arg.Value);
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
}
