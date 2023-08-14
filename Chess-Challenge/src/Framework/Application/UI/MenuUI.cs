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

        public static Dictionary<string, ChallengeController.PlayerType> engines = new() // TODO automise it
        {
{ "EvilBot", ChallengeController.PlayerType.EvilBot },
            { "Human", ChallengeController.PlayerType.Human },
            { "MyBot", ChallengeController.PlayerType.MyBot },
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
                } catch (IndexOutOfRangeException _) 
                {
                    NextButtonInRow("", ref buttonPos, spacing, buttonSize);
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

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
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