using System.Collections;
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
    public Transform GetCaptureCameraTransform()
    {
        return m_visualCaptureCamera;
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
    #endregion
}
