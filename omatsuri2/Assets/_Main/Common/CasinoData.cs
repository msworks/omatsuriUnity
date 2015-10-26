using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CasinoData : MonoBehaviour {
	//====================================================================================================
	//Field
	//====================================================================================================
    private string[,] sevenSegSpriteName = new string[,]{
        {"7segR0", "7segR1", "7segR2", "7segR3", "7segR4", "7segR5", "7segR6", "7segR7", "7segR8", "7segR9", "7segNone"},
        {"7segO0", "7segO1", "7segO2", "7segO3", "7segO4", "7segO5", "7segO6", "7segO7", "7segO8", "7segO9", "7segNone"},
        {"7segG0", "7segG1", "7segG2", "7segG3", "7segG4", "7segG5", "7segG6", "7segG7", "7segG8", "7segG9", "7segNone"}};
    [SerializeField] private UISprite[] gameCounterSprites = null;
    private int gameCount = 0;
    private enum AVG_STATE
    {
        BB,
        RB,
        ATART
    }
    private AVG_STATE avgState = AVG_STATE.ATART;
    [SerializeField] private UISprite onePerLabel = null;
    private string[] onePerSpriteName = new string[]{"1perR", "1perO", "1perG"};
    [SerializeField] private UISprite[] avgSprites = null;
    private int avg = 0;
    [SerializeField] private UISprite exchangeMark = null;
    public enum EXCHANGE
    {
        yen,
        gen,
        euro,
        doller
    }
    public EXCHANGE exchange;
    [SerializeField] private UISprite[] exchangeSprites = null;
    [SerializeField] private UISprite exchangeDotSprites = null;
    private float exchangeNum = 0;
    [SerializeField] private UISprite[] exchangeRateSprites = null;
    [SerializeField] private UISprite exchangeRateDotSprites = null;
    private float exchangeRateNum = 0;
    [SerializeField] private UISprite[] bbSprites = null;
    private int bbNum = 0;
    [SerializeField] private UISprite[] rbSprites = null;
    private int rbNum = 0;
    [SerializeField] private UISprite[] atSprites = null;
    private int atNum = 0;
    [SerializeField] private UISprite[] pre1BbSprites = null;
    private int pre1BbNum = 0;
    [SerializeField] private UISprite[] pre2BbSprites = null;
    private int pre2BbNum = 0;
    [SerializeField] private UISprite[] pre1RbSprites = null;
    private int pre1RbNum = 0;
    [SerializeField] private UISprite[] pre2RbSprites = null;
    private int pre2RbNum = 0;
    [SerializeField] private UISprite[] pre1AtSprites = null;
    private int pre1AtNum = 0;
    [SerializeField] private UISprite[] pre2AtSprites = null;
    private int pre2AtNum = 0;
    [SerializeField] private UISprite[] historySprites = null;
    public List<int> history = new List<int>();
    private string[] historySpriteName = new string[]{"level0", "level1", "level2", "level3", "level4", "level5", "level6", "level7", "level8", "level9"};
    
    //====================================================================================================
    //Property
    //====================================================================================================
    /// UI-ゲームカウンターを操作する.4桁を超える数字は9999に置換して表示.
    public int GameCount { get { return this.gameCount; } set { this.gameCount = (value > 9999) ? 9999 : value; this.UpdateGameCounter(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-AVGを操作する.4桁を超える数字は9999に置換して表示.
    public int AVG { get { return this.avg; } set { this.avg = (value > 9999) ? 9999 : value; this.UpdateAVG(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-BBを操作する.2桁を超える数字は99に置換して表示.
    public int BB { get { return this.bbNum; } set { this.bbNum = (value > 99) ? 99 : value; this.UpdateBB(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-RBを操作する.2桁を超える数字は99に置換して表示.
    public int RB { get { return this.rbNum; } set { this.rbNum = (value > 99) ? 99 : value; this.UpdateRB(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-RBを操作する.2桁を超える数字は99に置換して表示.
    public int AT { get { return this.atNum; } set { this.atNum = (value > 99) ? 99 : value; this.UpdateAT(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-Exchangeを操作する.5桁を超える数字は99999に置換し,小数点は第1位のみを表示.
    public float Exchange { get { return this.exchangeNum; } set { this.exchangeNum = (value > 99999.9f) ? 99999.9f : value; this.UpdateExchange(); } }
    
    //----------------------------------------------------------------------------------------------------
    /// UI-Exchangeを操作する.2桁を超える数字は99に置換し,小数点は第1位のみを表示.
    public float ExchangeRate { get { return this.exchangeRateNum; } set { this.exchangeRateNum = (value > 99.9f) ? 99.9f : value; this.UpdateExchangeRate(); } }

    private static CasinoData _instance;
    public static CasinoData Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
    }

    //====================================================================================================
    //Method
    //====================================================================================================
    void Start()
    {
        this.UpdateGameCounter();
        this.Update1Per();
        this.ChangeExchangeMark(EXCHANGE.doller);
    }

    //----------------------------------------------------------------------------------------------------
	void Update()
	{
        /*
        GameCount += 1;
        AVG += 1;
        Exchange += 0.1f;
        ExchangeRate += 0.1f;
        PushDataButton();
        PushDispButton();
        BB += 1;
        RB += 1;
        AT += 1;
        UpdatePastBB(BB, BB);
        UpdatePastRB(BB, BB);
        UpdatePastAT(BB, BB);
        AddHistory(GameCount % 10);
        */
    }

    //----------------------------------------------------------------------------------------------------
    private void UpdateGameCounter()
    {
        string count = string.Empty;
        int digit = this.gameCount.ToString().Length;
        if (digit < 4)
        {
            for(int i = 0; i < (4 - digit); ++i){
                count += "0";
            }
        }
        count += this.gameCount.ToString();
        for (int i = 0; i < 4; ++i)
        {
            if(4 - digit > i)
                this.gameCounterSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
            else
                this.gameCounterSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
        }
    }

    //----------------------------------------------------------------------------------------------------
    private void Update1Per()
    {
        this.onePerLabel.spriteName = this.onePerSpriteName [(int)this.avgState];
    }
    
    //----------------------------------------------------------------------------------------------------
    /// Dataボタン押下時の処理.
    public void PushDataButton()
    {
        if ((int)this.avgState == 2)
            this.avgState = 0;
        else
            ++this.avgState;

        this.Update1Per();
        this.UpdateAVG();
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateAVG()
    {
        string count = string.Empty;
        int digit = this.avg.ToString().Length;
        if (digit < 4)
        {
            for(int i = 0; i < (4 - digit); ++i){
                count += "0";
            }
        }
        count += this.avg.ToString();
        for (int i = 0; i < 4; ++i)
        {
            if(4 - digit > i)
                this.avgSprites[i].spriteName = this.sevenSegSpriteName[(int)this.avgState, 10];
            else
                this.avgSprites[i].spriteName = this.sevenSegSpriteName[(int)this.avgState, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateBB()
    {
        string count = string.Empty;
        int digit = this.bbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.bbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.bbSprites[i].spriteName = this.sevenSegSpriteName[0, 10];
            else
                this.bbSprites[i].spriteName = this.sevenSegSpriteName[0, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateRB()
    {
        string count = string.Empty;
        int digit = this.rbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.rbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.rbSprites[i].spriteName = this.sevenSegSpriteName[1, 10];
            else
                this.rbSprites[i].spriteName = this.sevenSegSpriteName[1, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateAT()
    {
        string count = string.Empty;
        int digit = this.atNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.atNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.atSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
            else
                this.atSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    /// <para>ボーナス状態過去（BB）を表示</para>
    /// <para>【第1引数】前日データ</para>
    /// <para>【第2引数】前々日データ</para>
    public void UpdatePastBB(int ago1, int ago2)
    {
        this.pre1BbNum = ago1;
        this.pre2BbNum = ago2;
        string count = string.Empty;
        int digit = this.pre1BbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre1BbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre1BbSprites[i].spriteName = this.sevenSegSpriteName[0, 10];
            else
                this.pre1BbSprites[i].spriteName = this.sevenSegSpriteName[0, int.Parse(count[i].ToString())];
        }
        count = string.Empty;
        digit = this.pre2BbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre2BbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre2BbSprites[i].spriteName = this.sevenSegSpriteName[0, 10];
            else
                this.pre2BbSprites[i].spriteName = this.sevenSegSpriteName[0, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    /// <para>ボーナス状態過去（RB）を表示</para>
    /// <para>【第1引数】前日データ</para>
    /// <para>【第2引数】前々日データ</para>
    public void UpdatePastRB(int ago1, int ago2)
    {
        this.pre1RbNum = ago1;
        this.pre2RbNum = ago2;
        string count = string.Empty;
        int digit = this.pre1RbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre1RbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre1RbSprites[i].spriteName = this.sevenSegSpriteName[1, 10];
            else
                this.pre1RbSprites[i].spriteName = this.sevenSegSpriteName[1, int.Parse(count[i].ToString())];
        }
        count = string.Empty;
        digit = this.pre2RbNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre2RbNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre2RbSprites[i].spriteName = this.sevenSegSpriteName[1, 10];
            else
                this.pre2RbSprites[i].spriteName = this.sevenSegSpriteName[1, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    /// <para>ボーナス状態過去（AT/ART）を表示</para>
    /// <para>【第1引数】前日データ</para>
    /// <para>【第2引数】前々日データ</para>
    public void UpdatePastAT(int ago1, int ago2)
    {
        this.pre1AtNum = ago1;
        this.pre2AtNum = ago2;
        string count = string.Empty;
        int digit = this.pre1AtNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre1AtNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre1AtSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
            else
                this.pre1AtSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
        }
        count = string.Empty;
        digit = this.pre2AtNum.ToString().Length;
        if (digit < 2)
        {
            for(int i = 0; i < (2 - digit); ++i){
                count += "0";
            }
        }
        count += this.pre2AtNum.ToString();
        for (int i = 0; i < 2; ++i)
        {
            if(2 - digit > i)
                this.pre2AtSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
            else
                this.pre2AtSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    /// Exchangeのマークを変更.
    public void ChangeExchangeMark(EXCHANGE exchange)
    {
        this.exchangeMark.spriteName = exchange.ToString();
    }
    
    //----------------------------------------------------------------------------------------------------
    /// Dispボタン押下時の処理.
    public void PushDispButton()
    {
        if ((int)this.exchange == 3)
            this.exchange = 0;
        else
            ++this.exchange;

        this.exchangeMark.spriteName = this.exchange.ToString();
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateExchange()
    {
        if (this.exchangeNum == 0)
        {
            this.exchangeSprites [0].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeSprites [1].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeSprites [2].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeSprites [3].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeSprites [4].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeSprites[5].spriteName = this.sevenSegSpriteName[2, 10];
            this.exchangeSprites[6].spriteName = this.sevenSegSpriteName[2, 10];
            this.exchangeDotSprites.color = new Color(1, 1, 1, 0.05f);
        }
        else
        {
            this.exchangeDotSprites.color = Color.white;
            if(this.exchangeNum < 1){
                this.exchangeSprites [0].spriteName = this.sevenSegSpriteName [2, 10];
                this.exchangeSprites [1].spriteName = this.sevenSegSpriteName [2, 10];
                this.exchangeSprites [2].spriteName = this.sevenSegSpriteName [2, 10];
                this.exchangeSprites [3].spriteName = this.sevenSegSpriteName [2, 10];
                this.exchangeSprites [4].spriteName = this.sevenSegSpriteName [2, 0];
                this.exchangeSprites[5].spriteName = this.sevenSegSpriteName[2, int.Parse(this.exchangeNum.ToString()[2].ToString())];
                this.exchangeSprites[6].spriteName = this.sevenSegSpriteName[2, int.Parse(this.exchangeNum.ToString()[3].ToString())];
            }
            else
            {
                string count = string.Empty;
                bool dot = false;
                foreach(char c in this.exchangeNum.ToString()){
                    if(c.ToString() == ".")
                        dot = true;
                }
                int digit = ((int)this.exchangeNum).ToString().Length;
                if (digit < 5)
                {
                    for(int i = 0; i < (5 - digit); ++i){
                        count += "0";
                    }
                }
                count += this.exchangeNum.ToString();
                if(dot){
                    for (int i = 0; i < 8; ++i)
                    {
                        if(i < 5){
                            if(5 - digit > i)
                                this.exchangeSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
                            else
                                this.exchangeSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }else if(i == 6){
                            this.exchangeSprites[i - 1].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }
                        else if (i == 7)
                        {
                            this.exchangeSprites[i - 1].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }
                    }
                }else{
                    for (int i = 0; i < 5; ++i)
                    {
                        if(5 - digit > i)
                            this.exchangeSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
                        else{
                            this.exchangeSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }
                    }
                    this.exchangeSprites[5].spriteName = this.sevenSegSpriteName[2, 0];
                    this.exchangeSprites[6].spriteName = this.sevenSegSpriteName[2, 0];
                }
            }
        }
    }
    
    //----------------------------------------------------------------------------------------------------
    private void UpdateExchangeRate()
    {
        if (this.exchangeRateNum == 0)
        {
            this.exchangeRateSprites [0].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeRateSprites [1].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeRateSprites [2].spriteName = this.sevenSegSpriteName [2, 10];
            this.exchangeRateDotSprites.color = new Color(1, 1, 1, 0.05f);
        }
        else
        {
            this.exchangeRateDotSprites.color = Color.white;
            if(this.exchangeRateNum < 1){
                this.exchangeRateSprites [0].spriteName = this.sevenSegSpriteName [2, 10];
                this.exchangeRateSprites [1].spriteName = this.sevenSegSpriteName [2, 0];
                this.exchangeRateSprites [2].spriteName = this.sevenSegSpriteName [2, int.Parse(this.exchangeRateNum.ToString()[2].ToString())];
            }else{
                string count = string.Empty;
                bool dot = false;
                foreach(char c in this.exchangeRateNum.ToString()){
                    if(c.ToString() == ".")
                        dot = true;
                }
                int digit = ((int)this.exchangeRateNum).ToString().Length;
                if (digit < 2)
                {
                    for(int i = 0; i < (2 - digit); ++i){
                        count += "0";
                    }
                }
                count += this.exchangeRateNum.ToString();
                if(dot){
                    for (int i = 0; i < 4; ++i)
                    {
                        if(i < 2){
                            if(2 - digit > i)
                                this.exchangeRateSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
                            else
                                this.exchangeRateSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }else if(i == 3){
                            this.exchangeRateSprites[i - 1].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }
                    }
                }else{
                    for (int i = 0; i < 2; ++i)
                    {
                        if(2 - digit > i)
                            this.exchangeRateSprites[i].spriteName = this.sevenSegSpriteName[2, 10];
                        else{
                            this.exchangeRateSprites[i].spriteName = this.sevenSegSpriteName[2, int.Parse(count[i].ToString())];
                        }
                    }
                    this.exchangeRateSprites [2].spriteName = this.sevenSegSpriteName [2, 0];
                }
            }
        }
    }

    //----------------------------------------------------------------------------------------------------
    /// UI-履歴を追加する.
    public void AddHistory (int num)
    {
        if (num > 9)
            this.history.Insert(0, 9);
        else
            this.history.Insert(0, num);

        if (this.history.Count > 10)
            this.history.RemoveAt(10);

        for (int i = 0; i < this.history.Count; ++i)
        {
            this.historySprites[i].spriteName = this.historySpriteName[this.history[i]];
        }
    }
}
