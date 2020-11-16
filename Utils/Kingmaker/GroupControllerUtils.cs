using System.Collections.Generic;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using UnityEngine;

namespace BagOfTricks.Utils.Kingmaker
{

    class GroupControllerUtils
    {

        public static T GetField<T>(string fieldName)
        {
            return Traverse.Create(GroupController.Instance).Field(fieldName).GetValue<T>();
        }

        public static T GetProperty<T>(string propertyName)
        {
            return Traverse.Create(GroupController.Instance).Property(propertyName).GetValue<T>();
        }

        public static bool GetWithRemote()
        {
            return GetProperty<bool>("WithRemote");
        }

        public static bool GetWithPet()
        {
            return GetProperty<bool>("WithPet"); ;
        }

        public static List<GroupCharacter> GetCharacters()
        {
            return GetField<List<GroupCharacter>>("m_Characters");
        }

        public static int GetStartIndex()
        {
            return GetField<int>("m_StartIndex"); ;
        }

        public static GameObject GetNaviBlock()
        {
            return GetField<GameObject>("m_NaviBlock"); ;
        }

        public static void SetCharacter(UnitEntityData character, int index)
        {
            Traverse.Create(GroupController.Instance).Method("SetCharacter", character, index);
        }

        public static void SetArrowsInteractable()
        {
            Traverse.Create(GroupController.Instance).Method("SetArrowsInteracteble");
        }

        public static void NaviBlockShowDefault()
        {
            NaviBlockUpdateState((GetWithRemote() || GetWithPet()) && UIUtility.GetGroup(GetWithRemote(), GetWithPet()).Count > 6);
        }

        public static void NaviBlockShowAllPartyMembers()
        {
            NaviBlockUpdateState(UIUtility.GetGroup(GetWithRemote(), GetWithPet()).Count > 6);
        }

        public static void NaviBlockUpdateState(bool condition)
        {
            if (condition) GetNaviBlock().SetActive(true);
            SetArrowsInteractable();
        }
    }
}
