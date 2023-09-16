using Raylib_cs;
using System.Numerics;
using System;
using System.IO;
using ChessChallenge.API;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {

        public static Dictionary<string, ChallengeController.PlayerType> engines = new()
        {
{ "ZaphBot", ChallengeController.PlayerType.ZaphBot },
{ "Zambroni", ChallengeController.PlayerType.Zambroni },
{ "yoda2", ChallengeController.PlayerType.yoda2 },
{ "Velocity", ChallengeController.PlayerType.Velocity },
{ "Tyrant", ChallengeController.PlayerType.Tyrant },
{ "TinyChessDuck", ChallengeController.PlayerType.TinyChessDuck },
{ "ThrowverEngineered", ChallengeController.PlayerType.ThrowverEngineered },
{ "Theseus", ChallengeController.PlayerType.Theseus },
{ "SirBlundersaLot", ChallengeController.PlayerType.SirBlundersaLot },
{ "SillyBot", ChallengeController.PlayerType.SillyBot },
{ "Sentinel", ChallengeController.PlayerType.Sentinel },
{ "SemiroseBot", ChallengeController.PlayerType.SemiroseBot },
{ "Scarlet", ChallengeController.PlayerType.Scarlet },
{ "Sardine", ChallengeController.PlayerType.Sardine },
{ "ReverieV", ChallengeController.PlayerType.ReverieV },
{ "Radium", ChallengeController.PlayerType.Radium },
{ "Peeter1", ChallengeController.PlayerType.Peeter1 },
{ "Onion", ChallengeController.PlayerType.Onion },
{ "ObfuscoWeed", ChallengeController.PlayerType.ObfuscoWeed },
{ "NaviBot", ChallengeController.PlayerType.NaviBot },
{ "NarvvhalBot", ChallengeController.PlayerType.NarvvhalBot },
{ "MrJB73", ChallengeController.PlayerType.MrJB73 },
{ "minor-moves", ChallengeController.PlayerType.minorMoves },
{ "MagnuthCarlthen", ChallengeController.PlayerType.MagnuthCarlthen },
{ "Loevbotv1_2", ChallengeController.PlayerType.Loevbotv1_2 },
{ "LetMeAlive", ChallengeController.PlayerType.LetMeAlive },
{ "Leonidas", ChallengeController.PlayerType.Leonidas },
{ "Krabot", ChallengeController.PlayerType.Krabot },
{ "KnightToE4", ChallengeController.PlayerType.KnightToE4 },
{ "InfuehrDaniel", ChallengeController.PlayerType.InfuehrDaniel },
{ "GhostEngine", ChallengeController.PlayerType.GhostEngine },
{ "FloppyChessGaming", ChallengeController.PlayerType.FloppyChessGaming },
{ "FloppyChessBetter", ChallengeController.PlayerType.FloppyChessBetter },
{ "FloppyChess", ChallengeController.PlayerType.FloppyChess },
{ "DriedCod", ChallengeController.PlayerType.DriedCod },
{ "DeltaWeakness", ChallengeController.PlayerType.DeltaWeakness },
{ "DappsBot", ChallengeController.PlayerType.DappsBot },
{ "damlamen", ChallengeController.PlayerType.damlamen },
{ "Cosmos", ChallengeController.PlayerType.Cosmos },
{ "ChaosBot", ChallengeController.PlayerType.ChaosBot },
{ "BoyaChess", ChallengeController.PlayerType.BoyaChess },
{ "Botje9000", ChallengeController.PlayerType.Botje9000 },
{ "boohowaer", ChallengeController.PlayerType.boohowaer },
{ "Blaze", ChallengeController.PlayerType.Blaze },
{ "Better", ChallengeController.PlayerType.Better },
{ "BalooDominator", ChallengeController.PlayerType.BalooDominator },
{ "BadMeetsEvil", ChallengeController.PlayerType.BadMeetsEvil },
{ "Badbot", ChallengeController.PlayerType.Badbot },
{ "AngelBot", ChallengeController.PlayerType.AngelBot },
{ "Andiefietebel", ChallengeController.PlayerType.Andiefietebel },
{ "AlgernonB4729", ChallengeController.PlayerType.AlgernonB4729 },
{ "Ace", ChallengeController.PlayerType.Ace },
            { "MyBot", ChallengeController.PlayerType.MyBot },
            { "Human", ChallengeController.PlayerType.Human },
        };

        public static string[] EngineOrder = engines.Keys.ToArray();
        public static int max_page = EngineOrder.Length / 3 - (EngineOrder.Length % 3 == 0 ? 1 : 0);

        public static void DrawButtons(ChallengeController controller)
        {

            Vector2 buttonPos = UIHelper.Scale(new Vector2(210, 210));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(260, 45));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            string text = "";

            if (controller.first == null)
            {
                text += "null";
            } else
            {
                text += controller.first.ToString();
            }

            text += " : ";

            if (controller.second == null)
            {
                text += "null";
            }
            else
            {
                text += controller.second.ToString();
            }

            DrawNextText(text, ref buttonPos, spacing, buttonSize);

            buttonPos.X += 50;

            if (NextButtonInRow("<< Previous", ref buttonPos, spacing, buttonSize))
            {
                --controller.page;

                if (controller.page < 0)
                {
                    controller.page = max_page;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                int ind = controller.page * 3 + i;

                try
                {
                    if (NextButtonInRow(EngineOrder[ind], ref buttonPos, spacing, buttonSize))
                    {
                        if (controller.first == null || controller.second != null)
                        {
                            controller.first = engines[EngineOrder[ind]];
                            controller.second = null;
                        }
                        else
                        {
                            controller.second = engines[EngineOrder[ind]];
                        }
                    }
                } catch (IndexOutOfRangeException)
                {
                    NextButtonInRow("", ref buttonPos, spacing, buttonSize, true);
                }
            }

            if (NextButtonInRow("Next >>", ref buttonPos, spacing, buttonSize))
            {
                ++controller.page;

                if (controller.page > max_page)
                {
                    controller.page = 0;
                }
            }

            if (NextButtonInRow("PLAY", ref buttonPos, spacing, buttonSize))
            {
                if (controller.first != null && controller.second != null)
                {
                    controller.StartNewBotMatch(
                        (ChallengeController.PlayerType)controller.first, 
                        (ChallengeController.PlayerType)controller.second
                    );
                }
            }

            /*
            if (NextButtonInRow("Bob vs TrinketBot", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Bob, ChallengeController.PlayerType.TrinketBot);
            }
            */

            // Page buttons
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size, bool is_disabled = false)
            {
                bool pressed = UIHelper.Button(name, pos, size, is_disabled);
                pos.Y += spacingY;
                return pressed;
            }

            void DrawNextText(string text, ref Vector2 pos, float spacingY, Vector2 size)
            {
                UIHelper.DrawText(text, pos, 20, 1, Color.WHITE);
                pos.Y += spacingY;
            }
        }
    }
}