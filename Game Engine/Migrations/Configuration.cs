namespace Game_Engine.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Game_Engine.GameContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Game_Engine.GameContext context)
        {
            var repo = new MapRepository(context);
            /*repo.Store(new MapObject { X = 12, Y = 11 });
            repo.Store(new MapObject { X = 15, Y = 16 });
            repo.Store(new MapObject { X = 13, Y = 17 });
            repo.Store(new MapObject { X = 18, Y = 14 });*/
            var terrainTypeGrass = new TerrainType { TileImageUrl = "/Images/grass.png" };
            var terrainTypeRock = new TerrainType { TileImageUrl = "/Images/rock.png" };
            var terrainTypeDirt = new TerrainType { TileImageUrl = "/Images/dirt.png" };
            repo.Store(terrainTypeGrass);
            repo.Store(terrainTypeRock);
            repo.Store(terrainTypeDirt);

            Random random = new Random();
            for (int x = 0; x <= 20; ++x)
                for (int y = 0; y <= 20; ++y)
                {
                    var chanceGrass = x + y;
                    var chanceRock = 101 - x + (101 - y);
                    var chanceDirt = ((20 - x)^2) + ((20 - y)^2);
                    if (chanceDirt > 0) chanceDirt = (int)Math.Sqrt(chanceDirt);
                    var r = random.Next(chanceGrass + chanceRock + chanceDirt);
                    TerrainType tileType;
                    r -= chanceGrass;
                    if (r < 0)
                        tileType = terrainTypeGrass;
                    else
                    {
                        r -= chanceRock;
                        if (r < 0)
                            tileType = terrainTypeRock;
                        else
                            tileType = terrainTypeDirt;
                    }
                    var t = repo.Store(new Terrain { X = x, Y = y, TerrainTypeId = tileType.Id });
                }
            //var map = repo.GetMap(10, 10, 10, 10);
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
