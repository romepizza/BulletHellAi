using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JaggedArrayContainer
{
    public float[] data;
    public JaggedArrayContainer[] array;

    public JaggedArrayContainer()
    {

    }
    public JaggedArrayContainer(int dataLength, int arrayLenth)
    {
        data = new float[dataLength];
        array = new JaggedArrayContainer[arrayLenth];
    }
    public JaggedArrayContainer(float[] data)
    {
        this.data = data;
    }
    public JaggedArrayContainer(JaggedArrayContainer[] array)
    {
        this.array = array;
    }
}
