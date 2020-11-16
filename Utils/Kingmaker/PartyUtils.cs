using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace BagOfTricks.Utils.Kingmaker
{
    internal class PartyUtils
    {

        public static List<UnitEntityData> GetCustomCompanions()
        {
            return Game.Instance.Player.AllCharacters.Where(unit => unit.IsCustomCompanion()).ToList();
        }

        public static List<UnitEntityData> GetPets()
        {
            return Game.Instance.Player.AllCharacters.Where(unit => unit.Descriptor.IsPet).ToList();
        }

        public static List<UnitEntityData> GetRemoteCompanions()
        {
            return Game.Instance.Player.AllCharacters.Except(Game.Instance.Player.Party).Where(unit => !unit.Descriptor.IsPet).ToList();
        }
    }
}
