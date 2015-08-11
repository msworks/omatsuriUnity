using UnityEngine;
using System.Collections;

public class SpriteChanger : MonoBehaviour{
	[SerializeField] private UISprite uiSprite;
	[SerializeField] private string[] spriteNames;
	public float spriteNum = 0;
	private int currentNum = 0;

	void Start(){
		if (this.uiSprite == null) Debug.LogError ("編集するUISpriteがアタッチされていません！"+this.gameObject.name+"のSpriteChangerの変数[UiSprite]を確認してください！");
		if (this.spriteNames.Length == 0) Debug.LogError ("変更するSpriteの名前が登録されていません！"+this.gameObject.name+"のSpriteChangerの変数[SpriteName]を確認してください！");
	}

	void Update(){
		if (this.spriteNum < 0) {
			Debug.LogWarning("AnimationClipにて操作しているSpriteNumの数値がマイナスになっています！AnimationClipを確認してください！" +
				"\n[ヒント]\n" +
				"AnimationClipのKeyの設定を右クリック→Flatにしていますか？\n" +
				"Keyの数値設定が間違っていませんか？");
			this.spriteNum = 0;
		}
		if (this.spriteNum > this.spriteNames.Length) {
			Debug.LogError("AnimationClipにて操作しているSpriteNumの数値がSpriteNamesをオーバーフローしました！AnimationClipを確認してください！");
			return;
		}
		if (this.currentNum != (int)this.spriteNum) {
			this.currentNum = (int)this.spriteNum;
			this.uiSprite.spriteName = this.spriteNames[this.currentNum];
		}
	}
}
