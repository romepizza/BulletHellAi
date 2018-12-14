using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceLeftRight : SpawnSequence
{
    [Header("------- Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private int m_ignoreNumber;
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
    private float m_cooldownRdyTime;
    private int m_index = 1;
    private int m_direction = 1;
    private float m_sequenceActiveTime;


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
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin * curveFactor, m_cooldownMax * curveFactor);


        bool dontSpawn = false;

        m_index += m_direction;
        int index = m_index;
        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
        if (m_direction < 0 && m_index <= m_ignoreNumber - 1)
        {
            dontSpawn = true;
        }
        if (m_direction > 0 && m_index >= width - m_ignoreNumber)
        {
            dontSpawn = true;
        }
        if (m_direction < 0 && m_index <= 0)
        {
            if(m_stopAtLeft)
            {
                DeactivateSequence();
                return null;
            }
            m_direction = 1;
            index = 0;
            m_index -= m_direction;
        }
        if (m_direction > 0 && m_index >= width - 1)
        {
            if (m_stopAtRight)
            {
                DeactivateSequence();
                return null;
            }
            m_direction = -1;
            index = width - 1;
            m_index -= m_direction;
        }

        if (dontSpawn)
            return null;


        Sequence sequence = new Sequence();

        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;

        Vector3 position = Vector3.zero;
        position = neg - (neg.x - pos.x) * new Vector3((0.5f + index) / width, 0, 0);


        sequence.position = position;
        sequence.velocity = new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));

        List<Sequence> sequences = new List<Sequence>();
        sequences.Add(sequence);

        return sequences;
    }

    public override bool CheckDeactivateSequence()
    {
        return false;
    }
    public override void InitializeSequence()
    {
        m_cooldownRdyTime = 0;
        m_sequenceActiveTime = 0;

        if (m_startLeftToRight)
        {
            m_index = -1;
            m_direction = 1;
        }
        else
        {
            m_index = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
            m_direction = -1;
        }
        base.InitializeSequence();
    }
}
