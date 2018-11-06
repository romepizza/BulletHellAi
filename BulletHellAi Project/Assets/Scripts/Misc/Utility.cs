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
    public static Component getComponentInParents<T>(Transform startTransform) where T : Component
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
}
