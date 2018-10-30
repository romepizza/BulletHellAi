using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static float GetSigmoid(float rawValue)
    {
        return 1f / (1f + Mathf.Exp(-rawValue));
    }
    public static float GetSigmoidPrime(float rawValue)
    {
        float sigmoid = GetSigmoid(rawValue);
        return sigmoid * (1 - sigmoid);
    }
}
