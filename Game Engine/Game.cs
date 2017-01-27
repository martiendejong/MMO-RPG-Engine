using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public interface IGame
    {
        IMapRepository MapRepository { get; }
    }

    public class Game : IGame
    {
        public IMapRepository MapRepository { get; }

        public ICharacterRepository CharacterRepository { get; }

        public Game(IMapRepository mapRepository, ICharacterRepository characterRepository)
        {
            MapRepository = mapRepository;
        }
    }
}
