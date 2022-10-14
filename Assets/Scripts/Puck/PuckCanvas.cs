using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuckCanvas : MonoBehaviour
{
    [SerializeField] private PuckManager puck;
    [SerializeField] private Transform canvas;
    [SerializeField] private Image slideBarImage;
    [SerializeField] private GameObject infoTextPrefab;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private float randomSpawnTextArea;
    [SerializeField] private Color damageTextColor;

    [Header("DamageText")]
    [SerializeField] private float damageText_fontMin;
    [SerializeField] private float damageText_fontMax;

    private int currentIndexClip = 0;
    private SlideBar healthSlideBar;

    public void Start()
    {
        healthSlideBar = new SlideBar(puck.maxHealth, 1000);
    }

    public void UpdSlideBar(float health)
    {
        if (healthSlideBar.sliding != null) StopCoroutine(healthSlideBar.sliding);
        healthSlideBar.sliding = StartCoroutine(healthSlideBar.Sliding(slideBarImage, health));
    }
    public void ActiveHealText(int healths)
    {
        var randPos = Random.insideUnitCircle * randomSpawnTextArea;
        var newPos = new Vector3(randPos.x, randPos.y, 0) + transform.position;

        var textDamage = Instantiate(infoTextPrefab, newPos, transform.rotation, canvas).GetComponent<TextMeshProUGUI>();
        textDamage.color = Color.green;
        textDamage.fontSize = CalcSizeDamageText(healths);
        textDamage.text = "+" + healths.ToString();
    }

    public void ActiveInfoText(bool isActive, int damage)
    {

        var randPos = Random.insideUnitCircle * randomSpawnTextArea;
        var newPos = new Vector3(randPos.x, randPos.y, 0) + transform.position;

        var textDamage = Instantiate(infoTextPrefab, newPos,transform.rotation,canvas).GetComponent<TextMeshProUGUI>();
        textDamage.color = damageTextColor;
        textDamage.fontSize = CalcSizeDamageText(damage);
        textDamage.text = damage.ToString();
    }
    public void ActiveInfoText(bool isActive, string text = "")
    {
        infoText.color = Color.red;
        infoText.text = text;
        infoText.gameObject.SetActive(isActive);
        ActiveSlideBar(!isActive);
    }
    public void ActiveSlideBar(bool isActive)
    {
        slideBarImage.transform.parent.gameObject.SetActive(isActive);
    }

    private float CalcSizeDamageText(int damage)
    {
        float MaxCalcDamage = 500;
        float part = (float)damage / MaxCalcDamage;
        float lengthFont = damageText_fontMax - damageText_fontMin;
        float result = part * lengthFont + (damageText_fontMax - lengthFont);
        return Mathf.Clamp(result, damageText_fontMin, damageText_fontMax);
    }
}
