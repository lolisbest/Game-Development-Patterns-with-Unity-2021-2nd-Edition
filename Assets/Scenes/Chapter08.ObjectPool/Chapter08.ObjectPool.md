# 8. 오브젝트 풀 패턴으로 최적화하기
- 다양한 오브젝트가 눈 깜짝할 사이에 로드되고 렌더링됨. CPU에 부담 주지 않으려면 자주 생성되는 요소를 메모리에 미리 예약하는 것이 좋음. 메모리에서 없애는 대신 다시 사요알 수 있도록 오브젝트 풀에 추가. 이것으로 엔티티의 새로운 인스턴스를 로드하는 초기의 초기화 비용을 없앨 수 있음. 또한 파괴하지 않아 가비지 컬렉터를 파괴된 인스턴스를 정리하지 않아도 됨.

- 유니티 버전 2021부터 오브젝트 풀링이 API에 통합되어 있음. 패턴을 직접 구현하지 않아도 됨

### 8.1 기술적 요구 사항
- C#

### 8.2 오브젝트풀 패턴 이해하기
- 풀에 초기화된 오브젝트들을 가지고 있고 쿨라이언트가 필요할 때 넘겨주고 이때 풀에서 제거, 사용 가능한 인스턴스가 없다면 새로운 인스턴스를 생성.

- 풀에서 빠져 나온 객체는 클라이언트에서 더 이상 사용되지 않으면 풀로 돌아가려고 함. 오브젝트 풀에 공간이 없다면 객체를 파괴. 풀은 지속적으로 채워지며 일시적으로 비워질 수는 있지만 넘치지는 않음

#### 8.2.1 오브젝트 풀 패턴의 장단점
- 장점
    - 예측할 수 있는 메모리 사용 : 오브젝트 풀에 저장된 인스턴스가 일정한 수를 유지하므로
    - 성능 향상 : 생성하고 초기화에 드는 로딩 비용을 없앨 수 있음
- 단점
    - 이미 관리되는 메모리에 대한 레이어링 : C#처럼 최신 관리 프로그래밍 언어가 이미 메모리 할당을 최적으로 관리하여 대부분 오브젝트 풀 패턴이 불필요하다고 말하기도 함. 어떤 경우에는 사실이지만 어떤 경우에는 거짓
        > ???
    - 예측 불가능한 객체 상태 : 잘못 처리한 경우 객체가 초기 상태 대신 현재 상태로 풀에 되돌아 올 수 있음. 풀에 들어온 객체가 손상되거나 파괴될 수 있는 경우 문제가 됨.


#### 8.2.2 오브젝트 풀 패턴을 사용하는 경우
- 최종 보스처럼 간헐적으로 스폰되는 경우 오브젝트 풀에 넣는 것은 메모리 낭비
- 총알이나 파티클, 적 캐릭터처럼 게임 플레이 중 자주 생성되고 파괴되는 엔티티가 있는 경우 사용. 생성과 파괴 같은 반복적인 함수 호출을 줄여 CPU에 가해지는 부담을 줄여줌

### 8.3 오브젝트 풀 패턴 구현하기
- https://docs.unity3d.com/ScriptReference/Pool.IObjectPool_1.html

#### 8.3.1 오브젝트 풀 패턴 구현하기
```csharp
using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace Chapter.ObjectPool
{
    public class Drone : MonoBehaviour
    {
        public IObjectPool<Drone> Pool { get; set; }

        public float _currentHealth;

        [SerializeField] private float maxHealth;
        [SerializeField] private float timeToSelfDestruct = 3.0f;

        void Start()
        {
            _currentHealth = maxHealth;
        }

        void OnEnable()
        {
            AttackPlayer();
            StartCoroutine(SelfDestruct());
        }

        void OnDisable()
        {
            ResetDrone();
        }

        IEnumerator SelfDestruct()
        {
            yield return new WaitForSeconds(timeToSelfDestruct);
            TakeDamage(maxHealth);
        }

        private void ReturnToPool()
        {
            Pool.Release(this);
        }

        private void ResetDrone()
        {
            _currentHealth = maxHealth;
        }

        public void AttackPlayer()
        {
            Debug.Log("Attack Player!");
        }

        public void TakeDamage(float amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0f)
            {
                ReturnToPool();
            }
        }
    }
}
```

```csharp
using UnityEngine;
using UnityEngine.Pool;

namespace Chapter.ObjectPool
{
    public class DroneObjectPool : MonoBehaviour
    {
        public int maxPoolSize = 10;
        public int stackDefaultCapacity = 10;

        public IObjectPool<Drone> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new ObjectPool<Drone>(
                        CreatePooledItem,
                        OnTakeFromPool,
                        OnReturnedToPool,
                        OnDestroyPoolObject,
                        true,
                        stackDefaultCapacity,
                        maxPoolSize
                    );
                }

                return _pool;
            }
        }

        private IObjectPool<Drone> _pool;

        private Drone CreatePooledItem()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);

            Drone drone = go.AddComponent<Drone>();
            go.name = "Drone";
            drone.Pool = Pool;

            return drone;
        }

        private void OnReturnedToPool(Drone drone)
        {
            drone.gameObject.SetActive(false);
        }

        private void OnTakeFromPool(Drone drone)
        {
            drone.gameObject.SetActive(true);
        }

        private void OnDestroyPoolObject(Drone drone)
        {
            Destroy(drone.gameObject);
        }

        public void Spawn()
        {
            var amount = Random.Range(1, 10);

            for (int i = 0; i < amount; ++i)
            {
                var drone = Pool.Get();
                drone.transform.position = Random.insideUnitSphere * 10;
            }
        }
    }
}
```

#### 8.3.2 오브젝트 풀 구현 테스트하기

```csharp
using UnityEngine;

namespace Chapter.ObjectPool
{
    public class ClientObjectPool : MonoBehaviour
    {
        private DroneObjectPool _pool;

        void Start()
        {
            _pool = gameObject.AddComponent<DroneObjectPool>();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Spawn Drones"))
            {
                _pool.Spawn();
            }
        }
    }
}
```

1) 새로운 빈 씬 생성
2) 모든 스크립트 생성
3) 빈 게임 오브젝트 추가 후 클라이언트 스크립트 추가


#### 8.3.3 오브젝트 풀 구현 살펴보기

### 8.4 대안 살펴보기
- 오브젝트 풀 패턴과 비슷한 패턴으로 포로토타입 패턴이 있음. 두 패턴 모두 생성과 관련된 패턴(Creational Pattern). 프로토타입은 복제 메커니즘을 사용하는데, 새로운 객체를 생성할 때 비용이
덜 든다. 메모리에서 메모리로 복사하기 때문. 복사는 경우에 따라 얕은 복사 혹은 깊은 복사를 진행
    > 비용이 들지 않는다 -> 비용이 덜 든다

    > 얕은 복사 -> 얕은 복사 혹은 깊은 복사

### 8.5 정리
- CPU spike와 lag을 피하는 데 도움