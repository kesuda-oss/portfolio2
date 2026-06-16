using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private float attackRange = 9.0f;
    [SerializeField] private float rotationSpeed = 0.5f;

    public float EnemySpeed;
    public Animator EnemyAnimator;
    public float screamDuration = 3.0f;
    public float attackCooldown;
    private float lastAttackTime;
    public Collider headCollider;
    public Collider chestCollider;
    public Collider defendCollider;
    public Collider flameCollider;
    public AudioSource audioSource;
    public AudioSource attack1S;
    public AudioSource attack2S;
    public AudioSource attack3S;
    public AudioSource attack4S;
    public AudioSource BGM;
    public AudioClip ScreameSE;
    public AudioClip attack1SE;
    public AudioClip attack2SE;
    public AudioClip attack3SE;
    public AudioClip attack4SE;
    public AudioClip BGMSE;
    

    [SerializeField] private float smallTurnThreshold = 45f;
    [SerializeField] private float largeTurnThreshold = 120f;
    [SerializeField] private float turnMotionDuration = 1.0f;

    private float screamTimer = 0f;
    private bool isScreaming = false;
    private bool hasScreamed = false;
    private bool isDash = false;
    private bool canMove = true;
    private bool isTurning = false;
    private GameObject Target;
    private Vector3 MoveDirection;

    private void Start()
    {
        AttackOff();
    }

    void Update()
    {
        Vector3 speed = Vector3.zero;
        Transform currentTargetTransform = Target != null ? Target.transform : playerTransform;
        float distanceToPlayer = currentTargetTransform != null
            ? Vector3.Distance(transform.position, currentTargetTransform.position)
            : float.MaxValue;

        if (Target != null || distanceToPlayer < 36f)
        {
            if (Target == null && playerTransform != null)
                Target = playerTransform.gameObject;

            // --- 開幕の叫び ---
            if (!hasScreamed && !isScreaming)
            {   
                
                isScreaming = true;
                audioSource.PlayOneShot(ScreameSE);
                EnemyAnimator.SetBool("scream", true);
                EnemyAnimator.SetBool("run", false);
            }

            if (isScreaming)
            {
                RotateTowardsTarget();
                screamTimer += Time.deltaTime;

                if (screamTimer >= screamDuration)
                {
                    isScreaming = false;
                    hasScreamed = true;
                    isDash = true;
                    
                    // 【修正】ここで瞬間振り向き（LookRotation）させず、進行方向のベクトルだけをセット
                    MoveDirection = (Target.transform.position - transform.position).normalized;
                    MoveDirection.y = 0;
                    
                    EnemyAnimator.SetBool("scream", false);
                    EnemyAnimator.SetBool("dash", true);
                }
            }

            // --- 移動と軸合わせロジック ---
            if (isDash && canMove && !isTurning)
            {
                // ダッシュ移動中も、ターゲットの方へじわじわ向き直るようにする
                RotateTowardsTarget();

                // 常に現在の正面（transform.forward）を移動方向として更新（滑らかな追従のため）
                MoveDirection = transform.forward;

                if (distanceToPlayer > attackRange - 2.0f)
                {
                    speed.z = EnemySpeed * 2; 
                    EnemyAnimator.SetBool("dash", true);
                    EnemyAnimator.SetBool("run", false);
                }
                else
                {
                    speed.z = 0; 
                    EnemyAnimator.SetBool("dash", false);
                    EnemyAnimator.SetBool("run", true);
                }
            }
            else if (!canMove || isTurning)
            {
                speed.z = 0;
                EnemyAnimator.SetBool("run", true); 
                EnemyAnimator.SetBool("dash", false);
            }
        }
        else
        {
            EnemyAnimator.SetBool("run", false);
            EnemyAnimator.SetBool("dash", false);
            ResetState();
        }

        // 世界軸で移動
        transform.position += MoveDirection * speed.z * Time.deltaTime;

        if (playerTransform == null || enemyAnimator == null) return;

        // --- 攻撃の実行（コルーチン経由に変更） ---
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && canMove && !isTurning && !isScreaming)
        {
            // 【変更】ダイレクトにAttackを呼ばず、振り向きコルーチンを挟む
            StartCoroutine(TurnAndAttack(distanceToPlayer));
        }
    }

    // =============================================
    // モンハン風：攻撃前のじわじわ振り向きコルーチン
    // =============================================
    private IEnumerator TurnAndAttack(float distance)
    {
        isTurning = true;
        canMove = false; // 旋回中は勝手に移動しないようにロック

        // ティガレックスのような重いモンスターなら 1.2f 前後に調整
        float turnDuration = 0.8f; 
        float timer = 0f;

        Quaternion startRot = transform.rotation;

        while (timer < turnDuration)
        {
            if (Target == null) break;

            timer += Time.deltaTime;

            // 旋回中もプレイヤーが動くことを想定し、毎フレーム最新のターゲット位置への回転を計算
            Vector3 direction = (Target.transform.position - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                // 時間経過とともにじわじわ補間
                transform.rotation = Quaternion.Slerp(startRot, targetRot, timer / turnDuration);
            }

            yield return null;
        }

        isTurning = false;
        // 振り向きが完了したら実際の攻撃処理へ
        Attack(distance);
    }

    // =============================================
    // モンハン風振り向き（通常移動・咆哮時用）
    // =============================================
    private void RotateTowardsTarget()
    {
        if (Target == null || isTurning) return;

        Vector3 direction = (Target.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction == Vector3.zero) return;

        float angleDiff = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        float absAngle = Mathf.Abs(angleDiff);

        if (absAngle > largeTurnThreshold)
        {
            isTurning = true;
            StartCoroutine(LargeTurnRoutine(angleDiff > 0 ? "turnRight" : "turnLeft"));
        }
        else if (absAngle > smallTurnThreshold)
        {
            float adaptiveSpeed = rotationSpeed * Mathf.Clamp(1f - absAngle / 180f, 0.1f, 0.5f);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, adaptiveSpeed * Time.deltaTime);
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator LargeTurnRoutine(string triggerName)
    {
        if (Target == null)
        {
            isTurning = false;
            canMove = true;
            yield break;
        }

        canMove = false;

        Vector3 direction = (Target.transform.position - transform.position).normalized;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < turnMotionDuration)
        {
            if (Target == null)
            {
                isTurning = false;
                canMove = true;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / turnMotionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            yield return null;
        }

        transform.rotation = targetRotation;
        isTurning = false;
        canMove = true;
    }

    // =============================================
    // 距離に応じた攻撃の抽選AI
    // =============================================
    void Attack(float distance)
    {
        lastAttackTime = Time.time;
        canMove = false;
        
        // 【修正】ここにあった瞬間振り向き（LookRotation）の2行を完全削除！
        // 攻撃アニメーションの開始方向は、直前のコルーチンが作った回転を引き継ぎます。

        int randomAttackID = 1;
        float closeRangeLimit = attackRange * 0.5f; 

        if (distance <= closeRangeLimit)
        {
            int dice = Random.Range(1, 101); 

            if (dice <= 45)       randomAttackID = 1; 
            else if (dice <= 90)  randomAttackID = 2; 
            else                  randomAttackID = 3; 
            
            Debug.Log($"【インファイト】距離 {distance:F1}: 近接技を優先抽選しました (ID: {randomAttackID})");
        }
        else
        {
            int dice = Random.Range(1, 101);

            if (dice <= 40)       randomAttackID = 3; 
            else if (dice <= 80)  randomAttackID = 4; 
            else                  randomAttackID = 1; 
            
            Debug.Log($"【アウトレンジ】距離 {distance:F1}: 遠距離技・突進を優先抽選しました (ID: {randomAttackID})");
        }

        if (randomAttackID == 1)
        {
            enemyAnimator.SetTrigger("enemyattack");
            
        }
        else if (randomAttackID == 2)
        {
            enemyAnimator.SetTrigger("enemyattack2");
           
        }
        else if (randomAttackID == 3)
        {
            enemyAnimator.SetTrigger("enemyattack3");
            
        }
        else if (randomAttackID == 4)
        {
            enemyAnimator.SetTrigger("enemyattack4");
            
        }
    }

    void AttackOn(string attackType)
    {
        if (attackType == "head" && headCollider != null) headCollider.enabled = true;
        else if (attackType == "chest" && chestCollider != null) chestCollider.enabled = true;
        else if (attackType == "defend" && defendCollider != null) defendCollider.enabled = true;
        else if (attackType == "flame" && flameCollider != null) flameCollider.enabled = true;
    }

    void AttackOff()
    {
        if (headCollider != null) headCollider.enabled = false;
        if (chestCollider != null) chestCollider.enabled = false;
        if (defendCollider != null) defendCollider.enabled = false;
        if (flameCollider != null) flameCollider.enabled = false;
        canMove = true;
        isTurning = false;
    }
    public void Claw()
    {
        attack1S.PlayOneShot(attack1SE);
    }
    public void Basic()
    {
        attack2S.PlayOneShot(attack2SE);
    }
    public void Defend()
    {
        attack3S.PlayOneShot(attack3SE);
    }
    public void Flame()
    {
        attack4S.PlayOneShot(attack4SE);
    }

    // アニメーションイベントから呼ばれる関数
    void CanMove()
    {
        canMove = true;
        // 【修正】1フレームだけの意味のない RotateTowardsTarget() を削除し、移動フラグのクリーンアップのみに
    }

    private void ResetState()
    {
        isScreaming = false;
        hasScreamed = false;
        isDash = false;
        screamTimer = 0f;
        isTurning = false;
        canMove = true;
        EnemyAnimator.SetBool("dash", false);
        EnemyAnimator.SetBool("run", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Target = other.gameObject;

            if (!BGM.isPlaying)
            {
                BGM.clip = BGMSE;
                BGM.loop = true;
                BGM.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) Target = null;
    }
}