using Microsoft.Xna.Framework;

namespace Game
{
    public class MenueEintrag
    {
        #region Variablen
        private string name;
        private int x_position;
        private int y_position;
        private Color color;
        #endregion

        #region Properties
        public string Name
        {
            get
            {
                return this.name;
            }
        }
        public int X_position
        {
            get
            {
                return this.x_position;
            }
        }
        public int Y_position
        {
            get
            {
                return this.y_position;
            }
        }
        public Color Color
        {
            get
            {
                return this.color;
            }
        }
        #endregion

        #region Konstruktor
        public MenueEintrag(string name, int x_position, int y_position, Color color)
        {
            this.name = name;
            this.x_position = x_position;
            this.y_position = y_position;
            this.color = color;
        }
        #endregion
    }
}