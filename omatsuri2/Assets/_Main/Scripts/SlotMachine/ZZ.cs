using System;

public class ZZ
{
    /** int 変数 */
    public static readonly int[] ZzIntField = new int[Defines.DEF_Z_INT_MAX];

    /** イメージ */
    static readonly Image[] images = new Image[Defines.DEF_RES_IMAGE_MAX];
    public class Image { }

    /** 音 */
    static readonly MediaSound[] medias = new MediaSound[Defines.DEF_RES_SOUND_MAX];
    class MediaSound { }// TODO C#移植 スタブ

    /** TOBE サウンド処理キュー */
    static readonly int[,] soundQueue = new int[2, Defines.DEF_Z_SOUND_QUE_MAX];

    /** jar test */
    //private static JarInflater[] jif = new JarInflater[Defines.DEF_RES_JAR_MAX];
    /** reschar */
    public static readonly char[][] reschar = new char[Defines.DEF_RES_JAR_MAX][];

    /** 3D Figure_data in resource.d_* */
    static readonly Figure[] figures = new Figure[Defines.DEF_RES_FIGURE_MAX];
    class Figure { }// TODO C#移植 スタブ

    /** Figureに複数Textureを貼る場合のwork */
    static readonly Texture[][] figures_texture = new Texture[Defines.DEF_RES_FIGURE_MAX][];
    class Texture { }// TODO C#移植 スタブ

    /** 3D Action_data in resource.d_* */
    static readonly ActionTable[] actions = new ActionTable[Defines.DEF_RES_ACTIONTABLE_MAX];
    class ActionTable { }// TODO C#移植 スタブ

    /** 3D Texture_data in resource.d_* */
    static readonly Texture[] textures = new Texture[Defines.DEF_RES_TEXTURE_MAX];
    /** 3D Texture_data in resource.d_* */
    static Texture env_texture = null;

    /** ActionTableのFrame*/
    static int[] arrFrame = new int[Defines.DEF_RES_ACTIONTABLE_MAX];
    /** ActionTableNo */
    static int[] arrActNo = new int[Defines.DEF_RES_FIGURE_MAX];

    public static readonly int[] clipRegion = new int[4];

    /** offsetX */
    static int ofX;

    /** offsetY */
    static int ofY;

    /** 1ループ時間(ms) */
    static int threadSpeed;

    /** 接続先URL */
    static string serverURL;

    /** 通信フラグ */
    static bool isUpdate;

    /** サーバーコイン:初期値 -2 */
    static int serverCoin;

    /** 獲得コイン数 */
    static int winCoin;

    /** 送信パラメータ */
    static int[] sendParam;

    /** ブラウザ連携 */
    static bool isWebTo;

    /** ブラウザリンク先ＵＲＬ */
    private static string webURL = ""; // TOBE ＷＥＢ　ＴＯ変更

    /** バージョンアップ */
    private static bool isWebToVerUp; // TOBE 旧バージョンアプリ 

    /** 広告文 */
    public static string http_content = "";

    private static int authResult = Defines.DEF_AUTHMEMBER_NON;//DEF_AUTH_ERROR;

    /** アプリバージョンを取得 */
    private static int getAppVersion()
    {
        int ret = 0;
        string str = Defines.DEF_APPLI_VER;
        string strTmp = "";
        char cTmp;
        try
        {
            for (int i = 0; i < str.Length; i++)
            {
                cTmp = str[i];// TODO C#移植 str.charAt(i) -> str[i]に変更
                if (cTmp == '.')
                {
                    continue;
                }
                strTmp = strTmp + cTmp;
            }
        }
        catch (Exception e)
        { // TODO C#移植 Throwable -> Exceptionに変更
            Defines.TRACE("ダメ = " + e);
        }

        ret = int.Parse(strTmp);// TODO C#移植 Integer.parseInt -> int.Parseに変更

        Defines.TRACE("アプリバージョン:(10進)" + ret);
        return ret;
    }

    /** ランダム */
    public static Random RANDOM = new Random();

