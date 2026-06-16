using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform Camera;
    public float PlayerSpeed;
    public float RotateSpeed;
    public float DashSpeed;
    public Animator PlayerAnimator;
    public Collider WeaponCollider;
    public Collider WeaponCollider2;
    public Collider WeaponCollider3;
    public AudioSource RunSe;
    public AudioSource attackS;
    public AudioClip RunSE;
    public AudioClip attackSE;


    private playerStatus PS;
    private int conbo = 0;
    private float comboTimer = 0f;
    public float comboLimit = 0.5f;

    Vector3 dash = Vector3.zero;
    Vector3 speed = Vector3.zero;
    Vector3 rot = Vector3.zero;

    bool isRun;
    bool isDash;
    bool canMove = true;
    bool canCombo = false;

    Rigidbody rb;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PS = GetComponent<playerStatus>();
    }

    void Update()
    {
        if (canCombo)
        {
            comboTimer += Time.deltaTime;

            if (comboTimer >= comboLimit)
            {
                canCombo = false;
                comboTimer = 0f;

                
                if (conbo == 1 || conbo == 2)
                {
                    conbo = 0;
                    canMove = true;

                    ResetAllAttackTriggers();
                }
            }
        }

        Move();
        Rotation();
        Attack();

        Camera.transform.position = transform.position;
    }

    void Move()
    {
        if (!canMove)
        {
            return;
        }

        float h = 0;
        float v = 0;

        if (Input.GetKey(KeyCode.A)) h = -1;
        if (Input.GetKey(KeyCode.D)) h = 1;
        if (Input.GetKey(KeyCode.W)) v = 1;
        if (Input.GetKey(KeyCode.S)) v = -1;

        Vector3 forward = Camera.forward;
        Vector3 right = Camera.right;

        forward.y = 0;
        right.y = 0;

        Vector3 move = forward * v + right * h;

        if (move.magnitude > 0)
        {
            move = move.normalized;

            float speedValue = PlayerSpeed;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speedValue += DashSpeed;
                isDash = true;
            }
            else
            {
                isDash = false;
            }
            transform.Translate(move * speedValue * Time.deltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(move);
            isRun = true;
        }
        else
        {
            isRun = false;
            isDash = false;
        }

        PlayerAnimator.SetBool("run", isRun);
        PlayerAnimator.SetBool("dash", isDash);
        
    }

    void MoveSet()
    {
        speed.z = PlayerSpeed;
        transform.eulerAngles = Camera.transform.eulerAngles + rot;
        isRun = true;
    }

    void Rotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 rot = Camera.transform.eulerAngles;
        rot.y += mouseX * RotateSpeed;
        rot.x -= mouseY * RotateSpeed;

        rot.x = Mathf.Clamp(rot.x > 180 ? rot.x - 360 : rot.x, -60f, 60f);

        Camera.transform.eulerAngles = rot;
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0)&&!PS.isHiting&&!PS.isHealing)
        {
            if (conbo == 0)
            {
                conbo = 1;
                canMove = false;
                canCombo = false;

                ResetAllAttackTriggers();
                PlayerAnimator.SetTrigger("attack1");
            }
            else if (conbo == 1 && canCombo)
            {
                conbo = 2;
                canMove = false;
                canCombo = false;
                comboTimer = 0f;

                ResetAllAttackTriggers();
                PlayerAnimator.SetTrigger("attack2");
            }
            else if (conbo == 2 && canCombo)
            {
                Debug.Log("Attack3 Trigger");
                conbo = 3;
                canMove = false;
                canCombo = false;
                comboTimer = 0f;

                ResetAllAttackTriggers();
                PlayerAnimator.SetTrigger("attack3");
            }
        }
    }

    public void ConMoveHit()
    {
        canMove = false;
        isRun = false;
        isDash = false;

        PlayerAnimator.SetBool("run", false);
        PlayerAnimator.SetBool("dash", false);
    }

    void HitOff()
    {
        canMove = true;
    }

    public void RunAudio()
    {
        RunSe.PlayOneShot(RunSE);
    }
    public void AttackSE()
    {
        attackS.PlayOneShot(attackSE);
    }
    void AttackOn(string attackType)
    {
        if (attackType == "attack1" && WeaponCollider != null)
        {
            WeaponCollider.enabled = true;
        }
        else if (attackType == "attack2" && WeaponCollider2 != null)
        {
            WeaponCollider2.enabled = true;
        }
        else if (attackType == "attack3" && WeaponCollider3 != null)
        {
            WeaponCollider3.enabled = true;
        }
    }

    void AttackOff()
    {
        WeaponCollider.enabled = false;
        WeaponCollider2.enabled = false;
        WeaponCollider3.enabled = false;

        // attack3終了時だけ完全リセット
        if (conbo == 3)
        {
            conbo = 0;
            canCombo = false;
            comboTimer = 0f;
            canMove = true;

            ResetAllAttackTriggers();
        }
    }

    public void Conbo()
    {
        canCombo = true;
        comboTimer = 0f;
    }

    void CanMove()
    {
        canMove = true;
    }

    public void ForceCanMove()
    {
        canMove = true;
        conbo = 0;
        canCombo = false;
        comboTimer = 0f;

        WeaponCollider.enabled = false;
        WeaponCollider2.enabled = false;
        WeaponCollider3.enabled = false;

        ResetAllAttackTriggers();
    }

    void ResetAllAttackTriggers()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.ResetTrigger("attack1");
            PlayerAnimator.ResetTrigger("attack2");
            PlayerAnimator.ResetTrigger("attack3");
        }
    }
}