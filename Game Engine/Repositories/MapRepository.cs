using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public class Repository
    {
        protected IGameContext Context;

        public Repository(IGameContext context)
        {
            Context = context;
        }
    }

    public interface IMapRepository
    {
        TerrainType[] GetTerrainTypes();
        TerrainType Store(TerrainType terrainType);
        Terrain Store(Terrain terrain);
        List<MapObject>[,] GetMap(int x, int y, int width, int height);
    }

    public class MapRepository : Repository, IMapRepository
    {
        public MapRepository(IGameContext context) : base(context)
        {
        }

        public TerrainType[] GetTerrainTypes()
        {
            var types = new TerrainType[Context.TerrainTypes.Max(t => t.Id) + 1];
            foreach (var type in Context.TerrainTypes.ToList())
                types[type.Id] = type;
            return types;
        }

        public TerrainType Store(TerrainType terrainType)
        {
            Context.TerrainTypes.AddOrUpdate(terrainType);
            Context.SaveChanges();
            return terrainType;
        }

        public Terrain Store(Terrain terrain)
        {
            // Remove everything at the location except the stored terrain
            Context.Terrain.RemoveRange(Context.Terrain.Where(m => m.X == terrain.X && m.Y == terrain.Y && m.Id != terrain.Id));
            Context.Terrain.Add(terrain);
            Context.SaveChanges();
            return terrain;
        }

        public List<MapObject>[,] GetMap(int x, int y, int width, int height)
        {
            var result = new List<MapObject>[width, height];
            var mapObjects = Context.MapObjects.Where(m => m.X >= x && m.X < x + width && m.Y >= y && m.Y < y + height);
            foreach (var mapObject in mapObjects)
            {
                if (result[mapObject.Y - y, mapObject.X - x] == null)
                    result[mapObject.Y - y, mapObject.X - x] = new List<MapObject>();
                result[mapObject.Y - y, mapObject.X - x].Add(mapObject);
            }
            return result;
        }
    }
}
