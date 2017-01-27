using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public interface IPlayer
    {
        [Key]
        string Id { get; set; }
    }

    public class Character : MapObject
    {
        public IPlayer Player { get; set; }
        public string PlayerId { get; set; }
        public virtual CharacterAction CurrentAction { get; set; }
        public int? CurrentActionId { get; set; }
        public DateTime? CurrentActionFinishTime { get; set; }
        public virtual ICollection<CharacterAction> QueuedActions { get; set; }
        public virtual ICollection<CharacterAction> Actions { get; set; }
    }

    public interface IGameEngineHub
    {

    }

    public interface ICharacterAction : IGameEvent
    {
        TimeSpan GetDuration();
    }

    public abstract class CharacterAction : EntityWithNumericalKey, ICharacterAction
    {
        public int CharacterId { get; set; }
        public virtual Character Character { get; set; }
        public int? QueCharacterId { get; set; }
        public virtual Character QueCharacter { get; set; }
        public virtual Character ExecutingCharacter { get; set; }
        protected CharacterAction()
        {

        }

        public CharacterAction(Character character)
        {
            Character = character;
        }

        public abstract TimeSpan GetDuration();
        public abstract void Start(IGameEngineHubConnector gameEngineHubConnector, IGameContext GameContext, DateTime finishTime);

        public abstract void Finish(IGameEngineHubConnector gameEngineHubConnector, IGameContext GameContext, IGameScheduler GameScheduler);
    }

    [Flags]
    public enum Direction
    {
        None = 0,
        Left = 1,
        West = Left,
        Up = 2,
        North = Up,
        Right = 4,
        East = Right,
        Down = 8,
        South = Down
    }

    public class Move : CharacterAction
    {
        public Direction Direction { get; set; }

        protected Move()
        {

        }

        public Move(Character character, Direction direction) : base(character)
        {
            Character = character;
            Direction = direction;
        }

        public void MoveCharacter()
        {
            switch (Direction)
            {
                case Direction.Left:
                    Character.X -= 1;
                    break;
                case Direction.Right:
                    Character.X += 1;
                    break;
                case Direction.Up:
                    Character.Y -= 1;
                    break;
                case Direction.Down:
                    Character.Y += 1;
                    break;
            }
        }

        override public TimeSpan GetDuration()
        {
            return TimeSpan.FromSeconds(2);
        }
        override public void Start(IGameEngineHubConnector gameEngineHubConnector, IGameContext gameContext, DateTime finishTime)
        {
            int oldX = Character.X;
            int oldY = Character.Y;

            MoveCharacter();

            gameEngineHubConnector.StartMoveCharacter(oldX, oldY, Character.X, Character.Y, finishTime);

            // reset values
            Character.X = oldX;
            Character.Y = oldY;
        }

        override public void Finish(IGameEngineHubConnector gameEngineHubConnector, IGameContext gameContext, IGameScheduler GameScheduler)
        {
            gameContext.Characters.Attach(Character);

            int oldX = Character.X;
            int oldY = Character.Y;

            MoveCharacter();

            var entry = gameContext.Entry(Character);
            entry.State = EntityState.Modified;

            gameContext.SaveChanges();
            gameEngineHubConnector.FinishMoveCharacter(oldX, oldY, Character.X, Character.Y);

            // todo put in base class
            var nextAction = gameContext.CharacterActions.Where(a => a.QueCharacter.Id == Character.Id).OrderBy(a => a.Id).FirstOrDefault();
            if(nextAction == null)
            {
                Character.CurrentActionId = null;
            }
            else
            {
                nextAction.QueCharacterId = null;
                Character.CurrentAction = nextAction;
                Character.CurrentActionId = nextAction.Id;
                Character.CurrentActionFinishTime = DateTime.Now.Add(nextAction.GetDuration());
                GameScheduler.Add(Character.CurrentActionFinishTime.Value, nextAction);
            }
            gameContext.CharacterActions.Remove(this);
            gameContext.SaveChanges();
        }
    }
    public interface IGameEngineHubConnector
    {
        void FinishMoveCharacter(int oldX, int oldY, int newX, int newY);
        void StartMoveCharacter(int oldX, int oldY, int newX, int newY, DateTime finishTime);
    }


    public interface IGameEvent
    {
        void Start(IGameEngineHubConnector GameEngineHubConnector, IGameContext GameContext, DateTime finishTime);
        void Finish(IGameEngineHubConnector GameEngineHubConnector, IGameContext GameContext, IGameScheduler GameScheduler);
    }


    public interface IGameScheduler
    {
        void Add(DateTime time, ICharacterAction action);
    }

    public interface IGameEventWrapper
    {
        IGameEvent Event { get; set; }
        IGameContext GameContext { get; set; }
        void Execute(Object stateInfo);
    }
}
