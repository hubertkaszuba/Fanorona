using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour {
    //public static ButtonManager Instance { set; get; }
    public Transform currentMount;
    public bool set = false;
    public GameObject newBoard;
    public void Start()
    {

    }

    public void Update()
    {
        if (set == true)
        {
            transform.position = Vector3.Lerp(transform.position, currentMount.position, 0.03f);
            transform.rotation = Quaternion.Slerp(transform.rotation, currentMount.rotation, 0.03f);
        }
    }

    public void setMount(Transform newMount)
    {
        currentMount = newMount;
        set = true;
    }
}
