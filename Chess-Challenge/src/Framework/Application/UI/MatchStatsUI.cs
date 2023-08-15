using Raylib_cs;
using System.Numerics;
using System;

namespace ChessChallenge.Application
{
    public static class MatchStatsUI
    {
        public static void DrawMatchStats(ChallengeController controller)
        {
            if (controller.PlayerWhite.IsBot && controller.PlayerBlack.IsBot)
            {
                int nameFontSize = UIHelper.ScaleInt(40);
                int regularFontSize = UIHelper.ScaleInt(35);
                int headerFontSize = UIHelper.ScaleInt(45);
                Color col = new(180, 180, 180, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 250));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                DrawNextText($"Score: {controller.BotStatsA.NumWins} - {controller.BotStatsB.NumWins}", headerFontSize, Color.WHITE);
                DrawNextText($"Draws: {controller.BotStatsA.NumDraws}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);
           

                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                }
           
                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            }
        }

        public static void MatchEnded(ChallengeController controller)
        {
            int nameFontSize = UIHelper.ScaleInt(40);
            int regularFontSize = UIHelper.ScaleInt(35);
            int headerFontSize = UIHelper.ScaleInt(45);
            Color col = new(180, 180, 180, 255);
            Vector2 startPos = UIHelper.Scale(new Vector2(1500, 250));
            float spacingY = UIHelper.Scale(35);

            DrawNextText($"{controller.BotStatsA.BotName}: {controller.BotStatsA.NumWins}", nameFontSize, Color.WHITE);
            DrawNextText($"{controller.BotStatsB.BotName}: {controller.BotStatsB.NumWins}", nameFontSize, Color.WHITE);

            if (controller.BotStatsA.NumWins == controller.BotStatsB.NumWins)
            {
                DrawNextText($"Draw!", nameFontSize, Color.WHITE);
            } else if (controller.BotStatsA.NumWins > controller.BotStatsB.NumWins) {
                DrawNextText($"{controller.BotStatsA.BotName}: wins!", nameFontSize, Color.WHITE);
            } else {
                DrawNextText($"{controller.BotStatsB.BotName}: wins!", nameFontSize, Color.WHITE);
            }

            void DrawNextText(string text, int fontSize, Color col)
            {
                UIHelper.DrawText(text, startPos, fontSize, 1, col);
                startPos.Y += spacingY;
            }
        }
    }
}