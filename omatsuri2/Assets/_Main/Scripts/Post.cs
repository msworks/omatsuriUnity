using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

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
    MODE mode = MODE.DESKTOP;

    static string serverHead = "http://web.ee-gaming.net/ps/pachinko/";
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
        {"config", "config.html"},
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
        public FsmInt bet;

        public override void OnEnter()
        {
            post.PostPlay(bet.Value);
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

    /// <summary>
    /// サーバーにConfigコマンド発行
    /// </summary>
    public void PostConfig()
    {
        var verb = verbs["config"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();

        var webParam = new Dictionary<string, string>()
        {
        };

        var desktopParam = new Dictionary<string, string>()
        {
            { "gameId", "2" },
            { "userId", "1" },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            Debug.Log(www.text);
            var json = new JSONObject(www.text);
            var setting = json.GetField("setting").ToString().ParseInt();
            var reelleft = json.GetField("reelleft").ToString().ParseInt();
            var reelcenter = json.GetField("reelcenter").ToString().ParseInt();
            var reelright = json.GetField("reelright").ToString().ParseInt();
            var seed = json.GetField("seed").ToString().ParseInt();

            clOHHB_V23.mInitializaion(seed);
            clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)setting);

            fsm.SendEvent("Succeed");
        };

        Action<WWW> webAction = (www) =>
        {
            Debug.Log(www.text);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(www.text));
            var res = xmlDoc.GetElementsByTagName("response");
            var associate = new Dictionary<string, string>();
            foreach (XmlNode node in res[0].ChildNodes)
            {
                associate.Add(node.Name, node.InnerText);
            }

            var balance = Decimal.Parse(associate["balance"].ToString());
            var setting = associate["setting"].ToString().ParseInt();
            var reelleft = associate["reelLeft"].ToString().ParseInt();
            var reelcenter = associate["reelCenter"].ToString().ParseInt();
            var reelright = associate["reelRight"].ToString().ParseInt();
            var seed = associate["seed"].ToString().ParseInt();

            clOHHB_V23.mInitializaion(seed);
            clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)setting);

            // コインチャージ
            // balance  : 所持金($)
            // balanceCent : 所持金（セント）
            // rateCent : コイン１毎あたりの値段（単位：セント）
            // coinNum  : コイン枚数
            // coinNum = balanceCent / rateCent
            var rateCent = Rate.Instanse.GetRate();
            var balanceCent = balance * 100;
            var coinNum = (int)(balanceCent / rateCent);
            mOmatsuri.GPW_chgCredit(coinNum);

            fsm.SendEvent("Succeed");
        };

        var param = mode == MODE.WEB ? webParam : desktopParam;
        var success = mode == MODE.WEB ? webAction : desktopAction;

        PostWWW(url, param, success, failed);
    }

    public void PostInit()
    {
        var verb = verbs["init"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();
        var webParam = new Dictionary<string, string>()
        {
        };

        var desktopParam = new Dictionary<string, string>()
        {
            { "gameId", "2" },
            { "userId", "1" },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            Debug.Log(www.text);
            var json = new JSONObject(www.text);
            fsm.SendEvent("Succeed");
        };

        Action<WWW> webAction = (www) =>
        {
            Debug.Log(www.text);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(www.text));
            var res = xmlDoc.GetElementsByTagName("response");
            var associate = new Dictionary<string, string>();
            foreach (XmlNode node in res[0].ChildNodes)
            {
                associate.Add(node.Name, node.InnerText);
            }

            var balance = Decimal.Parse(associate["balance"].ToString());

            fsm.SendEvent("Succeed");
        };

        var param = mode == MODE.WEB ? webParam : desktopParam;
        var success = mode == MODE.WEB ? webAction : desktopAction;

        PostWWW(url, param, success, failed);
    }

    public void PostPlay(int bet)
    {
        var verb = verbs["play"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();

        var betcount = bet;
        var rate = (float)Rate.Instanse.GetRate() / 100;

        var webParam = new Dictionary<string, string>()
        {
            { "rate", rate.ToString() },
            { "betCount",betcount.ToString() },
            { "power", "0" },
        };

        var desktopParam = new Dictionary<string, string>()
        {
            { "gameId", "2" },
            { "userId", "1" },
            { "betCount", betcount.ToString() },
            { "rate", rate.ToString() },
            { "power", "0" },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            Debug.Log(www.text);
            var json = new JSONObject(www.text);

            var yaku = json.GetField("yaku").ToString().ParseInt();

            fsm.SendEvent("Succeed");
        };

        Action<WWW> webAction = (www) =>
        {
            Debug.Log(www.text);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(www.text));
            var res = xmlDoc.GetElementsByTagName("response");
            var associate = new Dictionary<string, string>();
            foreach (XmlNode node in res[0].ChildNodes)
            {
                associate.Add(node.Name, node.InnerText);
            }

            var yaku = associate["yaku"].ToString().ParseInt();
            var balance = Decimal.Parse(associate["balance"].ToString());

            fsm.SendEvent("Succeed");
        };

        var param = mode == MODE.WEB ? webParam : desktopParam;
        var success = mode == MODE.WEB ? webAction : desktopAction;

        PostWWW(url, param, success, failed);

    }

    public void PostCollect()
    {
        var verb = verbs["collect"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();

        var webParam = new Dictionary<string, string>()
        {
            { "reelStopLeft", "1" },
            { "reelStopCenter", "1" },
            { "reelStopRight", "1" },
            { "oshijun", "1" },
        };

        var desktopParam = new Dictionary<string, string>()
        {
            { "gameId", "2" },
            { "userId", "1" },
            { "reelStopLeft", "1" },
            { "reelStopCenter", "1" },
            { "reelStopRight", "1" },
            { "oshijun", "1" },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            Debug.Log(www.text);
            var json = new JSONObject(www.text);

            var result = json.GetField("result").ToString();
            //var winnings = Decimal.Parse(json.GetField("winnings").ToString());

            fsm.SendEvent("Succeed");
        };

        Action<WWW> webAction = (www) =>
        {
            Debug.Log(www.text);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(www.text));
            var res = xmlDoc.GetElementsByTagName("response");
            var associate = new Dictionary<string, string>();
            foreach (XmlNode node in res[0].ChildNodes)
            {
                associate.Add(node.Name, node.InnerText);
            }

            var balance = Decimal.Parse(associate["balance"].ToString());
            var result = associate["result"].ToString();
            var winnings = Decimal.Parse(associate["winnings"].ToString());

            fsm.SendEvent("Succeed");
        };

        var param = mode == MODE.WEB ? webParam : desktopParam;
        var success = mode == MODE.WEB ? webAction : desktopAction;

        PostWWW(url, param, success, failed);
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
        if (mode == MODE.DESKTOP)
        {
            // コインを補充
            GameManager.Instance.InsertCoin(1000);
        }

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
        var fsm = GetComponent<PlayMakerFSM>();
        fsm.SendEvent("Succeed");

        // デバッグ用にアラートを出す
        //Application.ExternalCall("AlertByUnity", msg);

        //var param = new Dictionary<string, string>();

        //var kvs = msg.Split('&')
        //   .Select(query => query.Split('='))
        //   .Select(strings => new KeyValuePair<string, string>(strings[0], strings[1]));

        //foreach(var kv in kvs)
        //{
        //    param.Add(kv.Key, kv.Value);
        //}

        //// OpenをPOST
        //PostOpen(param);
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
