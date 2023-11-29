using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void OnParticleCollision(GameObject other)
    {
        // Do something when a particle collides with this object
        Debug.Log("Particle collided with: " + other.name);
    }
}
