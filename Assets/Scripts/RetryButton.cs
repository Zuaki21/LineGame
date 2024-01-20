using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetryButton : MonoBehaviour
{
    //Rでリトライ
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //現在のシーンを再読み込みする
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
