using ChessChallenge.Chess;
using Raylib_cs;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ChessChallenge.Application.Settings;
using static ChessChallenge.Application.ConsoleHelper;
using System.Collections.Generic;
using ListRandomizer;
using ChessChallenge.Example;

namespace ChessChallenge.Application
{
    public class ChallengeController
    {

        public PlayerType? first = null;
        public PlayerType? second = null;
        public int page = 0;
        public bool isSettings = false;
        public enum PlayerType
        {
Valibot,
KurentPosition,
KingGambot,
ZaphBot,
Zambroni,
yoda2,
Velocity,
Tyrant,
TinyChessDuck,
ThrowverEngineered,
Theseus,
SirBlundersaLot,
SillyBot,
Sentinel,
SemiroseBot,
Scarlet,
Sardine,
ReverieV,
Radium,
Peeter1,
Onion,
ObfuscoWeed,
NaviBot,
NarvvhalBot,
MrJB73,
minorMoves,
MagnuthCarlthen,
Loevbotv1_2,
LetMeAlive,
Leonidas,
Krabot,
KnightToE4,
InfuehrDaniel,
GhostEngine,
FloppyChessGaming,
FloppyChessBetter,
FloppyChess,
DriedCod,
DeltaWeakness,
DappsBot,
damlamen,
Cosmos,
ChaosBot,
BoyaChess,
Botje9000,
boohowaer,
Blaze,
Better,
BalooDominator,
BadMeetsEvil,
Badbot,
AngelBot,
Andiefietebel,
AlgernonB4729,
Ace,
            MyBot,
            Human,
        }

        public static Dictionary<string, string[]> modes = new() { };

        public static Dictionary<string, string> fens = new()
        {
            {"standart", "Fens.txt" },
            {"chess960", "chess960Fens.txt" }
        };

        public string key = "standart";

        // Game state
        readonly Random rng;
        int gameID;
        public bool isPlaying;
        Board board;
        public ChessPlayer PlayerWhite { get; private set; }
        public ChessPlayer PlayerBlack { get; private set; }

        float lastMoveMadeTime;
        bool isWaitingToPlayMove;
        Move moveToPlay;
        float playMoveTime;
        public bool HumanWasWhiteLastGame { get; private set; }

        // Bot match state
        int botMatchGameIndex;
        public BotMatchStats BotStatsA { get; private set; }
        public BotMatchStats BotStatsB { get; private set; }
        bool botAPlaysWhite;


        // Bot task
        AutoResetEvent botTaskWaitHandle;
        bool hasBotTaskException;
        ExceptionDispatchInfo botExInfo;

        // Other
        readonly BoardUI boardUI;
        readonly MoveGenerator moveGenerator;
        readonly StringBuilder pgns;
        public int GamesCount = 10;
        string[] GameFens;

        public bool isMatchFinished = false;

        public ChallengeController()
        {
            Log($"Launching Tournament-Chess version {Settings.Version}");
            Warmer.Warm();

            rng = new Random();
            moveGenerator = new();
            boardUI = new BoardUI();
            board = new Board();
            pgns = new();

            BotStatsA = new BotMatchStats("IBot");
            BotStatsB = new BotMatchStats("IBot");
            foreach (string key in fens.Keys)
            {
                modes.Add(key, FileHelper.ReadResourceFile(fens[key]).Split('\n').Where(fen => fen.Length > 0).ToArray());
            }
            botTaskWaitHandle = new AutoResetEvent(false);

            StartNewGame(PlayerType.Human, PlayerType.Human);
        }

