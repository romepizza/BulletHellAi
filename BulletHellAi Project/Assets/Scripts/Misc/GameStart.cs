using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    [SerializeField] private int m_targetFrameRate;
    [SerializeField] private float m_targetFixedDeltaTime = 0.02f;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = m_targetFrameRate;
        if(m_targetFixedDeltaTime > 0)
            Time.fixedDeltaTime = m_targetFixedDeltaTime;
    }
}
