﻿using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

public class Post : MonoBehaviour
{
    enum Mode
    {
        Web,
        Desktop,
    }

    // WEB - 本番
    // DESKTOP - TEST MODE
    // MODE mode = MODE.WEB;
    Mode mode = Mode.Desktop;

    static string serverHead = "../pachinko/";
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
        if( mode==Mode.Web)
        {
            head = serverHead;
            verbs = verbsServer;
        }
        else if( mode == Mode.Desktop)
        {
            head = logicHead;
            verbs = verbsLogic;
        }
    }

    /// <summary>
    /// コインチャージ - デモ用
    /// </summary>
    [ActionCategory("Ginpara")]
    public class Charge : FsmStateAction
    {
        public FsmInt fsmCoinCount;

        public override void OnEnter()
        {
            var coinCount = fsmCoinCount.Value;
            mOmatsuri.GPW_chgCredit(coinCount);
            CasinoData.Instance.Exchange = coinCount * Rate.Instanse.GetRate();
        }
    }

    /// <summary>
    /// 役に応じてリプレイかベット待ちかイベントを発行
    /// </summary>
    [ActionCategory("Ginpara")]
    public class ReplayCheck : FsmStateAction
    {
        public FsmInt CurrentYaku;
        public FsmEvent WaitBet;
        public FsmEvent Replay;

        public override void OnEnter()
        {
            var replay = CurrentYaku.Value == (int)Yaku.Replay ? true : false;

            if(replay)
            {
                Fsm.Event(Replay);
            }
            else
            {
                Fsm.Event(WaitBet);
            }
        }
    }

    /// <summary>
    /// WebPageにパラメータを要求
    /// レスポンスは Response(string) で得られることを期待
    /// </summary>
    [ActionCategory("Ginpara")]
    public class GetParameter : FsmStateAction
    {
        public override void OnEnter()
        {
            // Webページに対してパラメータ送信要求
            Application.ExternalCall("GetParameter");
        }
    }

    /// <summary>
    /// Webページからのレスポンス
    /// </summary>
    /// <param name="msg">gameId=2&token=aaa&language=ja&operatorId=1&mode=1</param>
    public void Response(string msg)
    {
        var param = new Dictionary<string, string>();
        var fsm = GetComponent<PlayMakerFSM>();

        var kvs = msg.Split('&')
           .Select(query => query.Split('='))
           .Select(strings => new KeyValuePair<string, string>(strings[0], strings[1]));

        foreach (var kv in kvs)
        {
            param.Add(kv.Key, kv.Value);
        }

        if (param["mode"] == "0")
        {
            // デモモード
            GameMode.Mode = GameModeType.Demo;
            fsm.SendEvent("Demo");
        }
        else if(param["mode"] == "1")
        {
            // リアルモード
            GameMode.Mode = GameModeType.Real;
            fsm.SendEvent("Succeed");
        }
        else
        {
            fsm.SendEvent("Failed");
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
    public class CreditCehck : FsmStateAction
    {
        public FsmEvent Succeed;
        public FsmEvent Failed;

        public override void OnEnter()
        {
            // コイン枚数を金額に変換する
            var coin = mOmatsuri.int_s_value[Defines.DEF_INT_SLOT_COIN_NUM];

            if (coin <= 2)
            {
                Fsm.Event(Failed);
            }
            else
            {
                Fsm.Event(Succeed);
            }
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
        public FsmInt Reel0Index;
        public FsmInt Reel1Index;
        public FsmInt Reel2Index;
        public FsmString ReelStopOrder;

        public override void OnEnter()
        {
            var reelStopIndex = new []
            {
                Reel0Index.Value,
                Reel1Index.Value,
                Reel2Index.Value,
            };

            var order = ReelStopOrder.Value;

            post.PostCollect(reelStopIndex, order);
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
            { "setting", "0%2C0%2C0%2C0%2C20%2C30%2C50" },
            { "gameId", "101" },
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
            //var reelleft = json.GetField("reelleft").ToString().ParseInt();
            //var reelcenter = json.GetField("reelcenter").ToString().ParseInt();
            //var reelright = json.GetField("reelright").ToString().ParseInt();
            var seed = json.GetField("seed").ToString().ParseInt();

            if( setting == 0)
            {
                GameManager.Instance.SettingZeroMode = true;
                setting = 1;
            }

            clOHHB_V23.mInitializaion(seed);
            clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)setting);

            var status = "ok";

            // 1000枚セット
            var coinCount = 100;
            mOmatsuri.GPW_chgCredit(coinCount);
            CasinoData.Instance.Exchange = coinCount * Rate.Instanse.GetRate();

            if (status.Contains("error"))
            {
                fsm.SendEvent("Failed");
            }
            else
            {
                fsm.SendEvent("Succeed");
            }
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

            var status = "";
            var balance = 0m;
            var setting = 0;
            var reelleft = 0;
            var reelcenter = 0;
            var reelright = 0;
            var seed = 0;

            try
            {
                status = associate["status"].ToString();
                balance = Decimal.Parse(associate["balance"].ToString());
                setting = associate["setting"].ToString().ParseInt();
                reelleft = associate["reelLeft"].ToString().ParseInt();
                reelcenter = associate["reelCenter"].ToString().ParseInt();
                reelright = associate["reelRight"].ToString().ParseInt();
                seed = associate["seed"].ToString().ParseInt();
            }
            catch(Exception e)
            {
                Debug.Log(e);
                ErrorCode.Set("2", "1", "0");
                fsm.SendEvent("Failed");
            }

            Debug.Log("L:"+reelleft+"C:"+reelcenter+"R:"+reelright+"}");

            if (setting == 0)
            {
                GameManager.Instance.SettingZeroMode = true;
                setting = 1;
            }

            //mOmatsuri.chgPrayer();
            clOHHB_V23.mInitializaion(seed);
            clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)setting);

            var rateCent = Rate.Instanse.GetRate();
            var balanceCent = balance * 100m;
            var coinNum = (int)(balanceCent / rateCent);

            CasinoData.Instance.Exchange = balance;

            // コインを１度０枚にしてからチャージ
            var deleteCoinCount = mOmatsuri.int_s_value[Defines.DEF_INT_SLOT_COIN_NUM];
            mOmatsuri.GPW_chgCredit(-deleteCoinCount);
            mOmatsuri.GPW_chgCredit(coinNum);

            if (status.Contains("error"))
            {
                ErrorCode.Set("2", "1", "0");
                fsm.SendEvent("Failed");
            }
            else
            {
                fsm.SendEvent("Succeed");
            }
        };

        var param = mode == Mode.Web ? webParam : desktopParam;
        var success = mode == Mode.Web ? webAction : desktopAction;

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
            { "gameId", "101" },
            { "userId", "1" },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            //Debug.Log(www.text);
            //var json = new JSONObject(www.text);
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

            var status = "";
            var balance = 0m;

            try
            {
                status = associate["status"].ToString();
                balance = Decimal.Parse(associate["balance"].ToString());
            }
            catch(Exception e)
            {
                Debug.Log(e);
                ErrorCode.Set("2", "2", "0");
            }

            CasinoData.Instance.Exchange = balance;

            if (status.Contains("error"))
            {
                ErrorCode.Set("2", "2", "0");
                fsm.SendEvent("Failed");
            }
            else
            {
                fsm.SendEvent("Succeed");
            }
        };

        var param = mode == Mode.Web ? webParam : desktopParam;
        var success = mode == Mode.Web ? webAction : desktopAction;

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
            { "gameId", "101" },
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
            //Debug.Log(www.text);
            var json = new JSONObject(www.text);
            var yaku = json.GetField("yaku").ToString().ParseInt();
            fsm.FsmVariables.FindFsmInt("ServerYaku").Value = yaku;

            //mOmatsuri.currentYaku = (Yaku)yaku;

            var dic = new Dictionary<Yaku, Defines.ForceYakuFlag>()
            {
                { Yaku.None, Defines.ForceYakuFlag.HAZURE },
                { Yaku.Cherry, Defines.ForceYakuFlag.CHERRY },
                { Yaku.Bell, Defines.ForceYakuFlag.BELL },
                { Yaku.YAKU_WMLN, Defines.ForceYakuFlag.SUIKA },
                { Yaku.Replay, Defines.ForceYakuFlag.REPLAY },
                { Yaku.RegulerBonus, Defines.ForceYakuFlag.REG },
                { Yaku.JAC, Defines.ForceYakuFlag.REPLAY },
                { Yaku.BigBonus, Defines.ForceYakuFlag.BIG },
                //{ Yaku.JACIN, Defines.ForceYakuFlag.REPLAY },
            };

            var forceYaku = Defines.ForceYakuFlag.HAZURE;

            if(dic.ContainsKey((Yaku)yaku))
            {
                forceYaku = dic[(Yaku)yaku];
            }
            else
            {
                forceYaku = Defines.ForceYakuFlag.HAZURE;
                Debug.Log("UNDEFINED YAKU");
            }
            
            if((clOHHB_V23.getWork(Defines.DEF_GAMEST) & 0x01) != 0)
            {
                forceYaku = Defines.ForceYakuFlag.REG;
            }

            // 役強制
            GameManager.forceYakuValue = (Defines.ForceYakuFlag)forceYaku;

            Debug.Log("サーバー成立役："+ forceYaku.ToString()+"["+ (int)forceYaku + "]");

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

            var status = "";
            var yaku = 0;
            var balance = 0m;

            try
            {
                status = associate["status"].ToString();
                yaku = associate["yaku"].ToString().ParseInt();
                balance = Decimal.Parse(associate["balance"].ToString());
            }
            catch(Exception e)
            {
                ErrorCode.Set("2", "3", "0");
                Debug.Log(e);
            }

            Debug.Log("ServerYaku:" + yaku);
            fsm.FsmVariables.FindFsmInt("ServerYaku").Value = yaku;
            //mOmatsuri.currentYaku = (Yaku)yaku;

            var dic = new Dictionary<Yaku, Defines.ForceYakuFlag>()
            {
                { Yaku.None, Defines.ForceYakuFlag.HAZURE },
                { Yaku.Cherry, Defines.ForceYakuFlag.CHERRY },
                { Yaku.Bell, Defines.ForceYakuFlag.BELL },
                { Yaku.YAKU_WMLN, Defines.ForceYakuFlag.SUIKA },
                { Yaku.Replay, Defines.ForceYakuFlag.REPLAY },
                { Yaku.RegulerBonus, Defines.ForceYakuFlag.REG },
                { Yaku.JAC, Defines.ForceYakuFlag.REPLAY },
                { Yaku.BigBonus, Defines.ForceYakuFlag.BIG },
                //{ Yaku.JACIN, Defines.ForceYakuFlag.REPLAY },
            };

            var forceYaku = Defines.ForceYakuFlag.HAZURE;

            if (dic.ContainsKey((Yaku)yaku))
            {
                forceYaku = dic[(Yaku)yaku];
            }
            else
            {
                forceYaku = Defines.ForceYakuFlag.HAZURE;
                Debug.Log("UNDEFINED YAKU");
            }

            // 役強制
            GameManager.forceYakuValue = (Defines.ForceYakuFlag)forceYaku;
            Debug.Log("サーバー成立役：" + forceYaku.ToString() + "[" + (int)forceYaku + "]");

            CasinoData.Instance.Exchange = balance;

            if (status.Contains("error"))
            {
                ErrorCode.Set("2", "3", "0");
                fsm.SendEvent("Failed");
            }
            else
            {
                fsm.SendEvent("Succeed");
            }
        };

        var param = mode == Mode.Web ? webParam : desktopParam;
        var success = mode == Mode.Web ? webAction : desktopAction;

        PostWWW(url, param, success, failed);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reelStopIndex">ボタン押下リール位置</param>
    /// <param name="order">押し順</param>
    public void PostCollect(int[] reelStopIndex, string order)
    {
        var verb = verbs["collect"];
        var url = head + verb;
        var fsm = GetComponent<PlayMakerFSM>();

        var webParam = new Dictionary<string, string>()
        {
            { "reelStopLeft", reelStopIndex[0].ToString() },
            { "reelStopCenter", reelStopIndex[1].ToString() },
            { "reelStopRight", reelStopIndex[2].ToString() },
            { "oshijun", order },
        };

        var desktopParam = new Dictionary<string, string>()
        {
            { "gameId", "101" },
            { "userId", "1" },
            { "reelStopLeft", reelStopIndex[0].ToString() },
            { "reelStopCenter", reelStopIndex[1].ToString() },
            { "reelStopRight", reelStopIndex[2].ToString() },
            { "oshijun", order },
        };

        Action<WWW> failed = (www) =>
        {
            fsm.SendEvent("Failed");
        };

        Action<WWW> desktopAction = (www) =>
        {
            Debug.Log("Collet Response:"+www.text);
            //var json = new JSONObject(www.text);
            //var result = json.GetField("result").ToString();
            //var winnings = Decimal.Parse(json.GetField("winnings").ToString());

            var yaku = (Yaku)fsm.FsmVariables.FindFsmInt("ServerYaku").Value;

            if (yaku == Yaku.BigBonus)
            {
                fsm.SendEvent("BigBonusCut");
            } else if(yaku == Yaku.RegulerBonus)
            {
                fsm.SendEvent("RegBonusCut");
            }

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

            var status = "";
            var balance = 0m;

            try
            {
                status = associate["status"].ToString();
                balance = Decimal.Parse(associate["balance"].ToString());
            }
            catch(Exception e)
            {
                Debug.Log(e);
                ErrorCode.Set("2", "4", "0");
                fsm.SendEvent("Failed");
            }

            CasinoData.Instance.Exchange = balance;

            var yaku = (Yaku)fsm.FsmVariables.FindFsmInt("ServerYaku").Value;

            if (yaku == Yaku.BigBonus)
            {
                fsm.SendEvent("BigBonusCut");
            }
            else if (yaku == Yaku.RegulerBonus)
            {
                fsm.SendEvent("RegBonusCut");
            }

            if (status.Contains("error"))
            {
                ErrorCode.Set("2", "4", "0");
                fsm.SendEvent("Failed");
            }
            else
            {
                fsm.SendEvent("Succeed");
            }
        };

        var param = mode == Mode.Web ? webParam : desktopParam;
        var success = mode == Mode.Web ? webAction : desktopAction;

        PostWWW(url, param, success, failed);
    }

    /// <summary>
    /// ログイン
    /// </summary>
    public void _Open()
    {
        var fsm = GetComponent<PlayMakerFSM>();

        if (mode == Mode.Desktop)
        {
            var msg = "gameId=101&token=aaa&language=ja&operatorId=1&mode=1";
            Response(msg);
        }
        else if (mode == Mode.Web)
        {
            // Webページに対してパラメータ送信要求
            Application.ExternalCall("GetParameter");
        }
        else
        {
            ErrorCode.Set("1", "0", "0");
            fsm.SendEvent("Failed");
        }
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

    /// <summary>
    /// WWW Post
    /// </summary>
    /// <param name="url"></param>
    /// <param name="post"></param>
    /// <param name="success">成功時のコールバック</param>
    /// <param name="failed">失敗時のコールバック</param>
    /// <returns></returns>
    IEnumerator PostWWWCore(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
        //Debug.Log("POST:url=" + url);

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
            try
            {
                success(www);
            }
            catch (Exception)
            {
                failed(www);
            }
        }
        else
        {
            failed(www);
        }
    }
}
