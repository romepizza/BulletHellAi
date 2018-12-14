using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacles : MonoBehaviour
{
    [Header("------- Settings -------")]
    [SerializeField] private GameObject m_spawnPrefab;

    [Header("--- Sequences ---")]
    [SerializeField] private bool m_repeatSequences;
    [SerializeField] private SpawnSequences m_sequnces;
    //[SerializeField] private List<SpawnSequence> m_spawnSequences;

    [Header("--- Spawn Position ---")]
    [SerializeField] private Vector3 m_relativeRestrictionPos;
    [SerializeField] private Vector3 m_relativeRestrictionNeg;

    [Header("--- Gizmos ---")]
    [SerializeField] private bool m_showGizmos;

    [Header("--- Objects ---")]
    [SerializeField] private LevelOptions m_levelOptions;
    [SerializeField] private TakeScreenshot m_screenshotScript;

    [Header("------- Debug -------")]
    //private float m_cooldownRdyTime;
    private bool m_isActive;
    private List<IsObstacle> m_activeObstacles = new List<IsObstacle>();
    private int m_currentSequenceIndex;


    #region Mono
    private void Awake()
    {
        if (m_sequnces == null)
        {
            Debug.Log("Warning: No sequences detected! Created a default one!");
            m_sequnces = gameObject.AddComponent<SpawnSequences>();
            m_sequnces.m_sequences = new List<SpawnSequence>();
        }
        if (m_sequnces.m_sequences == null)
        {
            Debug.Log("Warning: Sequences sequences were null! Created an empty List!");
            m_sequnces.m_sequences = new List<SpawnSequence>();
        }
    }
    void Start()
    {
        for (int i = 0; i < m_sequnces.m_sequences.Count; i++)
        {
            m_sequnces.m_sequences[i].SetSpawnScript(this);
        }
    }
    void Update()
    {
        if (!m_isActive)
            return;
        ManageSpawn();
    }
    #endregion Mono

    #region ManageSpawn
    void ManageSpawn()
    {
        if (/*m_sequnces.m_sequences == null ||*/ m_sequnces.m_sequences.Count == 0)
            return;

        List<SpawnSequence.Sequence> spawns = new List<SpawnSequence.Sequence>();

        List<SpawnSequence.Sequence> l = m_sequnces.m_sequences[m_currentSequenceIndex].ManageSequence(m_relativeRestrictionPos, m_relativeRestrictionNeg);
        if (l == null)
            return;
        for (int i = 0; i < l.Count; i++)
        {
            spawns.Add(l[i]);
        }

        for (int i = 0; i < spawns.Count; i++)
        {
            GameObject spawnedObject = Instantiate(m_spawnPrefab, transform);
            Vector3 scale = m_screenshotScript.GetObstacleScale(spawnedObject.transform.localScale, 0);
            IsObstacle obstacleScript = spawnedObject.GetComponent<IsObstacle>();
            if (scale != Vector3.zero)
                obstacleScript.GetCaptureCameraTransform().transform.localScale = scale;
            spawnedObject.GetComponent<Rigidbody>().velocity = spawns[i].velocity;
            spawnedObject.transform.position = spawns[i].position;

            obstacleScript.SetLevelOptionsScript(m_levelOptions);
            obstacleScript.SetSpawnObstaclesScript(this);

            m_activeObstacles.Add(obstacleScript);
        }
    }
    #endregion

    #region Manage State
    public void Activate()
    {
        m_isActive = true;
        m_currentSequenceIndex = 0;
        if (/*m_sequnces.m_sequences != null ||*/ m_sequnces.m_sequences.Count >= 1)
            m_sequnces.m_sequences[0].InitializeSequence();
    }
    public void Deactivate()
    {
        m_isActive = false;
        if (/*m_sequnces.m_sequences != null && */m_currentSequenceIndex < m_sequnces.m_sequences.Count)
        {
            m_sequnces.m_sequences[m_currentSequenceIndex].EndSequence();
        }
        for (int i = m_activeObstacles.Count - 1; i >= 0; i--)
        {
            m_activeObstacles[i].DestroySelf();
        }
    }
    #endregion

    #region Manage Obstacles
    public void SetScaleOfActiveObstacles(Vector3 scale)
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
    #endregion

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
    //public List<IsObstacle> GetActiveObstacles()
    //{
    //    return m_activeObstacles;
    //}
    public GameObject GetSpawnPrefab()
    {
        return m_spawnPrefab;
    }
    public TakeScreenshot GetScreenshotScript()
    {
        return m_screenshotScript;
    }
    #endregion

    #region Misc
    //private void RegisterStartSequence()
    //{

    //}
    public void RegisterEndSequence(SpawnSequence sequence)
    {
        m_currentSequenceIndex++;
        if(m_currentSequenceIndex >= m_sequnces.m_sequences.Count)
        {
            if (m_repeatSequences)
            {
                m_currentSequenceIndex = 0;
            }
            else
            {
                m_isActive = false;
                return;
            }
        }
        m_sequnces.m_sequences[m_currentSequenceIndex].InitializeSequence();
    }
    public bool NoObstaclesPresent()
    {
        return m_activeObstacles.Count == 0;
    }
    #endregion
}
