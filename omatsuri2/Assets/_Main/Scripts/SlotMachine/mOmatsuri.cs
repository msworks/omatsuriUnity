﻿using System;
using System.Linq;

public partial class mOmatsuri
{
    public static SlotInterface gp = null;
    private static bool BonusCutFg = false;
    public static long reelwait = 0;
    private static bool reelStartFg;            // リールスタートキーがすでに押されている場合
    private static bool reqMenuFg = false;      // リール停止時にメニューを表示する
    public static bool reqMenuFg2 = false;      // リール停止時にメニューを表示する
    public static bool bgm_resumeFg = false;    // BGMのレジューム再生フラグ
    public static int bgm_no = -1;
    public static bool bgm_loop = false;
    public static int maj_ver;                  // メインバージョン
    public static int sub_ver;	                // サブバージョン
    public static int prevHttpTime = 0;
    public static int kasidasiMedal = 0;
    public static int prevkasidasiMedal = 0;

    static bool IS_HALL()
    {
        return Mobile.getGameMode() == Defines.DEF_GMODE_HALL;
    }

    public static int[] hallData = new int[Defines.DEF_H_PARAM_NUM];

    public static readonly int[] int_s_value = new int[Defines.DEF_INT_SLOT_VALUE_MAX];

    /** ｻｳﾝﾄﾞ演奏時間 */
    private static long _soundTime;

    /** WAIT３０秒でランプが消える */
    private static long _lampTime;

    /** ４０秒で回転は止まる */
    private static long _spinTime;

    /** ４thリールの一時停止 */
    private static long _4thTime;

    /** リールの角度 [1.15.16] の固定少数 */
    private static readonly int ANGLE_2PI_BIT = 16;

    /** リールの角度マスク(0xFFFF)。 */
    private static readonly int ANGLE_2PI_MASK = (1 << ANGLE_2PI_BIT) - 1;

    /** リールの角度が未定義（INDEX2ANGLE では出ない値）)。 */
    private static readonly int ANGLE_UNDEF = -1;

    /**
	 * ボーナス間情報グラフ作成データ.
	 * bonus_Data[x(?回前)][y(100回転刻み)]
	 */
    private static readonly ushort[,] bonus_Data =
        new ushort[Defines.DEF_INFO_GAME_HISTORY, Defines.DEF_INFO_GAMES];

    public static readonly ushort[][] REELTB =
    {
        new ushort[]{ Defines.DEF_RPLY, Defines.DEF_BAR_, Defines.DEF_BELL, Defines.DEF_WMLN, Defines.DEF_CHRY, Defines.DEF_BSVN, Defines.DEF_RPLY,
        Defines.DEF_BELL, Defines.DEF_DON_, Defines.DEF_DON_, Defines.DEF_DON_, Defines.DEF_RPLY, Defines.DEF_BELL,
        Defines.DEF_CHRY, Defines.DEF_BSVN, Defines.DEF_WMLN, Defines.DEF_RPLY, Defines.DEF_BELL, Defines.DEF_WMLN,
        Defines.DEF_BELL, Defines.DEF_BSVN },
        new ushort[]{ Defines.DEF_BELL, Defines.DEF_BAR_, Defines.DEF_CHRY, Defines.DEF_BELL, Defines.DEF_RPLY, Defines.DEF_BSVN, Defines.DEF_WMLN,
        Defines.DEF_CHRY, Defines.DEF_BELL, Defines.DEF_RPLY, Defines.DEF_BAR_, Defines.DEF_CHRY, Defines.DEF_BAR_,
        Defines.DEF_BELL, Defines.DEF_RPLY, Defines.DEF_CHRY, Defines.DEF_DON_, Defines.DEF_BELL, Defines.DEF_RPLY,
        Defines.DEF_WMLN, Defines.DEF_RPLY },
        new ushort[]{ Defines.DEF_BELL, Defines.DEF_CHRY, Defines.DEF_RPLY, Defines.DEF_WMLN, Defines.DEF_BELL, Defines.DEF_BSVN, Defines.DEF_CHRY,
        Defines.DEF_RPLY, Defines.DEF_WMLN, Defines.DEF_BELL, Defines.DEF_CHRY, Defines.DEF_RPLY, Defines.DEF_WMLN,
        Defines.DEF_BELL, Defines.DEF_DON_, Defines.DEF_RPLY, Defines.DEF_CHRY, Defines.DEF_BELL, Defines.DEF_BAR_,
        Defines.DEF_RPLY, Defines.DEF_WMLN}
    };

    /**
	 * 次のランプ用ステータスを取得する
	 * @param idx
	 * @return ランプの次のステータス
	 */
    public static int getLampStatus(int idx)
    {
        if ((int_s_value[Defines.DEF_INT_LAMP_1 + (idx / 32)] & (1 << (idx % 32))) != 0)
        {
            return Defines.DEF_LAMP_STATUS_ON;
        }
        else
        {
            return Defines.DEF_LAMP_STATUS_OFF;
        }
    }

    /**
	 * ランプ用スイッチ
	 * @param idx:index
	 * @param act:action
	 */
    public static void lampSwitch(int idx, int act)
    {
        if (act == Defines.DEF_LAMP_ACTION_ON)
        {
            // ビットを1にする
            int_s_value[Defines.DEF_INT_LAMP_1 + (idx / 32)] |= (1 << (idx % 32));
        }
        else if (act == Defines.DEF_LAMP_ACTION_OFF)
        {
            // ビットを0にする
            int_s_value[Defines.DEF_INT_LAMP_1 + (idx / 32)] &= ~(1 << (idx % 32));
        }
    }

    /**
	 * リール位置番号から角度を求める。 <BR>
	 * INDEX2ANGLE(n) ((ANGLE_2PI+NUM_REEL-1)/NUM_REEL*((n)%NUM_REEL)) <BR>
	 * 切り上げ計算も含む <BR>
	 * @param n:絵柄ｲﾝﾃﾞｯｸｽ [0, NUM_REEL)
	 * @return ﾘｰﾙ角度？
	 */
    private static int INDEX2ANGLE(int n)
    {
        return 0xC31 * (n);
    }

    /**
	 * 角度から次に止まれる絵柄インデックスに変換。 <BR>
	 * 端数切り上げ
	 * @param i:角度
	 * @return 絵柄ｲﾝﾃﾞｯｸｽ [0, NUM_REEL)
	 */
    public static int ANGLE2INDEX(int i)
    {
        return ((Defines.DEF_N_FRAME * i + ANGLE_2PI_MASK) >> ANGLE_2PI_BIT)
                % Defines.DEF_N_FRAME;
    }

    /**
	 * モードをリクエストする。
	 * @param mod:スロットゲームのモード
	 * @return リクエストされたスロットゲームのモード
	 */
    private static int REQ_MODE(int mod)
    {
        return int_s_value[Defines.DEF_INT_REQUEST_MODE] = mod;
    }

