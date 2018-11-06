using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement s_instance;
    [Header("------- Settings -------")]
    [Header("--- Movement ---")]
    [SerializeField] private float m_movementSpeed;
    [SerializeField] private float m_slowFactor;
    [SerializeField] private bool m_allowUpDownMovement;

    [Header("--- Key Binding ---")]
    [SerializeField] private KeyCode m_keyUp;
    [SerializeField] private KeyCode m_keyDown;
    [SerializeField] private KeyCode m_keyLeft;
    [SerializeField] private KeyCode m_keyRight;
    [SerializeField] private KeyCode m_keySlow;

    [Header("--- Objects ---")]
    [SerializeField] private RestrictedArea m_restrictedArea;

    [Header("------- Debug -------")]
    [SerializeField] private Vector3 m_forceVector;
    [SerializeField] private Vector3 m_currentMoveDirectionLocal;

    [SerializeField] private bool m_isPressingUp;
    [SerializeField] private bool m_isPressingDown;
    [SerializeField] private bool m_isPressingLeft;
    [SerializeField] private bool m_isPressingRight;
    [SerializeField] private bool m_isPressingSlow;



    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: More than two instances of PlayerMovement have been found!");
        s_instance = this;
    }
    private void Update()
    {
        GetInput();
        GetForcVectorDc();
        ApplyForceDc();
    }
    #endregion

    #region Manage Movement
    void GetForcVectorDc()
    {
        m_forceVector = Vector3.zero;
        if (m_isPressingUp && !m_restrictedArea.IsOutOfRestrictionPosY(transform.position))
            m_forceVector.y += m_movementSpeed * (m_isPressingSlow ? m_slowFactor : 1);
        if (m_isPressingDown && !m_restrictedArea.IsOutOfRestrictionNegY(transform.position))
            m_forceVector.y -= m_movementSpeed * (m_isPressingSlow ? m_slowFactor : 1);
        if (m_isPressingLeft && !m_restrictedArea.IsOutOfRestrictionNegX(transform.position))
            m_forceVector.x -= m_movementSpeed * (m_isPressingSlow ? m_slowFactor : 1);
        if (m_isPressingRight && !m_restrictedArea.IsOutOfRestrictionPosX(transform.position))
            m_forceVector.x += m_movementSpeed * (m_isPressingSlow ? m_slowFactor : 1);

        m_forceVector *= Time.deltaTime;
    }
    void ApplyForceDc()
    {
        if (m_forceVector == Vector3.zero)
            return;

        transform.position += m_forceVector;
    }
    #endregion

    #region Input
    void GetInput()
    {
        m_isPressingUp = false;
        m_isPressingDown = false;
        m_isPressingLeft = false;
        m_isPressingRight = false;
        m_isPressingSlow = false;

        if (Input.GetKey(m_keyUp))
            m_isPressingUp = true;
        if (Input.GetKey(m_keyDown))
            m_isPressingDown = true;
        if (Input.GetKey(m_keyLeft))
            m_isPressingLeft = true;
        if (Input.GetKey(m_keyRight))
            m_isPressingRight = true;
        if (Input.GetKey(m_keySlow))
            m_isPressingSlow = true;

        if (!m_allowUpDownMovement)
            m_isPressingUp = m_isPressingDown = false;
    }
    public float[] GenerateInputData()
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

        if (!m_allowUpDownMovement)
            input[2] = input[3] = 0;

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
