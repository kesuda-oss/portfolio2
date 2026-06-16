using UnityEngine;

// 部位の状態（正常、負傷、欠損）を定義する「型」
public enum BodyPartStatus { Healthy, Damaged, Severed }

public class PlayerBodySystem : MonoBehaviour
{
    [Header("Body Part Status")] // インスペクター上に見出しを表示
    public BodyPartStatus leftArm = BodyPartStatus.Healthy; // 左腕の状態（初期値：正常）
    public BodyPartStatus legs = BodyPartStatus.Healthy;    // 脚の状態（初期値：正常）

    [Header("Settings")]
    [SerializeField] float damagedAttackMultiplier = 0.4f; // 負傷した時の攻撃力倍率（3倍）
    [SerializeField] float damagedMoveSpeedScale = 0.4f;   // 負傷した時の移動速度倍率（0.4倍）

    // 現在の脚の状態から、移動速度の補正値を計算して返す関数
    public float GetMoveSpeedMultiplier()
    {
        // もし脚が負傷していたら、設定した鈍足倍率（0.4）を返す
        if (legs == BodyPartStatus.Damaged) return damagedMoveSpeedScale;
        // もし脚が完全に欠損していたら、移動不可（0）を返す
        if (legs == BodyPartStatus.Severed) return 0f; 
        // どちらでもなければ、通常速度（1.0）を返す
        return 1.0f;
    }

    // 現在の腕の状態から、最終的なダメージ量を計算して返す関数
    public float GetAttackDamage(float baseDamage)
    {
        // もし左腕が負傷状態なら（痛みと引き換えに魔力発動）
        if (leftArm == BodyPartStatus.Damaged)
        {
            Debug.Log("威力低下"); 
            return baseDamage * damagedAttackMultiplier; 
        }
        // 正常なら、そのままの攻撃力を返す
        return baseDamage;
    }

    // 外部（敵の攻撃判定など）から部位ダメージを通知するための関数
    public void TakePartDamage(string partName)
    {
        // 引数が"Legs"なら脚を負傷状態にする
        if (partName == "Legs") legs = BodyPartStatus.Damaged;
        // 引数が"LeftArm"なら左腕を負傷状態にする
        if (partName == "LeftArm") leftArm = BodyPartStatus.Damaged;
        
        // 見た目（エフェクトやアニメーション）を更新する
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // ここにVFXの再生や、シェーダーのパラメータ変更処理を記述する
    }
}