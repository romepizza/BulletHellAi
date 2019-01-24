using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceArrow : SpawnSequence {

    [Header("------- This Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private List<int> m_gateFromLeft;
    [SerializeField] private List<int> m_gateFromRight;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;

    [Header("--- Cooldown ---")]
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;
    [SerializeField] private float m_curveTimeRange;
    [SerializeField] private AnimationCurve m_cooldownCurve;

    [Header("------- Debug -------")]
    //private bool m_deactivateNextFrame;
    private float m_sequenceActiveTime;
    private float m_cooldownRdyTime;
    private int m_currentIndex;

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


        List<Sequence> sequences = new List<Sequence>();

        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;
        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;

        int indexPos = m_currentIndex;
        int indexNeg = width - m_currentIndex - 1;

        if (!m_gateFromLeft.Contains(m_currentIndex))
        {
            Vector3 position = neg - (neg.x - pos.x) * new Vector3((0.5f + indexPos) / width, 0, 0);

            Sequence sequence = new Sequence();
            sequence.position = position;
            sequence.velocity = new Vector3(GetRandom(m_directionMin.x, m_directionMax.x), GetRandom(m_directionMin.y, m_directionMax.y), GetRandom(m_directionMin.z, m_directionMax.z));
            sequences.Add(sequence);
        }

        if(!m_gateFromRight.Contains(m_currentIndex))
        {
            Vector3 position = neg - (neg.x - pos.x) * new Vector3((0.5f + indexNeg) / width, 0, 0);

            Sequence sequence = new Sequence();
            sequence.position = position;
            sequence.velocity = new Vector3(GetRandom(m_directionMin.x, m_directionMax.x), GetRandom(m_directionMin.y, m_directionMax.y), GetRandom(m_directionMin.z, m_directionMax.z));
            sequences.Add(sequence);
        }

        m_currentIndex++;

        return sequences;
    }
    public override bool CheckDeactivateSequence()
    {
        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
        if (m_currentIndex >= width / 2)
            return true;

        return false;
    }
    public override void InitializeSequence()
    {
        //m_currentIndex = 0;
        //m_cooldownRdyTime = 0;
        //m_sequenceActiveTime = 0;
        //m_deactivateNextFrame = false;

        base.InitializeSequence();
    }
    public override void ReInitializeSequence()
    {
        m_currentIndex = 0;
        m_cooldownRdyTime = 0;
        m_sequenceActiveTime = 0;
        //m_deactivateNextFrame = false;

        base.ReInitializeSequence();
    }
}
