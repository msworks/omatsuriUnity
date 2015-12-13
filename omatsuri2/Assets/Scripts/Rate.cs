public enum Cents
{
    cent5   = 5,
    cent20  = 20,
    cent40  = 40,
    cent100 = 100,
}

public class Rate
{
    static Rate _instance = new Rate();
    static public Rate Instanse { get { return _instance; } }

    Cents cent = Cents.cent20;

    /// <summary>
    /// コイン１枚が何セントかを返却する
    /// </summary>
    /// <returns></returns>
    public int GetRate()
    {
        return (int)cent;
    }

	public Rate SetRate(int cent)
    {
        this.cent = (Cents)cent;
        return this;
    }

    /// <summary>
    /// コイン枚数を金額に変換する
    /// </summary>
    /// <param name="coin"></param>
    /// <returns></returns>
    public int Coin2Cent(int coin)
    {
        return coin * (int)cent;
    }
}
