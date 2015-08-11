using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NumberDisplay : MonoBehaviour {

    private Text text;
    public Image[] digits;
    public DigitDefine digitDefine;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        for (int idx = 0; idx < digits.Length; idx++) {
            // 一桁ずつTextの内容をスプライトに変換
            if (idx < text.text.Length) {
                digits[idx].enabled = true;
                int digitId = 10; // スプライト10番はハイフン
                if (text.text[text.text.Length - idx - 1] != '-') {
                    digitId = int.Parse(text.text[text.text.Length - idx - 1].ToString());
                }
                digits[idx].sprite = digitDefine.digits[digitId];
            } else {
                // 桁不足分はスプライトを非表示にする
                digits[idx].enabled = false;
            }
        }
	}
}
