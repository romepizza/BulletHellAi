using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleGenerationBase : MonoBehaviour
{
    [Header("----- Base Settings -----")]
    [SerializeField] private int m_initSeed;
    [SerializeField] protected float m_selfDistribution;
    [SerializeField] protected ScreenshotManager m_screenshotManager; // needed for the output number
    
    protected List<SampleContainer> m_data = new List<SampleContainer>();
    private int m_currentSeed;

    private void Awake()
    {
        m_currentSeed = m_initSeed;
    }
    public virtual List<SampleContainer> GenerateSamples(int width, int height, int obstacleLength, int playerLength)
    {
        return null;
    }
    public SampleContainer GetRandomSample()
    {
        if(m_data == null || m_data.Count == 0)
        {
            Debug.Log("Warning: m_data was corrupt!");
            return new SampleContainer(false);
        }
        int randomIndex = Random.Range(0, m_data.Count);
        if (m_initSeed >= 0)
            randomIndex = Utility.GetRandomWithSeed(0, m_data.Count, m_currentSeed++);
        return m_data[randomIndex];
    }
    public float GetDistribution()
    {
        return m_selfDistribution;
    }
}
