using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOptions : MonoBehaviour
{
    //public Text m_aiText;

    [Header("------- Settings ------")]
    [SerializeField] private bool m_isPlayerLevel;
    [SerializeField] private bool m_isAiLevel;
    [SerializeField] private KeyCode m_startGameKeyCode;

    [Header("--- Objets ---")]
    [SerializeField] private GameObject m_objectSpawner;
    [SerializeField] private Text m_textCurrent;
    [SerializeField] private Text m_textLast;
    [SerializeField] private Text m_textMax;


    [Header("------ Debug -------")]
    [SerializeField] private bool m_isRunning;
    [SerializeField] private bool m_isPressingGameStart;
    [SerializeField] private float m_currentTime;
    [SerializeField] private float m_timeMax;
    [SerializeField] private float m_timeLast;
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

        if (m_currentTime > m_timeMax)
            m_timeMax = m_currentTime;
        m_textMax.text = m_timeMax.ToString("0.00");
        m_textLast.text = m_currentTime.ToString("0.00");
    }
    public void RestartGame()
    {
        EndGame();
        StartGame();
    }
    #endregion

    #region Misc
    void GetInput()
    {
        m_isPressingGameStart = false;
        if (Input.GetKeyDown(m_startGameKeyCode))
            m_isPressingGameStart = true;
    }
    void DoUiStuff()
    {
        if (!m_isRunning)
            return;

        //if(m_isPlayerLevel)
        //{
        m_currentTime += Time.deltaTime;
        m_textCurrent.text = m_currentTime.ToString("0.00");
        //}
        //if (m_isAiLevel)
        //{
        //    m_currentAiTime += Time.deltaTime;
        //    m_aiText.text = "" + m_currentPlayerTime;
        //}
    }
    #endregion

    #region Getter
    public bool GetIsPlayerLevel()
    {
        return m_isPlayerLevel;
    }
    public bool GetIsAiLevel()
    {
        return m_isAiLevel;
    }
    #endregion
}
