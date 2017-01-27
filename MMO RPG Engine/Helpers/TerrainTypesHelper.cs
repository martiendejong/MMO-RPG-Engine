using Game_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMO_RPG_Engine.Helpers
{
    // TODO use DI
    public class TerrainTypesHelper
    {
        public static TerrainType[] GetTerrainTypes()
        {
            return new MapRepository(new GameContext()).GetTerrainTypes();
        }
    }
}