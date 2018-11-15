using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOptions : MonoBehaviour
{
    //public Text m_aiText;

    [Header("------- Settings ------")]
    //[SerializeField] private bool m_isPlayerLevel;
    //[SerializeField] private bool m_isAiLevel;
    [SerializeField] private KeyCode m_startGameKeyCode;

    [SerializeField] private int m_meanConsiderNumber;

    [Header("--- Objets ---")]
    [SerializeField] private GameObject m_objectSpawner;
    [SerializeField] private Text m_textCurrent;
    [SerializeField] private Text m_textLast;
    [SerializeField] private Text m_textMean;
    [SerializeField] private Text m_textMax;


    [Header("------ Debug -------")]
    private bool m_isRunning;
    private bool m_isPressingGameStart;
    private float m_currentTime;
    private float m_timeLast;
    private float m_timeMean;
    private float m_timeMax;

    private Queue<float> m_times = new Queue<float>();
    //public float m_currentAiTime;

    #region Mono
    public void Update()
    {
        GetInput();
        CheckStartGame();
        DoUiStuff();
    }
    #endregion

    #region GameStates
    void CheckStartGame()
    {
        if (m_isPressingGameStart && !m_isRunning)
        {
            StartGame();
        }
        else if (m_isPressingGameStart && m_isRunning)
        {
            EndGame();
        }
    }
    public void StartGame()
    {
        m_isRunning = true;
        SpawnObstacles[] objectSpawners = m_objectSpawner.GetComponents<SpawnObstacles>();
        for (int i = 0; i < objectSpawners.Length; i++)
        {
            objectSpawners[i].activate();
        }

        m_currentTime = 0;
    }
    public void EndGame()
    {
        m_isRunning = false;
        SpawnObstacles[] objectSpawners = m_objectSpawner.GetComponents<SpawnObstacles>();
        for (int i = 0; i < objectSpawners.Length; i++)
        {
            objectSpawners[i].deactivate();
        }

        AddTime();
    }
    public void RestartGame()
    {
        EndGame();
        StartGame();
    }
    #endregion

    #region Misc
    private void GetInput()
    {
        m_isPressingGameStart = false;
        if (Input.GetKeyDown(m_startGameKeyCode))
            m_isPressingGameStart = true;
    }
    private void DoUiStuff()
    {
        if (!m_isRunning)
            return;

        m_currentTime += Time.deltaTime;
        m_textCurrent.text = "" + m_currentTime.ToString("0.00");
        
    }
    private void AddTime()
    {
        if (m_currentTime > m_timeMax)
            m_timeMax = m_currentTime;
        m_textMax.text = "" + m_timeMax.ToString("0.00");
        m_textLast.text = "" + m_currentTime.ToString("0.00");

        // mean time
        if (m_times.Count < m_meanConsiderNumber)
        {
            if (m_times.Count > 1)
                m_timeMean *= m_times.Count;

            m_times.Enqueue(m_currentTime);
            m_timeMean += m_currentTime;
            m_timeMean /= m_times.Count;
        }
        else
        {
            float oldValue = m_times.Dequeue();
            m_timeMean -= oldValue / (m_times.Count + 1);
            m_times.Enqueue(m_currentTime);
            m_timeMean += m_currentTime / m_times.Count;
        }

        m_textMean.text = "" + m_timeMean.ToString("0.00");
    }
    #endregion

    #region Getter
    //public bool GetIsPlayerLevel()
    //{
    //    return m_isPlayerLevel;
    //}
    //public bool GetIsAiLevel()
    //{
    //    return m_isAiLevel;
    //}
    #endregion
}
