using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RateChange : MonoBehaviour
{
    [SerializeField]
    List<Toggle> toggles;

    public void Confirm()
    {
        var selected = toggles.Where(toggle => toggle.isOn).First();
        var name = selected.gameObject.name;
        var length = name.Length;
        var cent = name.Substring(startIndex: 0, length: length - 4).ParseInt();

        // レートを設定
        Rate.Instanse.SetRate(cent);
    }
}

static class StringExtension
{
    static public int ParseInt(this string str)
    {
        return int.Parse(str);
    }
}

