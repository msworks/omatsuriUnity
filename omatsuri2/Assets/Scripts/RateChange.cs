using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RateChange : MonoBehaviour
{
    [SerializeField]
    List<Toggle> toggles;

    [SerializeField]
    Text ZandakaField;

    [SerializeField]
    Text RateField;

    public void display()
    {
        var cent = Rate.Instanse.GetRate();

        var table = new[]
        {
            new { cent = 5,   toggle = toggles[0] },
            new { cent = 20,  toggle = toggles[1] },
            new { cent = 40,  toggle = toggles[2] },
            new { cent = 100, toggle = toggles[3] },
        };

        foreach(var e in table)
        {
            if (e.cent == cent)
            {
                e.toggle.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                e.toggle.GetComponent<Toggle>().isOn = false;
            }
        }

        var z = GameManager.Instance.casinoData.Exchange;

        ZandakaField.text = string.Format("{0:N2}", z);

        RateField.text = cent.ToString();
    }

    public void Confirm()
    {
        var selected = toggles.Where(toggle => toggle.isOn).First();
        var name = selected.gameObject.name;
        var length = name.Length;
        var cent = name.Substring(startIndex: 0, length: length - 4).ParseInt();

        // レートを設定
        Rate.Instanse.SetRate(cent);

        // アプリケーションリセット
        Application.LoadLevel("Main");
    }
}
