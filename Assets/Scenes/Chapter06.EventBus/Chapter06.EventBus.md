# 6. 이벤트 버스로 게임 이벤트 관리하기

### 6.1 기술적 요구 사항
    
- 스태틱
- UnityEvent
- UnityActions

### 6.2 이벤트 버스 패턴 이해하기
- 어느 행동하는 객체(게시자)가 이벤트를 발생시키면 다른 객체(구독자)가 받을 수 있는 신호를 보냄
- 신호는 작업이 생겼다는 것을 알리는 알림 형식
- 오브젝트는 이벤트 시스템에서 이벤트를 브로드캐스트함
- 구독하는 오브젝트만 알림을 받으며 어떻게 처리할지 결정

- 이벤트 버스 패턴은 메시징 시스템과 발행/구독 패턴과 가까운 관계
- 발행/구독 시스템이라고 부르는 것이 이벤트 버스가하는 일을 봤을 때 더 정확한 명칭
- 이벤트 버스 패턴의 키워드는 버스(Bus)
- 이벤트에서 발행/구독 모델을 사용하여 오브젝트를 연결하는 방법을 이벤트 버스라고 함

- 옵저버 및 기본 C# 이벤트와 같은 패턴으로 유사한 모델을 만들 수 있다.
    - 단점 : 일반적인 옵저버 패턴 구현에서 옵저버(구독자)와 서브젝트(발행자)가 서로 의존하고 인식하여 강력한 결합을 발생.

- 이벤트 버스와 게시자와 구독자 간 관계를 추상화 및 단순화하여 전혀 서로를 인식하지 못 한다.

- 단 한줄의 코드로 게시자나 구독자 역할을 할당하는 과정을 줄일 수 있다
> ???

#### 6.2.1 이벤트 버스 패턴의 장단점
- 분리 : 오브젝트를 분리한다는 점. 오브젝트는 직접 서로를 참조하는 대신 이벤트로 통신.
- 단순성 : 이벤트 버스는 이벤트의 구독 혹은 게시 메커니즘을 추상화하여 단순성을 제공.

- 성능 : 모든 이벤트 시스템의 내부에는 오브젝트 간 메시지를 관리하는 저수준 메커니즘이 있음. 따라서 이벤트 시스템을 사용할 때 약간의 성능 비용이 발생할 수 있음
    > ???

- 전역 : 

#### 6.2.2 이벤트 버스를 사용하는 시기

#### 6.2.3 전역 레이스 이벤트 관리하기

### 6.3 레이스 이벤트 버스 구현하기

```csharp
namespace Chapter.EventBus
{
    public enum RaceEventType
    {
        COUNTDOWN, START, RESTART, PAUSE, STOP, FINISH, QUIT
    }
}
```

```csharp
using UnityEngine.Events;
using System.Collections.Generic;

namespace Chapter.EventBus
{
    public class RaceEventBus
    {
        private static readonly IDictionary<RaceEventType, UnityEvent> Events = new Dictionary<RaceEventType, UnityEvent>();

        public static void Subscribe(RaceEventType eventType, UnityAction listener)
        {
            UnityEvent thisEvent;

            if (Events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                Events.Add(eventType, thisEvent);
            }
        }

        public static void Unsubscribe(RaceEventType eventType, UnityAction listener)
        {
            UnityEvent thisEvent;

            if (Events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void Publish(RaceEventType type)
        {
            UnityEvent thisEvent;

            if (Events.TryGetValue(type, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}
```

#### 6.3.1 레이스 이벤트 버스 테스트하기
1) COUNTDOWN 레이스 이벤트 타입을 구독하는 카운트다운 타이머 작성.
    - 이벤트가 게시되면 3초의 카운트다운이 레이스 시작까지 트리거됨
    - 카운트가 끝나는 순간 레이스의 시작을 알리는 START 이벤트를 게시
```csharp
using UnityEngine;
using System.Collections;

namespace Chapter.EventBus
{
    public class CountdownTimer : MonoBehaviour
    {
        private float _currentTime;
        private float duration = 3.0f;

        void OnEnable()
        {
            RaceEventBus.Subscribe(RaceEventType.COUNTDOWN, StartTimer);
        }

        void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventType.COUNTDOWN, StartTimer);
        }

        private void StartTimer()
        {
            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            _currentTime = duration;

            while (_currentTime > 0)
            {
                yield return new WaitForSeconds(1f);
                _currentTime--;
            }

            RaceEventBus.Publish(RaceEventType.START);
        }

        void OnGUI()
        {
            GUI.color = Color.blue;
            GUI.Label(new Rect(125, 0, 100, 20), "COUNTDOWN: " + _currentTime);
        }
    }
}
```

