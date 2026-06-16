using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class playerStatus : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;


    public GameObject main;
    public TMP_Text ItemText;
    public int HP;
    public int maxHP;
    public string TagName;
    public string TagName2;
    public string TagName3;
    public string TagName4;
    public float HitDuration = 0.4f;
    public bool isHiting = false;
    public int item = 5;
    public bool isHealing = false;
    public GameObject LosePanel;

    // UnityのUI.Imageとして正しく認識させます
    public Slider HPGage;

    private PlayerDodge dodge;

    void Start()
    {
        dodge = GetComponent<PlayerDodge>();

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        HPGage.maxValue = maxHP;
        HPGage.value = HP;

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        UpdateItemUI();
    }

    private void Update()
    {
        if (HP <= 0)
        {
            HP = 0;
            
            EnemyController enemy = FindFirstObjectByType<EnemyController>();
            if (enemy != null && enemy.BGM != null)
            {
                enemy.BGM.Stop();
            }
            LosePanel.SetActive(true);
            Destroy(main);
            this.enabled = false;
        }

        if (HPGage != null && maxHP > 0)
        {

            HPGage.value = HP;
        }

        Heal();
        ItemText.text = ""+item;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagName)
        {
            if (dodge != null && dodge.isDodging)
            {
                Debug.Log("回避");
                return;
            }

            Damage("head");
            Debug.Log("Damage");
        }

        if (other.tag == TagName2)
        {
            if (dodge != null && dodge.isDodging)
            {
                Debug.Log("回避");
                return;
            }

            Damage("chest");
            Debug.Log("Damage");
        }

        if (other.tag == TagName3)
        {
            if (dodge != null && dodge.isDodging)
            {
                Debug.Log("回避");
                return;
            }

            Damage("defend");
            Debug.Log("Damage");
        }

        if (other.tag == TagName4)
        {
            if (dodge != null && dodge.isDodging)
            {
                Debug.Log("回避");
                return;
            }

            Damage("flame");
            Debug.Log("Damage");
        }
    }

    void Damage(string attackType)
    {
        ReseHitTriggers();

        if (attackType == "head")
        {
            playerAnimator.SetTrigger("Hit");
            HP -= 20;
        }
        else if (attackType == "chest")
        {
            playerAnimator.SetTrigger("Hit");
            HP -= 70;
        }
        else if (attackType == "defend")
        {
            playerAnimator.SetTrigger("Hit");
            HP -= 40;
        }
        else if (attackType == "flame")
        {
            playerAnimator.SetTrigger("Hit");
            HP -= 90;
        }

        if (!isHiting)
        {
            StartCoroutine(HitRoutine());
        }

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ConMoveHit();
        }
    }

    void UpdateItemUI()
    {
        ItemText.text = "×" + item;
    }

    public void Heal()
    {
        if (item == 0) return;

        if (Input.GetKeyDown(KeyCode.R) && !isHealing)
        {
            StartCoroutine(HealRoutine());

        }
    }

    IEnumerator HealRoutine()
    {
        isHealing = true;

        playerAnimator.SetTrigger("Heal");

        yield return new WaitForSeconds(2.0f);

        HP += 50;
        item--;
        UpdateItemUI();

        if (HP > maxHP)
        {
            HP = maxHP;
        }

        isHealing = false;
    }

    IEnumerator HitRoutine()
    {
        isHiting = true;
        float timer = 0f;

        while (timer < HitDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isHiting = false;

        ReseHitTriggers();
    }

    void ReseHitTriggers()
    {
        playerAnimator.ResetTrigger("Hit");
    }
}
 