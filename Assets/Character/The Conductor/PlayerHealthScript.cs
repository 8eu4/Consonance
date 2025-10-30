using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthScript : MonoBehaviour
{
    GameObject Player;
    [SerializeField] private TextMeshProUGUI HealthPointUI;
    [SerializeField] private float MaxHP;
    private float CurrentHP;
    [SerializeField] private Slider slider;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        CurrentHP = MaxHP;
    }

    void Update()
    {
        HealthPointUI.text = CurrentHP.ToString() + "%";
        slider.value = CurrentHP / MaxHP;

        if (CurrentHP <= 0f)
        {
            //Destroy(Player);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            TakeDamage(10f);
        }

    }
    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0f)
        {
            CurrentHP = 0f;
        }

        
    }
}
