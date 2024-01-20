using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class WaterGenerator : MonoBehaviour
{
    List<GameObject> waterParticleList = new List<GameObject>();
    Coroutine generateWaterCoroutine;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateWater(transform, 100);
        }
    }

    void GenerateWater(Transform transform, int count)
    {
        if (generateWaterCoroutine != null)
        {
            StopCoroutine(generateWaterCoroutine);
        }
        generateWaterCoroutine = StartCoroutine(GenerateWaterCoroutine(transform, count));
    }

    IEnumerator GenerateWaterCoroutine(Transform transform, int count)
    {
        for (int i = 0; i < count; i++)
        {
            //もし生成数が多すぎたら古いものから消していく
            if (waterParticleList.Count > 500)
            {
                GameObject oldWaterParticle = waterParticleList[0];
                waterParticleList.RemoveAt(0);

                //Rigidbody2Dを取得してあれば削除する
                Rigidbody2D oldRigidbody2D = oldWaterParticle.GetComponent<Rigidbody2D>();
                if (oldRigidbody2D != null)
                {
                    Destroy(oldRigidbody2D);
                    //CircleCollider2Dを削除する
                    Destroy(oldWaterParticle.GetComponent<CircleCollider2D>());

                    //小さくしてから消す
                    oldWaterParticle.transform.DOScale(0, 0.5f).SetEase(Ease.InSine).OnComplete(() => Destroy(oldWaterParticle));
                }
            }

            GameObject waterParticle = Instantiate(PathDataScriptableObject.WaterParticlePrefab, transform.position, Quaternion.identity);
            waterParticle.transform.parent = transform;
            //ランダムな方向に力を加える
            Rigidbody2D rigidbody2D = waterParticle.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 0.001f);
            waterParticleList.Add(waterParticle);

            yield return null;
        }
    }
}
