using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictedArea : MonoBehaviour
{
    [Header("------- Settings ------")]
    [SerializeField] private float m_posXRelative;
    [SerializeField] private float m_negXRelative;
    [SerializeField] private float m_posYRelative;
    [SerializeField] private float m_negYRelative;
    [SerializeField] private bool m_showGizmos;
    [Header("------ Debug -------")]
    //[SerializeField] private Transform m_levelOrientation;
    [SerializeField] private Vector3 m_centerPosition;

    #region Mono
    private void Start()
    {
        m_centerPosition = transform.position + new Vector3(m_posXRelative + m_negXRelative, m_posYRelative + m_negYRelative, 0) * 0.25f;
    }
    #endregion

    #region OutOfBoundsChecks
    public bool IsOutOfRestrictionPosX(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        return localPosition.x > m_posXRelative;
    }
    public bool IsOutOfRestrictionNegX(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        return localPosition.x < m_negXRelative;
    }
    public bool IsOutOfRestrictionPosY(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        return localPosition.y > m_posYRelative;
    }
    public bool IsOutOfRestrictionNegY(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        return localPosition.y < m_negYRelative;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!m_showGizmos)
            return;

        Vector3 topLeft = transform.position + /*m_levelOrientation.rotation* */ new Vector3(m_negXRelative, m_posYRelative, 0);
        Vector3 topRight = transform.position +/* m_levelOrientation.rotation* */ new Vector3(m_posXRelative, m_posYRelative, 0);
        Vector3 botLeft = transform.position + /*m_levelOrientation.rotation **/ new Vector3(m_negXRelative, m_negYRelative, 0);
        Vector3 botRight = transform.position +/* m_levelOrientation.rotation* */ new Vector3(m_posXRelative, m_negYRelative, 0);

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
        Gizmos.DrawLine(botLeft, topLeft);
    }
    #endregion
}
