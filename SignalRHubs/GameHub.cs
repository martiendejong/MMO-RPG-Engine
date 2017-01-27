using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Game_Engine;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR.Client;

namespace MMO_RPG_Engine
{
    // This is where commands from the webclient (player ui) are executed
    public class GameHub : Hub
    {
        public static ISignalRClients SignalRClients = new SignalRClients();

        public static IHubProxy Backend;

        public void RegisterMapClient(int x, int y, int width, int height, string playerId)
        {
            MapClient client = new MapClient(Context.ConnectionId, x, y, width, height, playerId);
            SignalRClients.MapClients.Add(client);

            // todo DI gebruiken
            var repo = new MapRepository(new GameContext());
            Clients.Caller.updateMap(repo.GetMap(x, y, width, height));
        }

        public void MoveCharacter(int x, int y)
        {
            var mapClient = SignalRClients.MapClients.SingleOrDefault(cl => cl.ConnectionId == Context.ConnectionId);
            var playerId = mapClient.PlayerId;

            // todo DI gebruiken
            var c = new GameContext();
            var repo = new CharacterRepository(c);
            var character = repo.Get(playerId);
            if(character != null)
            {
                var pathX = character.X;
                var pathY = character.Y;

                var steps = 0;

                while ((pathX != x || pathY != y) && steps < 50)
                {
                    Direction? direction = null;
                    if (x > pathX)
                    {
                        direction = Direction.East;
                        ++pathX;
                    }
                    else if (x < pathX)
                    {
                        direction = Direction.West;
                        --pathX;
                    }
                    else if (y < pathY)
                    {
                        direction = Direction.North;
                        --pathY;
                    }
                    else if (y > pathY)
                    {
                        direction = Direction.South;
                        ++pathY;
                    }
                    if (direction != null)
                    {
                        try
                        {
                            if (steps == 0)
                                Backend.Invoke<string>("MoveCharacter", character, direction);
                            else
                            {
                                Backend.Invoke<string>("QueMoveCharacter", character, direction);

                            }
                        }
                        catch(Exception e)
                        {

                        }
                    }
                    ++steps;
                }
            }
        }

        public void Hello()
        {
            var targets = SignalRClients.MapClients.Where(client => client.X < 20);
            foreach(MapClient client in targets)
            {
                Clients.Client(client.ConnectionId).hello();
            }
        }

        public void MoveCharacter(int oldX, int oldY, int newX, int newY)
        {
            var targets = SignalRClients.MapClients.Where(client => client.X < 20);
            foreach (MapClient client in targets)
            {
                Clients.Client(client.ConnectionId).moveCharacter(oldX, oldY, newX, newY);
            }
        }

        public void Send(string name, string message)
        {
            Hello();
            // Call the broadcastMessage method to update clients.
            //Clients.All.broadcastMessage(name, message);
        }
    }
}