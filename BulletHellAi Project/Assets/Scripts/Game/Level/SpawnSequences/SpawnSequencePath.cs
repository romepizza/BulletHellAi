using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequencePath : SpawnSequence
{
    #region Member Variables
    [Header("------- Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private int m_gateWidth;
    [SerializeField] private bool m_waitOnTurn;
    [SerializeField] private bool m_addAdditionalSpace;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;

    [Header("--- Cooldown ---")]
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;
    [SerializeField] private float m_curveTimeRange;
    [SerializeField] private AnimationCurve m_cooldownCurve;

    [Header("--- Deactivision ---")]
    [SerializeField] private bool m_startLeftToRight;
    [SerializeField] private bool m_startRightToLeft;
    [SerializeField] private bool m_stopAtLeft;
    [SerializeField] private bool m_stopAtRight;

    [Header("------- Debug -------")]
    [SerializeField] private float m_cooldownRdyTime;
    //private int m_index = 1;
    [SerializeField] private int m_direction = 1;
    private float m_sequenceActiveTime;
    
    private List<int> m_currentIndices;
    private bool m_turned;
    #endregion

    public override List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        if (!m_isActive)
            return null;

        if (base.CheckDeactivateSequence() || CheckDeactivateSequence())
        {
            DeactivateSequence();
            return null;
        }

        m_sequenceActiveTime += Time.deltaTime;
        if (m_cooldownRdyTime > Time.time)
            return null;
        float curveFactor = m_curveTimeRange > 0 ? m_cooldownCurve.Evaluate(m_sequenceActiveTime / m_curveTimeRange) : 1;
        m_cooldownRdyTime = Time.time + GetRandom(m_cooldownMin * curveFactor, m_cooldownMax * curveFactor);

        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;

        if(m_waitOnTurn && m_turned)
        {
            ;
        }
        else if (m_direction < 0)
        {
            m_currentIndices.Insert(0, m_currentIndices[0] - 1);
            m_currentIndices.RemoveAt(m_currentIndices.Count - 1);
        }
        else if (m_direction > 0)
        {
            m_currentIndices.Add(m_currentIndices[m_currentIndices.Count - 1] + 1);
            m_currentIndices.RemoveAt(0);
        }
        else
            Debug.Log("Oops!");

        bool wasTurned = m_turned;
        int oldDirection = m_direction;
        if(m_waitOnTurn && m_turned)
        {
            m_turned = false;
        }
        if (m_direction < 0 && m_currentIndices[0] <= 0)
        {
            m_direction = 1;

            //Debug.Log("Turn");
            if (m_waitOnTurn)
                m_turned = true;

            if (m_stopAtLeft)
            {
                DeactivateSequence();
                return null;
            }
        }
        else if (m_direction > 0 && m_currentIndices[m_currentIndices.Count - 1] >= width - 1)
        {
            m_direction = -1;

            //Debug.Log("Turn");
            if (m_waitOnTurn)
                m_turned = true;

            if (m_stopAtRight)
            {
                DeactivateSequence();
                return null;
            }
        }

        if (m_addAdditionalSpace && !(m_waitOnTurn && wasTurned))
        {
            if (oldDirection < 0)
            {
                //Debug.Log("Added " + (m_currentIndices[m_currentIndices.Count - 1] + 1) + " at end");
                m_currentIndices.Add(m_currentIndices[m_currentIndices.Count - 1] + 1);
            }
            else if (oldDirection > 0)
            {
                //Debug.Log("Added " + (m_currentIndices[0] - 1) + " at start");
                m_currentIndices.Insert(0, m_currentIndices[0] - 1);
            }
        }

        List<Sequence> sequences = new List<Sequence>();
        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;

        for (int i = 0; i < width; i++)
        {
            if (m_currentIndices.Contains(i))
                continue;

            Sequence sequence = new Sequence();

            Vector3 position = neg - (neg.x - pos.x) * new Vector3((0.5f + i) / width, 0, 0);
            sequence.position = position;
            sequence.velocity = new Vector3(GetRandom(m_directionMin.x, m_directionMax.x), GetRandom(m_directionMin.y, m_directionMax.y), GetRandom(m_directionMin.z, m_directionMax.z));

            sequences.Add(sequence);
        }

        if (m_addAdditionalSpace && !(m_waitOnTurn && wasTurned))
        {
            if (oldDirection < 0)
            {
                //Debug.Log("Removed " + (m_currentIndices[m_currentIndices.Count - 1]) + " at end");
                m_currentIndices.RemoveAt(m_currentIndices.Count - 1);
            }
            else if (oldDirection > 0)
            {
                //Debug.Log("Removed " + (m_currentIndices[0]) + " at start");
                m_currentIndices.RemoveAt(0);
            }
        }

        return sequences;
    }

    public override bool CheckDeactivateSequence()
    {
        return false;
    }
    public override void InitializeSequence()
    {
        //m_cooldownRdyTime = 0;
        //m_sequenceActiveTime = 0;
        m_currentIndices = new List<int>();
        //m_turned = false;

        //if (m_startLeftToRight)
        //{
        //    for (int i = 0; i < m_gateWidth; i++)
        //        m_currentIndices.Add(i - 1);
        //    m_direction = 1;
        //}
        //else
        //{
        //    int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
        //    for (int i = width - m_gateWidth; i < width; i++)
        //        m_currentIndices.Add(i + 1);
        //    m_direction = -1;
        //}
        base.InitializeSequence();
    }
    public override void ReInitializeSequence()
    {
        m_cooldownRdyTime = 0;
        m_sequenceActiveTime = 0;
        //m_currentIndices = new List<int>();
        m_currentIndices.Clear();
        m_turned = false;

        if (m_startLeftToRight)
        {
            for (int i = 0; i < m_gateWidth; i++)
                m_currentIndices.Add(i - 1);
            m_direction = 1;
        }
        else
        {
            int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
            for (int i = width - m_gateWidth; i < width; i++)
                m_currentIndices.Add(i + 1);
            m_direction = -1;
        }
        base.ReInitializeSequence();
    }
}
