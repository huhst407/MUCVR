﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Obsolete]
public class VRPlayerController : NetworkBehaviour {
    private void Awake() {
        if(!isLocalPlayer) {
            transform.Find("Camera").gameObject.SetActive(false);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
