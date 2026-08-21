using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PluginLoader;

namespace TranscendPlugins
{
    [PluginDescription("Stops unrecognised /commands being sent to the server as public chat, so a mistyped plugin command never leaks to other players.")]
    public class SlashCommandPrivacy : PluginBase, IPluginChatCommand
    {
        private static readonly HashSet<string> AllowedCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p",
            "me",
            "playing",
            "players",
            "roll",
            "emoji",
            "e",
            "help",
            "rps",
            "death",
            "pvpdeath",
            "alldeath",
            "allpvpdeath",
            "g",
            "s",
            "d",
            "ich",
            "moi",
            "io",
            "já",
            "eu",
            "spielt",
            "spieler",
            "en",
            "joueurs",
            "gioca",
            "giocatori",
            "gra",
            "gracze",
            "jogando",
            "jogadores",
            "rollen",
            "lance",
            "numero",
            "rzuć",
            "rolar",
            "emotikon",
            "hilfe",
            "pomoc",
            "ajuda",
            "ssp",
            "kpn",
            "ppt",
            "tode",
            "pvptode",
            "alletode",
            "allepvptode"
        };

        public SlashCommandPrivacy() : base(toggleKey: Keys.None)
        { }

        public bool OnChatCommand(string command, string[] args)
        {
            if (string.IsNullOrEmpty(command))
                return true;

            return !AllowedCommands.Contains(command);
        }
    }
}
