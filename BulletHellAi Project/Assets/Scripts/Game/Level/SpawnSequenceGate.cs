using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceGate : SpawnSequence
{
    [Header("------- This Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private List<int> m_gateFromLeft;
    [SerializeField] private List<int> m_gateFromRight;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;

    [Header("------- Debug -------")]
    private bool m_deactivateNextFrame;

    public override List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        if (!m_isActive)
            return null;

        if (base.CheckDeactivateSequence() || CheckDeactivateSequence())
        {
            DeactivateSequence();
            return null;
        }
        
        List<Sequence> sequences = new List<Sequence>();

        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;
        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;

        for (int i = 0; i < width; i++)
        {
            if (m_gateFromLeft.Contains(i) || m_gateFromRight.Contains(width - i - 1))
                continue;

            Vector3 position = neg - (neg.x - pos.x) * new Vector3((0.5f + i) / width, 0, 0);

            Sequence sequence = new Sequence();
            sequence.position = position;
            sequence.velocity = new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));
            sequences.Add(sequence);
        }

        m_deactivateNextFrame = true;
        return sequences;
    }
    public override bool CheckDeactivateSequence()
    {
        if (m_deactivateNextFrame)
            return true;
        return false;
    }
    public override void InitializeSequence()
    {
        m_deactivateNextFrame = false;

        base.InitializeSequence();
    }
}
