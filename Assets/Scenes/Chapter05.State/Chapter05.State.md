# 5. 상태 패턴으로 캐릭터 상태 관리하기

- 캐릭터의 유한 상태를 관리하고자 상태 패턴을 사용
- 다양한 동작과 애니메이션을 상태 패턴으로 구현하다보면 상태 패턴의 한계를 경험하게 됨. 유한 상태 기계 개념을 도입해 해결
    > ???


### 5.1 기술적 요구 사항
- C#

### 5.2 상태 패턴의 개요
- Context 클래스 클라이언트가 객체의 내부 상태를 변경할 수 있도록 요청하는 인터페이스를 정의
- IState 인터페이스는 구체적인 상태 클래스로 연결할 수 있도록 설정
- ConcreteState 클래스는 IState 인터페이스를 구현하고 Context 오브젝트가 상태의 동작을 트리거하기 위해 호출하는 퍼블릭 메서드를 노출함

### 5.3 캐릭터 상태 정의하기

### 5.4 상태 패턴 구현하기

#### 5.4.1 상태 패턴 구현하기
```csharp
namespace Chapter.State
{
    public interface IBikeState
    {
        void Handle (BikeController controller);
    }

    public class BikeStateContext
    {
        public IBikeState CurrentState {get; set;}

        private readonly BikeController _bikeController;

        public BikeStateContext(BikeController bikeController)
        {
            _bikeController = bikeController;
        }

        public void Transition()
        {
            CurrentState.Handle(_bikeController);
        }

        public void Transition(IBikeState state)
        {
            CurrentState = state;
            CurrentState.Handle(_bikeController);
        }
    }
}
```


```csharp
using UnityEngine;

namespace Chapter.State
{
    public class BikeController : MonoBehaviour
    {
        public float maxSpeed = 2.0f;
        public float turnDistance = 2.0f;

        public float CurrentSpeed {get; set;}

        public Direction CurrentTurnDirection {get; private set;}

        private IBikeState _startState, _stopState, _turnState;

        private BikeStateContext _bikeStateContext;

        void Start()
        {
            _bikeStateContext = new BikeStateContext(this);

            _startState = gameObject.AddComponent<BikeStartState>();
            _stopState = gameObject.AddComponent<BikeStopState>();
            _turnState = gameObject.AddComponent<BikeTurnState>();

            _bikeStateContext.Transition(_stopState);
        }

        public void StartBike()
        {
            _bikeStateContext.Transition(_startState);
        }

        public void StopBike()
        {
            _bikeStateContext.Transition(_stopState);
        }

        public void Turn(Direction direction)
        {
            CurrentTurnDirection = direction;
            _bikeStateContext.Transition(_turnState);
        }
    }
}
```

```csharp
using UnityEngine;

namespace Chapter.State
{
    // BikeStopState
    public class BikeStopState : MonoBehaviour, IBikeState
    {
        private BikeController _bikeController;

        public void Handle(BikeController bikeController)
        {
            if (!_bikeController)
            {
                _bikeController = bikeController;
            }

            _bikeController.CurrentSpeed = 0;
        }
    }

    // BikeStartState
    public class BikeStartState : MonoBehaviour, IBikeState
    {
        private BikeController _bikeController;

        public void Handle(BikeController bikeController)
        {
            if (!_bikeController)
            {
                _bikeController = bikeController;
            }

            _bikeController.CurrentSpeed = _bikeController.maxSpeed;
        }

        void Update()
        {
            if (_bikeController)
            {
                if (_bikeController.CurrentSpeed > 0)
                {
                    _bikeController.transform.Translate(
                        Vector3.forward * (_bikeController.CurrentSpeed * Time.deltaTime)
                    );
                }
            }
        }
    }

    // BikeTurnState
    public class BikeTurnState : MonoBehaviour, IBikeState
    {
        private Vector3 _turnDirection;
        private BikeController _bikeController;

        public void Handle(BikeController bikeController)
        {
            if (!_bikeController)
            {
                _bikeController = bikeController;
            }

            _turnDirection.x = (float) _bikeController.CurrentTurnDirection;

            if (_bikeController.CurrentSpeed > 0)
            {
                transform.Translate(_turnDirection * _bikeController.turnDistance);
            }
        }
    }
}
```

```csharp
namespace Chapter.State
{
    public enum Direction
    {
        Left = -1,
        Right = 1
    }
}
```


#### 5.4.2 상태 패턴 구현 테스트하기
1) 새로운 씬 생성
2) 아무 3D 게임 오브젝트 하나 추가
3) BikeController 부착
4) 아래 스크립트도 부착

```csharp
using UnityEngine;

namespace Chapter.State
{
    public class ClientState : MonoBehaviour
    {
        private BikeController _bikeController;

        void Start()
        {
            _bikeController = (BikeController) FindObjectOfType(typeof(BikeController));
        }

        void OnGUI()
        {
            if (GUILayout.Button("Start Bike")) _bikeController.StartBike();

            if (GUILayout.Button("Turn Left")) _bikeController.Turn(Direction.Left);

            if (GUILayout.Button("Turn Right")) _bikeController.Turn(Direction.Right);

            if (GUILayout.Button("Stop Bike")) _bikeController.StopBike();
        }
    }
}
```


### 5.5 상태 패턴의 장단점
- 장점
    - 캡슐화 : 상태가 변할 때 개체에 동적으로 할당할 수 있는 컴포넌트의 집합. 개체의 상태별 행동을 구현
    - 유지 및 관리 : 쉽게 새로운 상태를 구현할 수 있음
- 단점
    - 블렌딩 : 애니메이션 블렌드 방법을 제공하지 않음
    - 전환 : 상태 간 관계를 정의하지 않았다. 관계와 조건에 따라 상태 간의 전환을 정의하고 싶다면 더 많은 코드를 작성해야함.
        > ???

```
상태 패턴의 단점은 유니티의 애니메이션 시스템과 기본 상태 기계로 극복할 수 있음
```

### 5.6 대안 살펴보기
- 블랙보드/행동 트리 : NPC 캐릭터의 복잡한 AI 동작을 구현하려고 한다면 블랙보드 같은 패턴 혹은 행동 트리 같은 개념을 고려. 동적으로 결정하는 동작을 하는 AI를 구현한다면 행동 트리를 사용하여 행동을구현하는 BT가 적절하다.
    > ???

- 유한 상태 기계 : 상태 패턴을 이야기할 때 자주 발생하는 질문은 유한 상태 기계와 상태 패턴 간의 차이점이다. 상태 패턴은 객체의 상태 종속적인 동작을 캡슐화하는 것과 관련이 있다는 점이 유한 상태 기계와 다르다. 유한 상태 기계는 특정 입력 트리거를 기반으로 하는 유한 상태 간 전환에 더 깊이 관여한다. 자동 기계 같은 시스템을 구현하는 데 더 적합하다.
    > ???

- 메멘토 : 상태 패턴과 비슷하지만 개체에 이전 상태로 돌아가는 기능을 제공. 자체적으로 변경된 것을 되돌리는 기능이 필요한 시스템을 구현할 때 유용하다.
    > ???


### 5.7 정리
- 상태 패턴은 특정 애니메이션 개체를 다룰 때의 한계.
- 유니티는 정교한 상태 기계와 비주얼 에디터로 애니메이션 캐릭터의 상태를 관리할 수 있는 기본 솔루션 제공
- 하지만 상태 패턴만의 장점도 있다.
