using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequence : MonoBehaviour
{
    #region Member Variables
    [Header("------- Parent Settings -------")]
    [SerializeField] protected bool m_repeat;

    [Header("--- Deactivision Conditions ---")]
    [SerializeField] protected float m_maxActiveTimeMin;
    [SerializeField] protected float m_maxActiveTimeMax;

    [Header("--- State Timer ---")]
    [SerializeField] private bool m_waitTillClear;
    [SerializeField] private float m_preWeightTimeMin;
    [SerializeField] private float m_preWeightTimeMax;
    [SerializeField] private float m_postWeightTimeMin;
    [SerializeField] private float m_postWeightTimeMax;

    [Header("------- Parent Debug -------")]
    protected SpawnObstacles m_spawnScript;

    private bool m_isInitialized;
    protected bool m_isActive;
    private bool m_isInactive;

    private float m_activationTimeRdy;
    private float m_deactivisionTimeRdy;
    private float m_endTimeRdy;
    #endregion

    public struct Sequence
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    #region Mono
    private void Update()
    {
        ManageState();
    }
    #endregion

    #region Manage Sequence
    public virtual List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        //List<Sequence> sequence = new List<Sequence>();
        Debug.Log("Warning!");
        return null;
    }
    #endregion

    #region Manage State
    private void ManageState()
    {
        if (m_isInitialized == false)
            return;

        if(!m_isActive && !m_isInactive && m_activationTimeRdy <= Time.time)
        {
            if (m_waitTillClear)
            {
                if(m_spawnScript.NoObstaclesPresent())
                    ActivateSequence();
            }
            else
                ActivateSequence();
        }
        
        if(m_isInactive && m_endTimeRdy <= Time.time)
        {
            EndSequence();
        }
    }

    public virtual void InitializeSequence()
    {
        m_isInitialized = true;
        m_activationTimeRdy = Time.time + Random.Range(m_preWeightTimeMin, m_preWeightTimeMax);
        m_endTimeRdy = float.MaxValue;
        ManageState();
    }
    private void ActivateSequence()
    {
        m_isActive = true;
        m_activationTimeRdy = float.MaxValue;
        m_deactivisionTimeRdy = (m_maxActiveTimeMin > 0 || m_maxActiveTimeMax > 0) ? Time.time + Random.Range(m_maxActiveTimeMin, m_maxActiveTimeMax) : float.MaxValue;
    }
    protected void DeactivateSequence()
    {
        m_isInactive = true;
        m_isActive = false;
        m_deactivisionTimeRdy = float.MaxValue;
        m_endTimeRdy = Time.time + Random.Range(m_postWeightTimeMin, m_postWeightTimeMax);
        ManageState();
    }
    public void EndSequence()
    {
        m_isInitialized = false;
        m_isActive = false;
        m_isInactive = false;

        m_activationTimeRdy = float.MaxValue;
        m_deactivisionTimeRdy = float.MaxValue;
        m_endTimeRdy = float.MaxValue;

        if (m_repeat)
            InitializeSequence();
        else
            m_spawnScript.RegisterEndSequence(this);
    }

    public virtual bool CheckDeactivateSequence()
    {
        if (!m_isActive)
            return false;

        if(m_deactivisionTimeRdy < Time.time)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Setter
    public void SetSpawnScript(SpawnObstacles script)
    {
        m_spawnScript = script;
    }
    #endregion
}
