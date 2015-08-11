using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 自動プレイパターン定義
/// </summary>
public class AutoPlayPatternDefine : ScriptableObject {

    /// <summary>
    /// 自動プレイパターン定義設定値
    /// -1はランダム
    /// </summary>
    [Serializable]
    public class AutoPlayPatternValue {
        /// <summary>
        /// 右から止める場合はTrue
        /// </summary>
        public bool isStopReverse;
        [Range(-1, 20)]
        public int[] targetRow;
    }
    [Header("通常")]
    public AutoPlayPatternValue[] normal;
    [Header("BB告知")]
    public AutoPlayPatternValue[] bigReach;
    [Header("RB告知")]
    public AutoPlayPatternValue[] regReach;
    [Header("BIG")]
    public AutoPlayPatternValue[] big;
    [Header("BIG逆順")]
    public AutoPlayPatternValue[] bigReverse;
    [Header("RB")]
    public AutoPlayPatternValue[] regular;

}
