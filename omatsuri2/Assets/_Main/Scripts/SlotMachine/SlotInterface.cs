using UnityEngine;

public partial class SlotInterface
{
	int slump_count = 0;
	public int bonus_type;    // 獲得種別(BB=1,RB=2)
	public int bonus_incount; // 回転数(ボーナス当選時のhall.dai_bns_rotを入れる）
	public int bonus_getcoin; // 獲得枚数(777時の15枚含む)

	// 設定系
	public int gpif_setting; // 台設定
	public int gpif_coin;    // 台総コイン(ユーザー不可視※クレジット表示とは別です)
	public bool gpif_bonuscyu_f; // ボーナス中フラグ
	public bool gpif_naibucyu_f; // ボーナス内部中フラグ
	public bool gpif_auto_f;        // オートフラグ

	// アイテム効果
	public bool gpif_bonuscut_f;    // ボーナスカットフラグ
	public bool gpif_tatsujin_f;    // 達人オートフラグ
	public bool gpif_triple_f;      // トリプルスリーカード
	public bool gpif_nonstop_f;	// ノンストップオート
	public short gpif_bonus_n;		//通常0～設定時BB＝1、RB＝2
	public short gpif_kakuhen_n = 1; // 確率アップ(通常1～設定時33)
    
	public short[][] gpif_oshijun_list = // メニューの押し順リスト
	{ 
		new short[]{0,1,2},	//0:左、中、右（順押し）
		new short[]{0,2,1},	//1:左、右、中
		new short[]{1,0,2},	//2:中、左、右
		new short[]{1,2,0},	//3:中、右、左
		new short[]{2,0,1},	//4:右、左、中
		new short[]{2,1,0}		//5:右、中、左（逆押し）
	};
		
	// コイン補充タイミング用
	bool get_coin_f = false;

	// 参照系
	public bool betFlag = false;

    /// <summary>
    /// バージョン更新処理
    /// </summary>
	public void versionUp()
	{
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                break;
            case RuntimePlatform.IPhonePlayer:
                break;
            default:
                Defines.TRACE("versionUp:想定外のプラットフォーム:"+Application.platform.ToString());
                break;
        }
	}
		
	// コールバック
	// クレジットが0で投入が必要な時に呼ばれる
	public void onCreditZero()
	{
		Defines.TRACE("call onCreditZero");
		get_coin_f = true;
	}

	// クレジット加算（コイン＋1時）時に呼ばれる
	public void onCreditUp()
	{
        gpif_coin++;
	}

	// クレジット減算（コイン－1時）時に呼ばれる
	public void onCreditUse()
	{
        gpif_coin--;
	}

	// レアシーンを表示した場合にその演出番号が呼ばれる
	public void onReaSceneGet()
	{
		userDirection = ""; // 安全のため消去
	}

	public bool getBusy()
	{
        return false;
	}
	
	// ボーナス内部当選時に呼ばれる
	public void onBonusNaibu()
	{
        Defines.TRACE("★ボーナス確定演出発生");
	}

	// ボーナス当選BB時に呼ばれる
	public void onBonusBB()
	{
		Defines.TRACE("★ビッグボーナス入賞");
        GameManager.Instance.OnBonusBB();
	}

	// ボーナス当選RB時に呼ばれる
	public void onBonusRB()
	{
		Defines.TRACE("★レギュラーボーナス入賞");
        GameManager.Instance.OnBonusRB();
	}

	// ボーナス中JACIN時に呼ばれる
	public void onBonusJACIN()
	{
	}

	// ボーナス終了時に呼ばれる
	public void onBonusEND()
	{
        // 大当たり間ゲーム数カウントクリア
        GameManager.Instance.OnBonusEnd(bonus_incount);
        bonus_incount = 0;
	}
		
	// レバーON（回転数が＋1※リプレイ含む）時に呼ばれる
	public void onCountUp()
	{
        GameManager.Instance.OnCountUp();
        if (!mOmatsuri.IS_BONUS())
        {
            bonus_incount++;
        }
	}

	// メニュー設定の取得
	public int getOptValue(int index)
	{
		return opt_value[ index ];
	}

    /// <summary>
    /// リソースデータ読み込み
    /// </summary>
    /// <param name="strPath">ファイル名</param>
    /// <returns></returns>
    public static sbyte[] getResourceData(string strPath)
    {
        var loadBytes = SaveData.LoadChipData();
        return loadBytes;
    }
}
