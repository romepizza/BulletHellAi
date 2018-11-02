using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;

    
    [Header("------ Debug ------")]
    bool b;


    #region Enums
    public enum InputType { screenshots, raycasts, worldInformation }
    #endregion


    public List<SampleContainer> m_samples { get; private set; }

    #region Sample Control
    public SampleContainer GenerateSample()
    {
        float[] input = GenerateInput();
        float[] desiredOutput = GenerateDesiredOutput();

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput);
        m_samples.Add(sampleContainer);

        return sampleContainer;
    }
    private float[] GenerateDesiredOutput()
    {
        return PlayerMovement.Instance().GenerateInputData();
    }
    private float[] GenerateInput()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.screenshots)
            input = GenerateInputScreenshot();
        else if (m_inputType == InputType.raycasts)
            input = GenerateInputRaycast();
        else if (m_inputType == InputType.worldInformation)
            input = GenerateInputWorldInformation();

        return input;
    }
    private float[] GenerateInputScreenshot()
    {
        float[] input = new float[0];

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

    #region Getter
    public InputType GetInputType()
    {
        return m_inputType;
    }
    #endregion
}
