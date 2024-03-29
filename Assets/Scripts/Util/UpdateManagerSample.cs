using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using kyon;

namespace kyon
{
	public class UpdateManagerTemplete : MonoBehaviour
	{
        private void Start()
        {
            //UpdateManagerのSubjectでManagedUpdateを登録
            //gameObjectが破棄されたときに自動で止めたい場合はAddTo(gameObject) (using UniRxが必要)
            UpdateManager.Subscribe(_ => ManagedUpdate()).AddTo(gameObject);
        }

        //Updateを使わず自分でそれっぽく定義する
        private void ManagedUpdate()
        {

        }
    }
}