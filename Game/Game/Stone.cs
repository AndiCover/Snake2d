namespace Game
{
    public class Stone
    {
        #region Variablen
        private const int stonegroesse = 10;
        public int x;
        public int y;
        public int xdir;
        public int ydir;
        public int pointsreached;
        #endregion

        #region Konstruktor
        public Stone(int x, int y, int xdir, int ydir)
        {
            this.x = x;
            this.y = y;
            this.xdir = xdir;
            this.ydir = ydir;
            this.pointsreached = 0;
        }
        #endregion

        #region Funktionen
        public bool istInnerhalb(int x, int y, int xdir, int ydir)
        {
            if (ydir < 0)
            {
                if (x >= this.x && x <= this.x + stonegroesse - 5 && y - stonegroesse >= this.y && y - stonegroesse <= this.y)
                {
                    return true;
                }
            }
            else if (ydir > 0)
            {
                if (x >= this.x && x <= this.x + stonegroesse - 5 && y + stonegroesse >= this.y && y + stonegroesse <= this.y)
                {
                    return true;
                }
            }
            else if (xdir < 0)
            {
                if (x - stonegroesse >= this.x && x - stonegroesse <= this.x + stonegroesse - 5 && y >= this.y && y <= this.y)
                {
                    return true;
                }
            }
            else
            {
                if (x + stonegroesse >= this.x && x + stonegroesse <= this.x + stonegroesse - 5 && y >= this.y && y <= this.y)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}