using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountCuttables : MonoBehaviour
{
    [SerializeField] private int cuttableCount;
    void Update()
    {
        //Tag"Cuttable"のついたオブジェクトの数を取得
        cuttableCount = GameObject.FindGameObjectsWithTag("Cuttable").Length;
    }
}
