using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacles : MonoBehaviour
{
    [Header("------- Settings -------")]
    [SerializeField] private GameObject m_spawnPrefab;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;
    [SerializeField] private Vector3 m_relativeRestrictionPos;
    [SerializeField] private Vector3 m_relativeRestrictionNeg;
    [Space]
    [SerializeField] private bool m_showGizmos;
    [Header("--- Objects ---")]
    [SerializeField] private LevelOptions m_levelOptions;
    [Header("------- Settings -------")]
    [SerializeField] private float m_cooldownRdyTime;
    [SerializeField] private bool m_isActive;
    [SerializeField] private List<IsObstacle> m_activeObstacles;

    #region Mono
    void Start()
    {
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin, m_cooldownMax);
        m_activeObstacles = new List<IsObstacle>();
    }
    void Update()
    {
        if (!m_isActive)
            return;
        manageSpawn();
    }
    #endregion Mono

    #region ManageSpawn
    void manageSpawn()
    {
        if (m_cooldownRdyTime > Time.time)
            return;
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin, m_cooldownMax);

        GameObject spawnedObject = Instantiate(m_spawnPrefab, transform);
        spawnedObject.transform.position = getRandomPosition();
        spawnedObject.GetComponent<IsObstacle>().GetCaptureCameraTransform().transform.localScale = ScreenshotManager.Instance().GetObstacleScale(spawnedObject.transform.localScale);
        spawnedObject.GetComponent<Rigidbody>().velocity = /*m_levelOrientation.rotation **/ new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));
        

        IsObstacle obstacleScript = spawnedObject.GetComponent<IsObstacle>();
        obstacleScript.SetLevelOptionsScript(m_levelOptions);
        obstacleScript.SetSpawnObstaclesScript(this);
        //if (m_levelOptions.GetIsPlayerLevel())
        //    obstacleScript.SetIsPlayer(true);
        //if (m_levelOptions.GetIsAiLevel())
        //    obstacleScript.SetIsAi(true);

        m_activeObstacles.Add(obstacleScript);
    }
    Vector3 getRandomPosition()
    {
        Vector3 pos = transform.position + /*m_levelOrientation.rotation **/ m_relativeRestrictionPos;
        Vector3 neg = transform.position + /*m_levelOrientation.rotation **/ m_relativeRestrictionNeg;

        Vector3 randomPosition = (pos + neg) * 0.5f + (Random.Range(-0.5f, 0.5f) * (neg - pos));
        return randomPosition;
    }
    public void activate()
    {
        m_isActive = true;
    }
    public void deactivate()
    {
        m_isActive = false;
        for (int i = m_activeObstacles.Count - 1; i >= 0; i--)
        {
            m_activeObstacles[i].DestroySelf();
        }
    }
    #endregion

    public void RemoveActiveObstacle(IsObstacle obstacle)
    {
        m_activeObstacles.Remove(obstacle);
    }

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!m_showGizmos)
            return;

        Vector3 posXPosY = transform.position + /*m_levelOrientation.rotation **/ new Vector3(m_relativeRestrictionPos.x, m_relativeRestrictionPos.y, 0);
        Vector3 posXNegY = transform.position + /*m_levelOrientation.rotation **/ new Vector3(m_relativeRestrictionPos.x, m_relativeRestrictionNeg.y, 0);
        Vector3 negXPosY = transform.position + /*m_levelOrientation.rotation **/ new Vector3(m_relativeRestrictionNeg.x, m_relativeRestrictionPos.y, 0);
        Vector3 negYNegY = transform.position + /*m_levelOrientation.rotation * */ new Vector3(m_relativeRestrictionNeg.x, m_relativeRestrictionNeg.y, 0);

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(posXPosY, negXPosY);
        Gizmos.DrawLine(negXPosY, negYNegY);
        Gizmos.DrawLine(negYNegY, posXNegY);
        Gizmos.DrawLine(posXNegY, posXPosY);
    }
    #endregion
}