    /**
	 * ゲーム情報の初期化。
	 */
    public static void initGameInfo()
    {
        int_s_value[Defines.DEF_INT_BIG_COUNT] = 0;
        int_s_value[Defines.DEF_INT_REG_COUNT] = 0;
        int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES] = 0;
        int_s_value[Defines.DEF_INT_TOTAL_GAMES] = 0;
        int_s_value[Defines.DEF_INT_GAME_INFO_MAX_GOT] = 0;
        int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] = Defines.DEF_NUM_START_COIN; // コイン数;
        int_s_value[Defines.DEF_INT_NUM_KASIDASI] = 1;
        initDataPaneHistory();
    }

    /**
	 * 新規ゲーム開始時に情報の履歴を初期化する
	 */
    private static void initDataPaneHistory()
    {
        int_s_value[Defines.DEF_INT_BONUS_DATA_BASE] = 0;
        for (int x = 0; x < Defines.DEF_INFO_GAME_HISTORY; x++)
        {
            for (int y = 0; y < Defines.DEF_INFO_GAMES; y++)
            {
                bonus_Data[x, y] = Defines.DEF_GAME_NONE;
            }
        }
    }

    /**
	 * ゲーム開始時に各値を初期化。
     * 0:通常1:シミュレーション
	 */
    public void newSlot()
    {
        for (int i = 0; i < int_s_value.Length; i++)
        {
            if (i != Defines.DEF_INT_REEL_SPEED)
            {
                // リールスピードだけ除外
                int_s_value[i] = 0;
            }
        }

        // メインループスピード
        int_s_value[Defines.DEF_INT_LOOP_SPEED] = ZZ.getThreadSpeed();

        // リールの初期化
        int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] = 0; // クレジットコイン数（０枚）
        int_s_value[Defines.DEF_INT_WIN_GET_COIN] = 0; // 払い出しコイン枚数
        int_s_value[Defines.DEF_INT_WIN_COIN_NUM] = 0; // １ゲーム中の獲得コイン枚数
        int_s_value[Defines.DEF_INT_BONUS_GOT] = 0; // ボーナス獲得数
        int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] = 0; //JACゲーム中の獲得枚数

        int_s_value[Defines.DEF_INT_BB_KIND] = Defines.DEF_BB_UNDEF; // ＢＢ入賞時の種別
        int_s_value[Defines.DEF_INT_BB_AFTER_1GAME] = 0; // BB 終了後の１ゲーム目？					

        // BB 終了後の１ゲーム目でそろえることができた？ミッションパラメータ		
        int_s_value[Defines.DEF_INT_BB_END_1GAME_REGET_BB] = 0;
        int_s_value[Defines.DEF_INT_BB_GET_OVER711] = 0; // BB獲得枚数 711枚以上？ミッションミッションパラメータ

        // モードの初期化
        int_s_value[Defines.DEF_INT_CURRENT_MODE] = Defines.DEF_RMODE_UNDEF;
        REQ_MODE(Defines.DEF_RMODE_WAIT);

        // ランプを全て消灯
        int_s_value[Defines.DEF_INT_LAMP_1] = 0;
        int_s_value[Defines.DEF_INT_LAMP_2] = 0;
        int_s_value[Defines.DEF_INT_LAMP_3] = 0;
        int_s_value[Defines.DEF_INT_KOKUCHI_ID] = 0;

        // Topランプアクション初期値
        int_s_value[Defines.DEF_INT_SEQUENCE_EFFECT] = 2;
        int_s_value[Defines.DEF_INT_RLL_COUNTER] = 0;
        int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] = 0;
        int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 0;
        int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] = Defines.DEF_RP19;

        playSE(Defines.DEF_RES_00);
        playBGM(Defines.DEF_RES_00, false);
        Mobile.stopSound(Defines.DEF_SOUND_UNDEF);

        // ゲーム情報の初期化
        initGameInfo();
        initDataPaneHistory();
        clOHHB_V23.mInitializaion(GameManager.GetRandomSeed());

        // 内部設定を6にする。
        clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)Mobile.getSetUpValue());

        // 停止フラグを立てる
        int_s_value[Defines.DEF_INT_REEL_STOP_R0] = int_s_value[Defines.DEF_INT_REEL_ANGLE_R0];
        int_s_value[Defines.DEF_INT_REEL_STOP_R1] = int_s_value[Defines.DEF_INT_REEL_ANGLE_R1];
        int_s_value[Defines.DEF_INT_REEL_STOP_R2] = int_s_value[Defines.DEF_INT_REEL_ANGLE_R2];
        int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] = 7; // リールストップ

        // 3D関係
        ZZ.setLight(0, 0, 100, 2024 * 100 / 4048, 1012 * 100 / 4048);

        restartSlot();
    }


    /**
	 * ゲーム再開。
	 */
    public void restartSlot()
    {
        // 再描画要求
        drawAll();
    }

    public static int getExitReason()
    {
        return int_s_value[Defines.DEF_INT_WHAT_EXIT];
    }

    private static int pressingSpan = 0;

    /// <summary>
    /// 大祭りプロセス
    /// </summary>
    /// <param name="keyTrigger"></param>
    /// <returns></returns>
	public bool process(int keyTrigger, CallbackToController callbacks)
    {
        int_s_value[Defines.DEF_INT_REEL_SPEED] =
            ZZ.getThreadSpeed()
            * Defines.DEF_REEL_COUNT_MIN
            * 0x10000
            / 60000
            * Mobile.getReelSpeed() / 100;

        if (Mobile.isMeoshi() || gp.gpif_auto_f || gp.gpif_nonstop_f || gp.gpif_tatsujin_f)
        {
            // ｵｰﾄﾌﾟﾚｲ
            if ((gp.gpif_auto_f) || (gp.gpif_nonstop_f) || (gp.gpif_tatsujin_f))
            {
                if (Mobile.int_m_value[Defines.DEF_INT_IS_MENU_AVAILABLE] == Defines.DEF_MENU_UNAVAILABLE)
                {
                    // メニューが無効の時
                    if ((keyTrigger & Defines.DEF_KEY_BIT_SOFT1) != 0)
                    {
                        reqMenuFg = true;
                    }

                    if ((keyTrigger & Defines.DEF_KEY_BIT_SOFT2) != 0)
                    {
                        reqMenuFg = true;
                    }
                }
            }

            keyTrigger = Defines.DEF_KEY_BIT_SELECT;
        }

        if (reqMenuFg == true)
        {
            // 自動メニュー画面描画
            if (Mobile.int_m_value[Defines.DEF_INT_IS_MENU_AVAILABLE] == Defines.DEF_MENU_AVAILABLE)
            {
                // メニューが無効の時
                reqMenuFg = false;
            }
        }

        if (gp.getBusy())
        {
            // GP用のウインドウが出ているので、筐体キーを無効化する
            keyTrigger = 0;
            return false;
        }

        // コイン枚数の更新
        int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] = gp.gpif_coin;

        // 40ms*10毎にタイミングを取ってみる
        pressingSpan++;
        pressingSpan %= 10;
        if (!IS_BONUS() && pressingSpan == 0) { }

        while (int_s_value[Defines.DEF_INT_CURRENT_MODE] != int_s_value[Defines.DEF_INT_REQUEST_MODE])
        {
            int_s_value[Defines.DEF_INT_CURRENT_MODE] = int_s_value[Defines.DEF_INT_REQUEST_MODE];

            // スロットゲームモード変更時に更新するフラグ
            int_s_value[Defines.DEF_INT_MODE_COUNTER] = 0;
            int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] = 0;
            int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] = 0;
            int_s_value[Defines.DEF_INT_TOP_LAMP] = 0;

            // 各モードでの初期化
            switch (int_s_value[Defines.DEF_INT_CURRENT_MODE])
            {
                case Defines.DEF_RMODE_WIN:
                    if (int_s_value[Defines.DEF_INT_WIN_LAMP] > 0 && int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] == 0)
                    {
                        // た～まや～点灯
                        int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 1;
                        // ボーナス当選時
                        gp.onBonusNaibu();
                        // セルフオート停止フラグを立てる
                        GameManager.Instance.StopAutoPlay("たまや点灯");
                    }

                    // さらに大当たりで止まっていたらかっきーん！
                    if (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] == 1)
                    {
                        if (int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] == Defines.DEF_RP08)
                        {
                            playSE(Defines.DEF_SOUND_12);
                            _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_12;
                            int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 2;

                            // 以後演出抽選しないようにする
                            clOHHB_V23.setWork(Defines.DEF_FOUT3, (ushort)1);
                        }
                    }
                    break;

                case Defines.DEF_RMODE_WAIT:

                    if (IS_HALL())
                    {
                        //不正防止用にここで保存
                        Mobile.saveMenuData(true);
                    }

                    // 押せるようにする
                    Mobile.setMenuAvarable(true);

                    if (!IS_BONUS())
                    {
                        lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_OFF);
                    }

                    // コイン７セグ描画用変数を初期化
                    int_s_value[Defines.DEF_INT_BETTED_COUNT] = 0;

                    // 演出帳のデータを転送する
                    GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_WEB, -1);

                    break;

                case Defines.DEF_RMODE_BET:

                    // MAXBETﾗﾝﾌﾟ表示期間(モード初期)
                    // リール全点滅は一回だけ
                    if (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] == 2)
                    {
                        int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 3;
                    }

                    // 払い出しコイン枚数（表示用）
                    int_s_value[Defines.DEF_INT_WIN_GET_COIN] = 0;
                    int_s_value[Defines.DEF_INT_WIN_COIN_NUM] = 0;  // １ゲーム中の獲得コイン枚数
                    int_s_value[Defines.DEF_INT_IS_TEMPAI] = 0;     // ﾃﾝﾊﾟｲ状態

                    // 告知はBET時にクリア
                    if (!IS_REPLAY())
                    {
                        int_s_value[Defines.DEF_INT_KOKUCHI_X] = 0;
                    }

                    // サウンド
                    if (!IS_BONUS())
                    {
                        // ボーナス消化中以外は毎回停止指示。
                        Mobile.stopSound(Defines.DEF_SOUND_UNDEF);
                    }

                    if (IS_HALL())
                    {
                        /*プレーヤーコイン要求*/
                        if (hallData[Defines.DEF_H_PLAYER_COIN] < int_s_value[Defines.DEF_INT_BET_COUNT])
                        {
                            break;
                        }
                    }

                    switch (int_s_value[Defines.DEF_INT_BET_COUNT])
                    {
                        case 1:
                            playSE(Defines.DEF_SOUND_25);
                            _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_25;
                            break;
                        case 2:
                            playSE(Defines.DEF_SOUND_26);
                            _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_26;
                            break;
                        case 3:
                            playSE(Defines.DEF_SOUND_22);
                            _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_22;
                            break;
                    }

                    // MENU遷移を禁止する
                    Mobile.setMenuAvarable(false);

                    break;

                case Defines.DEF_RMODE_SPIN:

                    // 回転開始(モード初期)
                    if (!IS_REPLAY())
                    {
                        int_s_value[Defines.DEF_INT_TOTAL_BET] += int_s_value[Defines.DEF_INT_BETTED_COUNT];
                        hallData[Defines.DEF_H_MEDAL_IN] += int_s_value[Defines.DEF_INT_BETTED_COUNT];
                        hallData[Defines.DEF_H_PLAYER_COIN] -= int_s_value[Defines.DEF_INT_BETTED_COUNT];
                    }

                    // Z80移植リール部分の初期化
                    // 1プレイ遊技用初期化
                    clOHHB_V23.clearWork(Defines.DEF_CLR_AREA_3);

                    // Z80移植
                    // パチスロ抽選用乱数を取得
                    int rand = clOHHB_V23.mRandomX();

                    // 設定変更チェック
                    chgWaveNum();

                    // 役の変更チェック
                    chgYaku();

                    // 役抽選
                    clOHHB_V23.mReelStart(rand, int_s_value[Defines.DEF_INT_BET_COUNT]);

                    var yaku = clOHHB_V23.getWork(Defines.DEF_WAVEBIT);
                    callbacks.ReelStart(yaku);

                    if (!IS_BONUS())
                    {
                        if ((clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & 0x01) != 0)
                        {
                            int_s_value[Defines.DEF_INT_CHRY_HIT]++;
                        }
                        else if ((clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & 0x04) != 0)
                        {
                            int_s_value[Defines.DEF_INT_WMLN_HIT]++;
                        }
                    }
                    else if (IS_BONUS_GAME())
                    {
                        // ビタ外し成功
                        if ((clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & 0x08) != 0)
                        {
                            int_s_value[Defines.DEF_INT_JAC_HIT]++;
                        }
                    }

                    if ((clOHHB_V23.getWork(Defines.DEF_GMLVSTS) & (0x08 | 0x10)) != 0)
                    {
                        int_s_value[Defines.DEF_INT_THIS_FLAG_GAME]++;
                    }

                    // 演出をセット
                    int flash0 = clOHHB_V23.getWork(Defines.DEF_FLASH + 0);
                    GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_CHK_LANP, (flash0 / 128));

                    // 確定ランプフラグ
                    int_s_value[Defines.DEF_INT_WIN_LAMP] = flash0 / 128;
                    flash0 %= 128;
                    int flash1 = clOHHB_V23.getWork(Defines.DEF_FLASH + 1);

                    // 演出によるチェック
                    GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_CHK_FLASH, flash1);
                    setFlash(flash1 / 32);
                    set4th(flash1 % 32);

                    // 開始音を鳴らす
                    int snd_id = Defines.DEF_SOUND_19;

                    if (flash1 % 32 > 0)
                    {
                        snd_id = Defines.DEF_SOUND_21;
                    }

                    if (flash0 / 64 > 0)
                    {
                        snd_id = Defines.DEF_SOUND_20;
                    }

                    if (Mobile.isJacCut() == false)
                    {
                        playSE(snd_id);
                    }

                    // 回転開始時間を記録する
                    _spinTime = Util.GetMilliSeconds() + Defines.DEF_WAIT_SPIN;
                    int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] = 0; // リールストップ

                    // 告知はSPIN時にクリア
                    int_s_value[Defines.DEF_INT_KOKUCHI_X] = 0;

                    // ボーナス（ＢＢ・ＲＢ）終了ですか？
                    if (int_s_value[Defines.DEF_INT_IS_BB_RB_END] > 0)
                    {
                        // ここはボーナス終了ゲームの次ゲームの回転開始です
                        if (!IS_HALL())
                        {
                            // データパネル更新
                            if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7
                                    || int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7)
                            {
                                // ＢＢ終了時のみ
                                shiftDataPanelHistory(int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES],
                                        Defines.DEF_PS_BB_RUN);
                            }
                            else if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_RB_IN)
                            {
                                // ＲＢ終了時のみ
                                shiftDataPanelHistory(int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES],
                                        Defines.DEF_PS_RB_RUN);
                            }
                        }

                        // ボーナス関係のフラグたちをクリア
                        int_s_value[Defines.DEF_INT_IS_BB_RB_END] = 0; // ボーナス終了後の次ゲームの回転でこのフラグおろす
                        int_s_value[Defines.DEF_INT_BB_KIND] = Defines.DEF_BB_UNDEF; // ＢＢ入賞時の種別

                        int_s_value[Defines.DEF_INT_BONUS_GOT] = 0; // ボーナス獲得枚数の値をクリア
                        int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] = 0; // JAC中の獲得枚数をクリア
                                                                        // ボーナス間ゲーム数をボーナス終了時にクリア
                        int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES] = 0;
                        hallData[Defines.DEF_H_BNS_0] = 0;

                        // TOBE 個別PARAM　BB 終了後の１ゲーム目ですよフラグを立てる
                        int_s_value[Defines.DEF_INT_BB_AFTER_1GAME] = 1;
                    }

                    if (!IS_BONUS())
                    {
                        // 通常ゲーム中
                        // ボーナス消化中のゲーム数を総回転数としてカウントしない
                        int_s_value[Defines.DEF_INT_TOTAL_GAMES]++;

                        // 総回転数を増やす
                        //DfMain.TRACE(("総回転数のカウントアップ");
                        gp.onCountUp();

                        hallData[Defines.DEF_H_GAME_COUNT]++;

                        // ボーナス間累積（ゲーム情報）
                        int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES]++;
                        hallData[Defines.DEF_H_BNS_0]++;
                    }

                    // データパネル情報更新（ゲーム数）
                    if (IS_HALL())
                    {
                        setCurrentDataPanel(hallData[Defines.DEF_H_BNS_0]);
                    }
                    else
                    {
                        setCurrentDataPanel(int_s_value[Defines.DEF_INT_UNTIL_BONUS_GAMES]);
                    }

                    // キーリジェクトの値。
                    int_s_value[Defines.DEF_INT_KEY_REJECT] = 5;

                    // 停止フラグ
                    int_s_value[Defines.DEF_INT_REEL_STOP_R0] = ANGLE_UNDEF;
                    int_s_value[Defines.DEF_INT_REEL_STOP_R1] = ANGLE_UNDEF;
                    int_s_value[Defines.DEF_INT_REEL_STOP_R2] = ANGLE_UNDEF;
                    int_s_value[Defines.DEF_INT_REEL_ANGLE_R0] -= int_s_value[Defines.DEF_INT_REEL_SPEED] * 2 / 4;
                    int_s_value[Defines.DEF_INT_REEL_ANGLE_R1] -= int_s_value[Defines.DEF_INT_REEL_SPEED] * 2 / 4;
                    int_s_value[Defines.DEF_INT_REEL_ANGLE_R2] -= int_s_value[Defines.DEF_INT_REEL_SPEED] * 2 / 4;

                    GameManager.Instance.OnStartPlay();
                    break;

                case Defines.DEF_RMODE_FLASH: // (モード初期)
                    break;

                case Defines.DEF_RMODE_RESULT:
                    // (モード初期)
                    // RESULTに入った時間を記録
                    _lampTime = Util.GetMilliSeconds() + Defines.DEF_WAIT_LAMP;

                    // 払い出しコイン枚数
                    var payOut = clOHHB_V23.mPayMedal();
                    int_s_value[Defines.DEF_INT_WIN_COIN_NUM] = payOut;
                    int_s_value[Defines.DEF_INT_TOTAL_PAY] += payOut;
                    hallData[Defines.DEF_H_MEDAL_OUT] += payOut;

                    callbacks.Payout(payOut);

                    if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_RB) != 0)
                    {
                        Defines.TRACE("RB入賞時");
                    }
                    else if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_BB) != 0)
                    {
                        Defines.TRACE("BB入賞時");
                    }

                    //DfMain.TRACE(("払い出し分加算");
                    if (!IS_BONUS())
                    {
                        if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & 0x01) != 0)
                        {
                            int_s_value[Defines.DEF_INT_CHRY_GOT]++;
                        }
                        else if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & 0x04) != 0)
                        {
                            int_s_value[Defines.DEF_INT_WMLN_GOT]++;
                        }
                    }
                    else if (IS_BONUS_GAME())
                    {
                        // ビタ外し成功
                        if (clOHHB_V23.getWork(Defines.DEF_HITFLAG) == 0
                                && ((clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & 0x08) != 0)
                                && (clOHHB_V23.getWork(Defines.DEF_ARAY11) == Defines.DEF_BAR_))
                        {
                            int_s_value[Defines.DEF_INT_HAZUSI_COUNT]++;
                        }
                    }

                    int_s_value[Defines.DEF_INT_BONUS_GOT] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];

                    if ((IS_BONUS_JAC() == true))
                    {
                        int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];
                    }

                    // 払い出し音の発生(獲得枚数がなくても鳴らさなくてはならないので、ここで呼ぶ)
                    playCoinSound();
                    break;

                case Defines.DEF_RMODE_BB_FANFARE:
                    // (モード初期)
                    int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 0;
                    int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] = 0;
                    lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_ON);
                    // BBﾌｧﾝﾌｧｰﾚ鳴らすﾓｰﾄﾞ(→Defines.DEF_RMODE_BB_FANFARE_VOICE)
                    if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7)
                    {
                        _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_03; // ﾌｧﾝﾌｧｰﾚ完奏時間設定
                        playBGM(Defines.DEF_SOUND_03, false); // BBﾌｧﾝﾌｧｰﾚ1(ﾄﾞﾝﾁｬﾝ揃い)
                    }
                    else if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7)
                    {
                        _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_02; // ﾌｧﾝﾌｧｰﾚ完奏時間設定
                        playBGM(Defines.DEF_SOUND_02, false); // BBﾌｧﾝﾌｧｰﾚ2(7揃い)
                    }
                    set4th(29);
                    break;

                case Defines.DEF_RMODE_RB_FANFARE: // (モード初期)
                    int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 0;
                    int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] = 0;
                    lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_ON);
                    // RBﾌｧﾝﾌｧｰﾚ鳴らすﾓｰﾄﾞ(→Defines.DEF_RMODE_BB_FANFARE_VOICE)
                    _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_04; // ﾌｧﾝﾌｧｰﾚ完奏時間設定
                    playBGM(Defines.DEF_SOUND_04, false);
                    set4th(29);
                    break;

                case Defines.DEF_RMODE_FIN_WAIT:
                    // (モード初期)
                    int_s_value[Defines.DEF_INT_KEY_REJECT] = 0;
                    // 毎ゲーム終了時にここを通る
                    ushort bonusEndFg;
                    bonusEndFg = clOHHB_V23.mBonusCounter();

                    if (IS_BONUS() == true)
                    {
                        // ボーナス終了フラグじゃないとき
                        if (cutBonusSystem(0))
                        {
                            // ﾎﾞｰﾅｽｶｯﾄ処理が必要の場合
                            // カット処理フラグON
                            BonusCutFg = true;

                            if ((int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7) ||
                                (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7))
                            {
                                // ＢＢ終了判定(RBの場合はRB終了時にメダル加算を行なう)
                                if ((IS_BONUS_JAC() == true))
                                {
                                    int num;
                                    num = 0;
                                    // JACゲームの獲得枚数
                                    if (int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] < Defines.JAC_BONUS_AVENUM)
                                    {   // JACゲームのカット枚数を加算する
                                        num = (Defines.JAC_BONUS_AVENUM - int_s_value[Defines.DEF_INT_BONUS_JAC_GOT]);
                                        int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] = 0; // JAC中の獲得枚数をクリア
                                        int_s_value[Defines.DEF_INT_BONUS_GOT] += num;
                                    }
                                    Defines.TRACE("JACカット分を追加:" + num);
                                    GPW_chgCredit(num);
                                    BonusCutFg = false; // JACのカットはここまでなので
                                }
                            }
                            bonusEndFg = clOHHB_V23.mBonusCounter();
                        }
                    }

                    if (bonusEndFg != 0)
                    {
                        BonusEnd(0);
                    }
                    break;

                case Defines.DEF_RMODE_NO_COIN:
                    // (モード初期)
                    return true; // コインなしで終了通知
            }

            break;
        }

        if (int_s_value[Defines.DEF_INT_PREV_GAMEST] != clOHHB_V23.getWork(Defines.DEF_GAMEST))
        {
            int_s_value[Defines.DEF_INT_PREV_GAMEST] = clOHHB_V23.getWork(Defines.DEF_GAMEST);
            if ((int_s_value[Defines.DEF_INT_PREV_GAMEST] & 0x01) != 0)
            {
                // JACBGMを鳴らす
                playBGM(Defines.DEF_SOUND_05, true);
            }
            else if ((int_s_value[Defines.DEF_INT_PREV_GAMEST] & 0x80) != 0)
            {
                // BIGBGMを鳴らす
                if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7)
                {
                    playBGM(Defines.DEF_SOUND_07, true);
                }
                else if (int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7)
                {
                    playBGM(Defines.DEF_SOUND_06, true);
                }
            }
        }

        int_s_value[Defines.DEF_INT_MODE_COUNTER]++; // モードが切り替わってからの累積カウンタ

        ctrlTopLamp();
        int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] = Util.GetMilliSeconds() % 1000 > 500 ? 1 : 0;

        ctrlLamp();
        ctrlBetLamp();

        DoModeAction(keyTrigger, callbacks);
        return false;
    }

    // ======================================
    // 各モードにおける毎回の処理
    // ======================================
    private void DoModeAction(int keyTrigger, CallbackToController callbacks)
    {
        switch (int_s_value[Defines.DEF_INT_CURRENT_MODE])
        {
            // TOBE [=0.RMODE_WAIT rp]
            case Defines.DEF_RMODE_WAIT: // （毎回処理）
                // 直前の停止音の完奏を待つ
                if (_soundTime < Util.GetMilliSeconds())
                {
                    // ﾘﾌﾟﾚｲが揃っているときは、自動的にRMODE_BETまで遷移する
                    if (bgm_resumeFg == true)
                    {
                        // 休憩中からの復帰
                        if (bgm_no != -1)
                        {
                            if (IS_BONUS())
                            {
                                // ボーナス時限定
                                Defines.TRACE("復帰サウンドの再生");
                                playBGM(bgm_no, bgm_loop); // 復帰サウンドの再生
                                bgm_resumeFg = false;
                            }
                            else {
                                // ボーナス消化中以外は毎回停止指示。
                                Mobile.stopSound(Defines.DEF_SOUND_UNDEF);
                            }
                        }
                    }

                    gp.betFlag = false;
                    if (IS_REPLAY())
                    {
                        gp.betFlag = true;
                        REQ_MODE(Defines.DEF_RMODE_BET); // MAXBETへ遷移
                    }
                    else
                    {
                        // BET開始
                        if ((keyTrigger & (Defines.DEF_KEY_BIT_SELECT | Defines.DEF_KEY_BIT_5)) != 0)
                        {
                            if (IS_BONUS_JAC())
                            {
                                int_s_value[Defines.DEF_INT_BET_COUNT] = 1;
                            }
                            else {
                                int_s_value[Defines.DEF_INT_BET_COUNT] = 3;
                            }

                            if (int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] < int_s_value[Defines.DEF_INT_BET_COUNT])
                            {
                                // コインがないからBETさせない
                                gp.onCreditZero();
                            }
                            else
                            {
                                REQ_MODE(Defines.DEF_RMODE_BET);
                                gp.betFlag = true;
                            }
                        }
                    }
                }
                break;
            // TOBE [=1.RMODE_BET rp]
            case Defines.DEF_RMODE_BET: // MAXBETﾗﾝﾌﾟ表示期間（毎回処理）
                #region DEF_RMODE_BET
                //DfMain.TRACE(("ベット処理２");
                // サウンドの終わりを待つ
                // 描画を増やす
                if (int_s_value[Defines.DEF_INT_BETTED_COUNT] < int_s_value[Defines.DEF_INT_BET_COUNT])
                {
                    // ココのタイミングの取り方は40msでループするのが前提
                    if (int_s_value[Defines.DEF_INT_MODE_COUNTER] % 2 == 0)
                    {
                        if (!IS_REPLAY())
                        {
                            // クレジットから減らす
                            if (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] > 0)
                            {
                                int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM]--;
                            }
                            int_s_value[Defines.DEF_INT_SLOT_COIN_NUM]--;

                            // BETした分だけ減算する
                            GPW_chgCredit(-1);

                            // コイン投入時処理
                            GameManager.Instance.OnCoinInsert();

                            callbacks.Bet();
                        }
                        int_s_value[Defines.DEF_INT_BONUS_GOT]--;
                        int_s_value[Defines.DEF_INT_BETTED_COUNT]++;


                        //GPW_chgCredit(0 - int_s_value[Defines.DEF_INT_BETTED_COUNT]);
                    }
                }
                else if (int_s_value[Defines.DEF_INT_BETTED_COUNT] > int_s_value[Defines.DEF_INT_BET_COUNT])
                {
                    // 減らす
                    // ココのタイミングの取り方は40msでループするのが前提
                    if (int_s_value[Defines.DEF_INT_MODE_COUNTER] % 2 == 0)
                    {
                        if (!IS_REPLAY())
                        {
                            if (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] < Defines.DEF_NUM_MAX_CREDIT)
                            {
                                int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM]++;
                            }
                            int_s_value[Defines.DEF_INT_SLOT_COIN_NUM]++;
                        }
                        int_s_value[Defines.DEF_INT_BONUS_GOT]++;
                        int_s_value[Defines.DEF_INT_BETTED_COUNT]--;
                    }
                }
                else
                {
                    if (_soundTime < Util.GetMilliSeconds())
                    {
                        // 回転開始
                        //DfMain.TRACE(("回転開始チェック:" + int_s_value[Defines.DEF_INT_BETTED_COUNT]);
                        if ((int_s_value[Defines.DEF_INT_BETTED_COUNT] > 0)
                            && (((keyTrigger & (Defines.DEF_KEY_BIT_SELECT | Defines.DEF_KEY_BIT_5)) != 0)
                            || (reelStartFg == true)))
                        {
                            //DfMain.TRACE(("回転開始ウェイト");
                            reelStartFg = true;
                            lampSwitch(Defines.DEF_LAMP_LEVER, Defines.DEF_LAMP_ACTION_ON);
                            if (Mobile.isJacCut() == true)
                            {	// ボーナスカットの場合
                                reelwait = -3200;
                            }
                            if ((reelwait + 3200) < Util.GetMilliSeconds())
                            {	// リールウェイト
                                //DfMain.TRACE(("回転開始");
                                REQ_MODE(Defines.DEF_RMODE_SPIN);
                                //DfMain.TRACE(("リールウェイト:" + (Util.GetMilliSeconds() - reelwait));
                                reelwait = Util.GetMilliSeconds();//リール全体用
                                reelStartFg = false;
                            }
                        }
                    }
                }
                break;
            #endregion
            // TOBE [=2.RMODE_SPIN rp]
            case Defines.DEF_RMODE_SPIN: // 回転中（毎回処理）
                // 4thをまわす。
                action4th();
                // 全部止まったらモード変わる
                if (int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] == 7)
                {
                    // 停止音 完奏を待って次のモードへ遷移
                    if (_soundTime < Util.GetMilliSeconds())
                    {
                        // 4thが止まったら
                        if (int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] == 0)
                        {
                            if (isPlay())
                            {
                                REQ_MODE(Defines.DEF_RMODE_FLASH);
                            }
                            else {
                                REQ_MODE(Defines.DEF_RMODE_WIN);
                            }
                        }
                    }
                    break;
                }

                // 前に鳴らしたサウンド待ち
                if (_soundTime < Util.GetMilliSeconds() || int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] != 0)
                {
                    if (int_s_value[Defines.DEF_INT_KEY_REJECT] > 0)
                    {
                        // 一定ターン待つ
                        int_s_value[Defines.DEF_INT_KEY_REJECT]--;
                    }
                    else
                    {
                        // 同時押しは出来ない ワンボタン(KEY_5)操作あり
                        // 自動停止の場合用
                        bool isTimeLimitStop = false;

                        if (_spinTime < Util.GetMilliSeconds())
                        {
                            isTimeLimitStop = true;
                        }

                        // 自動停止もあり
                        if ((keyTrigger & (Defines.DEF_KEY_BIT_SELECT | Defines.DEF_KEY_BIT_5)) != 0 || (isTimeLimitStop == true))
                        {
                            var isSpinning = true;
                            var buttons = Enumerable.Range(0, 3);
                            foreach (var btn in buttons)
                            {
                                if (isSpinning)
                                {
                                    isSpinning = setReelStopAngle(btn, callbacks);
                                }
                            }
                        }
                        else
                        {
                            bool isSpinning = true;

                            if (isSpinning && (keyTrigger & (Defines.DEF_KEY_BIT_1)) != 0)
                            {
                                isSpinning = setReelStopAngle(0, callbacks);
                            }

                            if (isSpinning && (keyTrigger & (Defines.DEF_KEY_BIT_2)) != 0)
                            {
                                isSpinning = setReelStopAngle(1, callbacks);
                            }

                            if (isSpinning && (keyTrigger & (Defines.DEF_KEY_BIT_3)) != 0)
                            {
                                isSpinning = setReelStopAngle(2, callbacks);
                            }
                        }
                    }
                }

                // 停止ボタンを点灯
                ctrlButtonLamp();

                // リールを進める。
                for (int i = 0; i < 3; i++)
                {
                    // 止まっていたら次
                    if ((int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] & BIT(i)) != 0)
                        continue;

                    if (int_s_value[Defines.DEF_INT_REEL_STOP_R0 + i] != ANGLE_UNDEF
                            && (((int_s_value[Defines.DEF_INT_REEL_STOP_R0 + i] - int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + i])
                                & ANGLE_2PI_MASK) <= int_s_value[Defines.DEF_INT_REEL_SPEED] ||
                                (Mobile.isJacCut() == true)))
                    {
                        // 止めにかかる
                        int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + i] = int_s_value[Defines.DEF_INT_REEL_STOP_R0 + i];
                        // 止まった
                        int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] |= BIT(i);
                        int_s_value[Defines.DEF_INT_KEY_REJECT] = 1;

                        // 次のボタンが押せるようにする
                        int stop_snd_id = Defines.DEF_SOUND_23;
                        _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_23 / 2;

                        if (!IS_BONUS())
                        {
                            int_s_value[Defines.DEF_INT_IS_TEMPAI] = 0;
                            int[] tempai = isTempai();
                            switch (clOHHB_V23.getWork(Defines.DEF_PUSHCTR))
                            {
                                case 0x02:// 第1停止
                                    if (i == 0)
                                    {	// 停止リールが左リールの時
                                        GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_CHK_REEL, (int)Defines.EVENT.EVENT_NO1);
                                    }
                                    break;

                                case 0x01:// 第2停止
                                    if (tempai[1] == 3)
                                    {
                                        //ﾄﾘﾌﾟﾙﾃﾝﾊﾟｲ音
                                        // (トリプルテンパイ（BIG確）)
                                        GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_CHK_REEL, (int)Defines.EVENT.EVENT_NO2);
                                        stop_snd_id = Defines.DEF_SOUND_15;
                                        _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_15;
                                        int_s_value[Defines.DEF_INT_IS_TEMPAI] = 1;
                                    }
                                    else if (tempai[0] != Defines.DEF_BB_UNDEF)
                                    {
                                        stop_snd_id = Defines.DEF_SOUND_14;
                                        _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_14;
                                        int_s_value[Defines.DEF_INT_IS_TEMPAI] = 1;
                                    }
                                    break;
                                case 0x00:// 第三停止
                                    if (tempai[0] == Defines.DEF_BB_B7)
                                    {
                                        int_s_value[Defines.DEF_INT_BB_KIND] = Defines.DEF_BB_B7;
                                        //TOBE 個別PARAM ＢＢ揃ったかの判定									
                                        if (int_s_value[Defines.DEF_INT_BB_AFTER_1GAME] > 0)
                                        {
                                            int_s_value[Defines.DEF_INT_BB_END_1GAME_REGET_BB] = 1; // 揃えた
                                            if (Defines.DEF_IS_DEBUG_MISSION_PARAM)
                                            {
                                                Defines.TRACE("1ゲーム目で青七をそろえた");
                                            }
                                        }
                                        gp.onBonusBB();
                                    }
                                    else if (tempai[0] == Defines.DEF_BB_R7)
                                    {
                                        int_s_value[Defines.DEF_INT_BB_KIND] = Defines.DEF_BB_R7;

                                        //TOBE 個別PARAM ＢＢ揃ったかの判定									
                                        if (int_s_value[Defines.DEF_INT_BB_AFTER_1GAME] > 0)
                                        {
                                            int_s_value[Defines.DEF_INT_BB_END_1GAME_REGET_BB] = 1; // 揃えた									
                                            if (Defines.DEF_IS_DEBUG_MISSION_PARAM)
                                            {
                                                Defines.TRACE("1ゲーム目で赤ﾄﾞﾝをそろえた");
                                            }
                                        }
                                        gp.onBonusBB();
                                    }

                                    // ゲチェナ
                                    GPW_eventProcess((int)Defines.EVENT_PROC.EVENT_PROC_CHK_REEL, (int)Defines.EVENT.EVENT_NO3);
                                    // TOBE 個別PARAM用フラグで使うフラグを必ず下ろす
                                    int_s_value[Defines.DEF_INT_BB_AFTER_1GAME] = 0;
                                    break;
                            }
                        }

                        if ((Mobile.isJacCut() == false))
                        {
                            playSE(stop_snd_id);
                        }

                    }
                    else
                    {
                        int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + i] =
                            (int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + i] + int_s_value[Defines.DEF_INT_REEL_SPEED]) & ANGLE_2PI_MASK;
                    }
                }

                break;

            case Defines.DEF_RMODE_FLASH: // 結果（毎回処理）

                //スピード調整
                if (ZZ.getThreadSpeed() < 40 && int_s_value[Defines.DEF_INT_MODE_COUNTER] % 2 == 0)
                {
                    break;
                }

                if (isPlay())
                {
                    int_s_value[Defines.DEF_INT_FLASH_DATA] = getNext();
                    // リールフラッシュ以外の部分
                    if ((int_s_value[Defines.DEF_INT_FLASH_DATA] & (1 << 10)) != 0)
                    {
                        lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_ON);
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_OFF);
                    }
                }
                else
                {
                    REQ_MODE(Defines.DEF_RMODE_WIN);
                }

                break;

            case Defines.DEF_RMODE_WIN:
                if (_soundTime < Util.GetMilliSeconds())
                {
                    REQ_MODE(Defines.DEF_RMODE_RESULT);
                }
                break;

            case Defines.DEF_RMODE_RESULT: // 結果（毎回処理）
                if ((Mobile.isJacCut() == true))
                {
                    // 内部でカウント
                    int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];
                    int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];
                    // 表示用は個々で増やす
                    int_s_value[Defines.DEF_INT_WIN_GET_COIN] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];
                    hallData[Defines.DEF_H_PLAYER_COIN] += int_s_value[Defines.DEF_INT_WIN_COIN_NUM];

                    // ５０枚まではクレジットへ貯める
                    if (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] > Defines.DEF_NUM_MAX_CREDIT)
                    {
                        int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] = Defines.DEF_NUM_MAX_CREDIT;
                    }

                    // ＭＡＸを超えないように。
                    if (int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] > Defines.DEF_NUM_MAX_COIN)
                    {
                        int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] = Defines.DEF_NUM_MAX_COIN;
                    }
                    _soundTime = 0;

                    // 払い出し分加算
                    GPW_chgCredit(int_s_value[Defines.DEF_INT_WIN_COIN_NUM]);
                }
                else
                {
                    // 一枚一枚移す
                    // satoh#暫定
                    if (int_s_value[Defines.DEF_INT_WIN_GET_COIN] < int_s_value[Defines.DEF_INT_WIN_COIN_NUM])
                    {
                        if ((int_s_value[Defines.DEF_INT_MODE_COUNTER] % (Defines.DEF_WAIT_COUNT_UP / int_s_value[Defines.DEF_INT_LOOP_SPEED] + 1)) == 0)
                        {
                            // 払い出し分加算
                            GPW_chgCredit(1);

                            // ５０枚まではクレジットへ貯めるぅ
                            if (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] < Defines.DEF_NUM_MAX_CREDIT)
                            {
                                int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM]++;
                            }

                            int_s_value[Defines.DEF_INT_SLOT_COIN_NUM]++;
                            hallData[Defines.DEF_H_PLAYER_COIN]++;

                            // 表示用は個々で増やす
                            int_s_value[Defines.DEF_INT_WIN_GET_COIN]++;

                            // ＭＡＸを超えないように。
                            if (int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] > Defines.DEF_NUM_MAX_COIN)
                            {
                                int_s_value[Defines.DEF_INT_SLOT_COIN_NUM] = Defines.DEF_NUM_MAX_COIN;
                            }
                        }
                        break;
                    }
                }

                // 払い出し音を待つ
                if (_soundTime < Util.GetMilliSeconds() || (int_s_value[Defines.DEF_INT_WIN_COIN_NUM] <= int_s_value[Defines.DEF_INT_WIN_GET_COIN] && !IS_REPLAY()))
                {
                    Mobile.stopSound(Defines.DEF_SOUND_MULTI_SE);
                    _soundTime = Util.GetMilliSeconds();
                    // REG入賞
                    if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_RB) != 0)
                    {
                        int_s_value[Defines.DEF_INT_BONUS_GOT] = 15;
                        int_s_value[Defines.DEF_INT_REG_COUNT]++;
                        int_s_value[Defines.DEF_INT_FLAG_GAME_COUNT] += int_s_value[Defines.DEF_INT_THIS_FLAG_GAME];
                        int_s_value[Defines.DEF_INT_THIS_FLAG_GAME] = 0;
                        hallData[Defines.DEF_H_RB_COUNT]++;/*HALL*/

                        // Jac-in 突入
                        int_s_value[Defines.DEF_INT_BB_KIND] = Defines.DEF_RB_IN;
                        REQ_MODE(Defines.DEF_RMODE_RB_FANFARE); // ＲＢファンファーレへ遷移

                        gp.onBonusRB();
                        break;
                    }
                    else if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_BB) != 0)
                    {
                        int_s_value[Defines.DEF_INT_BONUS_GOT] = 15;
                        int_s_value[Defines.DEF_INT_FLAG_GAME_COUNT] += int_s_value[Defines.DEF_INT_THIS_FLAG_GAME];
                        int_s_value[Defines.DEF_INT_THIS_FLAG_GAME] = 0;
                        int_s_value[Defines.DEF_INT_BIG_COUNT]++;
                        hallData[Defines.DEF_H_BB_COUNT]++;/*HALL*/
                        REQ_MODE(Defines.DEF_RMODE_BB_FANFARE); // ＢＢファンファーレへ遷移

                        // セルフオート停止フラグを立てる
                        GameManager.Instance.StopAutoPlay("BB入賞");
                        break;
                    }
                    REQ_MODE(Defines.DEF_RMODE_FIN_WAIT);
                    break;
                }
                break;

            case Defines.DEF_RMODE_BB_FANFARE:
            case Defines.DEF_RMODE_RB_FANFARE:
                action4th();
                if (_soundTime < Util.GetMilliSeconds() && int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] == 0)
                {
                    REQ_MODE(Defines.DEF_RMODE_FIN_WAIT);
                }
                break;

            case Defines.DEF_RMODE_FIN_WAIT:
                if (int_s_value[Defines.DEF_INT_IS_BB_RB_END] > 0 && int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_RB_IN)
                {
                    int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] += Defines.DEF_POS_4TH_TOTAL_W - 20;
                    int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] %= Defines.DEF_POS_4TH_TOTAL_W;
                    if (Defines.DEF_RP19 - 20 < int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE]
                            && int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] <= Defines.DEF_RP19 + 20)
                    {
                        int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] = Defines.DEF_RP19;
                    }
                    else
                    {
                        break;
                    }
                }

                // 直前の音の完奏を待つ
                if (_soundTime < Util.GetMilliSeconds())
                {
                    if (IS_HALL())
                    {
                        //ﾎﾞｰﾅｽ終了時通信
                        if (int_s_value[Defines.DEF_INT_IS_BB_RB_END] == 1)
                        {
                            hallData[Defines.DEF_H_APPLI_REQ] = Defines.DEF_HRQ_BNSEND;
                            REQ_MODE(Defines.DEF_RMODE_HTTP);
                            // REG入賞時通信
                        }
                        else if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_RB) != 0)
                        {
                            hallData[Defines.DEF_H_APPLI_REQ] = Defines.DEF_HRQ_BNSIN;
                            REQ_MODE(Defines.DEF_RMODE_HTTP);
                            // BIG入賞時通信
                        }
                        else if ((clOHHB_V23.getWork(Defines.DEF_HITFLAG) & Defines.DEF_HITFLAG_NR_BB) != 0)
                        {
                            hallData[Defines.DEF_H_APPLI_REQ] = Defines.DEF_HRQ_BNSIN;
                            REQ_MODE(Defines.DEF_RMODE_HTTP);
                        }
                        else if (prevHttpTime + (5 * 60) < Util.GetMilliSeconds() / 1000)
                        {
                            hallData[Defines.DEF_H_APPLI_REQ] = Defines.DEF_HRQ_NORMAL;
                            REQ_MODE(Defines.DEF_RMODE_HTTP);
                        }
                        else
                        {
                            REQ_MODE(Defines.DEF_RMODE_WAIT); // 回転待ちへ遷移
                        }
                    }
                    else
                    {
                        REQ_MODE(Defines.DEF_RMODE_WAIT); // 回転待ちへ遷移
                    }
                }
                break;
        }
    }

    /**
	 * クリップが使えるのでisRepaintを使用しない方向で
	 */
    private void drawAll()
    {
        ZZ.setClip(-ZZ.getOffsetX(), -ZZ.getOffsetY(), ZZ.getWidth(), ZZ.getHeight());
        ZZ.setColor(ZZ.getColor(0, 0, 0));
        ZZ.fillRect(-ZZ.getOffsetX(), -ZZ.getOffsetY(), ZZ.getWidth(), ZZ.getHeight());

        // 筐体背景部分
        ZZ.drawImage(Defines.DEF_RES_K1, Defines.DEF_POS_K1_X, Defines.DEF_POS_K1_Y);
        ZZ.drawImage(Defines.DEF_RES_K2, Defines.DEF_POS_K2_X, Defines.DEF_POS_K2_Y);
        ZZ.drawImage(Defines.DEF_RES_K3, Defines.DEF_POS_K3_X, Defines.DEF_POS_K3_Y);
        ZZ.drawImage(Defines.DEF_RES_K4, Defines.DEF_POS_K4_X, Defines.DEF_POS_K4_Y);
        ZZ.drawImage(Defines.DEF_RES_K5, Defines.DEF_POS_K5_X, Defines.DEF_POS_K5_Y);
        ZZ.drawImage(Defines.DEF_RES_K6, Defines.DEF_POS_K6_X, Defines.DEF_POS_K6_Y);
        ZZ.drawImage(Defines.DEF_RES_K7, Defines.DEF_POS_K7_X, Defines.DEF_POS_K7_Y);

        // ランプ
        {
            // drawK1 4thリールの左右のランプ
            int[] x = {
                Defines.DEF_POS_S1_X,
                Defines.DEF_POS_S2_X,
                Defines.DEF_POS_S3_X,
                Defines.DEF_POS_S4_X,
                Defines.DEF_POS_S5_X,
                Defines.DEF_POS_S6_X
            };

            int[] y = {
                Defines.DEF_POS_S1_Y,
                Defines.DEF_POS_S2_Y,
                Defines.DEF_POS_S3_Y,
                Defines.DEF_POS_S4_Y,
                Defines.DEF_POS_S5_Y,
                Defines.DEF_POS_S6_Y
            };

            // ランプの画像がなぜかでかすぎる為、いれとかないと枠外にでてしまう。
            ZZ.setClip(Defines.DEF_POS_K1_X, Defines.DEF_POS_K1_Y, Defines.DEF_POS_K1_W, Defines.DEF_POS_K1_H);

            for (int i = 0; i < 6; i++)
            {
                if (getLampStatus(Defines.DEF_LAMP_S1 + i) == Defines.DEF_LAMP_STATUS_ON)
                {
                    ZZ.drawImage(Defines.DEF_RES_S1_B + i, x[i], y[i]);
                }
            }

            // クリッピング領域の解除
            ZZ.setClip(-ZZ.getOffsetX(), -ZZ.getOffsetY(), ZZ.getWidth(), ZZ.getHeight());
        }

        {
            // drawK3 BETランプ
            int[] x = {
                Defines.DEF_POS_B1_X,
                Defines.DEF_POS_B2_X,
                Defines.DEF_POS_B3_X,
                Defines.DEF_POS_B4_X,
                Defines.DEF_POS_B5_X
            };

            int[] y = {
                Defines.DEF_POS_B1_Y,
                Defines.DEF_POS_B2_Y,
                Defines.DEF_POS_B3_Y,
                Defines.DEF_POS_B4_Y,
                Defines.DEF_POS_B5_Y
            };

            for (int i = 0; i < 5; i++)
            {
                if (getLampStatus(Defines.DEF_LAMP_BET_1 + i) == Defines.DEF_LAMP_STATUS_ON)
                {
                    ZZ.drawImage(Defines.DEF_RES_B1_B + i, x[i], y[i]);
                }
            }
        }

        {
            // drawK4 筐体左のかぎやランプやリプレイランプ
            int[] x = {
                Defines.DEF_POS_C1_X,
                Defines.DEF_POS_C2_X,
                Defines.DEF_POS_C3_X,
                Defines.DEF_POS_C4_X,
                Defines.DEF_POS_C5_X
            };

            int[] y = {
                Defines.DEF_POS_C1_Y,
                Defines.DEF_POS_C2_Y,
                Defines.DEF_POS_C3_Y,
                Defines.DEF_POS_C4_Y,
                Defines.DEF_POS_C5_Y
            };

            for (int i = 0; i < 5; i++)
            {
                if (getLampStatus(Defines.DEF_LAMP_WIN + i) == Defines.DEF_LAMP_STATUS_ON)
                {
                    ZZ.drawImage(Defines.DEF_RES_C1_B + i, x[i], y[i]);
                }
            }
        }

        // 4th
        draw4th();

        // リール
        drawSlot();

        // drawK7関数を分解してみた
        ZZ.drawImage(Defines.DEF_RES_K7, Defines.DEF_POS_K7_X, Defines.DEF_POS_K7_Y);
        if (getLampStatus(Defines.DEF_LAMP_CHANCE) == Defines.DEF_LAMP_STATUS_ON)
        {
            ZZ.drawImage(Defines.DEF_RES_D1_B, Defines.DEF_POS_CHANCE_X, Defines.DEF_POS_CHANCE_Y);
        }

        // ボーナスかうんと描画
        drawBonusCount();

        // クレジット描画
        drawCredit();

        // 払い出し描画
        drawPay();
    }

    /**
	 * 4thリールの描画
	 */
    private static void draw4th()
    {
        if (getLampStatus(Defines.DEF_LAMP_4TH) == Defines.DEF_LAMP_STATUS_ON)
        {
            GameManager.Instance.Set4thReelTexture(true);
        }
        else
        {
            GameManager.Instance.Set4thReelTexture(false);
        }
    }

    /**
	 * スロット回転部の描画。
	 */
    private static void drawSlot()
    {
        // １リールずつ処理
        int[] x = { 25, 92, 159 };

        for (int i = 0; i < 3; i++)
        {
            // 左のリールから描画しています
            // リール部分クリッピング
            ZZ.setClip(25, 114 + Defines.GP_DRAW_OFFSET_Y, 215 - 25, 96);
            // 消灯
            int[] state = { 0, 0, 0, 0, 0 };// 消灯
            if (int_s_value[Defines.DEF_INT_CURRENT_MODE] == Defines.DEF_RMODE_FLASH
                    || (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] == 2 && int_s_value[Defines.DEF_INT_CURRENT_MODE] == Defines.DEF_RMODE_WAIT))
            {
                // 点灯
                // 枠下&下段
                if ((int_s_value[Defines.DEF_INT_FLASH_DATA] & (1 << (i * 3 + 0))) != 0)
                {
                    state[0] = state[1] = 1;
                }
                // 中段
                if ((int_s_value[Defines.DEF_INT_FLASH_DATA] & (1 << (i * 3 + 1))) != 0)
                {
                    state[2] = 1;
                }
                // 上段&枠上
                if ((int_s_value[Defines.DEF_INT_FLASH_DATA] & (1 << (i * 3 + 2))) != 0)
                {
                    state[3] = state[4] = 1;
                }
            }
            else
            {
                state[0] = state[1] = state[2] = state[3] = state[4] = 1;
            }

            // ブラー
            if ((int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] & BIT(i)) == 0)
            {
                // リールスピードが２以下の場合ブラー画像は使わない！
                if (Mobile.getReelSpeed() >= 80)
                {
                    // ブラー
                    state[0] = state[1] = state[2] = state[3] = state[4] = 2;
                }
                ZZ.drawImage(Defines.DEF_RES_BACK_B, x[i], 114 + Defines.GP_DRAW_OFFSET_Y);
            }
            else
            {
                ZZ.drawImage(Defines.DEF_RES_BACK, x[i], 114 + Defines.GP_DRAW_OFFSET_Y);
            }

            // 例:1周21コマのうち15.75コマ回転していたら、per21=15.75<<16
            int per21 = Defines.DEF_N_FRAME
                    * (int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + i] & ANGLE_2PI_MASK);

            // 例:15.75コマ回転していたら15番を取得
            int mid = ((per21 >> 16) % Defines.DEF_N_FRAME); // 中段番号番号。

            // 各y座標に0.75コマ分下に
            int[] y = { 204, 176, 148, 120, 92 };

            // // リールの描画
            for (int j = 0; j < 5; j++)
            {
                // 0:枠下 1:下段 2:中段 3:上段 4:枠上
                int h = 28;
                y[j] += (/* １コマに満たない分 */(per21 & ANGLE_2PI_MASK) * h) >> 16;

                int rlnum = (mid + j - 2 + Defines.DEF_N_FRAME) % Defines.DEF_N_FRAME;// リール位置番号
                int sym = getReelId(REELTB[i][rlnum]);// 絵柄ID
                int id = 0;

                // soy TODO リールランプ処理
                GameManager.Instance.SetReelTexture(rlnum, i, state[j] == 1);

                id = Defines.DEF_RES_R1_01 + (Defines.DEF_RES_R1_02 - Defines.DEF_RES_R1_01) * sym + state[j];
                ZZ.drawImage(id, x[i], y[j] + Defines.GP_DRAW_OFFSET_Y);

                id = Defines.DEF_RES_R2_01 + (Defines.DEF_RES_R2_02 - Defines.DEF_RES_R2_01) * sym + state[j];
                ZZ.drawImage(id, x[i], y[j] + Defines.GP_DRAW_OFFSET_Y);

                id = Defines.DEF_RES_R3_01 + (Defines.DEF_RES_R3_02 - Defines.DEF_RES_R3_01) * sym + state[j];
                ZZ.drawImage(id, x[i], y[j] + Defines.GP_DRAW_OFFSET_Y);
            }
        }

        ZZ.setClip(25, 114 + Defines.GP_DRAW_OFFSET_Y, 215 - 25, 96);
        ZZ.scale3D(100);// スケール弄るよ
        int[] xx = { 25, 92, 159 };

        for (int i = 0; i < 3; i++)
        {
            // 上の左影
            _drawEffect(xx[i], 114, 56, 2, 61, 61, 61, Defines.DEF_INK_SUB);
            _drawEffect(xx[i], 116, 56, 4, 36, 36, 36, Defines.DEF_INK_SUB);
            _drawEffect(xx[i], 120, 56, 3, 18, 18, 18, Defines.DEF_INK_SUB);
            // 下の左影
            _drawEffect(xx[i], 208, 56, 2, 61, 61, 61, Defines.DEF_INK_SUB);
            _drawEffect(xx[i], 204, 56, 4, 36, 36, 36, Defines.DEF_INK_SUB);
            _drawEffect(xx[i], 201, 56, 3, 18, 18, 18, Defines.DEF_INK_SUB);
        }

        ZZ.scale3D(50);// 戻すよ～

        // クリッピング領域の解除
        ZZ.setClip(-ZZ.getOffsetX(), -ZZ.getOffsetY(), ZZ.getWidth(), ZZ.getHeight());
    }

    /**
	 * TODO クレジット描画
	 */
    private static void drawCredit()
    {
        int xx = Defines.DEF_POS_CREDIT_X;
        int val = int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM];

        for (int i = 0; i < Defines.DEF_POS_CREDIT_D; i++)
        {
            if (val > 0)
            {
                ZZ.drawImage(Defines.DEF_RES_SEG_R0 + (val % 10), xx, Defines.DEF_POS_CREDIT_Y);
            }
            else
            {
                if (i == 0)
                {
                    ZZ.drawImage(Defines.DEF_RES_SEG_R0, xx, Defines.DEF_POS_CREDIT_Y);
                }
            }
            val /= 10;
            xx -= Defines.DEF_POS_CREDIT_W;
        }
    }

    /**
	 * TODO 払い出し描画
	 */
    private static void drawPay()
    {
        int xx = Defines.DEF_POS_PAY_X;
        int val = int_s_value[Defines.DEF_INT_WIN_GET_COIN];
        for (int i = 0; i < Defines.DEF_POS_PAY_D; i++)
        {
            if (val > 0)
            {
                ZZ.drawImage(Defines.DEF_RES_SEG_R0 + (val % 10), xx, Defines.DEF_POS_PAY_Y);
            }
            val /= 10;
            xx -= Defines.DEF_POS_PAY_W;
        }
    }

    /**
	 * TODO ボーナスかうんと描画
	 */
    private static void drawBonusCount()
    {
        if (IS_BONUS_JAC())
        {
            // Jac 中
            int i = clOHHB_V23.getWork(Defines.DEF_BIGBCTR);
            i = (i == 0) ? 1 : i;
            // (JACイン中)残り回数(3～1)
            ZZ.drawImage(Defines.DEF_RES_SEG_G0 + i,
                    Defines.DEF_POS_BONUS_X - Defines.DEF_POS_BONUS_W * 2, Defines.DEF_POS_BONUS_Y);
            // -[ﾊｲﾌﾝ]
            ZZ.drawImage(Defines.DEF_RES_SEG_GB, Defines.DEF_POS_BONUS_X - Defines.DEF_POS_BONUS_W,
                    Defines.DEF_POS_BONUS_Y);
            // ボーナスカウント(JACイン中)を表示します(8～1)
            ZZ.drawImage(Defines.DEF_RES_SEG_G0 + clOHHB_V23.getWork(Defines.DEF_JAC_CTR),
                    Defines.DEF_POS_BONUS_X, Defines.DEF_POS_BONUS_Y);
        }
        else if (IS_BONUS_GAME())
        {
            // ＢＢ残り回数を表示
            int val = clOHHB_V23.getWork(Defines.DEF_BBGMCTR);
            int xx = Defines.DEF_POS_BONUS_X;
            for (int i = 0; i < 3; i++)
            {
                if (val > 0)
                {
                    ZZ.drawImage(Defines.DEF_RES_SEG_G0 + (val % 10), xx, Defines.DEF_POS_BONUS_Y);
                }
                val /= 10;
                xx -= Defines.DEF_POS_BONUS_W;
            }
        }
    }

    /// <summary>
    /// ボーナスカウンタ表示取得
    /// </summary>
    /// <returns></returns>
    public static string GetBonusCount()
    {
        if (IS_BONUS_JAC())
        {
            // Jac 中
            int i = clOHHB_V23.getWork(Defines.DEF_BIGBCTR);
            i = (i == 0) ? 1 : i;
            return i + "-" + clOHHB_V23.getWork(Defines.DEF_JAC_CTR);
        }
        else if (IS_BONUS_GAME())
        {
            // ＢＢ残り回数を表示
            return clOHHB_V23.getWork(Defines.DEF_BBGMCTR).ToString();
        }

        return "";
    }

    private static void set4th(int id)
    {
        // 待ちから動作状態へ
        if (int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] == 0 && id > 0)
        {
            int_s_value[Defines.DEF_INT_RLPTNDT] = id - 1;
            int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] = 0;
            int_s_value[Defines.DEF_INT_RLPTNDT_FLAG] = 0;// 0:回転開始可1:回転中2:回転終了
            int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] = 1;// セット完了（動作待ち）
            isCanStop = false;
        }
    }

    static bool isCanStop = false;//センサー通過フラグ

    private static void action4th()
    {
        // 動作状態でなければ飛ばす
        if (int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] != 1)
        {
            return;
        }

        // 一時停止中は何もしない
        if (_4thTime > Util.GetMilliSeconds())
        {
            return;
        }

        // 読込
        int[] data = RLPTNDT[int_s_value[Defines.DEF_INT_RLPTNDT]];

        // 回転タイミングがまだの時
        if (clOHHB_V23.getWork(Defines.DEF_PUSHCTR) > data[int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER]] % 8)
        {
            return;
        }

        // 回転パラ夢
        int dir = data[int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] + 1] / 4;
        int spe = data[int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] + 1] % 4;
        int pos = data[int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] + 2];
        dir = (dir == 0) ? -1 : 1;
        spe = ((ZZ.getThreadSpeed() >= 40) ? 20 : 10) / spe;
        bool snd4th = false;

        if (ZZ.getThreadSpeed() >= 40)
        {
            if (spe == 20)
            {
                snd4th = true;
            }
        }
        else
        {
            if (spe == 10)
            {
                snd4th = true;
            }
        }

        bool isNext = false;

        for (int i = 0; i < spe; i++)
        {
            // 回転停止タイミングか？
            if (data[int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER]] / 8 == 0 || clOHHB_V23.getWork(Defines.DEF_PUSHCTR) == 0)
            {
                //センサーは通過しているか?
                if (isCanStop)
                {
                    //さらに少なくとも赤ドン←大当り分以上は回らないといけない。
                    if (int_s_value[Defines.DEF_INT_RLPTNDT_FLAG] > 100 / spe)
                    {
                        if (int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] == pos)
                        {
                            isNext = true;
                            break;
                        }
                    }
                }
            }

            // 回す
            int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] += dir + 414;
            int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] %= 414;
            //センサーチェック
            if (dir < 0)
            {//正回転のセンサー位置:はずれ～青ドンの間にある
                if (int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] == 270)
                {
                    isCanStop = true;
                }
            }
            else {//逆回転のセンサー位置:大当り～赤ドンの間にある
                if (int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] == 60)
                {
                    isCanStop = true;
                }
            }

        }
        // DfMain.TRACE((int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE]);
        if (isNext)
        {
            isCanStop = false;
            int_s_value[Defines.DEF_INT_TOP_LAMP] = 0;
            if (int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] + 3 < data.Length)
            {
                // 次のデータへ
                int_s_value[Defines.DEF_INT_RLPTNDT_COUNTER] += 3;
                int_s_value[Defines.DEF_INT_RLPTNDT_FLAG] = 0;
                _4thTime = Util.GetMilliSeconds() + 1000;
            }
            else {
                // 演出完了
                int_s_value[Defines.DEF_INT_4TH_ACTION_FLAG] = 0;
            }
            if (!IS_BONUS()
                    && int_s_value[Defines.DEF_INT_CURRENT_MODE] != Defines.DEF_RMODE_BB_FANFARE
                    && int_s_value[Defines.DEF_INT_CURRENT_MODE] != Defines.DEF_RMODE_RB_FANFARE)
            {
                ZZ.stopSound(Defines.DEF_SOUND_MULTI_BGM);
            }
            return;
        }

        lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_ON);

        // 回転
        if (int_s_value[Defines.DEF_INT_RLPTNDT_FLAG] == 0 && !IS_BONUS()
                && int_s_value[Defines.DEF_INT_CURRENT_MODE] != Defines.DEF_RMODE_BB_FANFARE
                && int_s_value[Defines.DEF_INT_CURRENT_MODE] != Defines.DEF_RMODE_RB_FANFARE)
        {
            if (snd4th)
            {
                playBGM(Defines.DEF_SOUND_10, true);
            }
            else {
                playSE(Defines.DEF_SOUND_11);
            }
        }
        int_s_value[Defines.DEF_INT_RLPTNDT_FLAG]++;
        int_s_value[Defines.DEF_INT_TOP_LAMP] = (dir == -1) ? 4 : 5;
    }

    /**
	 * 動く上部ランプ
	 * 演出ＩＤ
	 * @param isRepaint
	 * @see #drawUpperLamp(int)
	 */
    private static void ctrlTopLamp()
    {
        if (IS_BONUS_JAC()
                || int_s_value[Defines.DEF_INT_CURRENT_MODE] == Defines.DEF_RMODE_RB_FANFARE)
        {
            int_s_value[Defines.DEF_INT_TOP_LAMP] = 1;
        }
        else if (IS_BONUS_GAME()
              || int_s_value[Defines.DEF_INT_CURRENT_MODE] == Defines.DEF_RMODE_BB_FANFARE)
        {
            int_s_value[Defines.DEF_INT_TOP_LAMP] = 2;
        }
        else if (IS_REPLAY())
        {
            int_s_value[Defines.DEF_INT_TOP_LAMP] = 3;
        }

        // 0: 点滅スピード(ms)
        if ((int_s_value[Defines.DEF_INT_MODE_COUNTER] % (FLLXX[int_s_value[Defines.DEF_INT_TOP_LAMP]][0]
                / int_s_value[Defines.DEF_INT_LOOP_SPEED] + 1)) == 0)
        {
            // ボーナス上部ランプの点滅スピード調整
            int_s_value[Defines.DEF_INT_SEQUENCE_EFFECT]++;
        }

        // Defines.DEF_INT_SEQUENCE_EFFECTを使いまわしているので、注意
        if (int_s_value[Defines.DEF_INT_SEQUENCE_EFFECT] >= FLLXX[int_s_value[Defines.DEF_INT_TOP_LAMP]].Length - 1)
        {
            int_s_value[Defines.DEF_INT_SEQUENCE_EFFECT] = 1;
        }

        int data = FLLXX[int_s_value[Defines.DEF_INT_TOP_LAMP]][int_s_value[Defines.DEF_INT_SEQUENCE_EFFECT]];
        for (int i = 0; i < 8; i++)
        {
            int action = ((data & (1 << i)) != 0) ? Defines.DEF_LAMP_ACTION_ON : Defines.DEF_LAMP_ACTION_OFF;
            if (i < 5)
            {
                lampSwitch(Defines.DEF_LAMP_TOP_1 + i, action);
            }
            else {
                lampSwitch(Defines.DEF_LAMP_S3 - (i - 5), action);
                lampSwitch(Defines.DEF_LAMP_S4 + (i - 5), action);
            }
        }
    }

    private static void ctrlButtonLamp()
    {
        for (int i = 0; i < 3; i++)
        {
            if (int_s_value[Defines.DEF_INT_REEL_STOP_R0 + i] != ANGLE_UNDEF
                    || (int_s_value[Defines.DEF_INT_KEY_REJECT] > 0))
            {
                // 止められている又はキーリジェクト前
                lampSwitch(Defines.DEF_LAMP_BUTTON_L + i, Defines.DEF_LAMP_ACTION_OFF);
            }
            else {
                // リールが回ってるところだけ着色
                lampSwitch(Defines.DEF_LAMP_BUTTON_L + i, Defines.DEF_LAMP_ACTION_ON);
            }
        }
    }

    /// BETランプのスイッチ
    private static void ctrlBetLamp()
    {
        switch (int_s_value[Defines.DEF_INT_CURRENT_MODE])
        {
            case Defines.DEF_RMODE_BET:
            case Defines.DEF_RMODE_SPIN:
                for (int i = 0; i < 3; i++)
                {
                    if (int_s_value[Defines.DEF_INT_BETTED_COUNT] > i)
                    {
                        lampSwitch(Defines.DEF_LAMP_BET_3 + i, Defines.DEF_LAMP_ACTION_ON);
                        lampSwitch(Defines.DEF_LAMP_BET_3 - i, Defines.DEF_LAMP_ACTION_ON);
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_BET_3 + i, Defines.DEF_LAMP_ACTION_OFF);
                        lampSwitch(Defines.DEF_LAMP_BET_3 - i, Defines.DEF_LAMP_ACTION_OFF);
                    }
                }
                break;
            case Defines.DEF_RMODE_WAIT:
            case Defines.DEF_RMODE_RESULT:
            case Defines.DEF_RMODE_BB_FANFARE:
            case Defines.DEF_RMODE_RB_FANFARE:
                // そろったラインを光らす
                for (int i = 0; i < 5; i++)
                {
                    if (_lampTime < Util.GetMilliSeconds())
                    {
                        lampSwitch(Defines.DEF_LAMP_BET_1 + i, Defines.DEF_LAMP_ACTION_OFF);
                    }
                    else
                    {
                        if ((clOHHB_V23.getWork(Defines.DEF_HITLINE) & (Defines.DEF__00001000B << i)) != 0)
                        {
                            int id = 0;
                            switch (i)
                            {
                                case 0:// センター
                                    id = Defines.DEF_LAMP_BET_3;
                                    break;
                                case 1:// トップ
                                    id = Defines.DEF_LAMP_BET_2;
                                    break;
                                case 2:// ボトム
                                    id = Defines.DEF_LAMP_BET_4;
                                    break;
                                case 3:// クロスダウン
                                    id = Defines.DEF_LAMP_BET_1;
                                    break;
                                case 4:// クロスｱｯﾌﾟ
                                    id = Defines.DEF_LAMP_BET_5;
                                    break;
                            }

                            if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                            {
                                lampSwitch(id, Defines.DEF_LAMP_ACTION_OFF);
                            }
                            else
                            {
                                lampSwitch(id, Defines.DEF_LAMP_ACTION_ON);
                            }
                        }
                    }
                }
                break;
        }
    }

    private static void ctrlLamp()
    {
        if (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] > 0)
        {
            if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
            {
                lampSwitch(Defines.DEF_LAMP_WIN, Defines.DEF_LAMP_ACTION_ON);
                lampSwitch(Defines.DEF_LAMP_BAR, Defines.DEF_LAMP_ACTION_ON);
            }
            else
            {
                lampSwitch(Defines.DEF_LAMP_WIN, Defines.DEF_LAMP_ACTION_OFF);
                lampSwitch(Defines.DEF_LAMP_BAR, Defines.DEF_LAMP_ACTION_OFF);
            }

            if (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] > 1)
            {
                if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                {
                    lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_OFF);
                }
                else
                {
                    lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_ON);
                }
            }
        }
        else
        {
            lampSwitch(Defines.DEF_LAMP_WIN, Defines.DEF_LAMP_ACTION_OFF);
            lampSwitch(Defines.DEF_LAMP_BAR, Defines.DEF_LAMP_ACTION_OFF);
        }

        switch (int_s_value[Defines.DEF_INT_CURRENT_MODE])
        {
            case Defines.DEF_RMODE_RESULT:
                if (IS_REPLAY())
                {
                    lampSwitch(Defines.DEF_LAMP_FRE, Defines.DEF_LAMP_ACTION_ON);
                }
                break;

            case Defines.DEF_RMODE_WAIT:
            case Defines.DEF_RMODE_BET:
                // TODO C#移植 フォールスルーしていたのでC#で動くように変更
                if (int_s_value[Defines.DEF_INT_CURRENT_MODE] == Defines.DEF_RMODE_WAIT)
                {
                    if (int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] == 2)
                    {
                        if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                        {
                            int_s_value[Defines.DEF_INT_FLASH_DATA] = 0x1ff;
                        }
                        else
                        {
                            int_s_value[Defines.DEF_INT_FLASH_DATA] = 0;
                        }
                    }
                }

                // ｽﾀｰﾄランプを点滅させる
                if (int_s_value[Defines.DEF_INT_BETTED_COUNT] > 0)
                {
                    if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                    {
                        lampSwitch(Defines.DEF_LAMP_STA, Defines.DEF_LAMP_ACTION_OFF);
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_STA, Defines.DEF_LAMP_ACTION_ON);
                    }
                }
                else
                {
                    lampSwitch(Defines.DEF_LAMP_STA, Defines.DEF_LAMP_ACTION_OFF);
                }

                // インサートコインランプを点滅させる
                if (!IS_REPLAY()
                        && ((IS_BONUS_JAC() && int_s_value[Defines.DEF_INT_BETTED_COUNT] < 1) || (!IS_BONUS_JAC() && (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] < Defines.DEF_NUM_MAX_CREDIT || int_s_value[Defines.DEF_INT_BETTED_COUNT] < 3))))
                {
                    if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                    {
                        lampSwitch(Defines.DEF_LAMP_INS, Defines.DEF_LAMP_ACTION_ON);
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_INS, Defines.DEF_LAMP_ACTION_OFF);
                    }
                }
                else
                {
                    lampSwitch(Defines.DEF_LAMP_INS, Defines.DEF_LAMP_ACTION_OFF);
                }

                // BETボタンを点滅
                if (int_s_value[Defines.DEF_INT_CREDIT_COIN_NUM] > 0)
                {
                    if ((IS_BONUS_JAC() && int_s_value[Defines.DEF_INT_BETTED_COUNT] < 1) ||
                        (!IS_BONUS_JAC() && int_s_value[Defines.DEF_INT_BETTED_COUNT] < 3))
                    {
                        if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                        {
                            lampSwitch(Defines.DEF_LAMP_MAXBET, Defines.DEF_LAMP_ACTION_OFF);
                        }
                        else
                        {
                            lampSwitch(Defines.DEF_LAMP_MAXBET, Defines.DEF_LAMP_ACTION_ON);
                        }
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_MAXBET, Defines.DEF_LAMP_ACTION_OFF);
                    }
                }
                else
                {
                    lampSwitch(Defines.DEF_LAMP_MAXBET, Defines.DEF_LAMP_ACTION_OFF);
                }

                break;

            case Defines.DEF_RMODE_SPIN:
                lampSwitch(Defines.DEF_LAMP_STA, Defines.DEF_LAMP_ACTION_OFF);
                lampSwitch(Defines.DEF_LAMP_INS, Defines.DEF_LAMP_ACTION_OFF);
                lampSwitch(Defines.DEF_LAMP_MAXBET, Defines.DEF_LAMP_ACTION_OFF);
                lampSwitch(Defines.DEF_LAMP_FRE, Defines.DEF_LAMP_ACTION_OFF);

                // ﾃﾝﾊﾟｲランプの点滅
                if (int_s_value[Defines.DEF_INT_IS_TEMPAI] == 1)
                {
                    if (int_s_value[Defines.DEF_INT_ON_OFF_EFFECT] > 0)
                    {
                        lampSwitch(Defines.DEF_LAMP_CHANCE, Defines.DEF_LAMP_ACTION_OFF);
                    }
                    else
                    {
                        lampSwitch(Defines.DEF_LAMP_CHANCE, Defines.DEF_LAMP_ACTION_ON);
                    }
                }
                else
                {
                    lampSwitch(Defines.DEF_LAMP_CHANCE, Defines.DEF_LAMP_ACTION_OFF);
                }

                break;
        }
    }

    /// <summary>
    /// 指定したリールの停止状態を取得する。
    /// </summary>
    /// <param name="stopNum">第？回胴停止(左=0, 中=1, 右=2)</param>
    /// <returns>既に止まっていたら true</returns>
    public static bool IsReelStopped(int stopNum)
    {
        if ((int_s_value[Defines.DEF_INT_IS_REEL_STOPPED] & BIT(stopNum)) != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 正式対応時に後で外す
    public static Yaku currentYaku;

    /**
	 * リールを止めるべき所に止める。
	 * @param button 第？回胴停止(左=0, 中=1, 右=2)
	 * @return true:既に止まっている
     *         false:回転中
	 */
    private static bool setReelStopAngle(int button, CallbackToController callbacks)
    {
        if (IsReelStopped(button))
        {
            // 止まってる
            return true;
        }

        // 止まる場所
        var result_index = 0;

        // 目押サポート付ボーナスイン
        result_index = ANGLE2INDEX(int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + button]);

        //// BB, RBで分岐
        //if ((clOHHB_V23.getWork(Defines.DEF_GAMEST) & 0x01) != 0)
        //{
        //    // ビタでＲＥＧを止めているので、
        //    // 正式対応時に後で外す
        //    result_index = EyeSniperRB(button);
        //}
        //else if (currentYaku == Yaku.BigBonus)
        //{
        //    // ビタで７７７を止めているので、
        //    // 正式対応時に後で外す
        //    result_index = EyeSniperBB(button);
        //}



        // BB, RBで分岐
        if (clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_RB)
        {
            // ビタでＲＥＧを止めているので、
            // 正式対応時に後で外す
            result_index = EyeSniperRB(button);
        }
        else if (clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_BB)
        {
            // ビタで７７７を止めているので、
            // 正式対応時に後で外す
            result_index = EyeSniperBB(button);
        }

        callbacks.ButtonStop(button, result_index);

        // 停止角度を決める
        int_s_value[Defines.DEF_INT_REEL_STOP_R0 + button] = INDEX2ANGLE(result_index);

        // 停止出目を覚えておく
        int_s_value[Defines.DEF_INT_PREV_GAME] &= ~(0x1F << button * 5);// 対象BITをクリア
        int_s_value[Defines.DEF_INT_PREV_GAME] |= (result_index << (button * 5));// 記憶

        if (Mobile.isJacCut() == true)
        {
            return true;
        }

        return false;
    }

    /**
	 * 各種払い出し音セット
	 */
    private static void playCoinSound()
    {
        int snd_id = Defines.DEF_SOUND_UNDEF;
        _soundTime = Util.GetMilliSeconds() + 0;
        if (int_s_value[Defines.DEF_INT_WIN_COIN_NUM] <= 0)
        {
            if (IS_REPLAY())
            {
                snd_id = Defines.DEF_SOUND_24;
                _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_24;
            }
        }
        else if (int_s_value[Defines.DEF_INT_WIN_COIN_NUM] < 15)
        {
            snd_id = Defines.DEF_SOUND_16;
            _soundTime = Util.GetMilliSeconds()
                    + (120 * int_s_value[Defines.DEF_INT_WIN_COIN_NUM]);
        }
        else
        {
            if (IS_BONUS_JAC())
            {
                snd_id = Defines.DEF_SOUND_18;
                _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_18;
            }
            else
            {
                snd_id = Defines.DEF_SOUND_17;
                _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_17;
            }
        }
        playSE(snd_id);
    }

    /**
	 * グラフ更新(シフト)
	 * ボーナス終了後の次ゲームのリール回転開始のタイミングで呼ぶ.
	 * @param current = 現在の回転数
	 * @param game_kind = BIG/REG/NORMAL
	 */
    private static void shiftDataPanelHistory(int current, int game_kind)
    {
        // Y軸 高さ.
        int idx = current / Defines.DEF_UNIT_GAMES;
        if (idx >= Defines.DEF_INFO_GAMES)
        {
            // == bonus_Data[x].Length
            idx = Defines.DEF_INFO_GAMES - 1;
        }

        // --- set --
        if (game_kind == Defines.DEF_PS_BB_RUN)
        {
            bonus_Data[int_s_value[Defines.DEF_INT_BONUS_DATA_BASE], idx] = Defines.DEF_GAME_BIG;
        }
        else if (game_kind == Defines.DEF_PS_RB_RUN)
        {
            bonus_Data[int_s_value[Defines.DEF_INT_BONUS_DATA_BASE], idx] = Defines.DEF_GAME_REG;
        }

        // -- shift --
        int_s_value[Defines.DEF_INT_BONUS_DATA_BASE]--;
        if (int_s_value[Defines.DEF_INT_BONUS_DATA_BASE] < 0)
        {
            int_s_value[Defines.DEF_INT_BONUS_DATA_BASE] = Defines.DEF_INFO_GAME_HISTORY - 1;
        }

        for (int i = 0; i < Defines.DEF_INFO_GAMES; i++)
        {
            bonus_Data[int_s_value[Defines.DEF_INT_BONUS_DATA_BASE], i] = Defines.DEF_GAME_NONE;
        }
    }

    /**
	 * 進行中のゲーム情報を蓄積していく
	 * 毎ゲーム呼ぶ.
	 * @param current = 現在のボーナス間回転数
	 */
    private static void setCurrentDataPanel(int current)
    {
        // Y軸 高さ.
        int idx = (current - 1) / Defines.DEF_UNIT_GAMES;
        if (idx >= Defines.DEF_INFO_GAMES)
        {
            // == bonus_Data[x].Length
            idx = Defines.DEF_INFO_GAMES - 1;
        }

        // 通常.
        if (bonus_Data[int_s_value[Defines.DEF_INT_BONUS_DATA_BASE], idx] == Defines.DEF_GAME_NONE)
        {
            bonus_Data[int_s_value[Defines.DEF_INT_BONUS_DATA_BASE], idx] = Defines.DEF_GAME_NORMAL;
        }
    }

    /**
	 * ＢＢ作動中ですか？
	 * @return true=ＢＢ作動中
	 */
    public static bool IS_BONUS_GAME()
    {
        return (clOHHB_V23.getWork(Defines.DEF_GAMEST) & 0x80) != 0 && !IS_BONUS_JAC();
    }

    /**
	 * BONUS中ですか？
	 * @return
	 */
    public static bool IS_BONUS()
    {
        return IS_BONUS_GAME() || IS_BONUS_JAC();
    }

    /**
	 * ＲＢ作動中ですか？
	 * @return true=ＲＢ作動中
	 */
    public static bool IS_BONUS_JAC()
    {
        return (clOHHB_V23.getWork(Defines.DEF_GAMEST) & 0x01) != 0;
    }

    public static bool IS_REPLAY()
    {
        return clOHHB_V23.getWork(Defines.DEF_GAMEST) == 0x4
                || (!IS_BONUS() && clOHHB_V23.getWork(Defines.DEF_HITFLAG) == 0x8);
    }

    public static int getReelId(int reelBit)
    {
        switch (reelBit)
        {
            case Defines.DEF_BAR_:
                return Defines.DEF_ID_REEL_BAR_;
            case Defines.DEF_BELL:
                return Defines.DEF_ID_REEL_BELL;
            case Defines.DEF_BSVN:
                return Defines.DEF_ID_REEL_BSVN;
            case Defines.DEF_CHRY:
                return Defines.DEF_ID_REEL_CHRY;
            case Defines.DEF_RPLY:
                return Defines.DEF_ID_REEL_RPLY;
            case Defines.DEF_DON_:
                return Defines.DEF_ID_REEL_RSVN;
            case Defines.DEF_WMLN:
                return Defines.DEF_ID_REEL_WMLN;
        }
        return 0;
    }

    private static int[] isTempai()
    {
        int[] tempai = { Defines.DEF_BB_UNDEF, 0 };
        int yukou = 5;// 有効ライン

        if (int_s_value[Defines.DEF_INT_BET_COUNT] == 1)
        {
            yukou = 1;
        }
        else if (int_s_value[Defines.DEF_INT_BET_COUNT] == 2)
        {
            yukou = 3;
        }

        for (int line = 0; line < yukou; line++)
        {
            int tmp = Defines.DEF_ARAY;
            for (int reel = 0; reel < 3; reel++)
            {
                tmp &= clOHHB_V23.getWork(Defines.DEF_ARAY11 + (line * 3) + reel);
            }

            if ((tmp & Defines.DEF_BSVN) != 0)
            {
                tempai[0] = Defines.DEF_BB_B7;
            }
            else if ((tmp & Defines.DEF_DON_) != 0)
            {
                tempai[0] = Defines.DEF_BB_R7;
                tempai[1]++;
            }
        }
        return tempai;
    }

    /// <summary>
    /// ｎビット左シフト。
    /// </summary>
    /// <param name="n">nﾋﾞｯﾄ左ｼﾌﾄ</param>
    /// <returns>ビット</returns>
    //[Obsolete("削除予定とのコメントあり")]
    public static int BIT(int n)
    {
        return 1 << n;
    }

    /// <summary>
    /// SE を再生(Director.directionデータから選ばず、直接鳴動を指定するときに使う)
    /// </summary>
    /// <param name="id"></param>
    public static void playSE(int id)
    {
        Mobile.playSound(id, false, Defines.DEF_SOUND_MULTI_SE);
    }

    /**
	 * BGM を再生(Director.directionデータから選ばず、直接鳴動を指定するときに使う)
	 * @param id サウンドＩＤ
	 * @param loop ループ再生
	 */
    public static void playBGM(int id, bool loop)
    {
        // 非重畳の場合は第３引数は無視されるのよ
        if (loop == true)
        {
            bgm_no = id;
            bgm_loop = loop;
        }
        else
        {
            bgm_no = -1;
            bgm_loop = false;
        }

        GameManager.PlayBGM(id, loop);
    }

    /** 現在位置 */
    private static int current = 0;

    /** 繰り返し回数 */
    private static int repeat = 0;

    /** 終了判定 */
    private static bool play = false;

    /**
	 * フラッシュを設定、各種初期化
	 */
    public static void setFlash(int idx)
    {
        if (idx > 0)
        {
            flash = FLASHTBL[idx - 1];
            current = -2;
            repeat = 0;
            play = true;
        }
    }

    /**
	 * @return 次のフラッシュ位置
	 */
    public static int getNext()
    {
        if (repeat <= 0)
        {
            current += 2;
            if (current < flash.Length)
            {
                repeat = flash[current];
                if (repeat != Defines.DEF_FEND)
                {
                    repeat = repeat * 3 / 5;
                    if (repeat == 0)
                    {
                        repeat = 1;
                    }
                    return flash[current + 1];
                }
            }
        }
        else
        {
            repeat--;
            return flash[current + 1];
        }

        play = false;
        return 0x1ff;
    }

    /**
	 * 終了判定
	 * @return
	 */
    public static bool isPlay()
    {
        return play;
    }

    public static readonly int[][] RLPTNDT =
    {
	    // RLPTNDT01
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP13, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_RVS, Defines.DEF_RP13, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP08, // 4
		},
		// RLPTNDT02
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP13, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_RVS, Defines.DEF_RP01, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP08, // 4
		},
		// RLPTNDT03
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP08, // 3
		},
		// RLPTNDT04
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP19, // 3
		},
		// RLPTNDT05
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP16, // 2
		},
		// RLPTNDT06
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP01, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP08, // 4
		},
		// RLPTNDT07
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_RVS, Defines.DEF_RP13, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_RVS, Defines.DEF_RP08, // 4
		},
		// RLPTNDT08
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP13, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP19, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_RVS, Defines.DEF_RP08, // 4
		},
		// RLPTNDT09
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP13, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP19, // 3
		},
		// RLPTNDT10
		new int[] { Defines.DEF_R_ST3 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP16, // 2
		},
		// RLPTNDT11
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP16, // 2
		},
		// RLPTNDT12
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP16, // 2
		},
		// RLPTNDT13
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP13, // 1
		},
		// RLPTNDT14
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 1
		},
		// RLPTNDT15
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP16, // 1
		},
		// RLPTNDT16
		new int[] { Defines.DEF_R_ST2 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 1
		},
		// RLPTNDT17
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP13, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 2
		},
		// RLPTNDT18
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 2
		},
		// RLPTNDT19
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP13, // 1
		},
		// RLPTNDT20
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP01, // 1
		},
		// RLPTNDT21
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP16, // 1
		},
		// RLPTNDT22
		new int[] { Defines.DEF_R_ST1 + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 1
		},
		// RLPTNDT23
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP08, // 1
		},
		// RLPTNDT24
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP19, // 1
		},
		// RLPTNDT25
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_T3, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP05, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP06, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP07, // 4
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP19, // 5
		},
		// RLPTNDT26
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_T3, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP04, // 1
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP05, // 2
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP06, // 3
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP07, // 4
				Defines.DEF_R_STE + Defines.DEF_ST_TE, Defines.DEF_R_SLW + Defines.DEF_R_NRL, Defines.DEF_RP08, // 5
		},
		// RLPTNDT27
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP13, // 1
		},
		// RLPTNDT28
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_RVS, Defines.DEF_RP01, // 1
		},
		// RLPTNDT29
		new int[] { Defines.DEF_R_STS + Defines.DEF_ST_TN, Defines.DEF_R_NRS + Defines.DEF_R_NRL, Defines.DEF_RP08, // 1
		},
    };

    private static char[] flash = "\u0000".ToCharArray();

    private static char[][] FLASHTBL = {
            "\u0003\u01F8\u0003\u01C7\u0003\u003F\uFFFF".ToCharArray(),
            "\u0003\u0E00\u0003\u0E03\u0003\u0E24\u0003\u0F80\u0003\u0E48\u0003\u0E10\u0003\u0E00\uFFFF"
                    .ToCharArray(),
            "\u0003\u0E00\u0003\u0E08\u0003\u0E10\u0003\u0E20\u0009\u0E00\u0006\u0E10\u0003\u0E00\u0003\u0E10\u0006\u0E00\uFFFF"
                    .ToCharArray(),
            "\u0003\u0E00\u0003\u0E08\u0003\u0E10\u0003\u0E20\u0009\u0E00\u0003\u0E10\u0003\u0EAA\u0004\u0FEF\u0006\u0E00\uFFFF"
                    .ToCharArray(),
            "\u0008\u0E00\u0003\u0E01\u0003\u0E02\u0003\u0E06\u0003\u0E04\u0003\u0E40\u0003\u0E80\u0003\u0F80\u0003\u0F08\u0003\u0E10\u0003\u0E30\u0003\u0E20\u000F\u0E00\u0003\u0E02\u0003\u0E15\n\u0E00\u0003\u0E80\u0003\u0F50\u000F\u0E00\u0003\u0E10\u0003\u0EAA\u0004\u0F45\n\u0E00\uFFFF"
                    .ToCharArray(),
            "\u0008\u0E00\u0003\u0E08\u0003\u0E10\u0004\u0E20\n\u0000\u0002\u0FFF\u000F\u0000\u0003\u0AA1\u0003\u030A\u0003\u0E51\u0003\u0CA5\u0003\u0650\u0003\u04A8\u0003\u0F0A\u0004\u0855\u0004\u0EA2\u000F\u0E00\u0005\u018E\u0008\u0E00\u0005\u018E\u0008\u0E00\u0005\u018E\n\u0E00\uFFFF"
                    .ToCharArray(),
            "\u0002\u0FFF\u0002\u01FF\u0002\u0FFF\uFFFF".ToCharArray(), };

    public static char[][] FLLXX = {
            "\u0064\u0000\u0000\u0000\u0000\u0000".ToCharArray(),
            "\u0078\u0091\u004E\u0024\u004E\u0000".ToCharArray(),
            "\u0050\u0035\u007B\u00E4\n\u0000".ToCharArray(),
            "\u00C8\u0011\u000E\u0011\u000E\u0000".ToCharArray(),
            "\u0050\u0092\u0049\u0024\u0000\u0000".ToCharArray(),
            "\u0050\u0089\u0052\u0024\u0000\u0000".ToCharArray(), };

    /** 3D Rect用 */
    private static int[] polygon = new int[4 * 3];// xyz*4

    /** 3D Rect用 */
    private static int[] polygon_color = new int[3];// rgb

    /**
	 * 透過矩形描画.
	 */
    public static void _drawEffect(int x0, int y0, int w, int h, int r, int g, int b, int ink)
    {
        const int bias_x = 120;// ZZ.centerX;
        const int bias_y = 120;// ZZ.centerY;

        // x0, y0, z0
        polygon[0] = x0 - bias_x;
        polygon[1] = -y0 - bias_y;
        polygon[2] = 0;

        // x1, y1, z1
        polygon[3] = x0 + w - bias_x;
        polygon[4] = -y0 + 0 - bias_y;
        polygon[5] = 0;

        // x2, y2, z2
        polygon[6] = x0 + w - bias_x;
        polygon[7] = -y0 + -h - bias_y;
        polygon[8] = 0;

        // x3, y3, z3
        polygon[9] = x0 + 0 - bias_x;
        polygon[10] = -y0 + -h - bias_y;
        polygon[11] = 0;

        int x = x0 - bias_x;
        int y = y0 - bias_y;

        // x0, y0, z0
        polygon[0] = x;
        polygon[1] = y;
        polygon[2] = 0;

        // x1, y1, z1
        polygon[3] = x + w;
        polygon[4] = y;
        polygon[5] = 0;

        // x2, y2, z2
        polygon[6] = x + w;
        polygon[7] = y + h;
        polygon[8] = 0;

        // x3, y3, z3
        polygon[9] = x + 0;
        polygon[10] = y + h;
        polygon[11] = 0;

        // color
        polygon_color[0] = r;
        polygon_color[1] = g;
        polygon_color[2] = b;

        if (ink == Defines.DEF_INK_SUB)
        {
            ZZ.drawPolygonRectSub(polygon, polygon_color);
        }
        else if (ink == Defines.DEF_INK_ADD)
        {
            ZZ.drawPolygonRectAdd(polygon, polygon_color);
        }
        else
        {
            ZZ.drawPolygonRect(polygon, polygon_color);
        }

        ZZ.flush3D();
    }
}
