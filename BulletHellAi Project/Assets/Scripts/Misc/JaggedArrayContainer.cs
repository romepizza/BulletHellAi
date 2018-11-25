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
    public JaggedArrayContainer(float[] data)
    {
        this.data = data;
    }
    public JaggedArrayContainer(JaggedArrayContainer[] array)
    {
        this.array = array;
    }
}
