using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WaterDestoyer : MonoBehaviour
{
    //Tag:WaterParticleのオブジェクトがぶつかったら消す
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "WaterParticle")
        {
            //Rigidbody2Dを取得してあれば削除する
            Rigidbody2D rigidbody2D = other.gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                Destroy(rigidbody2D);
                //CircleCollider2Dを削除する
                Destroy(other.gameObject.GetComponent<CircleCollider2D>());

                //小さくしてから消す
                other.transform.DOScale(0, 0.5f).SetEase(Ease.InSine).OnComplete(() => Destroy(other.gameObject));
            }
        }
    }
}
