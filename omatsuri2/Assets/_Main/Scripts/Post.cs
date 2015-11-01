using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Post : MonoBehaviour
{
    enum MODE
    {
        WEB,
        DESKTOP,
    }

    // WEB - 本番
    // DESKTOP - TEST MODE
    //MODE mode = MODE.WEB;
    MODE mode = MODE.WEB;

    static string serverHead = "http://web.ee-gaming.net/ps/";
    static string logicHead = "http://localhost:9876/";

    static Dictionary<string, string> verbsLogic = new Dictionary<string, string>()
    {
        {"config", "config"},
        {"init", "init"},
        {"play", "play"},
        {"collect", "collect"},
    };

    static Dictionary<string, string> verbsServer = new Dictionary<string, string>()
    {
        {"config", "login.html"},
        {"init", "init.html"},
        {"play", "play.html"},
        {"collect", "collect.html"},
    };

    string head = logicHead;
    Dictionary<string, string> verbs = verbsLogic;

    void Start()
    {
        if( mode==MODE.WEB)
        {
            head = serverHead;
            verbs = verbsServer;
        }
        else if( mode == MODE.DESKTOP)
        {
            head = logicHead;
            verbs = verbsLogic;
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
            post.PostConfig();
        }
    }

    [ActionCategory("Ginpara")]
    public class Init : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            post.PostInit();
        }
    }

    [ActionCategory("Ginpara")]
    public class Play : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            post.PostPlay();
        }
    }

    [ActionCategory("Ginpara")]
    public class Collect : FsmStateAction
    {
        public Post post;

        public override void OnEnter()
        {
            post.PostCollect();
        }
    }

    public void PostConfig()
    {
        var verb = verbs["config"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();

        var param = new Dictionary<string, string>()
        {
            { "gameId", "2" },
            { "userId", "1" },
        };

        PostWWW(
            url,
            param,
            www => {
                Debug.Log(www.text);
                fsm.SendEvent("Succeed");
            },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    public void PostInit()
    {
        var verb = verbs["init"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();
        var param = null as Dictionary<string, string>;

        if (mode == MODE.DESKTOP)
        {
            param = new Dictionary<string, string>()
            {
                { "gameId", "2" },
                { "userId", "1" },
            };
        }
        else if(mode == MODE.WEB)
        {
            param = new Dictionary<string, string>()
            {
            };
        }

        PostWWW(
            url,
            param,
            www =>
            {
                Debug.Log(www.text);
                fsm.SendEvent("Succeed");
            },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    public void PostPlay()
    {
        var verb = verbs["play"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();
        var param = null as Dictionary<string, string>;

        if (mode == MODE.DESKTOP)
        {
            param = new Dictionary<string, string>()
            {
                { "gameId", "2" },
                { "userId", "1" },
                { "betcount", "1" },
                { "rate", "1" },
            };
        }
        else if (mode == MODE.WEB)
        {
            param = new Dictionary<string, string>()
            {
                { "betcount", "1" },
                { "rate", "1" },
            };
        }

        PostWWW(
            url,
            param,
            www =>
            {
                Debug.Log(www.text);
                fsm.SendEvent("Succeed");
            },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    public void PostCollect()
    {
        var verb = verbs["collect"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();
        var param = null as Dictionary<string, string>;

        if (mode == MODE.DESKTOP)
        {
            param = new Dictionary<string, string>()
            {
                { "gameId", "2" },
                { "userId", "1" },
                { "reelstopleft", "1" },
                { "reelstopcenter", "1" },
                { "reelstopright", "1" },
                { "oshijun", "1" },
            };
        }
        else if (mode == MODE.WEB)
        {
            param = new Dictionary<string, string>()
            {
                { "reelstopleft", "1" },
                { "reelstopcenter", "1" },
                { "reelstopright", "1" },
                { "oshijun", "1" },
            };
        }

        PostWWW(
            url,
            param,
            www =>
            {
                Debug.Log(www.text);
                fsm.SendEvent("Succeed");
            },
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
        var fsm = GetComponent<PlayMakerFSM>();
        var url = head + "open.html";

        PostWWW(url, param,
            www => { fsm.SendEvent("Succeed"); },
            www => { fsm.SendEvent("Failed"); }
        );
    }

    /// <summary>
    /// Webページからのレスポンス
    /// </summary>
    /// <param name="msg">gameId=2&token=aaa&language=ja&operatorId=1&mode=1</param>
    public void Response(string msg)
    {
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

        //Debug.Log("PRE HASH:" + s);

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
        //Debug.Log("POST HASH:"+result.ToString());

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

        if (post != null)
        {
            foreach (KeyValuePair<string, string> post_arg in post)
            {
                form.AddField(post_arg.Key, post_arg.Value);
            }
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
