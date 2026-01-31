using System;
using System.Collections.Generic;
using PluginLoader;

namespace TranscendPlugins
{
    public class SlashCommandPrivacy : MarshalByRefObject, IPluginChatCommand
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

        public bool OnChatCommand(string command, string[] args)
        {
            if (string.IsNullOrEmpty(command))
                return true;

            return !AllowedCommands.Contains(command);
        }
    }
}
