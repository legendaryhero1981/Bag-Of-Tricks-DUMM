using Kingmaker;
using Kingmaker.Assets.UI;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace BagOfTricks.Utils.Kingmaker {
    public static class BlueprintScriptableObjectUtils {
        public static string GetDescription(BlueprintScriptableObject bpObejct) {
            MechanicsContext context = new MechanicsContext((UnitEntityData)null, Game.Instance.Player.MainCharacter.Value.Descriptor, bpObejct, (MechanicsContext)null, (TargetWrapper)null);
            return context?.SelectUIData(UIDataType.Description)?.Description ?? "";
        }
    }
}
