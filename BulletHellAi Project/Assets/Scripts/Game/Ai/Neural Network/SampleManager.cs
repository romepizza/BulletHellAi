﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    [Header("------ Settings ------")]
    [SerializeField] private InputType m_inputType;
    [SerializeField] private bool m_saveSamples;

    [Header("--- Filter ---")]
    [SerializeField] private bool m_tossNoOutputSamples;


    [Header("--- Screenshots ---")]
    [SerializeField] private TakeScreenshot m_screenshotScriptThis;
    [SerializeField] private TakeScreenshot m_screenshotScriptSource;

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
    public SampleContainer GenerateSampleSource()
    {
        float[] input = GenerateInputSource();
        float[] desiredOutput = GenerateDesiredOutput();
        bool isOkay = CheckFilter(input, desiredOutput);

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, isOkay);
        SaveSample(sampleContainer);

        return sampleContainer;
    }
    public SampleContainer GenerateSampleThis()
    {
        float[] input = GenerateInputThis();
        float[] desiredOutput = null;

        SampleContainer sampleContainer = new SampleContainer(input, desiredOutput, true);
        SaveSample(sampleContainer);

        return sampleContainer;
    }

    private float[] GenerateDesiredOutput()
    {
        return PlayerMovement.Instance().GenerateInputData();
    }
    private void SaveSample(SampleContainer sampleContainer)
    {
        if (m_saveSamples)
            m_samples.Add(sampleContainer);
    }
    #endregion

    #region Generate Input
    // source
    private float[] GenerateInputSource()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.Screenshots)
            input = GenerateInputSourceScreenshot();
        else if (m_inputType == InputType.Raycasts)
            input = GenerateInputSourceRaycast();
        else if (m_inputType == InputType.WorldInformation)
            input = GenerateInputSourceWorldInformation();

        return input;
    }
    private float[] GenerateInputSourceScreenshot()
    {
        float[] input = m_screenshotScriptSource.GetScreenshotComputedData();

        return input;
    }
    private float[] GenerateInputSourceRaycast()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    private float[] GenerateInputSourceWorldInformation()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }

    // this
    private float[] GenerateInputThis()
    {
        float[] input = new float[0];

        if (m_inputType == InputType.Screenshots)
            input = GenerateInputScreenshotThis();
        else if (m_inputType == InputType.Raycasts)
            input = GenerateInputRaycastThis();
        else if (m_inputType == InputType.WorldInformation)
            input = GenerateInputWorldInformationThis();

        return input;
    }
    private float[] GenerateInputScreenshotThis()
    {
        float[] input = m_screenshotScriptThis.GetScreenshotComputedData();

        return input;
    }
    private float[] GenerateInputRaycastThis()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    private float[] GenerateInputWorldInformationThis()
    {
        float[] input = new float[0];

        // TODO

        return input;
    }
    #endregion

    #region Layer Length
    public int GetInputLayerLengthDynamicly()
    {
        if (m_inputType == InputType.Screenshots)
            return GetInputLayerLengthDynamiclyScreenshot();
        else if (m_inputType == InputType.Raycasts)
            return GetInputLayerLengthDynamiclyRaycast();
        else if (m_inputType == InputType.WorldInformation)
            return GetInputLayerLengthDynamiclyWorldInfo();

        return -1;
    }
    public int GetInputLayerLengthDynamiclyScreenshot()
    {
        return ScreenshotManager.Instance().GetInputLayerLengthTotal();
    }
    public int GetInputLayerLengthDynamiclyRaycast()
    {
        return -1;
    }
    public int GetInputLayerLengthDynamiclyWorldInfo()
    {
        return -1;
    }
    #endregion

    #region Filter
    private bool CheckFilter(float[] input, float[] desiredOutput)
    {
        bool isOkay = true;

        if(m_tossNoOutputSamples)
        {
            isOkay = false;
            for(int i = 0; i < desiredOutput.Length; i++)
            {
                if (desiredOutput[i] != 0)
                    isOkay = true;
            }
        }

        return isOkay;
    }
    #endregion

    #region Getter
    public InputType GetInputType()
    {
        return m_inputType;
    }
    #endregion
}