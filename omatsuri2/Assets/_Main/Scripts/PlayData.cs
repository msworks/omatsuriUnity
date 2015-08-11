using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// 遊戯データ保持クラス
/// 項目：
///　 日付
///　 BB
///　 RB
///　 TR
///　 履歴(1～10)
/// </summary>
[Serializable]
public class PlayData {

    /// <summary>
    /// 日別遊戯データ保持クラス
    /// </summary>
    [Serializable]
    public class DailyPlayData {
        public DateTime timestamp;
        public int BB;
        public int RB;
        public int AT;
    }
    
    /// <summary>
    /// 当日の総回転数
    /// </summary>
    public int totalGameCount;
    
    /// <summary>
    /// 最後の当たりからの回転数
    /// </summary>
    public int gameCount;
    public List<DailyPlayData> dailyData = new List<DailyPlayData>();

    public List<int> history = new List<int>();

    static readonly string playDataSaveFileName = Application.persistentDataPath + "/playDataV2";

    /// <summary>
    /// 遊戯データセーブ
    /// BB RB TR 履歴
    /// </summary>
    /// <param name="o"></param>
    public static void Save(PlayData o) {
        try {
            BinaryFormatter b = new BinaryFormatter();
            using (FileStream fs = new FileStream(playDataSaveFileName, FileMode.Create)) {
                Debug.Log("遊戯データセーブ");
                b.Serialize(fs, o);
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    /// <summary>
    /// 遊戯データロード
    /// 失敗時は空データ(３日分)を返す
    /// </summary>
    /// <returns></returns>
    public static PlayData Load() {
        try {
            BinaryFormatter b = new BinaryFormatter();
            using(FileStream fs = new FileStream(playDataSaveFileName, FileMode.OpenOrCreate)){
                Debug.Log("遊戯データロード");
                return (PlayData)b.Deserialize(fs);
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
            PlayData p = new PlayData();
            for (int i = 0; i < 3; i++) {
                p.dailyData.Add(new DailyPlayData());
            }
            return p;
        }
    }

    /// <summary>
    /// データ更新
    /// 最新データの日付と現在日付の日数差分、遊戯データを空データでプッシュする。
    /// </summary>
    /// <param name="data"></param>
    /// <param name="currentDate"></param>
    public static void RefreshData(PlayData data) {
        DateTime now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        DateTime latest = data.dailyData[2].timestamp; // 2番が最新データ
        latest = new DateTime(latest.Year, latest.Month, latest.Day, 0, 0, 0);
        int diffDays = (now - latest).Days;
        Debug.Log("最新遊戯データ日付差:" + diffDays +
            " NOW:" + now.ToShortDateString() +
            " LATEST:" + latest.ToShortDateString());
        PushBlankPlayData(data, diffDays);
    }

    /// <summary>
    /// 指定回数分、遊戯データを空データ(日付は現在値)でプッシュする
    /// どれだけ多くても一度でプッシュするのは最大３日分まで
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count"></param>
    public static void PushBlankPlayData(PlayData data, int count) {
        if (count > 3) count = 3; 
        for (int i = 0; i < count; i++) {
            // プッシュされたらゲーム数をクリア
            data.totalGameCount = 0;
            data.gameCount = 0; 

            DailyPlayData p = new DailyPlayData();
            p.timestamp = DateTime.Now;
            data.dailyData.Add(p);
            data.dailyData.RemoveAt(0);
            Debug.Log("遊戯データPUSH:" +
                data.dailyData[0].timestamp.ToShortDateString() + ":" +
                data.dailyData[1].timestamp.ToShortDateString() + ":" +
                data.dailyData[2].timestamp.ToShortDateString());
        }
    }

    /// <summary>
    /// 空データを設定する
    /// </summary>
    public static void ClearData(PlayData data) {
        data.totalGameCount = 0;
        data.gameCount = 0;
        data.history = new List<int>();
        PushBlankPlayData(data, 3);
    }

    /// <summary>
    /// デバッグ用のダミーデータを設定する
    /// </summary>
    /// <param name="data"></param>
    public static void SetDummyData(PlayData data) {
        data.totalGameCount = UnityEngine.Random.Range(0, 9999);
        data.gameCount = UnityEngine.Random.Range(0, 9999);
        data.history = new List<int>();
        for (int idx = 0; idx < 10; idx++) {
            data.history.Add(UnityEngine.Random.Range(0, 9) * 100);
        }
        foreach (DailyPlayData day in data.dailyData) {
            day.BB = UnityEngine.Random.Range(0, 9);
            day.RB = UnityEngine.Random.Range(0, 9);
            day.AT = UnityEngine.Random.Range(0, 9);
            day.timestamp = DateTime.Now;
        }
    }
}