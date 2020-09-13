
namespace BagOfTricks.ModUI.CheatsUI {
    public static class CheatsMainMenu {
        private static Settings settings = Main.settings;

        public static void Render() {
            foreach (string method in settings.cheatsCategories) {
                typeof(BagOfTricks.MainMenu).GetMethod(method).Invoke(typeof(BagOfTricks.MainMenu), new object[] { });
            }
        }
    }
}
