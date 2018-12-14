using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SampleContainer
{
    public float[] m_input { get; private set; }
    public float[] m_desiredOutput { get; private set; }
    public bool m_isOkay { get; private set; }
    public bool[] m_filters { get; private set; }
    

    public SampleContainer(float[] input, float[] desiredOutput, bool[] filters)
    {
        m_input = input;
        m_desiredOutput = desiredOutput;
        m_filters = filters;
        m_isOkay = true;
    }
    public SampleContainer(bool isOkay)
    {
        m_isOkay = false;
    }
}
