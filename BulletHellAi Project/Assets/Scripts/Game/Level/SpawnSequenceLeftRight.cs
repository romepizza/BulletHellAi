using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequenceLeftRight : SpawnSequence
{
    [Header("------- Settings -------")]
    [SerializeField] private int m_width;
    [SerializeField] private Vector3 m_directionMin;
    [SerializeField] private Vector3 m_directionMax;
    [SerializeField] private float m_cooldownMin;
    [SerializeField] private float m_cooldownMax;

    [Header("------- Debug -------")]
    private float m_cooldownRdyTime;
    [SerializeField] private int m_index = 1;
    private int m_direction = 1;


    public override List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        if (m_cooldownRdyTime > Time.time)
            return null;
        m_cooldownRdyTime = Time.time + Random.Range(m_cooldownMin, m_cooldownMax);


        bool dontSpawn = false;

        m_index += m_direction;
        int index = m_index;
        int width = m_width <= 0 ? m_spawnScript.GetScreenshotScript().GetCaptureWidth() : m_width;
        if (m_direction < 0 && m_index <= 0)
        {
            dontSpawn = true;
        }
        if (m_direction > 0 && m_index >= width - 1)
        {
            dontSpawn = true;
        }
        if (m_direction < 0 && m_index <= 0)
        {
            m_direction = 1;
            index = 0;
            m_index -= m_direction;
        }
        if (m_direction > 0 && m_index >= width - 1)
        {
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
        position = neg - (neg.x - pos.x) * new Vector3( (float)(0.5f + index) / (width), 0, 0);// + new Vector3(0.5f * 1 / width, 0, 0);


        sequence.position = position;
        sequence.velocity = new Vector3(Random.Range(m_directionMin.x, m_directionMax.x), Random.Range(m_directionMin.y, m_directionMax.y), Random.Range(m_directionMin.z, m_directionMax.z));

        List<Sequence> sequences = new List<Sequence>();
        sequences.Add(sequence);

        return sequences;
    }
}
