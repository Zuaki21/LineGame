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
            //UpdateManager��Subject��ManagedUpdate��o�^
            //gameObject���j�����ꂽ�Ƃ��Ɏ����Ŏ~�߂����ꍇ��AddTo(gameObject) (using UniRx���K�v)
            UpdateManager.Subscribe(_ => ManagedUpdate()).AddTo(gameObject);
        }

        //Update���g�킸�����ł�����ۂ���`����
        private void ManagedUpdate()
        {

        }
    }
}