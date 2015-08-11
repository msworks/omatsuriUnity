using HutongGames.PlayMaker;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Post : MonoBehaviour
{

    //private string URI = "https://web.ee-gaming.net/game/";
    private string URI = "http://web.ee-gaming.net/game/";
    //private string URI = "http://192.168.3.4/game/";

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

    private void POST(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
        StartCoroutine(PostCore(url, post, success, failed));
    }

    private IEnumerator PostCore(string url, Dictionary<string, string> post, Action<WWW> success, Action<WWW> failed)
    {
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
}
