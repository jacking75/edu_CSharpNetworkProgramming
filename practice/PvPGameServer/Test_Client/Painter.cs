using System.Drawing;

namespace csharp_test_client
{
    public class Painter
    {
        public const int MokSize = 30;
        private const int BasePosX = 0;
        private const int BasePosY = 540;
        public int PanelPosX;
        public int PanelPosY;
        public Color MokColor;

        public void Input(int posX, int posY, Mok mok)
        {
            PanelPosX = BasePosX + posX * MokSize;
            PanelPosY = BasePosY - posY * MokSize;
            MokColor = mok == Mok.Black ? Color.Black : Color.White;
        }
    }
    
    public enum Mok
    {
        None = 0,
        Black = 1,
        White = 2,
        Skip = 3,
    }
}