        public void StartNewGame(PlayerType whiteType, PlayerType blackType)
        {
            // End any ongoing game
            isMatchFinished = false;
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            gameID = rng.Next();

            // Stop prev task and create a new one
            if (RunBotsOnSeparateThread)
            {
                // Allow task to terminate
                botTaskWaitHandle.Set();
                // Create new task
                botTaskWaitHandle = new AutoResetEvent(false);
                Task.Factory.StartNew(BotThinkerThread, TaskCreationOptions.LongRunning);
            }

            if (whiteType != PlayerType.Human && blackType != PlayerType.Human)
            {
                SelectPositions();
            } else
            {
                GameFens = modes[key];
            }
            // Board Setup
            board = new Board();
            bool isGameWithHuman = whiteType is PlayerType.Human || blackType is PlayerType.Human;
            int fenIndex = isGameWithHuman ? 0 : botMatchGameIndex / 2;
            board.LoadPosition(GameFens[fenIndex]);
            ConsoleHelper.Log(GameFens[fenIndex]);

            // Player Setup
            PlayerWhite = CreatePlayer(whiteType);
            PlayerBlack = CreatePlayer(blackType);
            PlayerWhite.SubscribeToMoveChosenEventIfHuman(OnMoveChosen);
            PlayerBlack.SubscribeToMoveChosenEventIfHuman(OnMoveChosen);

            // UI Setup
            boardUI.UpdatePosition(board);
            boardUI.ResetSquareColours();
            SetBoardPerspective();

            // Start
            isPlaying = true;
            NotifyTurnToMove();
        }

        void SelectPositions()
        {
            List<string> j = modes[key].ToList();
            ListRandomizer.ListExtension.Shuffle(j);
            GameFens = j.Take(GamesCount).ToArray();
        }


        void BotThinkerThread()
        {
            int threadID = gameID;
            //Console.WriteLine("Starting thread: " + threadID);

            while (true)
            {
                // Sleep thread until notified
                botTaskWaitHandle.WaitOne();
                // Get bot move
                if (threadID == gameID)
                {
                    var move = GetBotMove();

                    if (threadID == gameID)
                    {
                        OnMoveChosen(move);
                    }
                }
                // Terminate if no longer playing this game
                if (threadID != gameID)
                {
                    break;
                }
            }
            //Console.WriteLine("Exitting thread: " + threadID);
        }

        Move GetBotMove()
        {
            API.Board botBoard = new(board);
            try
            {
                API.Timer timer = new(PlayerToMove.TimeRemainingMs, PlayerNotOnMove.TimeRemainingMs, GameDurationMilliseconds, IncrementMilliseconds);
                API.Move move = PlayerToMove.Bot.Think(botBoard, timer);
                return new Move(move.RawValue);
            }
            catch (Exception e)
            {
                Log($"An error occurred while {PlayerToMove.PlayerType} was thinking.\n" + e.ToString(), true, ConsoleColor.Red);
                hasBotTaskException = true;
                botExInfo = ExceptionDispatchInfo.Capture(e);
            }
            return Move.NullMove;
        }



        void NotifyTurnToMove()
        {
            //playerToMove.NotifyTurnToMove(board);
            if (PlayerToMove.IsHuman)
            {
                PlayerToMove.Human.SetPosition(FenUtility.CurrentFen(board));
                PlayerToMove.Human.NotifyTurnToMove();
            }
            else
            {
                if (RunBotsOnSeparateThread)
                {
                    botTaskWaitHandle.Set();
                }
                else
                {
                    double startThinkTime = Raylib.GetTime();
                    var move = GetBotMove();
                    double thinkDuration = Raylib.GetTime() - startThinkTime;
                    PlayerToMove.UpdateClock(thinkDuration);
                    OnMoveChosen(move);
                }
            }
        }

        void SetBoardPerspective()
        {
            boardUI.SetPerspective(true);
        }

