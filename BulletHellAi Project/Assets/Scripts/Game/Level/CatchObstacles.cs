using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchObstacles : MonoBehaviour {

    private void OnTriggerEnter(Collider collider)
    {
        IsObstacle obstacleScript = collider.GetComponent<IsObstacle>();
        if (obstacleScript == null)
            return;

        obstacleScript.DestroySelf();
    }
}
