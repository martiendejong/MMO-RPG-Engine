using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR.Client;
using Ninject;
using System.Reflection;
using Game_Engine;
using System.Threading;
using System.Data.Entity;

namespace Console_Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:8080";
            using (WebApp.Start(url, Startup.Configuration))
            {
                Console.WriteLine("Server running on {0}", url);
                //Set connection
                var connection = new HubConnection("http://localhost:8080/");
                //Make proxy to hub based on hub name on server
                var myHub = connection.CreateHubProxy("GameEngineHub");
                connection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error opening the connection:{0}",
                                            task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine("Connected");
                    }
                }
                );
                var x = Console.ReadLine();
                while (x != "q")
                {
                    if(x == "m")
                        myHub.Invoke<string>("MoveCharacters").Wait();
                    if (x == "hello")
                        myHub.Invoke<string>("Hello").Wait();

                    x = Console.ReadLine();
                }
            }
        }
    }

    public class Startup
    {
        public static void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }


    public class GameEventWrapper : IGameEventWrapper
    {
        public IGameEvent Event { get; set; }
        public IGameContext GameContext { get; set; }
        public IGameScheduler GameScheduler { get; set; }
        public IGameEngineHubConnector GameEngineHubConnector;
        public void Execute(object stateInfo)
        {
            Event.Finish(GameEngineHubConnector, GameContext, GameScheduler);
        }
    }

    public class GameScheduler : IGameScheduler
    {
        protected IGameContext GameContext;
        protected IGameEngineHubConnector GameEngineHubConnector;
        public GameScheduler(IGameContext gameContext, IGameEngineHubConnector hubConnector)
        {
            GameContext = gameContext;
            GameEngineHubConnector = hubConnector; 
        }
        public void Add(DateTime time, ICharacterAction action)
        {
            // todo DI!
            action.Start(GameEngineHubConnector, GameContext, time);
            var wrapper = new GameEventWrapper { Event = action, GameContext = GameContext, GameScheduler = this, GameEngineHubConnector = GameEngineHubConnector };
            var ms = (int)(time - DateTime.Now).TotalMilliseconds;
            if (ms > 0)
            {
                var callback = new TimerCallback(wrapper.Execute);
                var timer = new Timer(callback);
                timer.Change(ms, Timeout.Infinite);
            }
            else
                wrapper.Execute(null);
        }
    }

    public class GameEngineHubConnector : IGameEngineHubConnector
    {
        public GameEngineHub GameEngineHub;

        public void FinishMoveCharacter(int oldX, int oldY, int newX, int newY)
        {
            GameEngineHub.Clients.All.FinishMoveCharacter(oldX, oldY, newX, newY);
        }

        public void StartMoveCharacter(int oldX, int oldY, int newX, int newY, DateTime finishTime)
        {
            GameEngineHub.Clients.All.StartMoveCharacter(oldX, oldY, newX, newY, finishTime);
        }
    }

    [HubName("GameEngineHub")]
    public class GameEngineHub : Hub, IGameEngineHub
    {
        protected IGameContext GameContext { get; set; }
        protected IGameScheduler GameScheduler { get; set; }

        protected IGameEngineHubConnector GameEngineHubConnector { get; set; }

        //public GameEngineHub(IGameContext gameContext, IGameScheduler gameScheduler)
        public GameEngineHub()
        {
            // TODO DI!
            GameContext = new GameContext();
            GameEngineHubConnector = new GameEngineHubConnector { GameEngineHub = this };
            GameScheduler = new GameScheduler(GameContext, GameEngineHubConnector);
        }

        public void MoveCharacters()
        {
            // todo normale code van maken
            var characters = GameContext.Characters.ToList();
            foreach(var character in characters)
            {
                var oldX = character.X;
                var oldY = character.Y;
                character.Y++;
                var newX = character.X;
                var newY = character.Y;
                GameContext.SaveChanges();
                Clients.All.MoveCharacter(oldX, oldY, newX, newY);
            }
        }

        public void QueMoveCharacter(Character character, Direction direction)
        {
            var action = new Move(character, direction);
            action.QueCharacter = character;
            GameContext.Characters.Attach(character);
            GameContext.CharacterActions.Add(action);
            GameContext.SaveChanges();
        }

        public void MoveCharacter(Character character, Direction direction)
        {
            var action = new Move(character, direction);
            if (!action.IsAllowed(GameContext)) return;

            GameContext.Characters.Attach(character);
            GameContext.CharacterActions.Add(action);
            GameContext.SaveChanges();

            if (character.CurrentActionId == null || character.CurrentActionFinishTime < DateTime.Now)
            {
                character.CurrentActionFinishTime = DateTime.Now.Add(action.GetDuration());
                character.CurrentAction = action;
                character.CurrentActionId = action.Id;

                GameContext.Entry(character).State = EntityState.Modified;
                GameContext.SaveChanges();

                GameScheduler.Add(character.CurrentActionFinishTime.Value, action);
            }
            else
            {
                GameContext.CharacterActions.RemoveRange(GameContext.CharacterActions.Where(a => a.QueCharacterId == character.Id && a.Id != action.Id));
                //character.QueuedActions.Clear();
                action.QueCharacter = character;
                GameContext.SaveChanges();
                //character.QueuedActions.Clear();
                //GameContext.SaveChanges();
                //character.QueuedActions.Add(action);
            }
            //GameContext.ac
        }

        public void Hello()
        {
            Clients.All.Hello();
        }
    }
}
