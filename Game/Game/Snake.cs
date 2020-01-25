using System.Collections.Generic;

namespace Game
{
    public class Snake
    {
        #region Variablen
        private const int stonegroesse = 10;
        public int xdir;
        public int ydir;
        public int len;
        public bool lost = false;
        public bool leftispressed = false;
        public bool rightispressed = false;
        public bool downispressed = false;
        public bool upispressed = false;
        public List<Stone> snake = new List<Stone>();
        public List<Turn> turns = new List<Turn>();
        #endregion

        #region Konstruktor
        public Snake(int xdir, int ydir, int x, int y, int len, int speed)
        {
            this.xdir = xdir;
            this.ydir = ydir;
            this.len = len;
            CreateSnake(x, y, len);
        }
        #endregion

        #region Funktionen
        public void CreateSnake(int x, int y, int len)
        {
            Stone s;
            for (int i = 0; i < len; i++)
            {
                s = new Stone(x, y, xdir, ydir);
                snake.Add(s);
                y += stonegroesse;
            }
        }
        public void Add()
        {
            this.len++;
            Stone s;
            if ((snake[snake.Count - 1].xdir > 0))
            {
                s = new Stone(snake[snake.Count - 1].x - stonegroesse, snake[snake.Count - 1].y, snake[snake.Count - 1].xdir, 0);
            }
            else if ((snake[snake.Count - 1].xdir < 0))
            {
                s = new Stone(snake[snake.Count - 1].x + stonegroesse, snake[snake.Count - 1].y, snake[snake.Count - 1].xdir, 0);
            }
            else if ((snake[snake.Count - 1].ydir > 0))
            {
                s = new Stone(snake[snake.Count - 1].x, snake[snake.Count - 1].y - stonegroesse, 0, snake[snake.Count - 1].ydir);
            }
            else
            {
                s = new Stone(snake[snake.Count - 1].x, snake[snake.Count - 1].y + stonegroesse, 0, snake[snake.Count - 1].ydir);
            }
            s.pointsreached = snake[snake.Count - 1].pointsreached - 1;
            snake.Add(s);
        }
        #endregion
    }
}