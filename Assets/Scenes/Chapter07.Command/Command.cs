namespace Chapter.Command
{
    public abstract class Command
    {
        public abstract void Execute();
    }

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