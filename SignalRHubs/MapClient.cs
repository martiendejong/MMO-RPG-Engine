using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Game_Engine;

namespace MMO_RPG_Engine
{
    public interface ISignalRClient
    {
        string ConnectionId { get; }
    }

    public class SignalRClient : ISignalRClient
    {
        public string ConnectionId { get; }

        public SignalRClient(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }

    public interface IMapClient : ISignalRClient
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
    }

    public class MapClient : SignalRClient, IMapClient
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string PlayerId { get; set; }

        public MapClient(string connectionId, int x, int y, int width, int height, string playerId) : base(connectionId)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            PlayerId = playerId;
        }
    }

    public interface ISignalRClients
    {
        List<MapClient> MapClients { get; }
    }

    public class SignalRClients : ISignalRClients
    {
        public List<MapClient> MapClients { get; }

        public SignalRClients()
        {
            MapClients = new List<MapClient>();
        }
    }
}