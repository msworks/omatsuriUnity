public class Mobile
{
    public const bool DEF_IS_DOCOMO = true;
    public static int keyTrigger = 0;
    private static int keyPressing = 0;
    private static int keyPressingCount = 0;
    public mOmatsuri mo = new mOmatsuri();
    public static SlotInterface gp = null;

    public void exec()
    {
        if (gp == null)
        {
            gp = new SlotInterface();
        }

        if (mOmatsuri.gp == null)
        {
            mOmatsuri.gp = gp;
        }

        keyTrigger = ZZ.getKeyPressed();
        keyPressing = ZZ.getKeyPressing();

        if (keyPressing == 0)
        {
            keyPressingCount = 0;
        } else {
            keyPressingCount++;
        }

        // モード切り替えチェック
        if (int_m_value[Defines.DEF_INT_MODE_CURRENT] != int_m_value[Defines.DEF_INT_MODE_REQUEST])
        {
            int_m_value[Defines.DEF_INT_MODE_CURRENT] = int_m_value[Defines.DEF_INT_MODE_REQUEST];
            int_m_value[Defines.DEF_INT_COUNTER] = 0;
        }

        // モードごとに処理分岐
        switch (int_m_value[Defines.DEF_INT_MODE_CURRENT])
        {
            case Defines.DEF_MODE_UNDEF:
                if (!loadMenuData())
                {
                    initConfig();
                    saveMenuData(false);//初期はホールPは保存しない
                    if (DEF_IS_DOCOMO)
                    {
                        break;
                    }
                }
                setMode(Defines.DEF_MODE_TITLE);
                break;

            /* タイトル */
            case Defines.DEF_MODE_TITLE:
                ctrlTitle();
                break;

            /* ゲーム中 */
            case Defines.DEF_MODE_RUN:
                ctrlRun();
                break;
        }
    }

    private void ctrlRun()
    {
        if (mo.process(keyTrigger))
        {
            mOmatsuri.getExitReason();
        }
        mo.restartSlot();
        int pos = (mOmatsuri.int_s_value[Defines.DEF_INT_4TH_REEL_ANGLE] % 414) * (2359296 / 414);
        ZZ.dbgDrawAll();
    }

    private void ctrlTitle()
    {
        // 分析モード
        // ゲームを走らす
        setSetUpValue(gp.gpif_setting);
        int_m_value[Defines.DEF_INT_GMODE] = Defines.DEF_GMODE_SIMURATION;
        mo.newSlot();
        setMode(Defines.DEF_MODE_RUN);
    }

    public static readonly int[] int_m_value = new int[Defines.DEF_INT_M_VALUE_MAX];

    public static bool isMeoshi()
    {
        return gp.l_m_bEyeSupport;
    }

    public static void setMenuAvarable(bool flag)
    {
        int_m_value[Defines.DEF_INT_IS_MENU_AVAILABLE] 
            = (flag) ? Defines.DEF_MENU_AVAILABLE
                     : Defines.DEF_MENU_UNAVAILABLE;
    }

    public static bool isJacCut()
    {
        if (mOmatsuri.cutBonus() != 0)
        {
            return true;
        }

        return false;
    }

    /**
     * 設定値を設定する<BR>
     * @return 設定値0~5
     */
    public static void setSetUpValue(int val)
    {
        int_m_value[Defines.DEF_INT_SETUP_VALUE] = val;
        clOHHB_V23.setWork(Defines.DEF_WAVENUM, (ushort)val);
    }

    /**
     * 設定値を取得する<BR>
     * ﾀｲﾄﾙから決定キー押下時に設定されるのでMobileで管理します。<BR>
     * @return 設定値0~5
     */
    public static int getSetUpValue()
    {
        return int_m_value[Defines.DEF_INT_SETUP_VALUE];
    }

    /**
     * ゲームモードを取得する。<BR>
     * ﾀｲﾄﾙ画面で設定する。
     * @return
     */
    public static int getGameMode()
    {
        return int_m_value[Defines.DEF_INT_GMODE];
    }

    /**
     * 静的コンストラクタ
     * 初期化ブロックです、 ロードは既に終わっているはずなので
     * タイトルモードから開始するようにアプリモードを初期化。
     */
    static Mobile()
    {
        int_m_value[Defines.DEF_INT_MODE_REQUEST] = Defines.DEF_MODE_UNDEF;
        int_m_value[Defines.DEF_INT_MODE_CURRENT] = Defines.DEF_MODE_UNDEF;
        // GPでは下詰めで描画する為
        int_m_value[Defines.DEF_INT_BASE_OFFSET_X] = (ZZ.getWidth() - Defines.DEF_POS_WIDTH);
        int_m_value[Defines.DEF_INT_BASE_OFFSET_Y] = (ZZ.getHeight() - Defines.DEF_POS_HEIGHT);
        ZZ.setOrigin(int_m_value[Defines.DEF_INT_BASE_OFFSET_X], int_m_value[Defines.DEF_INT_BASE_OFFSET_Y]);

        int_m_value[Defines.DEF_INT_TITLE_BG_START] = ZZ.getBitRandom(32);

        // 設定初期値
        int_m_value[Defines.DEF_INT_GMODE] = Defines.DEF_GMODE_GAME;
        int_m_value[Defines.DEF_INT_SETUP_VALUE_CURSOL] = 3;// 設定４
        setSetUpValue(3);	// 設定４
        int_m_value[Defines.DEF_INT_SUB_MENU_ITEM] = -1; // 選択メニューアイテムの初期化
        int_m_value[Defines.DEF_INT_IS_SOUND] = 1;// 音鳴るよ
        initConfig();
    }

    private static void initConfig()
    {
        int_m_value[Defines.DEF_INT_VOLUME] = 40;// 音量２
        int_m_value[Defines.DEF_INT_VOLUME_KEEP] = 40;// 音量２
        int_m_value[Defines.DEF_INT_ORDER] = Defines.DEF_SELECT_6_0;// 押し順順押し
        int_m_value[Defines.DEF_INT_KOKUCHI] = Defines.DEF_SELECT_3_OFF;// こくちOff
        int_m_value[Defines.DEF_INT_IS_JACCUT] = Defines.DEF_SELECT_2_OFF;// JACCUTオフ
        int_m_value[Defines.DEF_INT_IS_DATAPANEL] = Defines.DEF_SELECT_2_ON;// データパネルOFF
        int_m_value[Defines.DEF_INT_IS_VIBRATION] = Defines.DEF_SELECT_2_ON;// データパネルON
    }

    // アクセス関数の都合上-2しないとこける
    public static readonly int SAVE_BUFFER = Defines.DEF_SAVE_SIZE - 2;

    /**
     * メニューデータの書き込み
     */
    public static void saveMenuData(bool isHall)
    {
        if (!isHall)
        {
            mOmatsuri.prevHttpTime = 0;
            mOmatsuri.kasidasiMedal = 0;
        }

        sbyte[] buf = new sbyte[SAVE_BUFFER];
        int len;

        len = ZZ.getRecord(ref buf);

        if (len <= 0)
        {
            return;
        }

        // 新規作成
        buf[Defines.DEF_SAVE_ORDER] = (sbyte)int_m_value[Defines.DEF_INT_ORDER];
        buf[Defines.DEF_SAVE_DATAPANEL] = (sbyte)int_m_value[Defines.DEF_INT_IS_DATAPANEL];
        buf[Defines.DEF_SAVE_VOLUME] = (sbyte)int_m_value[Defines.DEF_INT_VOLUME];
        buf[Defines.DEF_SAVE_KOKUCHI] = (sbyte)int_m_value[Defines.DEF_INT_KOKUCHI];
        buf[Defines.DEF_SAVE_JACCUT] = (sbyte)int_m_value[Defines.DEF_INT_IS_JACCUT];
        buf[Defines.DEF_SAVE_VIBRATION] = (sbyte)int_m_value[Defines.DEF_INT_IS_VIBRATION];
        buf[Defines.DEF_SAVE_HTTP_TIME0] = (sbyte)(mOmatsuri.prevHttpTime & 0xff);
        buf[Defines.DEF_SAVE_HTTP_TIME1] = (sbyte)((mOmatsuri.prevHttpTime >> 8) & 0xff);
        buf[Defines.DEF_SAVE_HTTP_TIME2] = (sbyte)((mOmatsuri.prevHttpTime >> 16) & 0xff);
        buf[Defines.DEF_SAVE_HTTP_TIME3] = (sbyte)((mOmatsuri.prevHttpTime >> 24) & 0xff);
        buf[Defines.DEF_SAVE_KASIDASI_0] = (sbyte)(mOmatsuri.kasidasiMedal & 0xff);
        buf[Defines.DEF_SAVE_KASIDASI_1] = (sbyte)((mOmatsuri.kasidasiMedal >> 8) & 0xff);
        buf[Defines.DEF_SAVE_KASIDASI_2] = (sbyte)((mOmatsuri.kasidasiMedal >> 16) & 0xff);
        buf[Defines.DEF_SAVE_KASIDASI_3] = (sbyte)((mOmatsuri.kasidasiMedal >> 24) & 0xff);
        buf[Defines.DEF_SAVE_WRITTEN] = 1;
        ZZ.setRecord(buf);
    }

    /**
     * メニューデータの読込
     * @return
     */
    public static bool loadMenuData()
    {
        var buf = new sbyte[SAVE_BUFFER];
        var len = 0;

        len = ZZ.getRecord(ref buf);

        if (len <= 0)
        {
            return false;
        }

        if (buf[Defines.DEF_SAVE_WRITTEN] == 0)
        {
            return false;
        }

        int_m_value[Defines.DEF_INT_ORDER] = buf[Defines.DEF_SAVE_ORDER];
        int_m_value[Defines.DEF_INT_IS_DATAPANEL] = buf[Defines.DEF_SAVE_DATAPANEL];
        int_m_value[Defines.DEF_INT_VOLUME] = buf[Defines.DEF_SAVE_VOLUME];
        int_m_value[Defines.DEF_INT_KOKUCHI] = buf[Defines.DEF_SAVE_KOKUCHI];
        int_m_value[Defines.DEF_INT_IS_JACCUT] = buf[Defines.DEF_SAVE_JACCUT];
        int_m_value[Defines.DEF_INT_IS_VIBRATION] = buf[Defines.DEF_SAVE_VIBRATION];

        mOmatsuri.prevHttpTime = ((buf[Defines.DEF_SAVE_HTTP_TIME0] & 0xff)
                             | ((buf[Defines.DEF_SAVE_HTTP_TIME1] & 0xff) << 8)
                             | ((buf[Defines.DEF_SAVE_HTTP_TIME2] & 0xff) << 16)
                             | ((buf[Defines.DEF_SAVE_HTTP_TIME3] & 0xff) << 24));

        mOmatsuri.kasidasiMedal = ((buf[Defines.DEF_SAVE_KASIDASI_0] & 0xff)
                | ((buf[Defines.DEF_SAVE_KASIDASI_1] & 0xff) << 8)
                | ((buf[Defines.DEF_SAVE_KASIDASI_2] & 0xff) << 16)
                | ((buf[Defines.DEF_SAVE_KASIDASI_3] & 0xff) << 24)
                );

        return true;
    }

    /**
     * アプリモードアクセッサ
     * @param a カレントモード
     * @return ノーマルモード
     */
    private static int getNormalMode(int a)
    {
        return Defines.DEF_MODE_NORMAL_BITS & a;
    }

    /**
     * メニューアプリモードアクセッサ
     * @param a カレントモード
     * @return メニューモード
     */
    private static int getMenuMode(int a)
    {
        return Defines.DEF_MODE_MENU_BIT | getNormalMode(a);
    }

    /**
     * アプリのイベントモード切替指示
     * @param m 変更要求するアプリモード
     */
    private static void setMode(int m)
    {
        int_m_value[Defines.DEF_INT_MODE_REQUEST] = m;
    }

    /**
     * 強制停止. mobuilder と mobuilderA の差異を吸収する
     * @param mode サウンドモード
     */
    public static void stopSound(int mode)
    {
        if (Defines.DEF_USE_MULTI_SOUND) {
            if (mode == Defines.DEF_SOUND_UNDEF) {
                ZZ.stopSound(Defines.DEF_SOUND_MULTI_BGM);
                ZZ.stopSound(Defines.DEF_SOUND_MULTI_SE);
                mOmatsuri.bgm_no = -1;
                mOmatsuri.bgm_loop = false;
            } else {
                ZZ.stopSound(mode);
                if (Defines.DEF_SOUND_MULTI_BGM == mode) {
                    mOmatsuri.bgm_no = -1;
                    mOmatsuri.bgm_loop = false;
                }
            }
        }
    }

    /**
     * 再生
     * @param id サウンドID
     * @param isRepeat 繰り返し演奏するかどうか
     * @param mode サウンドモード
     */
    public static void playSound(int id, bool isRepeat, int mode)
    {
        if (Defines.DEF_USE_MULTI_SOUND)
        {
            ZZ.playSound(id, isRepeat, mode);
        }
    }

    /**
	 * リールスピードを取得します
	 */
    public static int getReelSpeed()
    {
        return (Defines.GP_DEF_INT_SPEED - 20) * 3 + 100;
    }
}