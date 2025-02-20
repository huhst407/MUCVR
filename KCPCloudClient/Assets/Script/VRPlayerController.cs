using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Valve.VR;

[System.Obsolete]
public class VRPlayerController : NetworkBehaviour {
    private void Awake() {


    }
    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) {
            transform.Find("Camera").gameObject.SetActive(false);
            transform.Find("right_hand").GetComponent<SteamVR_Behaviour_Skeleton>().enabled = false;
            transform.Find("left_hand").GetComponent<SteamVR_Behaviour_Skeleton>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
