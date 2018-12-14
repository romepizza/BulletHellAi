using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JaggedArrayContainer
{
    public float[] dataFloat;
    public bool[] dataBool;
    public JaggedArrayContainer[] array;

    public JaggedArrayContainer()
    {

    }
    public JaggedArrayContainer(int dataLength, int arrayLenth)
    {
        dataFloat = new float[dataLength];
        array = new JaggedArrayContainer[arrayLenth];
    }
    public JaggedArrayContainer(float[] data)
    {
        this.dataFloat = data;
    }
    public JaggedArrayContainer(bool[] data)
    {
        this.dataBool = data;
    }
    public JaggedArrayContainer(JaggedArrayContainer[] array)
    {
        this.array = array;
    }
}
