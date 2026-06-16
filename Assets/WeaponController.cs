using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject Effect;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "atflamehit")
        {
            var effect = Instantiate(Effect);
            effect.transform.position = other.transform.position;
            Destroy(effect, 1.2f);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
