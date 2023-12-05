# 7. 커맨드 패턴으로 리플레이 시스템 구현하기

### 7.1 기술적 요구 사항
- C# 기본 지식

### 7.2 커맨드 패턴 이해하기

```csharp
using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour
{
    [SerializedField] private Controller _characterController;

    private Command _spaceButton;

    void Start()
    {
        _spaceButton = new JumpCommand();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            _spaceButton.Execute(_characterController);
        }
    }
}
```

- invoker(호출자) : 명령을 실행하는 방법을 알고 실행한 명령을 즐겨찾기할 수도 있는 객체
- receiver(수신자) : 명령을 받아서 수행할 수 있는 종류의 객체
- CommandBase : 개별 ConcreteCommand 클래스가 무조건 상속해야 하는 추상 클래스. 호출자가 특정 명령을 실행하기 위해 호출할 수 있는 Execute() 메서드를 노출

#### 7.2.1 커맨드 패턴의 장단점
- 장점
    - 분리 : 커맨드 패턴은 실행 방법을 아는 객체에게서 작업을 호출하는 객체를 분리할 수 있음. 이 분리로 즐겨찾기와 시퀀스 작업을 수행하는 중개자를 추가할 수 있음
    - 시퀀싱 : 커맨드 패턴은 되될리기/다시 하기 기능, 매크로, 명령 큐의 구현을 허용하고 사용자 입력을 큐에 넣는 작업을 용이하게 함
        > ???
- 단점
    - 복잡성 : 각 명령이 그 자체로 클래스다. 커맨드 패턴을 구현하려면 수많은 클래스가 필요하며 패턴으로 만들어진 코드의 유지 및 보수를 위해 패턴을 잘 이해해야 함. 대부분 문제가 되지 않지만 특정 목표를 염두에 두지 않은 채 커맨드 패턴을 사용하는 경우 복잡하고 장황하며 불필요한 계층이 될 수 있음
        > ???

#### 7.2.2 커맨드 패턴을 사용하는 경우

- 실행 취소 : 대부분 텍스트와 이미지 에디터에서 볼 수 있는 실행 취소 및 재실행 시스템을 구현
- 매크로 : 공격 혹은 방어 콤보를 기록하고 자동으로 입력 키에 적용하여 실행할 수 있는 매크로 기록 시스템을 구현
- 자동화 : 봇이 자동으로 그리고 순차적으로 실행할 명령 집합을 기록하는 자동화 과정 혹은 행동을 구현

### 7.3 리플레이 시스템 설계하기

- 결정론적 : 게임의 모든 것은 결정론적이다. 마음대로 행동하는 객체가 없다. 이는 적 드론처럼 씬 내에서 움직이는 객체 위치나 상태를 기록하지 않아도 되어 리플레이 시스템을 더욱 쉽게 구현할 수 있도록 한다. 리플레이하는 동안 같은 방식으로 움직이고 행동한다.

- 물리 : 객체의 움직임은 다른 어떤 물리적 요소나 상호작용으로 결정되지 않아 유니티 엔진의 물리 기능을 최소한으로 사용한다. 객체가 충돌할 때 예상하지 못한 동작을 걱정하지 않아도 된다.

- 디지털 : 모든 입력은 디지털이다. 세분화된 아날로그 입력 데이터를 조이스틱이나 트리거 버튼으로 캡쳐하거나 처리하는 데 신경 쓰지 않아도 된다.

- 정확성 : 입력을 리플레이하는 타이밍이 정확하지 않은 것을 허용한다. 입력이 기록된 것과 동일한 시간 프레임에서 정확하게 재생될 것이라고 기대하지 않는다. 이 허용 수준은 리플레이 기능에 요구하는 정확성과 관련된 요소에 따라 변경할 수 있다.

### 7.4 리플레이 시스템 구현하기

#### 7.4.1 리플레이 시스템 구현하기

```csharp
public abstract class Command
{
    public abstract void Execute();
}
```

```csharp
namespace Chapter.Command
{
    public class ToggleTurbo : Command
    {
        private BikeController _controller;

        public ToggleTurbo(BikeController controller)
        {
            _controller = controller;
        }

        public override void Execute()
        {
            _controller.ToggleTurbo();
        }
    }

    public class TurnLeft : Command
    {
        private BikeController _controller;

        public TurnLeft(BikeController controller)
        {
            _controller = controller;
        }

        public override void Execute()
        {
            _controller.Turn(BikeController.Direction.Left);
        }
    }

    public class TurnRight : Command
    {
        private BikeController _controller;

        public TurnRight(BikeController controller)
        {
            _controller = controller;
        }

        public override void Execute()
        {
            _controller.Turn(BikeController.Direction.Right);
        }
    }
}
```

