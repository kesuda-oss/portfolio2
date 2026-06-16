using UnityEngine;

public class Status : MonoBehaviour
{   
    
    public GameObject main;
    public int HP;
    public int maxHP;
    public int Hirumi = 0;
    public int HirumiMax;
    public GameObject WinPanel;
    
    public Animator EnemyAnimator;
    public string TagName;  
    public string TagName2; 
    public string TagName3;
    public AudioSource Dead;
    public AudioSource attackSE1;
    public AudioSource Win;
    
    public AudioClip DeadSE;
    public AudioClip ATSE;
    public AudioClip WinSE;
    
    
    private bool isDead = false;
    private PlayerController pc;
    

    void Start()
    {
        if (pc == null) 
        {
            pc = GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (HP <= 0 &&  !isDead)
        {
            HP = 0;
            EnemyController enemy = GetComponent<EnemyController>();
            if (enemy != null && enemy.BGM != null)
            {
                enemy.BGM.Stop();
            }
            Win.clip = WinSE;
            Win.loop = true;
            Win.Play();
            WinPanel.SetActive(true);
            Time.timeScale = 0f;
            Dead.PlayOneShot(DeadSE);
            EnemyAnimator.SetBool("Dead", true);
            isDead = true;
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       

        if (other.CompareTag(TagName))
        {
            Damage("attack1");
            Debug.Log("プレイヤーの武器1がヒット");
            attackSE1.PlayOneShot(ATSE);
        }
        else if (other.CompareTag(TagName2))
        {
            Damage("attack2");
            Debug.Log("プレイヤーの武器2がヒット");
            attackSE1.PlayOneShot(ATSE);
        }
        else if (other.CompareTag(TagName3))
        {
            Damage("attack3");
            Debug.Log("プレイヤーの武器3がヒット");
            attackSE1.PlayOneShot(ATSE);
        }
    }

    void Damage(string attackType)
    {
        if (attackType == "attack1")
        {
            HP -= 20;
            Hirumi += 10;
        }
        else if (attackType == "attack2")
        {
            HP -= 40;
            Hirumi += 20;
        }
        else if (attackType == "attack3")
        {
            HP -= 60;
            Hirumi += 40;
        }

        if (Hirumi == HirumiMax)
        {   
            Hirumi = 0;
            EnemyAnimator.SetTrigger("enemyHit");
        }
    }
}