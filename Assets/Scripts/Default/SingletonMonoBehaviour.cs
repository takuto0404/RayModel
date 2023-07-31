using System;
using UnityEngine;

namespace Default
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    Type t = typeof(T);

                    _instance = (T)FindObjectOfType(t);
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (this == Instance) return;
            
            Destroy(this);
            Debug.LogError(
                typeof(T) +
                " は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました." +
                " アタッチされているGameObjectは " + Instance.gameObject.name + " です.");
        }
    }
}