        ChessPlayer CreatePlayer(PlayerType type)
        {
            return type switch
            {
PlayerType.Valibot => new ChessPlayer(new Valibot(), type, GameDurationMilliseconds),
PlayerType.KurentPosition => new ChessPlayer(new KurentPosition(), type, GameDurationMilliseconds),
PlayerType.KingGambot => new ChessPlayer(new KingGambot(), type, GameDurationMilliseconds),
PlayerType.ZaphBot => new ChessPlayer(new ZaphBot(), type, GameDurationMilliseconds),
PlayerType.Zambroni => new ChessPlayer(new Zambroni(), type, GameDurationMilliseconds),
PlayerType.yoda2 => new ChessPlayer(new yoda2(), type, GameDurationMilliseconds),
PlayerType.Velocity => new ChessPlayer(new Velocity(), type, GameDurationMilliseconds),
PlayerType.Tyrant => new ChessPlayer(new Tyrant(), type, GameDurationMilliseconds),
PlayerType.TinyChessDuck => new ChessPlayer(new TinyChessDuck(), type, GameDurationMilliseconds),
PlayerType.ThrowverEngineered => new ChessPlayer(new ThrowverEngineered(), type, GameDurationMilliseconds),
PlayerType.Theseus => new ChessPlayer(new Theseus(), type, GameDurationMilliseconds),
PlayerType.SirBlundersaLot => new ChessPlayer(new SirBlundersaLot(), type, GameDurationMilliseconds),
PlayerType.SillyBot => new ChessPlayer(new SillyBot(), type, GameDurationMilliseconds),
PlayerType.Sentinel => new ChessPlayer(new Sentinel(), type, GameDurationMilliseconds),
PlayerType.SemiroseBot => new ChessPlayer(new SemiroseBot(), type, GameDurationMilliseconds),
PlayerType.Scarlet => new ChessPlayer(new Scarlet(), type, GameDurationMilliseconds),
PlayerType.Sardine => new ChessPlayer(new Sardine(), type, GameDurationMilliseconds),
PlayerType.ReverieV => new ChessPlayer(new ReverieV(), type, GameDurationMilliseconds),
PlayerType.Radium => new ChessPlayer(new Radium(), type, GameDurationMilliseconds),
PlayerType.Peeter1 => new ChessPlayer(new Peeter1(), type, GameDurationMilliseconds),
PlayerType.Onion => new ChessPlayer(new Onion(), type, GameDurationMilliseconds),
PlayerType.ObfuscoWeed => new ChessPlayer(new ObfuscoWeed(), type, GameDurationMilliseconds),
PlayerType.NaviBot => new ChessPlayer(new NaviBot(), type, GameDurationMilliseconds),
PlayerType.NarvvhalBot => new ChessPlayer(new NarvvhalBot(), type, GameDurationMilliseconds),
PlayerType.MrJB73 => new ChessPlayer(new MrJB73(), type, GameDurationMilliseconds),
PlayerType.minorMoves => new ChessPlayer(new minorMoves(), type, GameDurationMilliseconds),
PlayerType.MagnuthCarlthen => new ChessPlayer(new MagnuthCarlthen(), type, GameDurationMilliseconds),
PlayerType.Loevbotv1_2 => new ChessPlayer(new Loevbotv1_2(), type, GameDurationMilliseconds),
PlayerType.LetMeAlive => new ChessPlayer(new LetMeAlive(), type, GameDurationMilliseconds),
PlayerType.Leonidas => new ChessPlayer(new Leonidas(), type, GameDurationMilliseconds),
PlayerType.Krabot => new ChessPlayer(new Krabot(), type, GameDurationMilliseconds),
PlayerType.KnightToE4 => new ChessPlayer(new KnightToE4(), type, GameDurationMilliseconds),
PlayerType.InfuehrDaniel => new ChessPlayer(new InfuehrDaniel(), type, GameDurationMilliseconds),
PlayerType.GhostEngine => new ChessPlayer(new GhostEngine(), type, GameDurationMilliseconds),
PlayerType.FloppyChessGaming => new ChessPlayer(new FloppyChessGaming(), type, GameDurationMilliseconds),
PlayerType.FloppyChessBetter => new ChessPlayer(new FloppyChessBetter(), type, GameDurationMilliseconds),
PlayerType.FloppyChess => new ChessPlayer(new FloppyChess(), type, GameDurationMilliseconds),
PlayerType.DriedCod => new ChessPlayer(new DriedCod(), type, GameDurationMilliseconds),
PlayerType.DeltaWeakness => new ChessPlayer(new DeltaWeakness(), type, GameDurationMilliseconds),
PlayerType.DappsBot => new ChessPlayer(new DappsBot(), type, GameDurationMilliseconds),
PlayerType.damlamen => new ChessPlayer(new damlamen(), type, GameDurationMilliseconds),
PlayerType.Cosmos => new ChessPlayer(new Cosmos(), type, GameDurationMilliseconds),
PlayerType.ChaosBot => new ChessPlayer(new ChaosBot(), type, GameDurationMilliseconds),
PlayerType.BoyaChess => new ChessPlayer(new BoyaChess(), type, GameDurationMilliseconds),
PlayerType.Botje9000 => new ChessPlayer(new Botje9000(), type, GameDurationMilliseconds),
PlayerType.boohowaer => new ChessPlayer(new boohowaer(), type, GameDurationMilliseconds),
PlayerType.Blaze => new ChessPlayer(new Blaze(), type, GameDurationMilliseconds),
PlayerType.Better => new ChessPlayer(new Better(), type, GameDurationMilliseconds),
PlayerType.BalooDominator => new ChessPlayer(new BalooDominator(), type, GameDurationMilliseconds),
PlayerType.BadMeetsEvil => new ChessPlayer(new BadMeetsEvil(), type, GameDurationMilliseconds),
PlayerType.Badbot => new ChessPlayer(new Badbot(), type, GameDurationMilliseconds),
PlayerType.AngelBot => new ChessPlayer(new AngelBot(), type, GameDurationMilliseconds),
PlayerType.Andiefietebel => new ChessPlayer(new Andiefietebel(), type, GameDurationMilliseconds),
PlayerType.AlgernonB4729 => new ChessPlayer(new AlgernonB4729(), type, GameDurationMilliseconds),
PlayerType.Ace => new ChessPlayer(new Ace(), type, GameDurationMilliseconds),
                PlayerType.MyBot => new ChessPlayer(new MyBot(), type, GameDurationMilliseconds),
                _ => new ChessPlayer(new HumanPlayer(boardUI), type)
            };
        }

