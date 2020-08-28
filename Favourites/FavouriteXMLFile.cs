


using System.ComponentModel;


namespace BagOfTricks.Favourites {
    public enum FavouriteXMLFile {
        [Description("Abilities.xml")]
        Abilities,
        [Description("Buffs.xml")]
        Buffs,
        [Description("Feats.xml")]
        Feats,
        [Description("Functions.xml")]
        Functions,
        [Description("Items.xml")]
        Items,
        [Description("Toggles.xml")]
        Toggles, // outdated use Functions instead
        [Description("Units.xml")]
        Units,

    }
}
