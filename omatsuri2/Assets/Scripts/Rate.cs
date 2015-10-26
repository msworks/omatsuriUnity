using UnityEngine;
using System.Collections;

public class Rate : MonoBehaviour {

    static Rate _instance;
    static public Rate Instanse { get { return _instance; } }

    int cent = 20;

	void Start ()
    {
        _instance = this;
	}

	public Rate SetRate(int cent)
    {
        this.cent = cent;
        return this;
    }

    /// <summary>
    /// コイン枚数を金額に変換する
    /// </summary>
    /// <param name="coin"></param>
    /// <returns></returns>
    public int Coin2Cent(int coin)
    {
        return coin * cent;
    }
}