```csharp
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Chapter.Command
{
    public class Invoker : MonoBehaviour
    {
        private bool _isRecording;
        private bool _isReplaying;
        private float _replayTime;
        private float _recordingTime;

        private SortedList<float, Command> _recordedCommands = new SortedList<float, Command>();

        public void ExecuteCommand(Command command)
        {
            command.Execute();
            if (_isRecording)
            {
                _recordedCommands.Add(_recordingTime, command);
            }

            Debug.Log("Recorded Time: " + _recordingTime);
            Debug.Log("Recorded Command: " + command);
        }

        public void Record()
        {
            _recordingTime = 0.0f;
            _isRecording = true;
        }

        public void Replay()
        {
            _replayTime = 0.0f;
            _isReplaying = true;

            if (_recordedCommands.Count < 0)
            {
                Debug.LogError("No commands to replay!");
            }

            _recordedCommands.Reverse();
        }

        void FixedUpdate()
        {
            if (_isRecording)
            {
                _recordingTime += Time.fixedDeltaTime;
            }

            if (_isReplaying)
            {
                _replayTime += Time.deltaTime;

                if (_recordedCommands.Any())
                {
                    if (Mathf.Approximately(_replayTime, _recordedCommands.Keys[0]))
                    {
                        Debug.Log("Replay Time: " + _replayTime);
                        Debug.Log("Replay Command: " + _recordedCommands.Values[0]);

                        _recordedCommands.Values[0].Execute();
                        _recordedCommands.RemoveAt(0);
                    }
                }
                else
                {
                    _isReplaying = false;
                }
            }
        }
    }
}
```

```csharp
using UnityEngine;

namespace Chapter.Command
{
    public class InputHandler : MonoBehaviour
    {
        private Invoker _invoker;
        private bool _isReplaying;
        private bool _isRecording;
        private BikeController _bikeController;
        private Command _buttonA, _buttonD, _buttonW;

        void Start()
        {
            _invoker = gameObject.AddComponent<Invoker>();
            _bikeController = gameObject.AddComponent<BikeController>();

            _buttonA = new TurnLeft(_bikeController);
            _buttonD = new TurnRight(_bikeController);
            _buttonW = new ToggleTurbo(_bikeController);
        }

        void Update()
        {
            if (!_isReplaying && _isRecording)
            {
                if (Input.GetKeyUp(KeyCode.A)) _invoker.ExecuteCommand(_buttonA);
                if (Input.GetKeyUp(KeyCode.D)) _invoker.ExecuteCommand(_buttonD);
                if (Input.GetKeyUp(KeyCode.W)) _invoker.ExecuteCommand(_buttonW);
            }
        }

        void OnGUI()
        {
            if (GUILayout.Button("Start Recording"))
            {
                _bikeController.ResetPosition();
                _isReplaying = false;
                _isRecording = true;
                _invoker.Record();
            }

            if (GUILayout.Button("Stop Recording"))
            {
                _bikeController.ResetPosition();
                _isRecording = false;
            }

            if (!_isRecording)
            {
                if (GUILayout.Button("Start Replay"))
                {
                    _bikeController.ResetPosition();
                    _isRecording = false;
                    _isReplaying = true;
                    _invoker.Replay();
                }
            }
        }
    }
}
```

```csharp
using UnityEngine;

namespace Chapter.Command
{
    public class BikeController : MonoBehaviour
    {
        public enum Direction
        {
            Left = -1,
            Right = 1
        }

        private bool _isTurboOn;
        private float _distance = 1.0f;

        public void ToggleTurbo()
        {
            _isTurboOn = !_isTurboOn;
            Debug.Log("Turbo Active: " + _isTurboOn.ToString());
        }

        public void Turn(Direction direction)
        {
            if (direction == Dictionary.Left) transform.Translate(Vector3.left * _distance);
            if (direction == Dictionary.Right) transform.Translate(Vector3.right * _distance);
        }

        public void ResetPosition()
        {
            transform.position = Vector3.zero;
        }
    }
}
```

#### 7.4.3 구현 검토하기


### 7.5 대안 살펴보기
- 메멘토 : 메멘토 패턴은 객체를 이전 상태로 되돌리는 기능을 제공. 입력을 기록하고 이후 리플레이를 위해 큐에 넣는 데 초점을 맞춤. 커맨드 패턴의 지다인 의도와 매우 잘 맞음. 이전 상태로 되돌리는 기능을 가진 시스템을 구현한다면 메멘토 패턴을 선택하는 것이 좋음

- 큐/스택 : 큐/스택은 패턴이 아닌 데이터 구조체다. InputHandler 클래스의 큐에 직접 모든 입력을 인코딩하고 저장할 수 있음. 커맨드 패턴을 사용하는 것보다 더 간단하며 장황하지 않음


### 7.6 정리
