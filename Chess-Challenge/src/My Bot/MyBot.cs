using ChessChallenge.API;
using System;
using System.Collections.Generic;

public struct EMove
{
    public Move move;
    public float eval;
}

public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        return GetBestMove(board);
    }

    public Move GetBestMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        EMove best_move = new()
        {
            move = Move.NullMove, 
            eval = float.NegativeInfinity
        };

        foreach (Move move in moves)
        {
            EMove curr = EvalMove(board, move);
            if (curr.eval > best_move.eval)
            {
                best_move = curr;
            }
        }

        return best_move.move;
    }

    public EMove EvalMove(Board board, Move move)
    {
        board.MakeMove(move);
        float eval = evaluate(board);
        board.UndoMove(move);
        return new EMove { 
            eval = eval, 
            move = move 
        };
    }

    public float evaluate(Board board)
    {
        float value = 0;

        value += Evaluate.material(board, board.IsWhiteToMove) * 1.0f;

        return value;
    }
}


class Evaluate
{
    public static Dictionary<PieceType, int> value = new()
    {
        { PieceType.None, 0},
        { PieceType.Pawn, 1},
        { PieceType.Knight, 3},
        { PieceType.Bishop, 3},
        { PieceType.Rook, 5},
        { PieceType.Queen, 9},
        { PieceType.King, 0},

    };

    public static int material(Board board, bool isWhiteToMove)
    {
        int value = 0;

        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                value += Evaluate.value[piece.PieceType] * (piece.IsWhite == isWhiteToMove ? 1 : -1);
            }
        }

        return value;
    }
}