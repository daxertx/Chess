using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chess_Logic.classes.chess_piece;
using Chess_Logic.classes.chess_piece.pieces;

namespace Chess_Logic.classes.AiHandler
{
    // Runs the Stockfish chess engine process to calculate AI moves.
    public class Stock_Fish_Handler : IDisposable
    {
        private static string engine_path = "files\\ai_models\\stockfish\\stockfish-windows-x86-64-avx2.exe";

        private int default_think_ms = 500;
        private Process engine;
        private readonly object engine_lock = new object();

        public Stock_Fish_Handler()
        {
        }

        // Returns the full path to the Stockfish executable.
        public static string GetEnginePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, engine_path);
        }

        // Checks if the Stockfish executable exists on disk.
        public static bool IsAvailable()
        {
            return File.Exists(GetEnginePath());
        }

        // Starts the Stockfish process if it is not already running.
        private bool StartIfNeeded()
        {
            if (this.engine != null && !this.engine.HasExited)
                return true;

            if (!IsAvailable())
                return false;

            ProcessStartInfo info = new ProcessStartInfo(GetEnginePath());
            info.UseShellExecute        = false;
            info.CreateNoWindow         = true;
            info.RedirectStandardInput  = true;
            info.RedirectStandardOutput = true;

            this.engine = Process.Start(info);
            if (this.engine == null)
                return false;

            Send("uci");
            if (!WaitFor("uciok"))
                return false;

            Send("isready");
            if (!WaitFor("readyok"))
                return false;

            return true;
        }

        // Sends one UCI command line to Stockfish.
        private void Send(string command)
        {
            this.engine.StandardInput.WriteLine(command);
            this.engine.StandardInput.Flush();
        }

        // Reads lines from Stockfish until a line starts with 'word'.
        private bool WaitFor(string word)
        {
            while (true)
            {
                string line = this.engine.StandardOutput.ReadLine();
                if (line == null)
                    return false;
                if (line.StartsWith(word))
                    return true;
            }
        }

        // Reads and returns the line from Stockfish that starts with 'word'.
        private string ReadUntil(string word)
        {
            while (true)
            {
                string line = this.engine.StandardOutput.ReadLine();
                if (line == null)
                    return null;
                if (line.StartsWith(word))
                    return line;
            }
        }

        // Stops and cleans up the Stockfish process.
        public void Dispose()
        {
            lock (this.engine_lock)
            {
                if (this.engine == null)
                    return;

                try
                {
                    if (!this.engine.HasExited)
                    {
                        Send("quit");
                        this.engine.WaitForExit(500);
                    }
                }
                catch { }

                try
                {
                    if (!this.engine.HasExited)
                        this.engine.Kill();
                }
                catch { }

                this.engine = null;
            }
        }

        // Asks Stockfish for the best move using the default thinking time.
        public Game_move RunMoveStockFish(Chess_piece[,] board, Team_color to_move)
        {
            return RunMoveStockFish(board, to_move, default_think_ms);
        }

        // Sends the board in FEN format to Stockfish and returns its chosen move.
        public Game_move RunMoveStockFish(Chess_piece[,] board, Team_color to_move, int think_ms)
        {
            if (board == null)
                return new Game_move();

            string best;
            lock (this.engine_lock)
            {
                if (!StartIfNeeded())
                    return new Game_move();

                try
                {
                    Send("position fen " + BoardToFen(board, to_move));
                    Send("go movetime " + think_ms);

                    string line = ReadUntil("bestmove");
                    if (line == null)
                        return new Game_move();

                    string[] parts = line.Split(' ');
                    if (parts.Length < 2)
                        return new Game_move();
                    best = parts[1];
                }
                catch
                {
                    Dispose();
                    return new Game_move();
                }
            }

            if (best == "(none)")
                return new Game_move();

            return TranslateMoveFromStockFish(best, board);
        }

        // Converts a Game_move into UCI text format (e.g., "e2e4").
        public string TranslateMoveToStockFish(Game_move move)
        {
            string from = SquareToText(move.GetFromX(), move.GetFromY());
            string to   = SquareToText(move.GetToX(),   move.GetToY());

            string back = from + to;

            if (move.GetPieceType() != Chess_piece_type.Pawn && (move.GetToX() == 0 || move.GetToX() == 7))
                back = back + PieceToPromotionLetter(move.GetPieceType());

            return back;
        }

        // Converts UCI move text (e.g., "e2e4") back into a Game_move object.
        public Game_move TranslateMoveFromStockFish(string text, Chess_piece[,] board)
        {
            if (text == null || text.Length < 4)
                return new Game_move();

            int fromY = ToNumber(text[0]);
            int fromX = text[1] - '1';
            int toY   = ToNumber(text[2]);
            int toX   = text[3] - '1';

            if (!Logic.InBoard(fromX, fromY) || !Logic.InBoard(toX, toY))
                return new Game_move();

            Chess_piece_type type = board[fromX, fromY].Gettype();
            Team_color       team = board[fromX, fromY].GetTeamColor();

            if (text.Length >= 5)
                type = PromotionLetterToPiece(text[4]);

            Game_move move = new Game_move(fromX, fromY, toX, toY, type, team);
            move.SetMovedTwice(Logic.IsDoublePawnAdvance(board, move));

            return move;
        }

        // Converts board coordinates (x, y) to algebraic square name (e.g., "e4").
        private string SquareToText(int x, int y)
        {
            return ToLetter(y).ToString() + (x + 1).ToString();
        }

        public char ToLetter(int num)
        {
            return (char)('a' + num);
        }

        public int ToNumber(char c)
        {
            return c - 'a';
        }

        private char PieceToPromotionLetter(Chess_piece_type type)
        {
            if (type == Chess_piece_type.Rook)   return 'r';
            if (type == Chess_piece_type.Bishop) return 'b';
            if (type == Chess_piece_type.Knight) return 'n';
            return 'q';
        }

        private Chess_piece_type PromotionLetterToPiece(char c)
        {
            if (c == 'r') return Chess_piece_type.Rook;
            if (c == 'b') return Chess_piece_type.Bishop;
            if (c == 'n') return Chess_piece_type.Knight;
            return Chess_piece_type.Queen;
        }

        // Converts the 8x8 board into a FEN string for Stockfish.
        public string BoardToFen(Chess_piece[,] board, Team_color to_move)
        {
            StringBuilder fen = new StringBuilder();

            for (int x = 7; x >= 0; x--)
            {
                int empty_run = 0;

                for (int y = 0; y < 8; y++)
                {
                    char letter = PieceToFenLetter(board[x, y]);

                    if (letter == ' ')
                    {
                        empty_run++;
                        continue;
                    }

                    if (empty_run > 0)
                    {
                        fen.Append(empty_run);
                        empty_run = 0;
                    }
                    fen.Append(letter);
                }

                if (empty_run > 0)
                    fen.Append(empty_run);
                if (x > 0)
                    fen.Append('/');
            }

            fen.Append(' ');
            if (to_move == Team_color.White)
                fen.Append('w');
            else
                fen.Append('b');

            fen.Append(' ');
            fen.Append(CastlingRights(board));

            fen.Append(' ');
            fen.Append(EnPassantSquare(board));

            fen.Append(" 0 1");

            return fen.ToString();
        }

        // Returns the FEN character for a single piece.
        private char PieceToFenLetter(Chess_piece piece)
        {
            Chess_piece_type type = piece.Gettype();
            if (type == Chess_piece_type.Empty)
                return ' ';

            char letter = 'p';
            if (type == Chess_piece_type.Rook)   letter = 'r';
            if (type == Chess_piece_type.Knight) letter = 'n';
            if (type == Chess_piece_type.Bishop) letter = 'b';
            if (type == Chess_piece_type.Queen)  letter = 'q';
            if (type == Chess_piece_type.King)   letter = 'k';

            if (piece.GetTeamColor() == Team_color.White)
                letter = char.ToUpper(letter);

            return letter;
        }

        // Builds the FEN castling rights string (KQkq or -).
        private string CastlingRights(Chess_piece[,] board)
        {
            string rights = "";

            if (!HasKingMoved(board, 0))
            {
                if (!HasRookMoved(board, 0, 7))
                    rights = rights + "K";
                if (!HasRookMoved(board, 0, 0))
                    rights = rights + "Q";
            }

            if (!HasKingMoved(board, 7))
            {
                if (!HasRookMoved(board, 7, 7))
                    rights = rights + "k";
                if (!HasRookMoved(board, 7, 0))
                    rights = rights + "q";
            }

            if (rights == "")
                return "-";
            return rights;
        }

        private bool HasKingMoved(Chess_piece[,] board, int x)
        {
            King king = board[x, 4] as King;
            if (king == null)
                return true;
            return king.HasMoved();
        }

        private bool HasRookMoved(Chess_piece[,] board, int x, int y)
        {
            Rook rook = board[x, y] as Rook;
            if (rook == null)
                return true;
            return rook.HasMoved();
        }

        // Returns the en-passant target square in FEN notation, or "-" if none.
        private string EnPassantSquare(Chess_piece[,] board)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Pawn pawn = board[x, y] as Pawn;
                    if (pawn == null)
                        continue;
                    if (!pawn.HasMovedTwice())
                        continue;

                    if (pawn.GetTeamColor() == Team_color.White)
                        return SquareToText(x - 1, y);

                    return SquareToText(x + 1, y);
                }
            }

            return "-";
        }
    }
}
