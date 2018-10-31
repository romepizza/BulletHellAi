using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static float MapValuePercent(float minValue, float maxValue, float value)
    {
        float v = 0;

        float range = maxValue - minValue;
        v = (minValue + value) / range;

        return v;
    }
}
