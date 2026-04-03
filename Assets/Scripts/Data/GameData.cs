using UnityEngine;

/// <summary>
/// シーン間で保持されるプレイヤーの進行状況やグローバルステータスを管理する静的クラス
/// </summary>
public static class GameData
{
    /// <summary> 現在の累計獲得スコア </summary>
    public static int totalScore = 0;

    /// <summary> バリアアイテムの所持状態 </summary>
    public static bool hasShield = false;

    /// <summary> 武器の強化レベル（初期状態は1） </summary>
    public static int weaponLevel = 1;

    
    /// <summary>
    /// ゲームオーバー時やコンティニュー時に呼ばれる、進行データの完全初期化処理
    /// </summary>
    public static void ResetData()
    {
        totalScore = 0;
        hasShield = false;
        weaponLevel = 1;
    }
}
