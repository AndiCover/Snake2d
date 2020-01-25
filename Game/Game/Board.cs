namespace Game
{
    public class Board
    {
        #region Variablen
        private const int stonegroesse = 10;
        private int x;
        private int y;
        public int width;
        public int height;
        #endregion

        #region Konstruktor
        public Board(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        #endregion

        #region Funktionen
        public bool istInnerhalb(int x, int y)
        {
            if (x - stonegroesse > this.x && y - stonegroesse > this.y && x + stonegroesse < width && y + stonegroesse < height)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}