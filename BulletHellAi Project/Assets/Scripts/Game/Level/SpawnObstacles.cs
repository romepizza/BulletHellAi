using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacles : MonoBehaviour
{
    [Header("------- Settings -------")]
    [SerializeField] private GameObject m_spawnPrefab;
    //[SerializeField] private Vector3 m_directionMin;
    //[SerializeField] private Vector3 m_directionMax;
    //[SerializeField] private float m_cooldownMin;
    //[SerializeField] private float m_cooldownMax;

    [Header("--- Spawn Position ---")]
    //[SerializeField] private bool m_spawnForScreenshot;
    [SerializeField] private Vector3 m_relativeRestrictionPos;
    [SerializeField] private Vector3 m_relativeRestrictionNeg;

    [Header("--- Gizmos ---")]
    [SerializeField] private bool m_showGizmos;

    [Header("--- Objects ---")]
    [SerializeField] private List<SpawnSequence> m_spawnSequences;
    [SerializeField] private LevelOptions m_levelOptions;
    [SerializeField] private TakeScreenshot m_screenshotScript;

    [Header("------- Debug -------")]
    //private float m_cooldownRdyTime;
    private bool m_isActive;
    private List<IsObstacle> m_activeObstacles = new List<IsObstacle>();


    #region Mono
    void Start()
    {
        for(int i = 0; i < m_spawnSequences.Count; i++)
        {
            m_spawnSequences[i].SetSpawnScript(this);
        }
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
        //if (m_cooldownRdyTime > Time.time)
        //    return;
        //m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin, m_cooldownMax);
        List<SpawnSequence.Sequence> spawns = new List<SpawnSequence.Sequence>();

        for(int i = 0; i < m_spawnSequences.Count; i++)
        {
            List<SpawnSequence.Sequence> l = m_spawnSequences[i].ManageSequence(m_relativeRestrictionPos, m_relativeRestrictionNeg);
            if (l == null)
                continue;
            for(int j = 0; j < l.Count; j++)
            {
                spawns.Add(l[j]);
            }
        }

        for(int i = 0; i < spawns.Count; i++)
        {
            GameObject spawnedObject = Instantiate(m_spawnPrefab, transform);
            Vector3 scale = m_screenshotScript.GetObstacleScale(spawnedObject.transform.localScale, 0);
            if (scale != Vector3.zero)
                spawnedObject.GetComponent<IsObstacle>().GetCaptureCameraTransform().transform.localScale = scale;
            spawnedObject.GetComponent<Rigidbody>().velocity = spawns[i].velocity;// /*m_levelOrientation.rotation **/ new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));
            spawnedObject.transform.position = spawns[i].position;

            IsObstacle obstacleScript = spawnedObject.GetComponent<IsObstacle>();
            obstacleScript.SetLevelOptionsScript(m_levelOptions);
            obstacleScript.SetSpawnObstaclesScript(this);

            m_activeObstacles.Add(obstacleScript);
        }
    }
    //Vector3 getRandomPosition()
    //{
    //    Vector3 pos = transform.position + /*m_levelOrientation.rotation **/ m_relativeRestrictionPos;
    //    Vector3 neg = transform.position + /*m_levelOrientation.rotation **/ m_relativeRestrictionNeg;

    //    Vector3 randomPosition = Vector3.zero;
    //    if (m_spawnForScreenshot)
    //    {
    //        randomPosition = neg + new Vector3(m_screenshotScript.GetPixelToWorldScale(0) * (0.5f + Random.Range(0, m_screenshotScript.GetCaptureWidth())), 0, 0);
    //    }
    //    else
    //        randomPosition = (pos + neg) * 0.5f + (Random.Range(-0.5f, 0.5f) * (neg - pos));
    //    return randomPosition;
    //}
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

    public void setScaleOfActiveObstacles(Vector3 scale)
    {
        if (m_activeObstacles == null)
            return;

        if (scale == Vector3.zero)
            scale = m_spawnPrefab.transform.localScale;

        for(int i = 0; i < m_activeObstacles.Count; i++)
        {
            m_activeObstacles[i].GetCaptureCameraTransform().localScale = scale;
        }
    }
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

    #region Getter
    public List<IsObstacle> GetActiveObstacles()
    {
        return m_activeObstacles;
    }
    public GameObject GetSpawnPrefab()
    {
        return m_spawnPrefab;
    }
    public TakeScreenshot GetScreenshotScript()
    {
        return m_screenshotScript;
    }
    #endregion
}
