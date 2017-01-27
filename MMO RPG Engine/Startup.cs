using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using MMO_RPG_Engine.App_Start;
using Ninject;
using Owin;
using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using System.Linq;

[assembly: OwinStartupAttribute(typeof(MMO_RPG_Engine.Startup))]
namespace MMO_RPG_Engine
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();








            // this is where messages from the game server are executed and sent to the webclients

            //Set connection
            var connection = new HubConnection("http://localhost:8080/");
            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("GameEngineHub");
            GameHub.Backend = myHub;

            //Start connection
            //myHub.Subscribe("Hello");
            myHub.On("Hello", () => {

                var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
                context.Clients.All.hello();

            });

            myHub.On("FinishMoveCharacter", (int oldX, int oldY, int newX, int newY) => {
                var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

                var targets = GameHub.SignalRClients.MapClients.Where(c =>
                    (oldX - c.X >= 0
                    && oldY - c.Y >= 0
                    && oldX - c.X < c.Width
                    && oldY - c.Y < c.Height)
                    ||
                    (newX - c.X >= 0
                    && newY - c.Y >= 0
                    && newX - c.X < c.Width
                    && newY - c.Y < c.Height)
                );

                foreach (MapClient client in targets)
                {
                    context.Clients.Client(client.ConnectionId).finishMoveCharacter(oldX, oldY, newX, newY);
                }
            });

            myHub.On("StartMoveCharacter", (int oldX, int oldY, int newX, int newY, DateTime finishTime) => {
                var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

                var targets = GameHub.SignalRClients.MapClients.Where(c =>
                    (oldX - c.X >= 0
                    && oldY - c.Y >= 0
                    && oldX - c.X < c.Width
                    && oldY - c.Y < c.Height)
                    ||
                    (newX - c.X >= 0
                    && newY - c.Y >= 0
                    && newX - c.X < c.Width
                    && newY - c.Y < c.Height)
                );

                foreach (MapClient client in targets)
                {
                    context.Clients.Client(client.ConnectionId).startMoveCharacter(oldX, oldY, newX, newY, finishTime);
                }
            });

            connection.Start().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");

                    myHub.Invoke<string>("Hello").Wait();
                }

            }).Wait();

            
            //myHub.Invoke<string>("DoSomething", "I'm doing something!!!").Wait();


            //connection.Stop();
        }
    }
}
