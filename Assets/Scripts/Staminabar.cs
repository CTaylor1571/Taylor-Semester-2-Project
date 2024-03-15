using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Staminabar : MonoBehaviour
{
    [SerializeField] private Image staminaBarSprite;
    private float target = 1;
    public void UpdateStaminaBar(float maxStamina, float currentStamina)
    {
        target = currentStamina / maxStamina;

    }

    private void Update()
    {
        staminaBarSprite.fillAmount = Mathf.MoveTowards(staminaBarSprite.fillAmount, target, Time.deltaTime);
    }
}
