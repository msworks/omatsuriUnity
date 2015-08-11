using UnityEngine;
using System.Collections;
using System;

public class PictureDefine : ScriptableObject {
    public ReelFacePicture[] reelFaces;
    public Texture2D reel4thLit;
    public Texture2D reel4thUnLit;

    [Serializable]
    public class ReelFacePicture {
        public Sprite unlit;
        public Sprite lit;
        public Sprite blur;
    }
}
