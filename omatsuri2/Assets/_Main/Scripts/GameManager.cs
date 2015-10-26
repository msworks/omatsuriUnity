using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// ゲーム管理クラス
/// </summary>
public class GameManager : MonoBehaviour {

    /// <summary>
    /// シングルトン
    /// </summary>
    public static GameManager Instance {
        get { return _instance; }
    }
    private static GameManager _instance;

    // PC版か否かで接続先が変わる
    public string serverURL {
        get {
            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) {
                return "https://pc"; // TODO 要正式なURL
            } else {
                return "https://mobile"; // TODO 要正式なURL
            }
        }
    }

    public SlotMachine slotMachine;

    [Serializable]
    public class SlotMachine {
        public ReelBuilder[] reel;
        public GameObject reel4th;
    }
    public Animator lever;

    public SoundDefine soundDefine;
    public PictureDefine pictureDefine;
    public AutoPlayPatternDefine autoPlayFull;
    public AutoPlayPatternDefine autoPlaySelf;
    public AutoPlayPatternDefine currentAutoPlaySetting;
    private AutoPlayPatternDefine.AutoPlayPatternValue currentAutoPlayPattern;

    public Texture2D[] reelTexture = new Texture2D[3];
    public GameObject reel4th;

    public GameObject[] lamps;
    public Text creditCoinText;
    public Text bonusCoinText;
    public Text getCoinText;

    public CasinoData casinoData;

    public GameObject[] reelFaceLamps = new GameObject[9];
    public GameObject coinInsertSlotLamp;

    [Header("入力制御")]
    public GameObject[] reelTouchAreas = new GameObject[3];

    /// <summary>
    /// 強制役設定
    /// </summary>
    public static Defines.ForceYakuFlag forceYakuValue;

    private AudioSource audioSE;
    private AudioSource audioBGM;

    private float prev4thOffset;

    private float nextUpdateTime;
    private float updateInterval = 0.02f;

    public enum PAUSE_STATE {
        PAUSE,
        PLAY,
    }

    /// <summary>
    /// ポーズ状態
    /// </summary>
    private PAUSE_STATE pauseState = PAUSE_STATE.PLAY;
    public PAUSE_STATE PauseState {
        set { pauseState = value; }
        get { return pauseState; }
    }

    /// <summary>
    /// 遊戯データ
    /// </summary>
    private PlayData playData;

    Mobile core = new Mobile();

    void Awake() {
        _instance = this;
        Application.targetFrameRate = 60; // iOSデフォルトが30fpsなので、60fpsになるよう設定
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // スリープ状態にならないように設定
        audioSE = gameObject.AddComponent<AudioSource>();
        audioBGM = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// リール図柄設定
    /// リール配置と画像の定義からテクスチャを生成して各リールに適用する。
    /// </summary>
    void SetupReelTexture() {
        int tw = 64;
        int th = 32;
        int faceCount = 21; // リールの面数
        for (int reelIdx = 0; reelIdx < slotMachine.reel.Length; reelIdx++) {
            reelTexture[reelIdx] = new Texture2D(tw, th * faceCount, TextureFormat.ARGB32, false);
            for (int rowIdx = 0; rowIdx < faceCount; rowIdx++) {
                SetReelTexture(rowIdx, reelIdx, false);
            }
            slotMachine.reel[reelIdx].transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = reelTexture[reelIdx];
        }
    }

    /// <summary>
    /// リールのテクスチャを変更する。
    /// ランプ処理に使用。
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="col">列(リール)</param>
    /// <param name="isLit">ランプ点灯中？</param>
    public void SetReelTexture(int row, int col, bool isLit) {
        int id = mOmatsuri.getReelId(mOmatsuri.REELTB[col][row]);
        Sprite face;
        if (mOmatsuri.IsReelStopped(col)) {
            if (isLit) {
                face = pictureDefine.reelFaces[id].lit;
            } else {
                face = pictureDefine.reelFaces[id].unlit;
            }
        } else {
            face = pictureDefine.reelFaces[id].blur;
        }

        slotMachine.reel[col].faces[row].GetComponent<Image>().sprite = face;
    }

    /// <summary>
    /// 4thリールのテクスチャを変更する。
    /// ランプ処理に使用。
    /// </summary>
    /// <param name="isLit">ランプ点灯中？</param>
    public void Set4thReelTexture(bool isLit) {
        Texture2D face;
        if (isLit) {
            face = pictureDefine.reel4thLit;
        } else {
            face = pictureDefine.reel4thUnLit;
        }
        slotMachine.reel4th.GetComponent<Renderer>().material.mainTexture = face;
    }

    /// <summary>
    /// 4thリール図柄設定
    /// 4thリール画像の定義からテクスチャを生成して4thリールに適用する。
    /// </summary>
    void Setup4thReelTexture() {
        //reel4thTextureUnlit = pictureDefine.reel4th[0].unlit;
        //reel4thTextureLit = pictureDefine.reel4th[0].lit;
        slotMachine.reel4th.GetComponent<Renderer>().material.mainTexture = pictureDefine.reel4thUnLit;
    }

	void Start () {
        // リールをセットアップ
        //SetupReelTexture();
        Setup4thReelTexture();
        ZZ.setThreadSpeed(20);
        StartCoroutine(MainLoop());
        InitializeCasinoData();
        LoadPlayData();

        // WebView表示
        //WebViewManager.Instance.Open();
	}

    /// <summary>
    /// 履歴追加
    /// 当日の遊戯履歴データへも同時に書き込みを行う
    /// </summary>
    /// <param name="num">ゲーム数</param>
    public void AddHistory(int num) {
        int counterNum = (num + 99) / 100;// 100単位で切り上げ
        casinoData.AddHistory(counterNum);
        playData.history.Add(num);
    }

    /// <summary>
    /// 共通UI初期化
    /// UIの内容をクリア
    /// </summary>
    void InitializeCasinoData() {
        casinoData.GameCount = 0;
        casinoData.BB = 0;
        casinoData.RB = 0;
        casinoData.AT = 0;
        casinoData.AVG = 0;
        casinoData.UpdatePastBB(0, 0);
        casinoData.UpdatePastRB(0, 0);
        casinoData.UpdatePastAT(0, 0);
        for (int i = 0; i < 10; i++) {
            casinoData.AddHistory(0);
        }
    }

    void Update() {
        if (UIManager.Instance.autoPlayToggle.isOn || 
            UIManager.Instance.semiAutoPlayToggle.isOn ) {
            AutoPlay();
        }
        UpdateReel();
        UpdateCoinDisplay();
        Update4thReel();
        UpdateDebugUI();
        UpdateLamp();
        UpdateReelTouchArea();
        UpdateCommonUI();
    }

    /// <summary>
    /// リール選択エリアの更新
    /// 回転中のみ入力を受け付ける。対応するボタンのランプの状態で判断。
    /// </summary>
    void UpdateReelTouchArea() {
        for (int i = 0; i < 3; i++) {
            reelTouchAreas[i].SetActive(mOmatsuri.getLampStatus(Defines.DEF_LAMP_BUTTON_L + i) == Defines.DEF_LAMP_STATUS_ON);
        }
    }

    /// <summary>
    /// メインループ
    /// </summary>
    /// <returns></returns>
    IEnumerator MainLoop() {

        while (true) {
            try {
                core.exec();

                // コインが無くなったら自動で補充
                if (mOmatsuri.int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] <= 3) {
                    // 500ドル分のクレジットを設定
                    // 500ドル １枚20セント
                    // 1ドル 5枚
                    // 500 ドル * 5枚 = 2500枚
                    mOmatsuri.GPW_chgCredit(2500);
                }

            } catch (Exception e) {
                UIManager.Instance.errorText.text = e.ToString();
            }
            yield return new WaitForSeconds(0.02f); // TODO soy 要調整。とりあえず40msに合わせた
        }
    }

    /// <summary>
    /// 遊戯データロード処理
    /// </summary>
    void LoadPlayData() {
        playData = PlayData.Load();
        PlayData.RefreshData(playData);
        RefreshCommonUIData();
    }

    /// <summary>
    /// 共通UI更新
    /// 遊戯データをUIに反映
    /// </summary>
    void RefreshCommonUIData() {
        casinoData.GameCount = playData.gameCount;
        casinoData.BB = playData.dailyData[2].BB;
        casinoData.RB = playData.dailyData[2].RB;
        casinoData.AT = playData.dailyData[2].AT;
        casinoData.UpdatePastBB(playData.dailyData[1].BB, playData.dailyData[0].BB);
        casinoData.UpdatePastRB(playData.dailyData[1].RB, playData.dailyData[0].RB);
        casinoData.UpdatePastAT(playData.dailyData[1].AT, playData.dailyData[0].AT);

        UpdateCommonUIAvg();
    }

    /// <summary>
    /// 遊戯データセーブ処理
    /// </summary>
    void SavePlayData() {
        playData.gameCount = casinoData.GameCount;
        playData.dailyData[2].BB = casinoData.BB;
        playData.dailyData[2].RB = casinoData.RB;
        playData.dailyData[2].AT = casinoData.AT;
        PlayData.Save(playData);
        PlayData.RefreshData(playData);
        RefreshCommonUIData();
    }

    /// <summary>
    /// リール状態更新
    /// </summary>
    void UpdateReel() {
        SetReelAngle(slotMachine.reel[0].transform, mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0]);
        SetReelAngle(slotMachine.reel[1].transform, mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R1]);
        SetReelAngle(slotMachine.reel[2].transform, mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R2]);
    }

    /// <summary>
    /// コイン枚数表示更新
    /// </summary>
    void UpdateCoinDisplay() {
        creditCoinText.text = mOmatsuri.int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM].ToString("00");
        bonusCoinText.text = mOmatsuri.GetBonusCount();
        getCoinText.text = mOmatsuri.int_s_value[Defines.DEF_INT_WIN_GET_COIN].ToString("#");
    }

    /// <summary>
    /// 4thリール状態更新
    /// </summary>
    void Update4thReel() {
        float smooth = 0.5f;
        float angleCorrect = 0.245f; // 角度補正
        // 角度(0～414)をOffsetの値(0～1)に変換
        float offset = 1 - (mOmatsuri.int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] / 414f);

        // UVがループするタイミングでスムーズに回転するよう調整
        if (offset < 0.1f && 0.9f < prev4thOffset) {
            slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset =
                new Vector2(slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset.x - 1f, 0f);
        }
        if (prev4thOffset < 0.1f && 0.9f < offset) {
            slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset =
                new Vector2(slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset.x + 1f, 0f);
        }

        //Debug.Log("offset:" + offset + " p:" + prev4thOffset + " RAW:" + mOmatsuri.int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE]);
        slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset = Vector2.Lerp(
            slotMachine.reel4th.GetComponent<Renderer>().material.mainTextureOffset,
            new Vector2(offset + angleCorrect, 0f),
            smooth
            );
        prev4thOffset = offset;
    }

    /// <summary>
    /// ランプ点灯状態更新
    /// </summary>
    void UpdateLamp() {
        for (int i = 0; i < lamps.Length; i++) {
            if (lamps[i] != null) {
                lamps[i].SetActive(mOmatsuri.getLampStatus(i) == Defines.DEF_LAMP_STATUS_ON);
            }
        }
    }

    /// <summary>
    /// 共通UI更新
    /// </summary>
    void UpdateCommonUI() {

        // コイン枚数を金額に変換する
        var coin = mOmatsuri.int_s_value[Defines.DEF_INT_SLOT_COIN_NUM];
        var cent = Rate.Instanse.Coin2Cent(coin);

        // floatでやるの嫌なんだよな。。
        var doller = cent / 100f;

        casinoData.Exchange = doller;
    }

    /// <summary>
    /// コイン投入時処理
    /// </summary>
    public void OnCoinInsert() {
        LitCoinInsertSlotLamp();
        Invoke("UnLitCoinInsertSlotLamp", 0.03f);
    }

    /// <summary>
    /// コイン投入口ランプ点灯処理
    /// </summary>
    void LitCoinInsertSlotLamp() {
        coinInsertSlotLamp.SetActive(true);
    }

    /// <summary>
    /// コイン投入口ランプ消灯処理
    /// </summary>
    void UnLitCoinInsertSlotLamp() {
        coinInsertSlotLamp.SetActive(false);
    }

    /// <summary>
    /// 遊技状態
    /// </summary>
    public enum PlayStatus { 
        Normal,
        RegularBonusInternal,
        BigBonusInternal,
        RegularBonus,
        BigBonus,
        Unknown
    }

    /// <summary>
    /// 4thリール停止確認
    /// </summary>
    /// <returns></returns>
    bool Is4thReelStopped() {
        return mOmatsuri.int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] == 0;
    }

    /// <summary>
    /// オートプレイパターン設定
    /// オートプレイ定義と現在の状態を参照してリール停止パターンを決定する
    /// </summary>
    void SetAutoPlayPattern() {
        int patternSeed = UnityEngine.Random.Range(0, 100);
        AutoPlayPatternDefine.AutoPlayPatternValue[] patterns;
        switch (clOHHB_V23.getWork(Defines.DEF_GMLVSTS)) {
            case 1: // RB中
                patterns = currentAutoPlaySetting.regular;
                break;
            case 2: // BB中
                // 4th青ドン状態チェック
                bool isAodon = mOmatsuri.int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] == 237;
                // リプレイずらし判定
                if (currentAutoPlaySetting == autoPlayFull &&    
                    clOHHB_V23.getWork(Defines.DEF_BIGBCTR) == 1 &&
                    clOHHB_V23.getWork(Defines.DEF_BBGMCTR) >= 8 ) {
                    // フルなら4th青ドン＆残JAC回数１＆残ボーナスゲーム８回以上

                    // 4thが確定するまで待つ
                    if (!Is4thReelStopped()) {
                        Debug.Log("リプレイずらし・4th停止待機");
                        return;
                    } else {
                        // 4thが青ドンならリプレイずらし
                        if (isAodon) {
                            Debug.Log("リプレイずらし・BB逆(FULL)");
                            patterns = currentAutoPlaySetting.bigReverse;
                        } else {
                            Debug.Log("リプレイずらし無し");
                            patterns = currentAutoPlaySetting.big;
                        }
                    }
                    Debug.Log(
                        "オート判定：青ドン=" + isAodon.ToString() +
                        " 残B=" + clOHHB_V23.getWork(Defines.DEF_BIGBCTR) +
                        " 残JAC=" + clOHHB_V23.getWork(Defines.DEF_BBGMCTR));

                } else if (
                    currentAutoPlaySetting == autoPlaySelf &&
                    ((clOHHB_V23.getWork(Defines.DEF_BIGBCTR) == 2 && clOHHB_V23.getWork(Defines.DEF_BBGMCTR) >= 16) ||
                     (clOHHB_V23.getWork(Defines.DEF_BIGBCTR) == 1 && clOHHB_V23.getWork(Defines.DEF_BBGMCTR) >= 10))) {
                    // セルフなら4th青ドン＆
                    // ((残JAC回数２＆残ボーナスゲーム１６回以上)  ||
                    //  (残JAC回数１＆残ボーナスゲーム１０回以上))

                    // 4thが確定するまで待つ
                    if (!Is4thReelStopped()) {
                        Debug.Log("リプレイずらし・4th停止待機");
                        return;
                    } else {
                        // 4thが青ドンならリプレイずらし
                        if (isAodon) {
                            Debug.Log("リプレイずらし・BB逆(FULL)");
                            patterns = currentAutoPlaySetting.bigReverse;
                        } else {
                            Debug.Log("リプレイずらし無し");
                            patterns = currentAutoPlaySetting.big;
                        }
                    }
                } else {
                    Debug.Log("オート判定:BB");
                    patterns = currentAutoPlaySetting.big;
                }
                Debug.Log(
                    "オート判定：青ドン=" + isAodon.ToString() +
                    " 残B=" + clOHHB_V23.getWork(Defines.DEF_BIGBCTR) +
                    " 残JAC=" + clOHHB_V23.getWork(Defines.DEF_BBGMCTR));
                break;
            case 4: // 通常
                patterns = currentAutoPlaySetting.normal;
                break;
            case 8: // RB内部あたり
                patterns = currentAutoPlaySetting.regReach;
                break;
            case 16: // BB内部あたり
                patterns = currentAutoPlaySetting.bigReach;
                break;
            default:
                Debug.LogError("オート判定:想定外の値:" + clOHHB_V23.getWork(Defines.DEF_GMLVSTS));
                patterns = currentAutoPlaySetting.normal;
                break;
        }
        currentAutoPlayPattern = patterns[UnityEngine.Random.Range(0, 100) % patterns.Length];
        Debug.Log("自動プレイパターン:" + 
            (currentAutoPlayPattern.isStopReverse ? "右から" : "左から") + 
            currentAutoPlayPattern.targetRow[0] + "-" +
            currentAutoPlayPattern.targetRow[1] + "-" +
            currentAutoPlayPattern.targetRow[2]);
    }

    /// <summary>
    /// デバッグ用UI更新
    /// </summary>
    void UpdateDebugUI() {
        string text = "";
        text += 
            "FPS:" + FPSCounter.FPS +
            " SLOT_COIN:" + mOmatsuri.int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] + System.Environment.NewLine;
        
        text += "遊技ｽﾃｰﾀｽ(Defines.DEF_GMLVSTS):" + (clOHHB_V23.getWork(Defines.DEF_GMLVSTS) & 0xFFFF) + System.Environment.NewLine;
        
        int totalIn = mOmatsuri.int_s_value[Defines.DEF_INT_TOTAL_BET];
        int totalOut = mOmatsuri.int_s_value[Defines.DEF_INT_TOTAL_PAY];
        text +=
            "総IN=" + totalIn + " 総OUT=" + totalOut +
            " 機械割実績=" + (totalOut == 0 ? 0f : (float)totalOut / totalIn).ToString("0.000%") +
            " 総回転数=" + playData.totalGameCount;
        
        UIManager.Instance.rateText.text = "機械割設定:現在値=" + (Mobile.int_m_value[Defines.DEF_INT_SETUP_VALUE] + 1) + " 指示値=" + (mOmatsuri.gp.gpif_setting + 1);
        UIManager.Instance.debugText.text = text;

        /*
        Debug.Log(
            "リール1=" + mOmatsuri.ANGLE2INDEX(mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0]) + 
            " ANGLE=" + mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0]);
         */
    }

    /// <summary>
    /// リール角度設定
    /// </summary>
    /// <param name="reel">対象リール</param>
    /// <param name="angle">角度(0～65535)</param>
    void SetReelAngle(Transform reel, float angle) {
        // TODO soy 目押しする関係上、リールの回転位置は高精度で同期させつつ滑らかに回したい
        float smooth = .5f;
        reel.rotation = Quaternion.Lerp(reel.rotation, Quaternion.AngleAxis(angle * -360f / 65535f, Vector3.right), smooth);
    }

    /// <summary>
    /// 全てのリールが止まっているか判定
    /// </summary>
    /// <returns></returns>
    bool IsAllReelStopped() {
        return 
            mOmatsuri.IsReelStopped(0) && 
            mOmatsuri.IsReelStopped(1) && 
            mOmatsuri.IsReelStopped(2);
    }

    public void SetAutoPlayFull() {
        currentAutoPlaySetting = autoPlayFull;
        if (UIManager.Instance.autoPlayToggle.isOn)
        {
            UIManager.Instance.semiAutoPlayToggle.isOn = false;
            UIManager.Instance.semiAutoPlayToggle.interactable = false;
            //UIManager.Instance.semiAutoPlayToggle.gameObject.SetActive(false);
        }
        else
        {
            UIManager.Instance.semiAutoPlayToggle.isOn = false;
            UIManager.Instance.semiAutoPlayToggle.interactable = true;
            //UIManager.Instance.semiAutoPlayToggle.gameObject.SetActive(false);
        }
    }

    public void SetAutoPlaySelf() {
        currentAutoPlaySetting = autoPlaySelf;
        if (UIManager.Instance.semiAutoPlayToggle.isOn)
        {
            UIManager.Instance.autoPlayToggle.isOn = false;
            UIManager.Instance.autoPlayToggle.interactable = false;
            //UIManager.Instance.autoPlayToggle.gameObject.SetActive(false);
        }
        else
        {
            UIManager.Instance.autoPlayToggle.isOn = false;
            UIManager.Instance.autoPlayToggle.interactable = true;
            //UIManager.Instance.autoPlayToggle.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// オートプレイ停止（セルフオート時のみ）
    /// </summary>
    /// <param name="log">ログ出力内容</param>
    public void StopAutoPlay(string log) {
        // 強制続行中はオートプレイを停止しない
        if (UIManager.Instance.forceAutoPlayToggle.isOn) return;

        if (currentAutoPlaySetting == autoPlaySelf) {
            Debug.Log("オートプレイ停止:" + log);
            UIManager.Instance.autoPlayToggle.isOn = false;
            UIManager.Instance.semiAutoPlayToggle.isOn = false;
        }
    }

    /// <summary>
    /// プレイ開始時処理
    /// </summary>
    public void OnStartPlay() {
        // レバーをアニメーションさせる
        lever.SetTrigger("On");
        // オートプレイ用停止パターン要求
        StartCoroutine(RequestAutoPlayPattern());
        // 遊技状態保存
        SavePlayData();
    }

    /// <summary>
    /// オートプレイ用停止パターン要求
    /// </summary>
    /// <returns></returns>
    IEnumerator RequestAutoPlayPattern() {
        // オートプレイ設定が確定するまで繰り返す
        while (currentAutoPlayPattern == null) {
            SetAutoPlayPattern();
            yield return 0;
        }
        yield return 0;
    }

    /// <summary>
    /// オートプレイ処理
    /// </summary>
    public void AutoPlay() {
        // 全部止まってるならコイン投入＆リール回して抜ける
        if (IsAllReelStopped()) {
            currentAutoPlayPattern = null; // オートプレイ設定をクリア            
            KeyInput(5);
            return;
        }

        // オートプレイ設定が未確定なら抜ける
        if (currentAutoPlayPattern == null) return;

        // 自動停止順に基づき停止対象リールを選択
        int targetReel = -1;
        int[] stopOrder;
        if (currentAutoPlayPattern.isStopReverse) {
            stopOrder = new int[] { 2, 1, 0 };
        } else {
            stopOrder = new int[] { 0, 1, 2 };
        }
        for (int idx = 0; idx < 3; idx++) {
            if (!mOmatsuri.IsReelStopped(stopOrder[idx])) {
                targetReel = stopOrder[idx];
                break;
            }
        }

        // 目押し処理
        if (currentAutoPlayPattern.targetRow[targetReel] == -1) {
            // -1なら連打
            KeyInput(targetReel + 1);
        } else {
            // 目当ての場所を狙って止める
            // 偶にリール番号が跳ぶ(1フレームあたりの回転数が早すぎ？)なので条件を甘く
            float allowSlip = 0; // 許容する面のズレ
            for (int cnt = 0; cnt < 1 + allowSlip; cnt++) {
                int faceIdx = mOmatsuri.ANGLE2INDEX(mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + targetReel]);
                int idxCorrectValue = 7; // 仕様の面番号と実際の面番号がズレてるので補正する
                int targetIdx = currentAutoPlayPattern.targetRow[targetReel] + idxCorrectValue;
                targetIdx += cnt;
                if (targetIdx >= 21) targetIdx -= 21;
                if (faceIdx == targetIdx) {
                    KeyInput(targetReel + 1);
                    break;
                }
            }
        }
        
    }    

    /// <summary>
    /// キー入力
    /// </summary>
    /// <param name="key">
    /// 入力キー種別
    /// 1=左リール停止
    /// 2=中リール停止
    /// 3=右リール停止
    /// 5=ワンキープレイ用キー(コイン投入、プレイ開始、リール停止を共用)
    /// </param>
    public void KeyInput(int key) {

        // ポーズ中であれば入力をキャンセルする
        if (pauseState == PAUSE_STATE.PAUSE)
        {
            return;
        }

        //Debug.Log("KEY:" + key);
        ZZ.int_value[Defines.DEF_Z_INT_KEYPRESS] |= (1 << key);
    }

    /// <summary>
    /// SE再生処理
    /// </summary>
    /// <param name="soundID">音ID(-1は無視する)</param>
    public static void PlaySE(int soundID) {
        if (soundID == -1) return;
        Instance.audioSE.PlayOneShot(Instance.soundDefine.clip[soundID]);
    }

    /// <summary>
    /// BGM再生処理
    /// </summary>
    /// <param name="soundID">音ID(-1は無視する)</param>
    public static void PlayBGM(int soundID, bool isLoop) {
        if (soundID == -1) return;
        Instance.audioBGM.clip = Instance.soundDefine.clip[soundID];
        Instance.audioBGM.loop = isLoop;
        Instance.audioBGM.Play();
    }

    /// <summary>
    /// SE停止処理
    /// </summary>
    public static void StopSE() {
        Instance.audioSE.Stop();
    }

    /// <summary>
    /// BGM停止処理
    /// </summary>
    public static void StopBGM() {
        Instance.audioBGM.Stop();
    }

    /// <summary>
    /// 機械割設定
    /// </summary>
    /// <param name="rate"></param>
    public void SetRate(int rate) {
        //Mobile.setSetUpValue(rate);
        mOmatsuri.gp.gpif_setting = rate;
    }

    public static bool isUseServerSeed;
    public static int GetRandomSeed() {
        if (isUseServerSeed) {
            return 30568;
        } else {
            return (((int)(Util.GetMilliSeconds())) & 0xFFFF);
        }
    }

    public void ClearPlayData() {
        PlayData.ClearData(playData);
        PlayData.Save(playData);
        RefreshCommonUIData();
    }
    public void PushBlankPlayData() {
        PlayData.PushBlankPlayData(playData, 1);
        PlayData.Save(playData);
        RefreshCommonUIData();
    }

    public void SetDummyPlayData() {
        PlayData.SetDummyData(playData);
        PlayData.Save(playData);
        RefreshCommonUIData();
    }

    public void OnBonusEnd(int bonus_incount) {
        AddHistory(gameCountOnBonus);
        LightLamp.Instance.OFF();
        History.Instance.Shift();
        playData.gameCount = 0;
        casinoData.GameCount = 0;
    }

    public void OnCountUp() {
        playData.totalGameCount++;
        History.Instance.Add();
        //casinoData.GameCount++;
        UpdateCommonUIAvg();
    }

    /// <summary>
    /// ボーナス開始時のゲーム数を記録
    /// セーブデータの値を反映しつつボーナス中に日を跨いでも問題無いように
    /// </summary>
    private int gameCountOnBonus;

    public void OnBonusRB() {
        gameCountOnBonus = playData.gameCount; // ボーナス開始時のゲーム数を記録
        LightLamp.Instance.ON();
        casinoData.RB++;
        UpdateCommonUIAvg();
    }
    public void OnBonusBB() {
        gameCountOnBonus = playData.gameCount; // ボーナス開始時のゲーム数を記録
        LightLamp.Instance.ON();
        casinoData.BB++;
        UpdateCommonUIAvg();
    }

    public void UpdateCommonUIAvg() {
        int bonusCount = casinoData.BB + casinoData.RB;
        casinoData.AVG = bonusCount == 0 ? 0 : playData.totalGameCount / bonusCount;
    }
}
