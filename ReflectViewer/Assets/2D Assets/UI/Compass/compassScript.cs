using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class compassScript : MonoBehaviour {


    public Transform playerTransform;
    public float compensateValue;
    public bool flipY;
    private Vector3 dir;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update () {

        if (playerTransform == null)
        {
            return;
        }

        dir.z = playerTransform.eulerAngles.y;
        if (flipY)
        {
            dir.z = -dir.z;
        }
        dir.z += compensateValue;
        transform.localEulerAngles = dir;
	}
}
