using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        //slider.maxValue = maxHealth;
        slider.value = currentHealth / maxHealth;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
