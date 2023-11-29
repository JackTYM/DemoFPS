using System.Collections;
using System.Collections.Generic;
using Fragsurf.Movement;
using Unity.VisualScripting;
using UnityEngine;
public class BulletCollider : MonoBehaviour
{
    public Weapon.WeaponType weaponType;
    public GameObject playerObject;
    void OnCollisionEnter(Collision collision)
    {
        switch (weaponType)
        {
            case Weapon.WeaponType.Shotgun:
                Destroy(gameObject);
                break;
            case Weapon.WeaponType.RPG:
                GetComponent<ParticleSystem>().Play();
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<MeshRenderer>().enabled = false;
                Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);

                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
                    float dist = Vector3.Distance(player.transform.position, transform.position);
                    if (dist < 10) {
                        // Closer player makes bigger jump
                        playerObject.GetComponent<SurfCharacter>()._moveData.velocity += -transform.right * (1 / dist * 300);
                        playerObject.GetComponent<SurfCharacter>()._controller.jumping = true;
                    }
                }
                break;
        }
    }
}
