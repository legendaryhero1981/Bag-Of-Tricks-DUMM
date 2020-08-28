using BagOfTricks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;
using BagOfTricks.Favourites;

namespace BagOfTricks.ModUI {
    public static class FavouriteFunctions {
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
                settings.editFavouriteFunctionsPosition = GL.Toggle(settings.editFavouriteFunctionsPosition, RichTextUtils.Bold(Strings.GetText("toggle_MoveFavourites")), GL.ExpandWidth(false));
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
                        MenuTools.ToggleButtonFavouritesMenu(ref MenuTools.GetToggleButton(sA[0]), sA[1], sA[2]);
                        GL.FlexibleSpace();
                        if (GL.Button(Storage.favouriteTrueString, GL.ExpandWidth(false))) {
                            favouritesList.Remove(favouritesList[i]);

                        }
                        GL.EndHorizontal();
                        GL.EndVertical();
                    }
                    else if (sA.Length == 1) {

                        try {
                            if (settings.editFavouriteFunctionsPosition) {
                                GL.BeginVertical("box");
                                GL.BeginHorizontal();
                                MenuTools.AddUpDownButtons(favouritesList[i], ref favouritesList, 13);
                                GL.EndHorizontal();
                            }
                            typeof(BagOfTricks.Menu).GetMethod(sA[0]).Invoke(typeof(BagOfTricks.Menu), new object[] { });
                            if (settings.editFavouriteFunctionsPosition) {
                                GL.EndVertical();
                            }
                        }
                        catch (Exception e) {
                            modLogger.Log(e.ToString());
                        }
                    }

                }
            }
            GL.EndVertical();
        }

    }
}
