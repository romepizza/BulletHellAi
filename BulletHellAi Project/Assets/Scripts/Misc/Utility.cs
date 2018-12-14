using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static float MapValuePercent(float minValue, float maxValue, float value)
    {
        if (minValue == maxValue)
            return 0.5f;

        if(minValue > maxValue)
        {
            Debug.Log("Warning: minValue > maxValue! Values have been swapped!");
            float min = minValue;
            minValue = maxValue;
            maxValue = min;
        }

        float v = 0;

        float range = maxValue - minValue;
        v = (value - minValue) / range;

        return v;
    }
    //public static Component getComponentInParent<T>(Transform transform) where T : Component
    //{
    //    Component component = transform.GetComponent(typeof(T));
    //    if (component != null)
    //        return component;

    //    Transform parent = transform.parent;
    //    if (parent == null)
    //        return null;

    //    component = parent.GetComponent(typeof(T));

    //    return component;
    //}
    public static Component GetComponentInParents<T>(Transform startTransform) where T : Component
    {
        Transform currentTransform = startTransform;
        Component component = currentTransform.GetComponent(typeof(T));
        while (currentTransform != null && component == null)
        {
            currentTransform = currentTransform.parent;
            if (currentTransform == null)
                continue;

            component = currentTransform.GetComponent(typeof(T));
        }

        return component;
    }
    public static int ExpInt(int b, int e)
    {
        if (e == 0)
            return b;
        else if (e < 0)
        {
            Debug.Log("Warning: ExpInt doesn't take negative exponentials at the moment!");
            return b;
        }

        int result = b;
        for(int i = 0; i < e - 1; i++)
        {
            result *= b;
        }

        return result;
    }
}
