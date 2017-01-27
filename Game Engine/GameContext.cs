using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Game_Engine
{
    public interface IGameContext
    {
        DbSet<MapObject> MapObjects { get; set; }
        DbSet<Terrain> Terrain { get; set; }
        DbSet<TerrainType> TerrainTypes { get; set; }
        DbSet<Character> Characters { get; set; }
        DbSet<CharacterAction> CharacterActions { get; set; }
        DbEntityEntry Entry(object entity);
        int SaveChanges();
    }

    public class GameContext : DbContext, IGameContext
    {
        public GameContext() : base("name=GameContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<MapObject> MapObjects { get; set; }
        public virtual DbSet<Terrain> Terrain { get; set; }
        public virtual DbSet<TerrainType> TerrainTypes { get; set; }
        public virtual DbSet<Character> Characters { get; set; }
        public virtual DbSet<CharacterAction> CharacterActions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Character>()
                        .HasMany(c => c.Actions)
                        .WithRequired(a => a.Character)
                        .HasForeignKey(a => a.CharacterId);
            modelBuilder.Entity<Character>()
                        .HasMany(c => c.QueuedActions)
                        .WithOptional(a => a.QueCharacter)
                        .HasForeignKey(a => a.QueCharacterId);
            modelBuilder.Entity<Character>().HasOptional(c => c.CurrentAction).WithOptionalPrincipal(a=>a.ExecutingCharacter);
        }
    }

    public class EntityWithNumericalKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }

    public class MapObject : EntityWithNumericalKey
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Terrain : MapObject
    {
        public int TerrainTypeId { get; set; }
        [XmlIgnore]
        [ScriptIgnore]
        public TerrainType TerrainType { get; set; }
    }

    public class TerrainType : EntityWithNumericalKey
    {
        public string TileImageUrl { get; set; }
    }
}
