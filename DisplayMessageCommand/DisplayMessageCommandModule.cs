using Celeste.Mod.Helpers;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.DisplayMessageCommand {
    public class DisplayMessageCommandModule : EverestModule {
        private static Hook hookTASCommandSplit = null;
        private static string commandHolder = null;
        private static FieldInfo currentText = typeof(Monocle.Commands).GetField("currentText", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Load() {
            On.Monocle.Commands.EnterCommand += copyConsoleCommandToHolder;
            On.Monocle.Commands.ExecuteCommand += customParseDisplayMessageCommandConsole;
        }

        public override void Initialize() {
            base.Initialize();

            MethodInfo tasCommandSplit = FakeAssembly.GetFakeEntryAssembly().GetType("TAS.Input.Commands.ConsoleCommand")?.GetMethod("Console", BindingFlags.NonPublic | BindingFlags.Static);
            if (tasCommandSplit != null) {
                hookTASCommandSplit = new Hook(tasCommandSplit, typeof(DisplayMessageCommandModule).GetMethod("customParseDisplayMessageCommandTAS", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        public override void Unload() {
            On.Monocle.Commands.EnterCommand -= copyConsoleCommandToHolder;
            On.Monocle.Commands.ExecuteCommand -= customParseDisplayMessageCommandConsole;

            hookTASCommandSplit?.Dispose();
            hookTASCommandSplit = null;
        }

        private static void customParseDisplayMessageCommandTAS(Action<string[], string> orig, string[] arguments, string commandText) {
            if (arguments[0] == "display_message") {
                arguments = commandText.Substring(8).Split(new char[] { ' ' }, 6, StringSplitOptions.RemoveEmptyEntries);
            }

            orig(arguments, commandText);
        }

        private static void copyConsoleCommandToHolder(On.Monocle.Commands.orig_EnterCommand orig, Monocle.Commands self) {
            commandHolder = (string) currentText.GetValue(self);
            orig(self);
            commandHolder = null;
        }

        private static void customParseDisplayMessageCommandConsole(On.Monocle.Commands.orig_ExecuteCommand orig, Monocle.Commands self, string command, string[] args) {
            if (commandHolder != null && command == "display_message") {
                string[] split = commandHolder.Split(new char[] { ' ' }, 6, StringSplitOptions.RemoveEmptyEntries);

                args = new string[split.Length - 1];
                for (int i = 1; i < split.Length; i++) {
                    args[i - 1] = split[i];
                }
            }

            orig(self, command, args);
        }
    }
}
