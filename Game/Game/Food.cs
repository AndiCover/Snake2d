using System;

namespace Game
{
    class Food
    {
        #region Variablen
        private const int stonegroesse = 10;
        private const int randstonegroesse = 40;
        private Random random = new Random();
        public int x;
        public int y;
        #endregion

        #region Konstruktor
        public Food(int width, int height, Snake snake1, Snake snake2, bool multiplayer)
        {
            bool rightPosition = NewFoodPosition(width, height, snake1, snake2, multiplayer);
            while (!rightPosition)
            {
                rightPosition = NewFoodPosition(width, height, snake1, snake2, multiplayer);
            }
        }
        #endregion

        #region Funktionen
        private bool NewFoodPosition(int width, int height, Snake snake1, Snake snake2, bool multiplayer)
        {
            this.x = random.Next(randstonegroesse, width - randstonegroesse);
            this.y = random.Next(randstonegroesse, height - randstonegroesse);

            foreach (Stone s in snake1.snake)
            {
                if (this.x >= s.x - (3 * stonegroesse) && this.x <= s.x + (3 * stonegroesse) && this.y >= s.y - (3 * stonegroesse) && this.y <= s.y + (3 * stonegroesse))
                {
                    return false;
                }
            }
            if (multiplayer)
            {
                foreach (Stone s in snake2.snake)
                {
                    if (this.x >= s.x - (3 * stonegroesse) && this.x <= s.x + (3 * stonegroesse) && this.y >= s.y - (3 * stonegroesse) && this.y <= s.y + (3 * stonegroesse))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool istInnerhalb(int x, int y)
        {
            if (x >= this.x && x <= this.x + randstonegroesse + 5 && y >= this.y && y <= this.y + randstonegroesse + 8)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}