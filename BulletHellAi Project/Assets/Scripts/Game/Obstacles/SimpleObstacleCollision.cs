using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObstacleCollision : MonoBehaviour {

    [SerializeField] private IsObstacle m_obstacleScript;
    private void OnTriggerEnter(Collider collider)
    {
        GameObject colliderObject = collider.gameObject;
        IsPlayer isPlayerScript = colliderObject.GetComponent<IsPlayer>();
        if (isPlayerScript == null)
            return;

        m_obstacleScript.GetLevelOptions().Die();
        m_obstacleScript.DestroySelf();
    }
    private void OnCollisionEnter(Collision collision)
    {
        GameObject colliderObject = collision.gameObject;
        IsPlayer isPlayerScript = colliderObject.GetComponent<IsPlayer>();
        if (isPlayerScript == null)
            return;

        m_obstacleScript.GetLevelOptions().Die();
        m_obstacleScript.DestroySelf(); 
    }
}
