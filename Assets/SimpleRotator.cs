using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {

  void Update() {
    float scale = 20F * Time.deltaTime;
    Quaternion rot = Quaternion.Euler(scale, scale, scale);
    this.transform.rotation *= rot;
  }

}
