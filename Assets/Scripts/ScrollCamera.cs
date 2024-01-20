using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCamera : MonoBehaviour
{
    //マウスホイールでカメラを上下移動させる
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += new Vector3(0, scroll * 10, 0);
    }
}
