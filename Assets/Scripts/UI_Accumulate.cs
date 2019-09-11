using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Accumulate : MonoBehaviour{

    public EasyTween viewPan;
    public EasyTween[] hidePan;

    void Start() {

    }

    void Update() {

    }

    public void onClick_Accumulate() {
        if (!viewPan.IsObjectOpened()) {
            viewPan.OpenCloseObjectAnimation();
        }
        for (int i = 0; i < hidePan.Length; i++) {
            if (hidePan[i].IsObjectOpened()) {
                hidePan[i].OpenCloseObjectAnimation();
            }
        }

    }
}
