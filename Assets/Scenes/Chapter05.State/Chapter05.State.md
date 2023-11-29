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
        Right = 1;
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