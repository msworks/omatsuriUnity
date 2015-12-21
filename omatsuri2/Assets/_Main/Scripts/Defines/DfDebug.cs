/*
 * 作成日: 2003/08/19
 */


/**
 * デバッグ定義. <br>
 * 全て OFF の状態でしかリリースしてはいけません。 一つのフラグだけをオンにした場合は期待した動作をするでしょう。
 * 複数のフラグをオンにした場合の挙動は不定なので信頼してはいけません。
 * 
 * @author A03605WO
 */
public partial class Defines
{
	public static readonly bool DEF_IS_DEBUG_MISSION_PARAM = false;
	
	public static readonly bool DEF_IS_PRINT_FREEMEMORY = false;
	
	public static readonly bool DEF_IS_DEBUG_AUTO = false;

	/** デバッグ？ */
    public static readonly bool DEF_IS_DEBUG_PARAM = false;
	
    /** デバッグ？ */
    public static readonly bool DEF_IS_DEBUG 	 = false; // TODO trueになってた
    public static readonly bool DEF_IS_DEBUG_POS = false;

    /** Mobile のテスト */
    public static readonly bool DEF_IS_DEBUG_MOBILE = false;

    /** Mobileメニューのテスト */
    public static readonly bool DEF_IS_DEBUG_MENU = false;

    /** Reelsのテスト */
    public static readonly bool DEF_IS_DEBUG_REELS = false;

    /** インフォ画面のテスト */
    public static readonly bool DEF_IS_DEBUG_INFO = false;

    /** process() のテスト */
    public static readonly bool DEF_IS_DEBUG_PROCESS = false;

	/** スロットモードのテスト (カレントモードを確かめたければこれ) */
	public static readonly bool DEF_IS_DEBUG_PRINT_RMODE = false;

    /** 音のチェック */
    public static readonly bool DEF_IS_DEBUG_SOUND = false;

    /** 遊戯状態を表示？ */
    public static readonly bool DEF_IS_PRINT_PLAYSTATE = false; // 表示

	/** 遊戯状態を強制的に */
	public static readonly bool DEF_IS_FORCE_PLAYSTATE = false; // 強制フラグ

	/** 抽選役を表示？ */
	public static readonly bool DEF_IS_PRINT_YAKU = false; // 表示

	/** 遊戯状態を強制的に */
	public static readonly bool DEF_IS_FORCE_YAKU = false; // 強制フラグ

	public static readonly bool DEF_IS_FORCE_BIG = false;

    /** オートプレイモード */
    public static readonly bool DEF_IS_AUTO_PLAY = false;

	/** 設定値のテスト */
	public static readonly bool DEF_IS_PRINT_SETUP_VALUE = false;

	/** 小役カウンタのテスト */
	public static readonly bool DEF_IS_PRINT_COIN_COUNTER = false;

	public static readonly bool DEF_IS_DEBUG_PULL3 = false;	//第3停止引き込み処理の監視
}