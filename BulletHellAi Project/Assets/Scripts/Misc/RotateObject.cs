using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("------- Settings -------")]
    [SerializeField] private float m_rotationSpeed;
    [SerializeField] private Vector3 m_rotationAxis;
    [Header("------- Debug -------")]
    bool b;

    void Update()
    {
        if (m_rotationSpeed == 0)
            return;

        transform.Rotate(m_rotationAxis, m_rotationSpeed * Time.deltaTime);
    }
}