        static (int totalTokenCount, int debugTokenCount) GetTokenCount()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "src", "My Bot", "MyBot.cs");

            using StreamReader reader = new(path);
            string txt = reader.ReadToEnd();
            return TokenCounter.CountTokens(txt);
        }

        void OnMoveChosen(Move chosenMove)
        {
            if (IsLegal(chosenMove))
            {
                PlayerToMove.AddIncrement(IncrementMilliseconds);
                if (PlayerToMove.IsBot)
                {
                    moveToPlay = chosenMove;
                    isWaitingToPlayMove = true;
                    playMoveTime = lastMoveMadeTime + MinMoveDelay;
                }
                else
                {
                    PlayMove(chosenMove);
                }
            }
            else
            {
                string moveName = MoveUtility.GetMoveNameUCI(chosenMove);
                string log = $"Illegal move: {moveName} in position: {FenUtility.CurrentFen(board)}";
                Log(log, true, ConsoleColor.Red);
                GameResult result = PlayerToMove == PlayerWhite ? GameResult.WhiteIllegalMove : GameResult.BlackIllegalMove;
                EndGame(result);
            }
        }

        void PlayMove(Move move)
        {
            if (isPlaying)
            {
                bool animate = PlayerToMove.IsBot;
                lastMoveMadeTime = (float)Raylib.GetTime();

                board.MakeMove(move, false);
                boardUI.UpdatePosition(board, move, animate);

                GameResult result = Arbiter.GetGameState(board);
                if (result == GameResult.InProgress)
                {
                    NotifyTurnToMove();
                }
                else
                {
                    EndGame(result);
                }
            }
        }

        public void EndGame(GameResult result, bool log = true, bool autoStartNextBotMatch = true)
        {
            if (isPlaying)
            {
                isPlaying = false;
                isWaitingToPlayMove = false;
                gameID = -1;

                if (log)
                {
                    Log("Game Over: " + result, false, ConsoleColor.Blue);
                }

                string pgn = PGNCreator.CreatePGN(board, result, GetPlayerName(PlayerWhite), GetPlayerName(PlayerBlack));
                pgns.AppendLine(pgn);

                // If 2 bots playing each other, start next game automatically.
                if (PlayerWhite.IsBot && PlayerBlack.IsBot)
                {
                    UpdateBotMatchStats(result);
                    botMatchGameIndex++;
                    int numGamesToPlay = GameFens.Length * 2;

                    if (botMatchGameIndex < numGamesToPlay && autoStartNextBotMatch)
                    {
                        botAPlaysWhite = !botAPlaysWhite;
                        const int startNextGameDelayMs = 600;
                        System.Timers.Timer autoNextTimer = new(startNextGameDelayMs);
                        int originalGameID = gameID;
                        autoNextTimer.Elapsed += (s, e) => AutoStartNextBotMatchGame(originalGameID, autoNextTimer);
                        autoNextTimer.AutoReset = false;
                        autoNextTimer.Start();
                    }
                    else if (autoStartNextBotMatch)
                    {
                        Log("Match finished", false, ConsoleColor.Blue);
                        isMatchFinished = true;
                    }
                }
            }
        }

        private void AutoStartNextBotMatchGame(int originalGameID, System.Timers.Timer timer)
        {
            if (originalGameID == gameID)
            {
                StartNewGame(PlayerBlack.PlayerType, PlayerWhite.PlayerType);
            }
        }


        void UpdateBotMatchStats(GameResult result)
        {
            UpdateStats(BotStatsA, botAPlaysWhite);
            UpdateStats(BotStatsB, !botAPlaysWhite);

            void UpdateStats(BotMatchStats stats, bool isWhiteStats)
            {
                // Draw
                if (Arbiter.IsDrawResult(result))
                {
                    stats.NumDraws++;
                }
                // Win
                else if (Arbiter.IsWhiteWinsResult(result) == isWhiteStats)
                {
                    stats.NumWins++;
                }
                // Loss
                else
                {
                    stats.NumLosses++;
                    stats.NumTimeouts += (result is GameResult.WhiteTimeout or GameResult.BlackTimeout) ? 1 : 0;
                    stats.NumIllegalMoves += (result is GameResult.WhiteIllegalMove or GameResult.BlackIllegalMove) ? 1 : 0;
                }
            }
        }

        public void Update()
        {
            if (isPlaying)
            {
                PlayerWhite.Update();
                PlayerBlack.Update();

                PlayerToMove.UpdateClock(Raylib.GetFrameTime());
                if (PlayerToMove.TimeRemainingMs <= 0)
                {
                    EndGame(PlayerToMove == PlayerWhite ? GameResult.WhiteTimeout : GameResult.BlackTimeout);
                }
                else
                {
                    if (isWaitingToPlayMove && Raylib.GetTime() > playMoveTime)
                    {
                        isWaitingToPlayMove = false;
                        PlayMove(moveToPlay);
                    }
                }
            }

            if (hasBotTaskException)
            {
                hasBotTaskException = false;
                botExInfo.Throw();
            }
        }

        public void Draw()
        {
            boardUI.Draw();
            string nameW = GetPlayerName(PlayerWhite);
            string nameB = GetPlayerName(PlayerBlack);
            boardUI.DrawPlayerNames(nameW, nameB, PlayerWhite.TimeRemainingMs, PlayerBlack.TimeRemainingMs, isPlaying);
        }

        public void DrawOverlay()
        {
            MenuUI.DrawButtons(this);
            if (isMatchFinished)
            {
                MatchStatsUI.MatchEnded(this);
            } else {
                MatchStatsUI.DrawMatchStats(this);
            }
        }

        static string GetPlayerName(ChessPlayer player) => GetPlayerName(player.PlayerType);
        static string GetPlayerName(PlayerType type) => type.ToString();

        public void StartNewBotMatch(PlayerType botTypeA, PlayerType botTypeB)
        {
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            botMatchGameIndex = 0;
            string nameA = GetPlayerName(botTypeA);
            string nameB = GetPlayerName(botTypeB);
            if (nameA == nameB)
            {
                nameA += " (A)";
                nameB += " (B)";
            }
            BotStatsA = new BotMatchStats(nameA);
            BotStatsB = new BotMatchStats(nameB);
            botAPlaysWhite = true;
            
            Log($"Starting new match: {nameA} vs {nameB}", false, ConsoleColor.Blue);
            StartNewGame(botTypeA, botTypeB);
        }


        ChessPlayer PlayerToMove => board.IsWhiteToMove ? PlayerWhite : PlayerBlack;
        ChessPlayer PlayerNotOnMove => board.IsWhiteToMove ? PlayerBlack : PlayerWhite;

        public int TotalGameCount => GamesCount * 2;
        public int CurrGameNumber => Math.Min(TotalGameCount, botMatchGameIndex + 1);
        public string AllPGNs => pgns.ToString();


        bool IsLegal(Move givenMove)
        {
            var moves = moveGenerator.GenerateMoves(board);
            foreach (var legalMove in moves)
            {
                if (givenMove.Value == legalMove.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public class BotMatchStats
        {
            public string BotName;
            public int NumWins;
            public int NumLosses;
            public int NumDraws;
            public int NumTimeouts;
            public int NumIllegalMoves;

            public BotMatchStats(string name) => BotName = name;
        }

        public void Release()
        {
            boardUI.Release();
        }
    }
}