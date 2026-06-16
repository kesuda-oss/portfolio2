using UnityEngine;
using System.Collections;

public class PlayerDodge : MonoBehaviour
{
    private PlayerBodySystem bodySystem;
    private PlayerController playerController;
    private playerStatus status;
    
    [SerializeField] private Animator playerAnimator;

    [Header("Dodge Settings")]
    public float dodgeDuration = 0.4f;
    public bool isDodging = false;

    void Start()
    {
        bodySystem = GetComponent<PlayerBodySystem>();
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        status = GetComponent<playerStatus>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isDodging&&!status.isHealing)
        {
            StartCoroutine(DodgeRoutine());
        }
    }

    IEnumerator DodgeRoutine()
    {
        isDodging = true;
        playerController.ForceCanMove(); 
        playerAnimator.SetTrigger("Dodge");

        Vector3 dodgeDir = transform.forward;
        float timer = 0f;
        while (timer < dodgeDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
        ResetDodgeTriggers();
    }
    void ResetDodgeTriggers()
    {
        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger("Dodge");
            
        }
    }
}