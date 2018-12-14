using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceRandom : SpawnSequence
{
    [Header("------- This Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private bool m_spawnForScreenshot;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;

    [Header("--- Cooldown ---")]
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;
    [SerializeField] private float m_curveTimeRange;
    [SerializeField] private AnimationCurve m_cooldownCurve;

    [Header("--- Random Seed ---")]
    [SerializeField] private bool m_useSeed;
    [SerializeField] private int m_seed;
    [SerializeField] private int m_seedStep = 1;

    [Header("------- Debug -------")]
    private float m_cooldownRdyTime;
    private float m_sequenceActiveTime;
    private int m_currentSeed;
    //private Random.State m_randomState;

    #region Mono
    public void Awake()
    {
        m_currentSeed = m_seed; 
    }
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
        Random.State oldSate = Random.state;
        if(m_useSeed)
            Random.InitState(m_currentSeed);
        float curveFactor = m_curveTimeRange > 0 ? m_cooldownCurve.Evaluate(m_sequenceActiveTime / m_curveTimeRange) : 1;
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin * curveFactor, m_cooldownMax * curveFactor);

        Sequence sequence = new Sequence();

        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;

        Vector3 randomPosition = Vector3.zero;
        if (m_spawnForScreenshot)
        {
            int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
            randomPosition = neg + (pos - neg) * (0.5f + Random.Range(0, width))/ width;
        }
        else
            randomPosition = (pos + neg) * 0.5f + (Random.Range(-0.5f, 0.5f) * (neg - pos));

        sequence.position = randomPosition;
        sequence.velocity = new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));

        List<Sequence> sequences = new List<Sequence>();
        sequences.Add(sequence);

        m_currentSeed += m_seedStep;
        Random.state = oldSate;
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

        base.InitializeSequence();
    }
}