2) START와 STOP 이벤트를 테스트하고자 BikeController 클래스의 스켈레톤을 구현
```csharp
using UnityEngine;

namespace Chapter.EventBus
{
    public class BikeController : MonoBehaviour
    {
        private string _status;

        void OnEnable()
        {
            RaceEventBus.Subscribe(RaceEventType.START, StartBike);
            RaceEventBus.Subscribe(RaceEventType.STOP, StopBike);
        }

        void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventType.START, StartBike);
            RaceEventBus.Unsubscribe(RaceEventType.STOP, StopBike);
        }

        private void StartBike()
        {
            _status = "Started";
        }

        private void StopBike()
        {
            _status = "Stopped";
        }

        void OnGUI()
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 60, 200, 20), "BIKE STATUS: " + _status);
        }
    }
}
```

3) 마지막으로 HUDController 클래스 작성. 레이스가 시작되면 레이스 중지 버튼 표시.
```csharp
using UnityEngine;

namespace Chapter.EventBus
{
    public class HUDController : MonoBehaviour
    {
        private bool _isDisplayOn;

        void OnEnable()
        {
            RaceEventBus.Subscribe(RaceEventBus.START, DisplayHUD);
        }

        void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventBus.START, DisplayHUD);
        }

        private void DisplayHUD()
        {
            _isDisplayOn = true;
        }

        void OnGUI()
        {
            if (_isDisplayOn)
            {
                if (GUILayout.Button("Stop Race"))
                {
                    _isDisplayOn = false;
                    RaceEventBus.Publish(RaceEventBus.Stop);
                }
            }
        }
    }
}
```

4) 이벤트의 각 단계를 테스트하고 싶다면 빈 유니티 씬의 게임 오브젝트에 다음 클라이언트 클래스를 연결
```csharp
using UnityEngine;

namespace Chapter.EventBus
{
    public class ClientEventBus : MonoBehaviour
    {
        private bool _isButtonEnabled;

        void Start()
        {
            gameObject.AddComponent<HUDController>();
            gameObject.AddComponent<CountdownTimer>();
            gameObject.AddComponent<BikeController>();

            _isButtonEnabled = true;
        }

        void OnEnable()
        {
            RaceEventBus.Subscribe(RaceEventType.STOP, Restart);
        }

        void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventType.STOP, Restart);
        }

        private void Restart()
        {
            _isButtonEnabled = true;
        }

        void OnGUI()
        {
            if (_isButtonEnabled)
            {
                if (GUILayout.Button("Start Countdown"))
                {
                    _isButtonEnabled = false;
                    RaceEventBus.Publish(RaceEventType.COUNTDOWN);
                }
            }
        }
    }
}
```

#### 6.3.2 이벤트 버스 구현 살펴보기
- 이벤트 버스로 핵심 구성 요소가 분리된 채 동작을 트리거할 수 있으며 간단히 구독자나 게시자로 오브젝트를 추가 혹은 제거할 수 있다.

### 6.4 대안 살펴보기

- 옵저버 : 오브젝트(서브젝트)가 오브젝트(옵저버) 목록을 유지 및 관리하고 내부 상태 변경을 알리는 구식이지만 괜찮은 패턴. 엔티티 그룹 간의 일대다 관계를 설정할 때 고려해야할 패턴
    > ???

- 이벤트 큐 : 이벤트 큐 패턴을 사용하면 게시자가 생성한 이벤트를 큐에 저장하고 편한 시간에 구독자에게 전달할 수 있음. 게시자와 구독자 간의 시각적 관계를 분리
    > ???

- ScriptableObject : 유니티에서는 ScriptableObjects로 이벤트 시스템을 만들 수 있다. 새로운 커스텀 게임 이벤트를 더욱 쉽게 만들 수 있는 장점이 있음. 확장 및 조정할 수 있는 이벤트 시스템을 만들 때 괜찮은 패턴
    > ???

### 6.5 정리
- 관리할 전역 이벤트가 정의된 목록이 있을 때 효과
- 이벤트 버스 패턴에는 한계가 있기 때문에 전역적으로 접근할 수 있는 이벤트 버스를 사용하기로 결정하기 전에 다른 방법도 알아보는 것이 좋음