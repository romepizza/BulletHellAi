using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;
    [SerializeField] private bool m_saveSamples;


    [Header("--- Screenshots ---")]
    [SerializeField] private TakeScreenshot m_screenshotScript;

    
    [Header("------ Debug ------")]
    bool b;
    public List<SampleContainer> m_samples { get; private set; }


    #region Enums
    public enum InputType { Screenshots, Raycasts, WorldInformation }
    #endregion

    #region Mono
    private void Awake()
    {
        m_samples = new List<SampleContainer>();
    }
    #endregion

    #region Sample Control
    public SampleContainer GenerateSample()
    {
        float[] input = GenerateInput();
        float[] desiredOutput = GenerateDesiredOutput();

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput);
        SaveSample(sampleContainer);

        return sampleContainer;
    }
    private float[] GenerateDesiredOutput()
    {
        return PlayerMovement.Instance().GenerateInputData();
    }
    #region Generate Input
    private float[] GenerateInput()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.Screenshots)
            input = GenerateInputScreenshot();
        else if (m_inputType == InputType.Raycasts)
            input = GenerateInputRaycast();
        else if (m_inputType == InputType.WorldInformation)
            input = GenerateInputWorldInformation();

        return input;
    }
    private float[] GenerateInputScreenshot()
    {
        float[] input = m_screenshotScript.GetScreenshotComputedData();

        return input;
    }
    private float[] GenerateInputRaycast()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    private float[] GenerateInputWorldInformation()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
        #endregion
    private void SaveSample(SampleContainer sampleContainer)
    {
        if(m_saveSamples)
            m_samples.Add(sampleContainer);
    }
    #endregion

    #region Getter
    public InputType GetInputType()
    {
        return m_inputType;
    }
    #endregion
}
