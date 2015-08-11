using UnityEngine;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class History : MonoBehaviour {

    public UISprite[] Sprites;
    public UISprite[] LabelSprites;

    private int[] data = new int[10];

    private static History _instance;
    public static History Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// 表示中のヒストリーのインデックス（０～９）
    /// －１では全点灯
    /// </summary>
    int current = -1;

    /// <summary>
    /// データ取得プロパティ
    /// </summary>
    public int[] Data
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
        }
    }

    /// <summary>
    /// 選択中のヒストリーのゲーム数を表示する
    /// </summary>
    public void DisplayGameRound()
    {
        if (current == -1)
        {
            CasinoData.Instance.GameCount = data[0];
        }
        else
        {
            CasinoData.Instance.GameCount = data[current];
        }
    }

    /// <summary>
    /// 表示中のヒストリーをシフトする
    /// </summary>
    /// <param name="index"></param>
    public void ShiftDisplayHistory()
    {
        current++;
        if (current >= 10)
        {
            current = -1;
        }

        DisplayGameRound();

        // シフト時に点滅するようにする
        time = hz;
        sw = false;
    }

    /// <summary>
    /// ヒストリーを右にシフトする
    /// 大当たりしたときにヒストリー１を０に、
    /// ヒストリー２はヒストリー１の値に、
    /// ヒストリー３はヒストリー２の値に、
    /// ということをヒストリー９まで行う。
    /// ヒストリー１０の内容は捨てる
    /// </summary>
    public void Shift()
    {
        var list = data.ToList();
        list.Insert(0, 0);
        data = list.Take(10).ToArray();

        DisplayGameRound();
    }

    public void Rireki1001()
    {
        if (current == -1)
        {
            data = Enumerable.Repeat(1001, 10).ToArray();
        }
        else
        {
            data[current] = 1001;
        }
    }

    /// <summary>
    /// ヒストリーを１増加する
    /// </summary>
    public void Add()
    {
        data[0]++;
        DisplayGameRound();
    }

    float time = 0f;
    float hz = 1f / 3f;  // 3Hz
    bool sw = true;

    /// <summary>
    /// 更新
    /// </summary>
    void Update()
    {
        time += Time.deltaTime;
        if (time > hz) time -= hz;
        else return;

        //-------------------------------------//
        // 処理を重くしないよう、リターンしておく 
        //-------------------------------------//

        // スイッチをフリップ
        sw = (sw == true) ? false : true;

        for (int i = 0; i < 10; ++i)
        {
            var level = 0;
            var h = Data[i];
            if (h == 0)
            {
                level = 0;
            }
            else
            {
                level = (h - h % 100 + 100) / 100;
            }

            if (level >= 10)
            {
                // 点滅
                if (sw)
                {
                    Sprites[i].GetComponent<UISprite>().spriteName = "level9";
                }
                else
                {
                    Sprites[i].GetComponent<UISprite>().spriteName = "level8";
                }
            }
            else
            {
                // 点灯
                Sprites[i].GetComponent<UISprite>().spriteName = "level" + level.ToString();
            }

            // ラベルを点滅
            if (current == i)
            {
                if (sw)
                {
                    LabelSprites[i].GetComponent<UISprite>().spriteName = "level_num_r_" + string.Format("{0:00}", i + 1);
                }
                else
                {
                    LabelSprites[i].GetComponent<UISprite>().spriteName = "nothing";
                }
            }
            else
            {
                LabelSprites[i].GetComponent<UISprite>().spriteName = "level_num_r_" + string.Format("{0:00}", i + 1);
            }

            // -1を指している場合は全ての数字を＊点灯＊
            if (current == -1)
            {
                LabelSprites[i].GetComponent<UISprite>().spriteName = "level_num_r_" + string.Format("{0:00}", i + 1);
            }

        }

    }
}
