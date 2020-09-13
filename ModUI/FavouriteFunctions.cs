using BagOfTricks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;
using BagOfTricks.Favourites;

namespace BagOfTricks.ModUI {
    public static class FavouriteFunctionsMenu {
        private static Settings settings = Main.settings;
        private static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;
        private static List<string> favouritesList = FavouritesFactory.GetFavouriteFunctions.FavouritesList;

        public static void Render() {
            GL.BeginVertical("box");
            if (!favouritesList.Any()) {
                MenuTools.SingleLineLabel(Strings.GetText("message_NoFavourites"));
            }
            else {
                GL.BeginVertical("box");
                GL.BeginHorizontal();
                settings.editFavouriteFunctionsPosition = GL.Toggle(settings.editFavouriteFunctionsPosition, " " + RichTextUtils.Bold(Strings.GetText("toggle_MoveFavourites")), GL.ExpandWidth(false));
                GL.EndHorizontal();
                GL.EndVertical();
                GL.Space(10);
                for (int i = 0; i < favouritesList.Count; i++) {
                    String[] sA = favouritesList[i].Split(new Char[] { ',' });
                    if (sA.Length == 3) {
                        GL.BeginVertical("box");
                        if (settings.editFavouriteFunctionsPosition) {
                            GL.BeginHorizontal();
                            MenuTools.AddUpDownButtons(favouritesList[i], ref favouritesList, 13);
                            GL.EndHorizontal();
                        }
                        try {
                            MenuTools.ToggleButtonFavouritesMenu(ref MenuTools.GetToggleButton(sA[0]), sA[1], sA[2]);
                        }
                        catch (ArgumentException) {
                            GL.BeginHorizontal();
                            MenuTools.SingleLineLabel(sA[0] + " " + Strings.GetText("error_NotFound"));
                        }

                        GL.FlexibleSpace();
                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                            favouritesList.Remove(favouritesList[i]);

                        }
                        GL.EndHorizontal();
                        GL.EndVertical();
                    }
                    else if (sA.Length == 1) {


                        if (settings.editFavouriteFunctionsPosition) {
                            GL.BeginVertical("box");
                            GL.BeginHorizontal();
                            MenuTools.AddUpDownButtons(favouritesList[i], ref favouritesList, 13);
                            GL.EndHorizontal();
                        }
                        try {
                            typeof(BagOfTricks.MainMenu).GetMethod(sA[0]).Invoke(typeof(BagOfTricks.MainMenu), new object[] { });
                        }
                        catch (NullReferenceException) {
                            GL.BeginHorizontal();
                            MenuTools.SingleLineLabel(sA[0] + " " + Strings.GetText("error_NotFound"));
                            GL.FlexibleSpace();
                            if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                                favouritesList.Remove(favouritesList[i]);

                            }
                            GL.EndHorizontal();
                        }
                        if (settings.editFavouriteFunctionsPosition) {
                            GL.EndVertical();
                        }

                    }

                }
            }
            GL.EndVertical();
        }

    }
}
