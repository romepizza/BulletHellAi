using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    private static PlayerMovementManager s_instance;

    [Header("------- Settings -------")]
    [SerializeField] private ControllerType m_controllerType;
    [SerializeField] private bool m_allowUpDownMovement;

    [Header("--- Movement ---")]
    [SerializeField] private float m_movementSpeed;

    [Header("--- Objects ---")]
    [SerializeField] private RestrictedArea m_restrictedArea;

    [Header("------ Debug -------")]
    bool b;
    private Vector3 m_forceVector;
    private Vector3 m_currentMoveDirectionLocal;

    #region Enums
    private enum ControllerType { Player, Ai }
    #endregion

    #region Mono
    private void Awake()
    {
        if (s_instance != null)
            Debug.Log("Warning: Seems like more than one instance of PlayerMovementManager is running!");
        s_instance = this;
    }
    void Start ()
    {
		
	}
	void FixedUpdate ()
    {
        GetForcVectorDc();
        ApplyForceDc();
    }
    #endregion

    #region Manage Movement
    void GetForcVectorDc()
    {
        float[] inputData = GenerateInputData();

        bool isPressingLeft = inputData[0] == 1;
        bool isPressingRight = inputData[1] == 1;
        bool isPressingUp = false;
        bool isPressingDown = false;
        if (inputData.Length > 2)
        {
           isPressingUp = inputData[2] == 1;
           isPressingDown = inputData[3] == 1;
        }

        m_forceVector = Vector3.zero;
        if (isPressingLeft && !m_restrictedArea.IsOutOfRestrictionNegX(transform.position))
            m_forceVector.x -= m_movementSpeed;// * (m_isPressingSlow ? m_slowFactor : 1);
        if (isPressingRight && !m_restrictedArea.IsOutOfRestrictionPosX(transform.position))
            m_forceVector.x += m_movementSpeed;// * (m_isPressingSlow ? m_slowFactor : 1);
        if (inputData.Length > 2)
        {
            if (isPressingUp && !m_restrictedArea.IsOutOfRestrictionPosY(transform.position))
                m_forceVector.y += m_movementSpeed;// * (m_isPressingSlow ? m_slowFactor : 1);
            if (isPressingDown && !m_restrictedArea.IsOutOfRestrictionNegY(transform.position))
                m_forceVector.y -= m_movementSpeed;// * (m_isPressingSlow ? m_slowFactor : 1);
        }

        m_forceVector *= Time.deltaTime;
    }
    void ApplyForceDc()
    {
        if (m_forceVector == Vector3.zero)
            return;

        transform.position += m_forceVector;
    }
    #endregion

    #region Input Control
    public float[] GenerateInputData()
    {
        float[] inputData = null;

        if (m_controllerType == ControllerType.Player)
            inputData = PlayerMovement.Instance().GenerateInputDataPlayer();
        else if (m_controllerType == ControllerType.Ai)
            inputData = PlayerAiMovement.Instance().GenerateInputData();

        if (inputData.Length > 2 && !m_allowUpDownMovement)
            inputData[2] = inputData[3] = 0;

        return inputData;
    }
    #endregion

    #region Statics
    public static PlayerMovementManager Instance()
    {
        return s_instance;
    }
    #endregion
}
