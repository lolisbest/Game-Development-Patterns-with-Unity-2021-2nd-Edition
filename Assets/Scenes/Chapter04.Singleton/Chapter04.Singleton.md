# 4. 싱글턴으로 게임 매니저 구현하기

- 모든 핵심 시스템을 감싸고 관리하는 간단한 코드 아키텍처를 빠르게 만들 수 있다. 게임 내부의 복잡성을 숨기는 깔끔하고 직관적인 인터페이스를 노출하도록 하거나 매니저에 쉽게 접근하고 한번에 하나의 인스턴스만 있도록 구현할 수 있다.
- 핵심 구성 요소 간의 결합을 강력하게 하여 유닛 테스트를 어렵게 만든다.


### 4.1 기술적 요구 사항
- C# (+제네릭), 유니티

### 4.2 싱글턴 패턴 이해하기
- 주요 목표는 유일성 보장
    > 초기화된 후에는 런타임 동안 메모리에 오직 하나의 인스턴스만 존재한다는 것
        > > 일관되고 유일한 진입점에서 전역적으로 접근할 수 있는 시스템을 관리하는 클래스를 구현할 때


##### 4.2.1 싱글턴 패턴의 장단점
- 장점
    - 전역 접근 가능 : 전역 접근점을 만들 수 있다
    - 동시성 제어 : 공유자원에 동시 접근을 제한하고자 사용할 수 있음
- 단점
    - 유닛 테스트 : 싱글턴 오브젝트가 다른 싱글턴 오브젝트에 종속될 수도 있음. 단독으로 테스트하고 디버그하는 것이 힘들 수 있음
    - 잘못된 습관 : 싱글턴은 사용하기 쉬워 잘못도니 프로그래밍 습곤이 생길 수 있음. 싱글턴으로 어디서나 모든 것에 쉽게 접근 가능하게 만술 수 있기 때문에, 코드 작성 시 보다 정교하게 접근하여 테스트하는 것이 귀찮게 느껴질 수 있다.
        > ???

```
아키텍처의 유지 및 관리, 확장, 테스트 가능 여부를 항상 염두
```

### 4.3 게임 매니저 디자인하기
- 일반적으로 게임 매니저를 싱글턴으로 구현하지만 코드마다 그 관리 범위가 다르다.
- 게임 매니저는 게임의 전체 수명 동안 살아있어야 한다. 유일하게

### 4.4 게임 매니저 구현하기
```csharp
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
```

```csharp
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chapter.Singleton
{
    public class GameManager : Singleton<GameManager>
    {
        private DateTime _sessionStartTime;
        private DateTime _sessionEndTime;

        void Start()
        {
            _sessionStartTime = DateTime.Now;
            Debug.Log("Game session start @: " + DateTime.Now);
        }

        void OnApplicationQuit()
        {
            _sessionEndTime = DateTime.Now;

            TimeSpan timeDifference = _sessionEndTime.Subtract(_sessionStartTime);

            Debug.Log("Game session ended @: " + DateTime.Now);
            Debug.Log("Game session lasted: " + timeDifference);
        }

        void OnGUI()
        {
            // 버튼을 생성하고 버튼이 released할 때 true
            if (GUILayout.Button("Next Scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
```

#### 4.4.1 게임 매니저 테스트하기

1) Init라는 새로운 씬 생성
2) Init 씬에서 빈 GameObject를 추가하고 GameManager 클래스를 추가
3) 빈 유니티 씬을 원하는 만큼 생성
4) 'File > Build Settings'에서 Init 씬을 0번 인덱스에 추가


### 4.5 정리
- 지나친 사용은 과한 의존을 불러온다.