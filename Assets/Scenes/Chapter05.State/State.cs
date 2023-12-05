using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter.State
{
    public enum Direction
    {
        Left = -1,
        Right = 1,
    }

    public interface IBikeState
    {
        void Handle(BikeController controller);
    }

    public class BikeStateContext
    {
        public IBikeState CurrentState { get; set; }

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

        /// <summary>
        /// _bikeController.CurrentTurnDirection 만큼 X축 방향으로 이동
        /// </summary>
        /// <param name="bikeController"></param>
        public void Handle(BikeController bikeController)
        {
            if (!_bikeController)
            {
                _bikeController = bikeController;
            }

            _turnDirection.x = (float)_bikeController.CurrentTurnDirection;

            if (_bikeController.CurrentSpeed > 0)
            {
                transform.Translate(_turnDirection * _bikeController.turnDistance);
            }
        }
    }
}