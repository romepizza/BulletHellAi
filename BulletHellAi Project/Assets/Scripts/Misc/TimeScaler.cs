using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaler : MonoBehaviour
{
    [SerializeField] private Text m_text;

    public void OnSliderValueChanged(Slider slider)
    {
        float scale = Mathf.Max(0.0001f, slider.value);
        Time.timeScale = scale;

        m_text.text = scale.ToString("0.00");
    }
}
