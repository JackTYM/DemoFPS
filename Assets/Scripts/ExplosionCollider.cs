using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCollider : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(Vector3.Distance(transform.position, collision.transform.position));
    }
}
