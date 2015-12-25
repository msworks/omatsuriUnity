using System;
using System.Linq;
using UnityEngine;

public partial class mOmatsuri
{
    public static int Str2Str(int str)
    {
        return str;
    }

    public static int Str2Int(string str)
    {
        return (int)long.Parse(str);
    }

    public static ushort Str2Ushort(string str)
    {
        return (ushort)long.Parse(str);
    }

	// 基本がこれ
	enum EYE_TYPE {
		EYE_TYPE_NONE,		// 目押しなし
		EYE_TYPE_BB,		// BB当選時
		EYE_TYPE_RB,		// RB当選時
		EYE_TYPE_BONUS,		// ボーナス時の３連どんちゃんねらい
		EYE_TYPE_SP_NO,		// 達人オートの通常時
		EYE_TYPE_SP_REP,	// 達人オートのリプレイ外し
		EYE_TYPE_TIMEOUT	// 自動停止
	};

    public enum MeoshiType
    {
        AtariKotei,
        HazureKotei,
    }

    // 目押し制御用
    // 引数   stopNum	第？回胴停止(左=0, 中=1, 右=2)
    // 戻り値  true		目押し制御あり
    //        false		目押し制御なし
    public static int EyeSniperBB(int buttonId)
    {
        int result_index;
        ushort pos = 0;

        var p = new[] { 12, 5, 4 };

        pos = (ushort)p[buttonId];

        result_index = clOHHB_V23.mReelStop(buttonId, pos);

        return result_index;
    }

    // 目押し制御用
    // 引数   stopNum	第？回胴停止(左=0, 中=1, 右=2)
    // 戻り値  true		目押し制御あり
    //        false		目押し制御なし
    public static int EyeSniperRB(int buttonId)
    {
        int result_index;
        ushort pos = 0;

        var p = new[] { 2, 1, 17 };

        pos = (ushort)p[buttonId];

        result_index = clOHHB_V23.mReelStop(buttonId, pos);

        return result_index;
    }


