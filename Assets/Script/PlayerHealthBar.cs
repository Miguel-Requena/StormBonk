using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider              slider;
    [SerializeField] private Image               fill;
    [SerializeField] private TextMeshProUGUI     hpText;

    private Player _player;

    private static readonly Color FullHP = new Color(0.18f, 0.78f, 0.23f);
    private static readonly Color HalfHP = new Color(1f,    0.75f, 0f);
    private static readonly Color LowHP  = new Color(0.9f,  0.15f, 0.15f);

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        if (_player == null || slider == null) return;

        slider.minValue = 0;
        slider.maxValue = _player.maxHealth;
        slider.value    = _player.currentHealth;
        UpdateColor(1f);
        UpdateText();
    }

    private void Update()
    {
        if (_player == null) return;

        // Mantiene maxValue sincronizado (puede cambiar al subir de nivel)
        if (slider.maxValue != _player.maxHealth)
            slider.maxValue = _player.maxHealth;

        slider.value = _player.currentHealth;

        float t = (float)_player.currentHealth / _player.maxHealth;

        // Cuando la vida está al máximo, fuerza el fill a 1 para evitar
        // el hueco visual que deja el handle del Slider en Unity
        if (fill != null)
        {
            if (_player.currentHealth >= _player.maxHealth)
                fill.fillAmount = 1f;
        }

        UpdateColor(t);
        UpdateText();
    }

    private void UpdateColor(float t)
    {
        if (fill == null) return;
        fill.color = t > 0.5f
            ? Color.Lerp(HalfHP, FullHP, (t - 0.5f) * 2f)
            : Color.Lerp(LowHP,  HalfHP, t * 2f);
    }

    private void UpdateText()
    {
        if (hpText == null || _player == null) return;
        hpText.text = $"HP  {_player.currentHealth} / {_player.maxHealth}";
    }
}
