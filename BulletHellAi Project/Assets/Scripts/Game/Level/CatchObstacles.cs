using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchObstacles : MonoBehaviour {

    private void OnTriggerEnter(Collider collider)
    {
        IsObstacle obstacleScript = (IsObstacle)Utility.getComponentInParents<IsObstacle>(collider.transform);// collider.GetComponent<IsObstacle>();
        if (obstacleScript == null)
            return;

        obstacleScript.DestroySelf();
    }
}
