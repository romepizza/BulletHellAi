using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceRandom : SpawnSequence
{
    [Header("------- Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private bool m_spawnForScreenshot;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;

    [Header("------- Debug -------")]
    private float m_cooldownRdyTime;


    public override List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        if (m_cooldownRdyTime > Time.time)
            return null;
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin, m_cooldownMax);

        Sequence sequence = new Sequence();


        Vector3 pos = transform.position + spawnPositionPos;
        Vector3 neg = transform.position + spawnPositionNeg;

        Vector3 randomPosition = Vector3.zero;
        if (m_spawnForScreenshot)
        {
            int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
            randomPosition = neg + (pos - neg) * (0.5f + Random.Range(0, width))/ width;// new Vector3(m_spawnScript.GetScreenshotScript().GetPixelToWorldScale(0) * (0.5f + Random.Range(0, width)), 0, 0);
        }
        else
            randomPosition = (pos + neg) * 0.5f + (Random.Range(-0.5f, 0.5f) * (neg - pos));

        sequence.position = randomPosition;
        sequence.velocity = new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));

        List<Sequence> sequences = new List<Sequence>();
        sequences.Add(sequence);

        return sequences;
    }
}
