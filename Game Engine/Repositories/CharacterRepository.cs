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
    public interface ICharacterRepository
    {
        Character Create(Character character);
        Character Get(string playerId);
    }

    public class CharacterRepository : Repository, ICharacterRepository
    {
        public CharacterRepository(IGameContext context) : base(context)
        {
        }

        public Character Create(Character character)
        {
            Context.Characters.Add(character);
            Context.SaveChanges();
            return character;
        }

        public Character Get(string playerId)
        {
            return Context.Characters.FirstOrDefault(c => c.PlayerId == playerId);
        }
    }
}