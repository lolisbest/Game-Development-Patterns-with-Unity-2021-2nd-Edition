using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (_instance == null)
            {
                // 인스턴스가 없다면 자신의 인스턴스 대입
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // 이미 인스턴스가 존재하면 자신을 제거
                Destroy(gameObject);
            }
        }
    }
}