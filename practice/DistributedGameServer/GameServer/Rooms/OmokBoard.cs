using System;
using PvPGameServer.Enum;

namespace PvPGameServer.Rooms
{
    public enum Mok
    {
        None = 0,
        Black = 1,
        White = 2,
        Skip = 3,
    }

    public enum OMokResult
    {
        None = 0,
        WinBlack = 1,
        WinWhite = 2,
        DrawByAfk = 3,
    }
    public class OmokBoard
    {
        //오목판 길이
        const int ROW_COUNT = 18;

        Mok[,] Board;

        public int Turn { get; set; }

        public void Start()
        {
            Board = new Mok[ROW_COUNT, ROW_COUNT];
            for (var x = 0; x < ROW_COUNT; x++)
            {
                for (var y = 0; y < ROW_COUNT; y++)
                {
                    Board[x, y] = Mok.None;
                }
            }

            Turn = 1;
        }
        public ErrorCode PutMok(Mok mok, int posX, int posY, int turn)
        {
            if (turn != Turn)
            {
                return ErrorCode.OMOK_TURN_NOT_MATCH;
            }
            
            if (mok == Mok.Skip)
            {
                Turn++;
            }
            else if (mok == Mok.Black || mok == Mok.White)
            {
                // 위치 체크
                if (posX < 0 || posX >= ROW_COUNT || posY < 0 || posY >= ROW_COUNT)
                {
                    return ErrorCode.OMOK_OVERFLOW;
                }

                // 중복 체크
                if (Board[posX, posY] != Mok.None)
                {
                    return ErrorCode.OMOK_ALREADY_EXIST;
                }

                // 쌍삼 등 체크
                if (!IsValidPosition(posX, posY, mok))
                {
                    return ErrorCode.OMOK_RENJURULE;
                }

                // 턴 체크
                if ((Turn % 2 == 0 && mok != Mok.White) || Turn % 2 == 1 && mok != Mok.Black)
                {
                    return ErrorCode.OMOK_TURN_NOT_MATCH;
                }

                Board[posX, posY] = mok;
                Turn++;
            }

            return ErrorCode.None;
        }

        public OMokResult CheckWinMok(Mok mok, int turnSkipCnt)
        {
            // 임시로 5턴쨰 두는 사람이 승리
            if (Turn >= 5)
            {
                return (OMokResult) mok;
            }

            // 특정 횟수 연속 턴 스킵할 경우, 무승부
            if (turnSkipCnt >= 4)
            {
                return OMokResult.DrawByAfk;
            }
            
            return OMokResult.None;
        }
        
        bool IsValidPosition(int posX, int posY, Mok mok)
        {
            //TODO 렌주룰(쌍삼) 체크
            return true;
        }
    }
}