    /**
	 * ランダム取得 上位ビットほどランダム具合がいいので２の乗数の時は #getBitRandom(int) を使うこと！
	 * @see #getBitRandom(int)
	 * @param n	値の範囲
	 * @return (RANDOM.nextInt() >>> 1) % n;
	 */
    public static int getRandom(int n)
    {
        return getBitRandom(31) % n;
    }

    /**
	 * ランダム値の取得にはこれを使う
	 * @param n	値のビット数のランダム 1-32
	 * @return 32 を渡した場合負値も含まれるので注意
	 * @see Random#nextInt()
	 */
    public static int getBitRandom(int n)
    {
        return RANDOM.Next() >> (32 - n);
    }

    /**
	 * メインスレッドの間隔取得
	 * @return ミリ秒
	 */
    public static int getThreadSpeed()
    {
        return threadSpeed;
    }

    /**
	 * メインスレッドの間隔取得
	 *
	 * @return ミリ秒
	 */
    public static void setThreadSpeed(int n)
    {
        threadSpeed = n;
    }

    /**
	 * キーを取得する、渡したらクリアする
	 * @return キービット
	 */
    public static int getKeyPressed()
    {
        int returnKey = ZzIntField[Defines.DEF_Z_INT_KEYPRESS];
        ZzIntField[Defines.DEF_Z_INT_KEYPRESS] = 0;
        return returnKey;
    }

    /**
	 * 押されているキーを取得する
	 * @return キービット
	 */
    public static int getKeyPressing()
    {
        return ZzIntField[Defines.DEF_Z_INT_KEYPRESSING];
    }

    public static int getColor(int r, int g, int b)
    {
        return 0;
    }

    public static void setColor(int col) { }

    public static int getWidth()
    {
        return 0;
    }

    public static int getHeight()
    {
        return 0;
    }

    public static int getOffsetX()
    {
        return ofX;
    }

    public static int getOffsetY()
    {
        return ofY;
    }

    public static void setOrigin(int x, int y)
    {
        ofX = x;
        ofY = y;
    }


    public static void setClip(int x, int y, int w, int h) { }

    /**
	 * drawImage
	 * @param id イメージID
	 * @param x X座標
	 * @param y Y座標
	 */
    public static void drawImage(int id, int x, int y)
    {
        if (Defines.DEF_IS_DEBUG)
        {
            if (id >= images.Length)
            {
                Defines.TRACE("だめお" + id);
                return;
            }
            if (images[id] == null)
            {
                Defines.TRACE("images[" + id + "]:" + images[id]);
            }
        }
    }

    public static void fillRect(int x, int y, int w, int h) { }

    /**
	 * @param mode サウンドモード
	 */
    public static void stopSound(int mode)
    {
        switch (mode)
        {
            case Defines.DEF_SOUND_MULTI_BGM:
                GameManager.StopBGM();
                break;
            case Defines.DEF_SOUND_MULTI_SE:
                GameManager.StopSE();
                break;
        }
    }

    /**
	 * サウンド設定 利便性の為に作ってあるが，直接キューに入れて構わない。
	 * @param id 定義されていない負値を入れると落ちる
	 * @param isRepeat この曲をリピートするかどうか
	 * @param mode サウンドモード
	 */
    public static void playSound(int id, bool isRepeat, int mode)
    {
        if (id < 0 || Defines.DEF_RES_SOUND_MAX <= id)
        {
            return;
        }

        GameManager.PlaySE(id);
    }

    public static void setVolume(int vol, int mode)
    {
    }

    /**
	 * 本体からデータを読み出す.
	 * @param return_buff データを読み込む配列.
	 * @return 読み込んだサイズ.
	 */
    public static int getRecord(ref sbyte[] return_buff)
    {
        if (Defines.DEF_IS_DEBUG)
        {
            var cursor = 2; // TOBE 先頭は"読込済マーク"だったりするので要注意.
            Defines.TRACE("cursor: " + cursor);
        }

        var read_size = 0;

        try
        {
            SaveData.Load(ref return_buff);
            read_size = return_buff.Length;
        }
        catch (Exception e)
        {
            if (Defines.DEF_IS_DEBUG)
                Console.WriteLine(e.StackTrace);
            read_size = -1;
        }

        Defines.TRACE("getRecord:" + read_size);
        return read_size;
    }

