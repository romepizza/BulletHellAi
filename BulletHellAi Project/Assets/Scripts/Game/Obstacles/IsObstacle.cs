﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsObstacle : MonoBehaviour {

    [Header("------- Settings -------")]
    //[SerializeField] private bool m_isPlayerObstacle;
    //[SerializeField] private bool m_isAiObstacle;
    [Header("--- Objects ---")]
    [SerializeField] private Transform m_visualMainCamera;
    [SerializeField] private Transform m_visualCaptureCamera;
    [Header("------- Debug -------")]
    [SerializeField] private LevelOptions m_levelOptions;
    [SerializeField] private SpawnObstacles m_spawnObjectsScript;

    public void DestroySelf()
    {
        m_spawnObjectsScript.RemoveActiveObstacle(this);
        Destroy(gameObject);
    }

    #region Getter
    public Transform GetVisualCamera()
    {
        return m_visualMainCamera;
    }
    public LevelOptions GetLevelOptions()
    {
        return m_levelOptions;
    }
    #endregion

    #region Setter
    public void SetLevelOptionsScript(LevelOptions levelOptions)
    {
        m_levelOptions = levelOptions;
    }
    public void SetSpawnObstaclesScript(SpawnObstacles spawnObstacles)
    {
        m_spawnObjectsScript = spawnObstacles;
    }
    //public void SetIsPlayer(bool value)
    //{
    //    m_isPlayerObstacle = value;
    //}
    //public void SetIsAi(bool value)
    //{
    //    m_isAiObstacle = value;
    //}
    #endregion
}
