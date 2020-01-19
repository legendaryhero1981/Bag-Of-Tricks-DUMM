using System;
using System.IO;
using System.Media;

using GL = UnityEngine.GUILayout;
using UnityModManager = UnityModManagerNet.UnityModManager;

namespace BagOfTricks
{
    public static class DevTools
    {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static void Render()
        {
            GL.BeginVertical("box");
            GL.BeginHorizontal();
            GL.Label(RichText.MainCategoryFormat(Strings.GetText("mainCategory_DevTools")));
            GL.FlexibleSpace();
            MenuTools.AddFavouriteButton("DevToolsRender");
            GL.EndHorizontal();
            MenuTools.ToggleButton(ref settings.toggleDevTools, "misc_Enable", "tooltip_DevTools");

            if (Strings.ToBool(settings.toggleDevTools))
            {
                MenuTools.SingleLineLabel(Strings.GetText("label_SmartConsoleInfo"));
                GL.Space(10);
                MenuTools.ToggleButton(ref settings.toggleDevToolsLogToUmm, "buttonToggle_LogToUMM",
                    "tooltip_LogToUMM");
                GL.Space(10);
                if (GL.Button(
                    MenuTools.TextWithTooltip("buttonToggle_UberLogger", "tooltip_UberLogger",
                        $"{settings.toggleUberLogger}" + " "), GL.ExpandWidth(false)))
                {
                    if (settings.toggleUberLogger == Storage.isFalseString)
                    {
                        UberLogger.Logger.Enabled = true;
                        settings.toggleUberLogger = Storage.isTrueString;
                    }
                    else if (settings.toggleUberLogger == Storage.isTrueString)
                    {
                        UberLogger.Logger.ForwardMessages = false;
                        UberLogger.Logger.Enabled = false;
                        settings.toggleUberLoggerForwardPrefix = Storage.isFalseString;
                        settings.toggleUberLoggerForward = Storage.isFalseString;
                        settings.toggleUberLogger = Storage.isFalseString;
                    }
                }

                if (Strings.ToBool(settings.toggleUberLogger))
                {
                    if (GL.Button(
                        MenuTools.TextWithTooltip("buttonToggle_UberLoggerForward", "tooltip_UberLoggerForward",
                            $"{settings.toggleUberLoggerForward}" + " "), GL.ExpandWidth(false)))
                    {
                        if (settings.toggleUberLoggerForward == Storage.isFalseString)
                        {
                            UberLogger.Logger.ForwardMessages = true;
                            settings.toggleUberLoggerForward = Storage.isTrueString;
                        }
                        else if (settings.toggleUberLogger == Storage.isTrueString)
                        {
                            UberLogger.Logger.ForwardMessages = false;
                            settings.toggleUberLoggerForwardPrefix = Storage.isFalseString;
                            settings.toggleUberLoggerForward = Storage.isFalseString;
                        }
                    }

                    if (Strings.ToBool(settings.toggleUberLoggerForward))
                        MenuTools.ToggleButton(ref settings.toggleUberLoggerForwardPrefix,
                            "buttonToggle_UberLoggerForwardPrefix", "tooltip_UberLoggerForwardPrefix");
                }

                if (settings.settingShowDebugInfo)
                {
                    GL.Space(10);
                    MenuTools.SingleLineLabel("Application.persistentDataPath: " +
                                              UnityEngine.Application.persistentDataPath);
                    MenuTools.SingleLineLabel("UberLogger.Logger.Enable: " + UberLogger.Logger.Enabled);
                    MenuTools.SingleLineLabel("UberLogger.Logger.ForwardMessages: " +
                                              UberLogger.Logger.ForwardMessages);
                }
            }

            GL.EndVertical();
        }
    }

    public static class SmartConsoleCommands
    {
        public static UnityModManager.ModEntry.ModLogger modLogger = Main.modLogger;

        public static void Register()
        {
            SmartConsole.RegisterCommand("beep", "", "Plays the 'beep' system sound.",
                new SmartConsole.ConsoleCommandFunction(Beep));
            SmartConsole.RegisterCommand("bat", "bat fileName",
                "Executes commands from a file in the Bag of Tricks folder.",
                new SmartConsole.ConsoleCommandFunction(CommandBatch));
        }

        public static void Beep(string parameters)
        {
            SystemSounds.Beep.Play();
        }

        public static void CommandBatch(string parameters)
        {
            parameters = parameters.Remove(0, 4);
            if (File.Exists(Storage.modEntryPath + parameters))
                try
                {
                    var i = 0;
                    var commands = File.ReadAllLines(Storage.modEntryPath + parameters);
                    foreach (var s in commands)
                    {
                        SmartConsole.WriteLine($"[{i}]: {s}");
                        SmartConsole.ExecuteLine(s);
                        i++;
                    }
                }
                catch (Exception e)
                {
                    modLogger.Log(e.ToString());
                }
            else
                SmartConsole.WriteLine($"'{parameters}' {Strings.GetText("error_NotFound")}");
        }
    }
}