using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleContainer
{
    public float[] m_input { get; private set; }
    public float[] m_desiredOutput { get; private set; }
    public bool m_isOkay { get; private set; }
    

    public SampleContainer(float[] input, float[] desiredOutput, bool isOkay)
    {
        m_input = input;
        m_desiredOutput = desiredOutput;
        m_isOkay = isOkay;
    }
}
