namespace Game
{
    class Player
    {
        #region Variablen
        public Snake snake;
        public int score;
        public string name;
        public int wins = 0;
        #endregion

        #region Konstruktor
        public Player(int score, int xdir, int ydir, int x, int y, int len, int speed, string name)
        {
            this.score = score;
            this.name = name;
            snake = new Snake(xdir, ydir, x, y, len, speed);
        }
        #endregion
    }
}