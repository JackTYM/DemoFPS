using System;
using System.Collections.Generic;
using Fragsurf.Movement;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Linq;

[Serializable]
public class MeshDictionaryItem
{
    public Weapon.WeaponType meshType;
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation;
    public GameObject bullet;
    public float bulletSpeed = 5f;
    public float bulletTime = 1f;
    public AnimationClip reload;
    public bool jump = false;
    public float jumpAmount = 0f;
    public int clip;
}

public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        Shotgun,
        RPG
    }

    public WeaponType weapon;
    public List<MeshDictionaryItem> meshDictionaryList = new List<MeshDictionaryItem>();
    public bool inAnim = false;
    public Dictionary<WeaponType, int> bulletsLeft = new Dictionary<WeaponType, int>();

    private Dictionary<WeaponType, MeshDictionaryItem> meshDictionary;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        BuildDictionaries();

        SetWeapon(weapon);
    }

    // Update is called once per frame
    void Update()
    {
        transform.parent.GetChild(1).GetChild(0).GetComponent<TMPro.TMP_Text>().text = bulletsLeft[weapon] + "/" + meshDictionary[weapon].clip;
    }

    void FixedUpdate()
    {

    }

    public void SlotOne(InputAction.CallbackContext context) {
        if (context.performed && !inAnim) {
            SetWeapon(WeaponType.Shotgun);
        }
    }

    public void SlotTwo(InputAction.CallbackContext context) {
        if (context.performed && !inAnim) {
            SetWeapon(WeaponType.RPG);
        }
    }

    public void Reload(InputAction.CallbackContext context)
    {
        if (context.performed && !inAnim && bulletsLeft[weapon] != meshDictionary[weapon].clip)
        {
            inAnim = true;
            anim.Play("Reload", -1, 0);
            Invoke("ReloadBullets", anim.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name.Contains("Reload")).length);
            Invoke("EndAnimation", anim.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name.Contains("Reload")).length);
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (bulletsLeft[weapon] == 0)
        {
            Reload(context);
        }
        else
        {
            if (context.performed && !inAnim)
            {
                GameObject bullet = Instantiate(meshDictionary[weapon].bullet, transform.position, transform.rotation);
                bullet.transform.eulerAngles = meshDictionary[weapon].bullet.transform.eulerAngles + transform.eulerAngles;
                bullet.GetComponent<Rigidbody>().AddForce(transform.right * meshDictionary[weapon].bulletSpeed, ForceMode.VelocityChange);
                bullet.GetComponent<BulletCollider>().playerObject = transform.parent.parent.parent.gameObject;

                if (meshDictionary[weapon].jump)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(bullet.transform.position, transform.right, out hit, 15))
                    {
                        transform.parent.parent.parent.GetComponent<SurfCharacter>()._moveData.velocity += -transform.right * meshDictionary[weapon].jumpAmount;
                        transform.parent.parent.parent.GetComponent<SurfCharacter>()._controller.jumping = true;
                    }
                }

                Destroy(bullet, meshDictionary[weapon].bulletTime);
                bulletsLeft[weapon]--;

                if (bulletsLeft[weapon] > 0)
                {
                    inAnim = true;
                    anim.Play("Cockback", -1, 0);
                    Invoke("EndAnimation", anim.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name.Contains("Cockback")).length);
                }
            }
        }
    }

    void SetWeapon(WeaponType type)
    {
        weapon = type;
        transform.localPosition = meshDictionary[weapon].position;
        transform.localEulerAngles = meshDictionary[weapon].rotation;
        foreach (Transform t in transform) {
            if (!t.gameObject.name.Contains(type.ToString())) {
                t.gameObject.SetActive(false);
            } else {
                t.gameObject.SetActive(true);
                anim = t.gameObject.GetComponent<Animator>();
            }
        }
    }

    void BuildDictionaries()
    {
        meshDictionary = new Dictionary<WeaponType, MeshDictionaryItem>();

        foreach (var item in meshDictionaryList)
        {
            meshDictionary[item.meshType] = item;
            bulletsLeft[item.meshType] = item.clip;

            Instantiate(item.prefab, transform);
        }
    }

    void EndAnimation()
    {
        inAnim = false;
    }

    void ReloadBullets()
    {
        bulletsLeft[weapon] = meshDictionary[weapon].clip;
    }
}