    /// <summary>
    /// 目押し制御用
    /// </summary>
    /// <param name="button">第？回胴停止(左=0, 中=1, 右=2)</param>
    /// <returns>true:目押し制御あり false:目押し制御なし</returns>
    public static int EyeSniper(int button)
    {
	    int result_index;
	    int tmp;
        ushort pos = 0;

	    int[][] meoshi = { 
		    new int[]{ 9, 15, 13 }, // BB図柄
		    new int[]{ 13, 3, 3 }, // BB図柄
		    new int[]{ 0, 9, 17 }, // RB図柄
		    new int[]{ 2, 4, 11} // 通常時の達人オート
	    };

	    EYE_TYPE EyeType = EYE_TYPE.EYE_TYPE_NONE;

	    if( Mobile.isMeoshi() )
	    {
            // メニューからの目押しフラグ
		    if( mOmatsuri.int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] > 0
				    && clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_BB
				    && (clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & Defines.DEF__00001110B) == 0)
		    {
                // DfOHHB_V23_DEF.DEF__00001110B　ﾍﾞﾙとｽｲｶとﾘﾌﾟが当選してないかﾁｪｯｸしている
			    // DFMain.APP_TRACE("目押し　ビッグボーナス");
			    EyeType = EYE_TYPE.EYE_TYPE_BB;
		    }
		    else if (mOmatsuri.int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] > 0
					    && clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_RB
					    && (clOHHB_V23.getWork(Defines.DEF_WAVEBIT) & Defines.DEF__00001110B) == 0)
		    {
			    // DFMain.APP_TRACE("目押し　レギュラーボーナス");
			    EyeType = EYE_TYPE.EYE_TYPE_RB;
		    }
	    }
	
	    Defines.APP_TRACE("目押しタイプ:" + EyeType);
	    switch(EyeType)
	    {
	    case EYE_TYPE.EYE_TYPE_BB:
		    // BB
            pos = (ushort)meoshi[mOmatsuri.int_s_value[Defines.DEF_INT_BIG_COUNT] % 2][button];
		    break;
	    case EYE_TYPE.EYE_TYPE_RB:
		    // RB
            pos = (ushort)meoshi[2][button];
		    break;
	    case EYE_TYPE.EYE_TYPE_BONUS:
		    // 三連ドン狙い
		    if( button == 0)
		    {
                pos = (ushort)9;
		    }
		    break;

	    case EYE_TYPE.EYE_TYPE_SP_NO:
	    case EYE_TYPE.EYE_TYPE_SP_REP:
            if (EyeType == EYE_TYPE.EYE_TYPE_SP_NO) {
                // 達人オート通常時
                pos = (ushort)meoshi[3][button];
            }
		    // ボーナス中リプレイ外し
		    if( button == 0)
		    {
                pos = (ushort)1;
		    }
		    break;

	    case EYE_TYPE.EYE_TYPE_TIMEOUT:
		    // 自動停止
			    tmp = 0x1F & (mOmatsuri.int_s_value[Defines.DEF_INT_PREV_GAME] >> (button * 5));
			    tmp = (tmp + Defines.DEF_N_FRAME + 2) % Defines.DEF_N_FRAME;
                pos = (ushort)tmp;
		    break;
	    default:
		    // 目押しサポートなしの通常リール停止テーブル参照。
		    pos = (ushort) ANGLE2INDEX(mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0 + button]);
		    break;
	    }

	    result_index = clOHHB_V23.mReelStop(button, pos);
	    Defines.APP_TRACE("ﾘｰﾙ["+ button+ "] = "+(pos&0xFFFF)+" 停止位置:" + result_index);
        ZZ.reelStopStatus[button] = "ﾘｰﾙ[" + button + "] = " + (pos & 0xFFFF) + " 停止位置:" + result_index;

	    return result_index;
    }

    /// <summary>
    /// ストップさせるリール番号選択
    /// </summary>
    /// <param name="index">ストップさせるリール番号(0-2)</param>
    /// <param name="limit">特定の押し順で押したい場合</param>
    /// <returns><ストップさせるリール番号(0-2)</returns>
    static int getStopReel(int index, bool limit )
    {
	    int ret;
	    int num = 0;
	
	    if( limit == true )
	    {
            // 自動停止の場合は順押しにする
		    num = 0;
	    }

        ret = gp.gpif_oshijun_list[num][index];
	
	    return ret;
    }

    /// <summary>
    /// 強制役変更
    /// </summary>
    static void chgYaku()
    {
        Defines.ForceYakuFlag flag = Defines.ForceYakuFlag.NONE;

	    if( gp.gpif_bonus_n == 1)
	    {
            // BB強制
            flag = Defines.ForceYakuFlag.BIG;
		    gp.gpif_bonus_n = 0;
	    }
	    else if( gp.gpif_bonus_n == 2)
	    {
            // RB強制
            flag = Defines.ForceYakuFlag.REG;
		    gp.gpif_bonus_n = 0;
	    }

        if (GameManager.Instance.SettingZeroMode == true)
        {
            // 設定０のとき、17の倍数ゲーム（17､34､51､68・・・）に
            // 強制フラグでハズレフラグに差し替える
            var draw = GameManager.Instance.setting0Machine.Draw();
            if (draw == DrawSetting0.Hazure)
            {
                clOHHB_V23.mSetForceFlag(Defines.ForceYakuFlag.NONE);
            }
        }

        flag = GameManager.forceYakuValue;

        Debug.Log("FORCE YAKU:" + GameManager.forceYakuValue + 
            "[" + (int)GameManager.forceYakuValue + "]");

        //clOHHB_V23.mSetForceFlag(flag);
        if (flag != Defines.ForceYakuFlag.NONE)
        {
            // 強制役のセット
            clOHHB_V23.mSetForceFlag(flag);
        }
    }

    // 設定変更が必要ならば、変更する
    static void chgWaveNum()
    {
		Mobile.setSetUpValue(gp.gpif_setting);
    }

    /// <summary>
    /// コイン数が変化した時に加算、減算を行なう
    /// </summary>
    /// <param name="num"></param>
    public static void GPW_chgCredit(int num)
    {
	    int i;

        for (i=0;i<Math.Abs(num);i++)
	    {
		    if(num>0)
		    {
                // 換算
			    gp.onCreditUp();
		    }
		    else if(num < 0)
		    {
                // 減算
			    gp.onCreditUse();
		    }
	    }
    }

    // コイン枚数の変化(ボーナス)
    public static void GPW_chgCreditBonus()
    {
    }

    /// <summary>
    /// ボーナスの確率をアップさせる
    /// </summary>
    /// <returns></returns>
    public static int GPW_chgProba()
    {
	    if(gp.gpif_triple_f == true)
	    {
            //トリプルセブンアイテム使用時
		    //(num * gpif_kakuhen_n)
		    Defines.APP_TRACE("確率アップ:" + gp.gpif_kakuhen_n);
		    return gp.gpif_kakuhen_n;
	    }
	    return 1;
    }

    /// <summary>
    /// JACIN時に呼ばれる
    /// </summary>
    public static void JacIn()
    {
	    mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_JAC_GOT] = 0;
	    gp.onBonusJACIN();
    }

    /// <summary>
    /// ボーナスカット関係
    /// </summary>
    /// <returns></returns>
    public static int cutBonus()
    {
        // isJacCut
	    //DFMain.APP_TRACE("ボーナスカットチェック:" + gp.getOptValue(gp.OPT_BONUS_CUT));
	    // num=1 JACゲームのみ
	    // num=2 ボーナス全部
	    int num;
	
	    Defines.APP_TRACE("gp.gpif_bonuscut_f:" + gp.gpif_bonuscut_f);
	    if( gp.gpif_bonuscut_f == true)
	    {
            // ボーナスカットON
		    // 0ボーナスカット
		    // 1 OFF
		    // 2 JACカット
		    num = gp.getOptValue(PublicDefine.OPT_BONUS_CUT);
		
		    Defines.APP_TRACE("ボーナスカットフラグ:" + gp.gpif_bonuscut_f);
		    if ((mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7)
			    || (mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7))
		    {
                // ビッグボーナス中
			    Defines.APP_TRACE("ここまできてる？：" + num);
			
			    if( IS_BONUS_JAC() == true )
			    {
				    Defines.APP_TRACE("ボーナスJAC中:" + num);
				    if( (num == 0) || (num == 2))
				    {
                        // オール指定の時のみカット
					    Defines.APP_TRACE("JACカット");
					    return 2;
				    }
			    }
			    else
			    {
				    Defines.APP_TRACE("test");
				    if( num == 0 )
				    {
                        // オール指定の時のみカット
					    Defines.APP_TRACE("オールカット");
					    return 1;
				    }
			    }
		    }
		    else if (mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_RB_IN)
		    {
                // レギュラーボーナス
			    if( (IS_BONUS_JAC() == true) )
			    {
                    // レギュラーボーナス中
				    Defines.APP_TRACE("レギュラーボーナス中:" + num);
				    if( (num == 0) || (num == 2))
				    {
                        // JAC&ｵｰﾙ指定の時にJACゲームのカット
					    Defines.APP_TRACE("レギュラーカット");
					    return 2;
				    }
			    }
		    }
	    }
	    return 0;
    }

    /// <summary>
    /// ボーナスカット処理
    /// </summary>
    /// <param name="type">0=通常,1=離席や別ユーザーの為の強制ボーナスクリア</param>
    /// <returns></returns>
    static bool cutBonusSystem(int type)
    {
	    // 0 カットなし
	    // 1 ボーナスカット
	    // 2 JACカット
	    int cutType;
	
	    if( type == 1 )
	    {
            // 強制的にカットしたい場合
		    cutType = 1;
	    }
	    else
	    {
            // 通常はカットフラグを見て動作させる
		    cutType = cutBonus();
	    }

	    Defines.APP_TRACE(" ﾎﾞｰﾅｽ:" + mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] + " cutType:" + cutType);
	
	    if( cutType == 1 )
	    {
            // ﾎﾞｰﾅｽｹﾞｰﾑ
		    Defines.APP_TRACE("ボーナスオールカットだよ");
		    // 遊技状態 ｽﾃｰﾀｽをRB or JAC 中へ
		    clOHHB_V23.setWork(Defines.DEF_GMLVSTS, (ushort)1);
		    // 残りJACIN可能回数（0～3）
		    clOHHB_V23.setWork(Defines.DEF_BIGBCTR, (ushort)1);
		    // JACゲーム 遊技可能回数（0～12）
		    clOHHB_V23.setWork(Defines.DEF_JACGAME, (ushort)0);
		    // ヒット役を消す
		    clOHHB_V23.setWork(Defines.DEF_HITFLAG, (ushort)0);
		    return true;
	    }
	    else if( cutType == 2 )
	    {
            // JACｹﾞｰﾑ of RB
		    Defines.APP_TRACE("JACカットだよ");
		    // 遊技状態 ｽﾃｰﾀｽをRB or JAC 中へ
		    clOHHB_V23.setWork(Defines.DEF_GMLVSTS, (ushort)1);
		    // JACゲーム 遊技可能回数（0～12）
		    clOHHB_V23.setWork(Defines.DEF_JACGAME, (ushort)0);
		    // ヒット役を消す
		    clOHHB_V23.setWork(Defines.DEF_HITFLAG, (ushort)0);
		    return true;
	    }
	
	    return false;
    }

	static bool[] eventFlagList = new bool[(int)Defines.EVENT.EVENT_NO_MAX];

	// イベントフラグセット用
	public static void GPW_SET_EVENT(int n)
    {
        eventFlagList[n] = true; 
        eventFlagList[(int)Defines.EVENT.EVENT_WEB] = true;
    }

	// 演出帳のチェック用
	void GPW_eventProcess(int type, int flash)
	{
		int tmp;
		
		switch(type)
		{
		case (int)Defines.EVENT_PROC.EVENT_PROC_CHK_REEL:
			// リールの停止位置によるチェック
			if( (clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_BB) ||
				(clOHHB_V23.getWork(Defines.DEF_HITREQ) == Defines.DEF_HITFLAG_NR_RB))
			{	// ボーナス内部中ならば
				if(flash == (int)Defines.EVENT.EVENT_NO1)
				{	// 3連ドン（1確)
					tmp = ANGLE2INDEX(mOmatsuri.int_s_value[Defines.DEF_INT_REEL_ANGLE_R0]);
					// 角度補正で+1されてしまうので-1する
					if( (tmp-1) == 9)
					{	// 3連ドン（1確)
						Defines.APP_TRACE("演出１：3連ドン（1確)");
						GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO1);
					}
				}
				else if(flash == (int)Defines.EVENT.EVENT_NO2)
				{	// トリプルテンパイの場合
					Defines.APP_TRACE("演出２：トリプルテンパイ（BIG確）");
					GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO2);
				}
				else if(flash == (int)Defines.EVENT.EVENT_NO3)
				{
                    Defines.TRACE("★停止図柄1:" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY21) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY22) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY23) & 0xFFFF)));
                    Defines.TRACE("★停止図柄2:" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY11) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY12) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY13) & 0xFFFF)));
                    Defines.TRACE("★停止図柄3:" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY31) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY32) & 0xFFFF)) + ":" +
                        ZZ.hexShort((short)(clOHHB_V23.getWork(Defines.DEF_ARAY33) & 0xFFFF)));
					
                    if( Defines.CHECK_FLAG(clOHHB_V23.getWork(Defines.DEF_ARAY33), Defines.DEF_BSVN) &&
						Defines.CHECK_FLAG(clOHHB_V23.getWork(Defines.DEF_ARAY13), Defines.DEF_CHRY))
					{
                        // 右リール下段にﾁｪﾘｰ付き青七図柄
						if( Defines.CHECK_FLAG(clOHHB_V23.getWork(Defines.DEF_ARAY21),
                            (Defines.DEF_BSVN | Defines.DEF_DON_ | Defines.DEF_BAR_)) )
						{
                            // 左リール上段にボーナス図柄
							Defines.TRACE("演出３：上段タイプのゲチェナ");
							Defines.APP_TRACE("演出３：上段タイプのゲチェナ");
							GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO3);
						}
						else if( Defines.CHECK_FLAG( clOHHB_V23.getWork(Defines.DEF_ARAY31),
                            (Defines.DEF_BSVN | Defines.DEF_DON_ | Defines.DEF_BAR_) ) )
						{
                            // 左リール下段にボーナス図柄
							Defines.TRACE("演出３：上段タイプのゲチェナ");
							Defines.APP_TRACE("演出３：下段タイプのゲチェナ");
							GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO3);
						}
					}
				}
			}
			break;

		case (int)Defines.EVENT_PROC.EVENT_PROC_CHK_FLASH:
			// 演出によるチェック
			// あまりタイプ
			tmp = (flash % 32);
			// 演出チェック
			if (tmp == 1)
            {
				Defines.APP_TRACE("演出６：鉢巻リールアクション「赤ドン」３回停止");
				//enshutu += "6,";
				GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO6);
			}
			else if (tmp == 6) {
				Defines.APP_TRACE("演出７：鉢巻リールアクション「青ドン」３回停止");
				//enshutu += "7,";
				GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO7);
			}
			else if (tmp == 23 || tmp == 29) {
				Defines.APP_TRACE("演出５：レバーオン鉢巻リール始動からの「大当たり」");
				//enshutu += "5,";
				GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO5);
			}
			
			// 割り算タイプ
			tmp = (int)(flash / 32);
			if (tmp == 7) {
				Defines.APP_TRACE("演出４：真・線香花火");
				//enshutu += "4,";
				GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO4);
			}
			break;

		case (int)Defines.EVENT_PROC.EVENT_PROC_CHK_LANP:
			// 確定ランプフラグ
			if( (mOmatsuri.int_s_value[Defines.DEF_INT_WIN_LAMP] == 0) && (flash != 0) )
			{	// まだ確定ランプが点等していなく、演出番号が点等命令の場合
				Defines.APP_TRACE("演出８：「か～ぎや～」ランプ点灯");
				GPW_SET_EVENT((int)Defines.EVENT.EVENT_NO8);
			}
			break;

		case (int)Defines.EVENT_PROC.EVENT_PROC_WEB:
			// イベント情報の送信
			if( eventFlagList[(int)Defines.EVENT.EVENT_WEB] == true )
			{
				int i;
				String strTmp="";
				// ＠＠＠
				// グローバルの文字列い値を入れる
				for(i = (int)Defines.EVENT.EVENT_NO1; i < (int)Defines.EVENT.EVENT_NO_MAX; i++)
				{
					if( eventFlagList[i] == true)
					{
						if( strTmp != "")
						{
							strTmp = strTmp + ",";
						}
						strTmp = strTmp + i;
						eventFlagList[i] = false;
						
					}
				}
				
				eventFlagList[(int)Defines.EVENT.EVENT_WEB] = false;
				
				Defines.APP_TRACE("演出帳:" + strTmp);
				
				gp.userDirection = strTmp;
				
				// 通信を行なう
                gp.onReaSceneGet();
			}
			break;
		}
	}

    /// <summary>
    /// プレイヤーの変更時
    /// </summary>
    public static void chgPrayer()
    {
	    if( gp.gpif_bonuscyu_f )
	    {	// ボーナス中だった場合
		    Defines.TRACE("★別ユーザーのボーナス");
		    if( cutBonusSystem(1) )
		    {	// ボーナス状態を強制クリア
			    int bonusEndFg;
			
			    Defines.TRACE("★ボーナス中を消す");
			    // ボーナスを終わらせる
			    bonusEndFg = clOHHB_V23.mBonusCounter();
			    if (bonusEndFg != 0)
			    {
				    Defines.TRACE("終了処理");
		            mOmatsuri.int_s_value[Defines.DEF_INT_CURRENT_MODE] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_REQUEST_MODE] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_WIN_COIN_NUM] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_BET_COUNT] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_BETTED_COUNT] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_NUM_KASIDASI] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_WIN_LAMP] = 0;
		            mOmatsuri.int_s_value[Defines.DEF_INT_WIN_LAMP_STATUS] = 0;

                    //System.out.println("★別ユーザーのボーナス");

		            // ヒット役を消す
		            clOHHB_V23.setWork(Defines.DEF_HITFLAG, (ushort)0);

		            // 獲得枚数を消す
		            clOHHB_V23.setWork(Defines.DEF_INT_WIN_GET_COIN, (ushort)0);

				    //ここでランプを消す
				    mOmatsuri.int_s_value[Defines.DEF_INT_TOP_LAMP] = 0;

				    // 上部の左右ランプフラグ更新
				    ctrlTopLamp();

				    // 4THのランプフラグ更新
				    lampSwitch(Defines.DEF_LAMP_4TH, Defines.DEF_LAMP_ACTION_OFF);
				
				    BonusEnd(1);
				    mOmatsuri.int_s_value[Defines.DEF_INT_BB_TOTAL_GOT] = 0;
			    }
		    }
	    }
	    else if( IS_REPLAY() )
	    {
		    Defines.TRACE("★リプレイを消す:");
		    // リプレイを消す
		
		    // 状態を通常に戻す
		    clOHHB_V23.setWork(Defines.DEF_GAMEST, (ushort)0);
		    // ヒット役を消す
		    clOHHB_V23.setWork(Defines.DEF_HITFLAG, (ushort)0);
	
		    // ランプのビットを0にする
		    //mOmatsuri.int_s_value[DfOHHB_V23_DEF.DEF_INT_LAMP_1 + (DfOHHB_V23_DEF.DEF_LAMP_FRE / 32)] &= ~(1 << (DfOHHB_V23_DEF.DEF_LAMP_FRE % 32));
		    lampSwitch(Defines.DEF_LAMP_FRE, Defines.DEF_LAMP_ACTION_OFF);
		
		    // ベットした枚数
		    mOmatsuri.int_s_value[Defines.DEF_INT_BETTED_COUNT] = 0;
		    //mOmatsuri.int_s_value[DfOHHB_V23_DEF.DEF_INT_BETTED_COUNT] = 0;
	    }
    }

    /// <summary>
    /// ボーナス終了処理
    /// </summary>
    /// <param name="type">0=通常,1=離席や別ユーザーの為の強制ボーナスクリア</param>
    static void BonusEnd(int type)
    {
	    // ボーナス終了！！
	    Defines.TRACE("ボーナス終了！！:" + mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT]);
	    // mBonusCounter()内部でclearWork(DfOHHB_V23_DEF.DEF_CLR_AREA_2)を実行！

	    // ＪＡＣ ＆ ＢＢ終了時にここを通る
	    // ＢＢ・ＲＢが終了したことにするためここでフラグを初期化処理する
	    mOmatsuri.int_s_value[Defines.DEF_INT_IS_BB_RB_END] = 1;

        // セルフオート停止フラグを立てる
        GameManager.Instance.StopAutoPlay("ボーナス終了");

        // BGMを止める
        Mobile.stopSound(Defines.DEF_SOUND_MULTI_BGM);

	    if ((mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_B7) ||   
            (mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_BB_R7))
        {
            // ＢＢ終了判定
		    Defines.TRACE("BBボーナス終了:" + mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] + "枚:" + type);
		    if( type == 1)
		    {
                    // ボーナス状態を強制クリア
		    }
		    else
		    {	// 通常はこっち
			    _soundTime = Util.GetMilliSeconds() + Defines.DEF_SOUND_MS_09; // ﾌｧﾝﾌｧｰﾚ完奏時間設定
			    playBGM(Defines.DEF_SOUND_09, false); // BBEND音

			    if( BonusCutFg == true)
			    {	// ボーナスカットオールの場合限定
				    BonusCutFg = false;
				    if( mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] < Defines.BIG_BONUS_AVENUM)
				    {
					    int num;
					    num = (Defines.BIG_BONUS_AVENUM - mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT]);
					    Defines.TRACE("BBカット分を追加:" + Defines.BIG_BONUS_AVENUM + "-" + mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] + "=" + num);
					
					    mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] += num;
					    GPW_chgCredit(num);
				    }
			    }

			    mOmatsuri.int_s_value[Defines.DEF_INT_BB_TOTAL_GOT] += mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT];
		    }
		
		    // ボーナス
		    gp.onBonusEND();
		
		    // 消化中の使用コイン数があるため、－枚は０枚にしておく
		    mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] = Math.Max(0,mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT]);
				
	    }
	    else if (mOmatsuri.int_s_value[Defines.DEF_INT_BB_KIND] == Defines.DEF_RB_IN)
	    {
		    Defines.TRACE("RBボーナス終了:" + mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] + "枚:" + type);
		    if( type == 1)
		    {
                // ボーナス状態を強制クリア
		    }
		    else
		    {	// 通常はこっち
			    Defines.TRACE("RBカット:" + cutBonus());
			    if( BonusCutFg == true)
			    {
                    // カット処理フラグON
				    BonusCutFg = false;
				    if( mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] < Defines.REG_BONUS_AVENUM)
				    {
					    int num;
					    num = (Defines.REG_BONUS_AVENUM - mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT]);
					    Defines.TRACE("RBカット分を追加:" + Defines.REG_BONUS_AVENUM + "-" + mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] + "=" + num);
					    mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] += num;
					    GPW_chgCredit(num);
				    }
			    }
		    }

		    // ボーナス
		    gp.onBonusEND();

		    // 消化中の使用コイン数があるため、－枚は０枚にしておく
		    mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT] = Math.Max(0,mOmatsuri.int_s_value[Defines.DEF_INT_BONUS_GOT]);
	    }
    }
}
