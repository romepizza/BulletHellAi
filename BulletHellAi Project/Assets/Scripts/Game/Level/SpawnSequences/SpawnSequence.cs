using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequence : MonoBehaviour
{
    #region Member Variables
    [Header("------- Parent Settings -------")]
    [SerializeField] protected bool m_repeat;
    [SerializeField] protected int m_repeats;

    [Header("--- Deactivision Conditions ---")]
    [SerializeField] protected float m_maxActiveTimeMin;
    [SerializeField] protected float m_maxActiveTimeMax;

    [Header("--- State Timer ---")]
    [SerializeField] private bool m_waitTillClear;
    [SerializeField] private float m_preWeightTimeMin;
    [SerializeField] private float m_preWeightTimeMax;
    [SerializeField] private float m_postWeightTimeMin;
    [SerializeField] private float m_postWeightTimeMax;

    [Header("--- Seed ---")]
    [SerializeField] protected int m_initSeed;

    [Header("------- Parent Debug -------")]
    protected SpawnObstacles m_spawnScript;

    private bool m_isInitialized;
    protected bool m_isActive;
    private bool m_isInactive;

    private float m_activationTimeRdy;
    private float m_deactivisionTimeRdy;
    private float m_endTimeRdy;

    private int m_currentRepeats;
    protected int m_currentSeed;
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
        m_currentRepeats = 0;
        ReInitializeSequence();
    }
    public virtual void ReInitializeSequence()
    {
        m_isInitialized = true;
        m_activationTimeRdy = Time.time + GetRandom(m_preWeightTimeMin, m_preWeightTimeMax);
        m_endTimeRdy = float.MaxValue;
        ManageState();
    }
    private void ActivateSequence()
    {
        m_isActive = true;
        m_activationTimeRdy = float.MaxValue;
        m_deactivisionTimeRdy = (m_maxActiveTimeMin > 0 || m_maxActiveTimeMax > 0) ? Time.time + GetRandom(m_maxActiveTimeMin, m_maxActiveTimeMax) : float.MaxValue;
    }
    protected void DeactivateSequence()
    {
        m_isInactive = true;
        m_isActive = false;
        m_deactivisionTimeRdy = float.MaxValue;
        m_endTimeRdy = Time.time + GetRandom(m_postWeightTimeMin, m_postWeightTimeMax);
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
        {
            if (m_repeats <= 0)
                ReInitializeSequence();
            else if (m_currentRepeats >= m_repeats)
                m_spawnScript.RegisterEndSequence(this);
            else
            {
                m_currentRepeats++;
                ReInitializeSequence();
            }
        }
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

    #region Misc
    protected float GetRandom(float min, float max)
    {
        float random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    protected int GetRandom(int min, int max)
    {
        int random = 0;
        if (m_initSeed >= 0)
            random = Utility.GetRandomWithSeed(min, max, m_currentSeed++);
        else
            random = Random.Range(min, max);
        return random;
    }
    #endregion

    #region Setter
    public void SetSpawnScript(SpawnObstacles script)
    {
        m_spawnScript = script;
    }
    #endregion
}