    /**
	 * 本体にデータを書き込む.
	 * @param data 保存するデータ.
	 * @return 書き込んだサイズ.
	 */
    public static int setRecord(sbyte[] data)
    {
        int write_size = 0;

        if (data == null)
        {
            return -1;
        }

        if (Defines.DEF_IS_DEBUG)
        {
            int cursor = 2; // TOBE 先頭は"読込済マーク"だったりするので要注意.
            Defines.TRACE("cursor: " + cursor);
        }

        try
        {
            SaveData.Save(data);
            write_size = data.Length;
        }
        catch (Exception e)
        {
            if (Defines.DEF_IS_DEBUG)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
            }
            write_size = -1;
        }

        Defines.TRACE("setRecord:" + write_size);
        return write_size;
    }


    public static void setLight(int x, int y, int z, int d, int a) { }
    public static void flush3D() { }
    public static void activateEnvMap() { }
    public static void rotateX(int d) { }
    public static void rotateY(int d) { }
    public static void rotateZ(int d) { }

    public static void rotateV(int d, int x, int y, int z) { }

    public static void scale3D(int percent) { }

    public static void setViewTrans(
        int px, int py, int pz,
        int lx, int ly, int lz,
        int ux, int uy, int uz
    )
    { }

    public static void drawPolygonRect(int[] poly, int[] col) { }
    public static void drawPolygonRectAdd(int[] poly, int[] col) { }
    public static void drawPolygonRectSub(int[] poly, int[] col) { }
    public static void executeCommandListTextures(int[] cmd_list) { }

    public static void dbgYakuDraw()
    {
        int i;
        int[] yakuID = {
            0x00,
            Defines.LOT_YAKU_CHRY,
            Defines.LOT_YAKU_BELL,
            Defines.LOT_YAKU_WMLN,
            Defines.LOT_YAKU_REP,
            Defines.LOT_YAKU_RB,
            Defines.LOT_YAKU_BB
        };
        int yaku;
        yaku = clOHHB_V23.getWork(Defines.DEF_WAVEBIT);

        for (i = 0; i < 7; i++)
        {
            if (yakuID[i] == yaku)
            {
                innerHitPattern = "内部当選役:" + getYakuTxt(i + 1) + System.Environment.NewLine;
                break;
            }
        }
    }

    /// <summary>
    /// 内部当選役
    /// </summary>
    public static string innerHitPattern;

    /// <summary>
    /// リール停止位置
    /// </summary>
    public static string[] reelStopStatus = new string[3];

    public static String getYakuTxt(int index)
    {
        String[] str = {
            "ﾗﾝﾀﾞﾑ (NO)|(BB)|(JAC)",
            "(ﾊｽﾞﾚ)|(ﾊｽﾞﾚ)|(ﾊｽﾞﾚ)",
            "(ﾁｪﾘｰ)|(ﾄﾞﾝﾍﾞﾙ15)|(無効)",
            "(ﾍﾞﾙ)|(ﾍﾞﾙ)|(無効)",
            "(ｽｲｶ)|(ｽｲｶorﾁｪ)|(無効)",
            "(ﾘﾌﾟﾚｲ)|(JACIN)|(ﾘﾌﾟﾘﾌﾟ15)",
            "(ﾚｷﾞｭﾗ)|(無効)|(無効)",
            "(ﾋﾞｯｸﾞ)|(無効)|(無効)"
        };

        return str[index];
    }

    // デバッグ描画用
    public static void dbgDrawAll()
    {
        dbgYakuDraw();
    }

    private static string @hex2str(int d, int x)
    {
        string s;
        int l;

        s = "00000000" + Convert.ToString(d, 16);
        l = s.Length;
        return s.Substring(l - x, x);
    }

    public static string hexShort(short d)
    {
        int dd = (int)d;
        return @hex2str(dd & 0x0000ffff, 4);
    }
}


