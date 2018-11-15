using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSequence : MonoBehaviour
{
    protected SpawnObstacles m_spawnScript;

    public struct Sequence
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    public virtual List<Sequence> ManageSequence(Vector3 spawnPositionPos, Vector3 spawnPositionNeg)
    {
        List<Sequence> sequence = new List<Sequence>();
        Debug.Log("Warning!");
        return sequence;
    }

    public void SetSpawnScript(SpawnObstacles script)
    {
        m_spawnScript = script;
    }
}
