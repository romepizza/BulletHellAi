using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement s_instance;
    [Header("------- Settings -------")]

    [Header("--- Key Binding ---")]
    [SerializeField] private KeyCode m_keyUp;
    [SerializeField] private KeyCode m_keyDown;
    [SerializeField] private KeyCode m_keyLeft;
    [SerializeField] private KeyCode m_keyRight;

    
    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: More than two instances of PlayerMovement have been found!");
        s_instance = this;
    }
    #endregion

    #region Input Control
    public float[] GenerateInputDataPlayer()
    {
        float[] input = new float[4];

        if (Input.GetKey(m_keyLeft))
            input[0] = 1;
        if (Input.GetKey(m_keyRight))
            input[1] = 1;
        if (Input.GetKey(m_keyUp))
            input[2] = 1;
        if (Input.GetKey(m_keyDown))
            input[3] = 1;

        return input;
    }
    #endregion

    #region Statics
    public static PlayerMovement Instance()
    {
        return s_instance;
    }
    #endregion
}
