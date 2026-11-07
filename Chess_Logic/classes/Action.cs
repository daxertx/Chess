using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic.classes
{
    public enum Action
    {
        try_move,
        is_check,
        is_checkmate,
        is_stalemate,
        is_castling_move
    }
}
