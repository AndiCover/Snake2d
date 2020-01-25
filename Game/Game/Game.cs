using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game
{
    [Serializable]
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variablen
        const int randstonegroesse = 38, stonegroesse = 10;
        GraphicsDeviceManager graphics = null;
        GraphicsDevice device = null;
        SpriteBatch sprite = null;
        MenueEintrag eintrag, akteintr;
        List<MenueEintrag> menue = new List<MenueEintrag>();
        List<MenueEintrag> options = new List<MenueEintrag>();
        Stack<String> RenderScreen = new Stack<String>();
        SpriteFont font, selected, big, copyright, countdown;
        Texture2D stone, randstone, bg, head, gamebg, logo, pointer, star, explosion;
        Texture2D[] foodpic = new Texture2D[4];
        KeyboardState oldkey;
        Player player1, player2;
        Board board;
        Food food, specialfood;
        Turn turn;
        List<Rectangle> stars = new List<Rectangle>();
        Random rand = new Random();
        int menueposx = 1024;
        public bool[] option = new bool[3];
        bool end = false, musik, multiplayer, player2isselected = false, player1isselected = false, highscoreischanged = false, eaten = false, beerdraw = false, wait = true, fullscreen, playcrashsound, unentschieden = false;
        int choosen = 1, anzahl_eintraege, eintrnum = 0, ueberschrift = 0, speed, AufloesungWidth, AufloesungHeight, foodnum, starnum = 0, AufloesungWidthChanged, AufloesungHeightChanged, bildnum = 0, rownum = 0;
        string hilfetext, copyright_text = "\xa9 by Andreas Schöngruber", name, ladetext = "3";
        public string[] stroption = new string[3];
        string[] line;
        Song gamesong, menuesong;
        SoundEffect crashsound, clicksound, eatsound;
        readonly string HighScoresFilename = "higscore.lst";
        Thread t1, beerthread, t2, ladethread;
        Color choosencolor = Color.Blue;
        MouseState oldmousestate;
        public struct HighScoreData
        {
            public string[] Playername;
            public int[] Score;
            public int count;

            public HighScoreData(int count)
            {
                Playername = new string[count];
                Score = new int[count];

                this.count = count;
            }
        }
        #endregion

        #region Konstruktor
        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.IsFullScreen = fullscreen;
            this.graphics.ApplyChanges();
            this.Window.AllowUserResizing = false;
            this.Window.Title = "Snake";

            AufloesungWidth = this.graphics.PreferredBackBufferWidth;
            AufloesungHeight = this.graphics.PreferredBackBufferHeight;

            LoadContent();
            Initialize();
        }
        #endregion

        #region Properties
        private int Choosen
        {
            get
            {
                return this.choosen;
            }
            set
            {
                if (value > this.anzahl_eintraege - 1)
                {
                    this.choosen = 1;
                }
                else if (value < 1)
                {
                    this.choosen = this.anzahl_eintraege - 1;
                }
                else
                {
                    this.choosen = value;
                }
            }
        }
        private int Anzahl_eintraege
        {
            get
            {
                return this.anzahl_eintraege;
            }
        }
        public string[] Stroption
        {
            set
            {
                this.stroption = value;
                line = stroption[2].Split(' ');
                try
                {
                    fullscreen = Convert.ToBoolean(line[3]);
                }
                catch (Exception)
                {
                    fullscreen = false;
                }
                try
                {
                    if (Convert.ToInt32(line[0]) <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && Convert.ToInt32(line[2]) <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                    {
                        this.graphics.PreferredBackBufferWidth = Convert.ToInt32(line[0]);
                        this.graphics.PreferredBackBufferHeight = Convert.ToInt32(line[2]);
                    }
                    else
                    {
                        this.graphics.PreferredBackBufferWidth = 1024;
                        this.graphics.PreferredBackBufferHeight = 768;
                        stroption[2] = 1024 + " x " + 768;
                    }
                }
                catch (FormatException)
                {
                    this.graphics.PreferredBackBufferWidth = 1024;
                    this.graphics.PreferredBackBufferHeight = 768;
                    stroption[2] = 1024 + " x " + 768;
                }
                this.graphics.IsFullScreen = fullscreen;
                this.graphics.ApplyChanges();
                AufloesungWidth = this.graphics.PreferredBackBufferWidth;
                AufloesungHeight = this.graphics.PreferredBackBufferHeight;
                AufloesungHeightChanged = AufloesungHeight;
                AufloesungWidthChanged = AufloesungWidth;
                menueposx = AufloesungWidth;
            }
        }
        #endregion

        #region Funktionen
        private void Render()
        {
            ScreenSlide();
            if (RenderScreen.Peek() == "MENU")
            {
                this.sprite.Draw(this.bg, new Rectangle(menueposx, 0, AufloesungWidth, AufloesungHeight), Color.Gray);
                this.sprite.Draw(this.logo, new Rectangle(menueposx + randstonegroesse, randstonegroesse, 200, 100), new Color(255, 255, 255, 255));
                DrawTextWithShadow(this.copyright, "Snake Multiplayer", menueposx + 125, 50, Color.White, Color.Black);
                DrawMenue(menue);
                DrawPointer(Color.Gold, Color.Black);
            }
            else if (RenderScreen.Peek() == "GAME")
            {
                this.sprite.Draw(this.gamebg, new Rectangle(menueposx + randstonegroesse / 2, randstonegroesse / 2, AufloesungWidth - AufloesungWidth / 4, AufloesungHeight - randstonegroesse), Color.GreenYellow);
                DrawGame(Color.Blue);
                DrawFood();
                DrawSnake(player1, Color.Red);
                if (multiplayer)
                {
                    DrawSnake(player2, Color.Blue);
                }
                if (end)
                {
                    DrawGameover();
                    DrawExplosion(GetExplosionPosition()[0], GetExplosionPosition()[1]);
                }
            }
            else if (RenderScreen.Peek() == "HELP")
            {
                this.sprite.Draw(this.bg, new Rectangle(menueposx, 0, AufloesungWidth, AufloesungHeight), Color.Gray);
                DrawHelp();
                DrawPointer(Color.Gold, Color.Black);
            }
            else if (RenderScreen.Peek() == "SETTINGS")
            {
                this.sprite.Draw(this.bg, new Rectangle(menueposx, 0, AufloesungWidth, AufloesungHeight), Color.Gray);
                DrawTextWithShadow(this.font, "Enter oder Linke\nMaustaste zum Auswählen", menueposx + AufloesungWidth / 8, AufloesungHeight / 2, Color.Black, Color.Cyan);
                DrawMenue(options);
                DrawPointer(Color.Gold, Color.Black);
            }
            else if (RenderScreen.Peek() == "HIGHSCORE")
            {
                this.sprite.Draw(this.bg, new Rectangle(menueposx, 0, AufloesungWidth, AufloesungHeight), Color.Gray);
                DrawHighscore();
                DrawPointer(Color.Gold, Color.Black);
            }
            DrawRand(Color.Blue);
            if (!wait)
            {
                Thread.Sleep(10);
            }
            this.sprite.Draw(this.bg, new Rectangle(menueposx + AufloesungWidth, 0, 200, AufloesungHeight), Color.Black);
        }

        private int[] GetExplosionPosition()
        {
            int[] pos = new int[2];
            if (player1.snake.lost)
            {
                pos[0] = player1.snake.snake[0].x - 45;
                pos[1] = player1.snake.snake[0].y - 45;
            }
            else
            {
                pos[0] = player2.snake.snake[0].x - 45;
                pos[1] = player2.snake.snake[0].y - 45;
            }
            return pos;
        }
        private void ScreenSlide()
        {
            if (menueposx > 50)
            {
                menueposx -= 50;
            }
            else if (menueposx < -50)
            {
                menueposx += 50;
            }
            else
            {
                menueposx = 0;
            }
        }
        private void DrawExplosion(int x, int y)
        {
            this.sprite.Draw(this.explosion, new Rectangle(x, y, 100, 100), new Rectangle(65 * bildnum, 65 * rownum, 65, 65), Color.White);
            bildnum++;
            if (bildnum > 4)
            {
                bildnum = 0;
                rownum++;
            }
        }
        private void DrawPointer(Color randcolor, Color pointercolor)
        {
            foreach (Rectangle pos in stars)
            {
                DrawTextureWithRand(star, pos.X, pos.Y, Color.Black, Color.Gold, 1, pos.Width, pos.Height);
            }
            DrawTextureWithRand(pointer, Mouse.GetState().X, Mouse.GetState().Y, randcolor, pointercolor, 1, pointer.Width, pointer.Height);
        }
        private void DrawMenue(List<MenueEintrag> list)
        {
            DrawTextRand(this.copyright, copyright_text, menueposx + AufloesungWidth / 10, AufloesungHeight - (randstonegroesse * 2), Color.Black, Color.LightBlue);
            this.anzahl_eintraege = list.Count;
            foreach (MenueEintrag x in list)
            {
                if (this.ueberschrift == 0)
                {
                    DrawTextWithShadow(this.big, x.Name, menueposx + x.X_position, x.Y_position, Color.Black, Color.Blue);
                    this.ueberschrift++;
                }
                else
                {
                    if (x == list[choosen])
                    {
                        DrawTextRand(this.selected, x.Name, menueposx + x.X_position, x.Y_position, Color.Black, choosencolor);
                    }
                    else
                    {
                        DrawTextRand(this.font, x.Name, menueposx + x.X_position, x.Y_position, Color.Black, Color.LightBlue);
                    }
                }
            }
            this.ueberschrift = 0;
        }
        private void DrawHelp()
        {
            DrawTextRand(this.copyright, copyright_text, menueposx + AufloesungWidth / 10, AufloesungHeight - (randstonegroesse * 2), Color.Black, Color.LightBlue);
            DrawTextWithShadow(this.big, "HILFE", (float)(menueposx + AufloesungWidth / 3.84), (float)(AufloesungHeight / 5.4), Color.Black, Color.Blue);
            DrawTextRand(this.selected, "Spieler 1:\t        Spieler 2:", (float)(menueposx + AufloesungWidth / 4.26), (float)(AufloesungHeight / 3.32), Color.Black, Color.LightBlue);
            DrawTextRand(this.font, hilfetext, (float)(menueposx + AufloesungWidth / 4.26), (float)(AufloesungHeight / 3.32), Color.Black, Color.LightBlue);
        }
        private void DrawHighscore()
        {
            DrawTextRand(this.copyright, copyright_text, menueposx + AufloesungWidth / 10, AufloesungHeight - (randstonegroesse * 2), Color.Black, Color.LightBlue);
            DrawTextWithShadow(this.big, "RANGLISTE", (float)(menueposx + AufloesungWidth / 3.84), (float)(AufloesungHeight / 5.4), Color.Black, Color.Blue);
            HighScoreData data = LoadHighScores(HighScoresFilename);
            int x = (int)(menueposx + AufloesungWidth / 3.49);
            int y = (int)(AufloesungHeight / 3.08);
            DrawTextRand(this.selected, "SPIELER", x, y, Color.Black, Color.Blue);
            DrawTextRand(this.selected, "PUNKTE", x + (int)(AufloesungWidth / 4.8), y, Color.Black, Color.Blue);
            y += (int)(AufloesungHeight / 13.5);
            for (int i = 0; i < data.count; i++)
            {
                DrawTextRand(this.font, data.Playername[i], x, y, Color.Black, Color.LightBlue);
                DrawTextRand(this.font, "" + data.Score[i], x + (int)(AufloesungWidth / 4.8), y, Color.Black, Color.LightBlue);
                y += (int)(AufloesungHeight / 18);
            }
        }
        private void DrawGame(Color color)
        {
            DrawTextRand(this.selected, this.player1.name, (float)(menueposx + AufloesungWidth / 1.219), AufloesungHeight / 10, Color.Black, Color.Blue);
            DrawTextRand(this.font, "Punkte: " + player1.score, (float)(menueposx + AufloesungWidth / 1.219), AufloesungHeight / 5, Color.Black, Color.LightBlue);
            if (multiplayer)
            {
                DrawTextRand(this.selected, this.player2.name, (float)(menueposx + AufloesungWidth / 1.219), AufloesungHeight / 2, Color.Black, Color.Blue);
                DrawTextRand(this.font, "Punkte: " + player2.score, (float)(menueposx + AufloesungWidth / 1.219), AufloesungHeight / 10 * 6, Color.Black, Color.LightBlue);
            }
        }
        private void DrawRand(Color color)
        {
            for (int i = -2; i < AufloesungWidth; i += randstonegroesse - 5)
            {
                this.sprite.Draw(randstone, new Rectangle(menueposx + i, 0, randstonegroesse - 2, randstonegroesse - 2), color);
                this.sprite.Draw(randstone, new Rectangle(menueposx + i, AufloesungHeight - (randstonegroesse - 2), randstonegroesse - 2, randstonegroesse - 2), color);
            }
            for (int i = randstonegroesse - 5; i < AufloesungHeight - randstonegroesse; i += randstonegroesse - 3)
            {
                this.sprite.Draw(randstone, new Rectangle(menueposx - 2, i, randstonegroesse - 2, randstonegroesse), color);
                this.sprite.Draw(randstone, new Rectangle(menueposx + AufloesungWidth - (randstonegroesse - 2), i, randstonegroesse - 2, randstonegroesse), color);
                if (this.RenderScreen.Peek() == "GAME")
                {
                    this.sprite.Draw(randstone, new Rectangle(menueposx + AufloesungWidth - AufloesungWidth / 4, i, randstonegroesse - 2, randstonegroesse), color);
                }
            }
        }
        private void DrawSnake(Player player, Color color)
        {
            Bewegen(player);
            Stone s;
            for (int i = 1; i < player.snake.len; i++)
            {
                s = player.snake.snake[i];
                DrawTextureWithRand(stone, s.x, s.y, Color.Black, color, 1, stonegroesse, stonegroesse);
            }
            s = player.snake.snake[0];
            if (s.xdir > 0)
            {
                DrawHeadWithRand(s.x + 5, s.y + 5, color, (float)(Math.PI * 1.5));
            }
            else if (s.xdir < 0)
            {
                DrawHeadWithRand(s.x + 5, s.y + 5, color, (float)(Math.PI * 0.5));
            }
            else if (s.ydir > 0)
            {
                DrawHeadWithRand(s.x + 5, s.y + 5, color, (float)(0.0f));
            }
            else
            {
                DrawHeadWithRand(s.x + 5, s.y + 5, color, (float)(Math.PI));
            }
        }
        private void DrawFood()
        {
            DrawTextureWithRand(foodpic[foodnum], food.x, food.y, Color.Black, Color.White, 2, 30, 29);
            if (beerdraw)
            {
                DrawTextureWithRand(foodpic[3], specialfood.x, specialfood.y, Color.Black, Color.White, 2, 30, 29);
            }
        }
        private void DrawGameover()
        {
            DrawTextRand(this.font, "Crash!", (float)(AufloesungWidth / 3.1), (float)(AufloesungHeight / 2.5), Color.Black, Color.Red);
            if (multiplayer)
            {
                if (player1.snake.lost)
                {
                    DrawTextRand(this.font, "Spieler 2 hat das Spiel mit " + player2.score + " Punkten gewonnen!", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 2.16), Color.Black, Color.Blue);
                    player2.wins++;
                }
                else if (player2.snake.lost)
                {
                    DrawTextRand(this.font, "Spieler 1 hat das Spiel mit " + player1.score + " Punkten gewonnen!", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 2.16), Color.Black, Color.Blue);
                    player1.wins++;
                }
                else if (unentschieden)
                {
                    DrawTextRand(this.font, "Das Spiel ging unentschieden aus!", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 2.16), Color.Black, Color.Blue);
                }
            }
            else
            {
                DrawTextRand(this.font, "Sie haben " + player1.score + " Punkte erreicht!", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 2.16), Color.Black, Color.Blue);
            }
            DrawTextRand(this.font, "Enter zum Neustarten", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 1.84), Color.Black, Color.LightBlue);
            DrawTextRand(this.font, "Esc zum Beenden", (float)(AufloesungWidth / 3.84), (float)(AufloesungHeight / 1.68), Color.Black, Color.LightBlue);
            beerdraw = false;
            if (!highscoreischanged)
            {
                SaveHighScore(player1);
                SaveHighScore(player2);
                highscoreischanged = true;
            }
        }

        private void Left(Snake snake)
        {
            if (snake.xdir == speed)
            {
                return;
            }
            snake.leftispressed = true;
            snake.rightispressed = false;
            snake.downispressed = false;
            snake.upispressed = false;
            snake.xdir = -speed;
            snake.ydir = 0;
        }
        private void Right(Snake snake)
        {
            if (snake.xdir == -speed)
            {
                return;
            }
            snake.leftispressed = false;
            snake.rightispressed = true;
            snake.downispressed = false;
            snake.upispressed = false;
            snake.xdir = speed;
            snake.ydir = 0;
        }
        private void Up(Snake snake)
        {
            if (snake.ydir == speed)
            {
                return;
            }
            snake.leftispressed = false;
            snake.rightispressed = false;
            snake.downispressed = false;
            snake.upispressed = true;
            snake.xdir = 0;
            snake.ydir = -speed;
        }
        private void Down(Snake snake)
        {
            if (snake.ydir == -speed)
            {
                return;
            }
            snake.leftispressed = false;
            snake.rightispressed = false;
            snake.downispressed = true;
            snake.upispressed = false;
            snake.xdir = 0;
            snake.ydir = speed;
        }
        private void Bewegen(Player player)
        {
            if (wait)
            {
                if (ladethread == null)
                {
                    ladethread = new Thread(Ladethread);
                    ladethread.Start();
                }
                DrawTextRand(this.countdown, ladetext, menueposx + board.width / 2, board.height / 2, Color.Black, Color.Red);
            }
            else
            {
                Snake snake = player.snake;
                if (snake.lost == true)
                {
                    end = true;
                }

                if (!end)
                {
                    turn = new Turn(snake.snake[0].x, snake.snake[0].y);
                    snake.turns.Add(turn);
                    SelfCrash(player);
                    EatFood(player);

                    snake.snake[0].xdir = snake.xdir;
                    snake.snake[0].ydir = snake.ydir;
                    snake.snake[0].x += snake.snake[0].xdir;
                    snake.snake[0].y += snake.snake[0].ydir;
                    for (int i = 1; i < snake.len; i++)
                    {
                        if (snake.turns.Count != 0)
                        {
                            if (snake.turns.Count > snake.snake[i].pointsreached)
                            {
                                if (snake.turns[snake.snake[i].pointsreached].x != snake.snake[i].x)
                                {
                                    if (snake.turns[snake.snake[i].pointsreached].x > snake.snake[i].x)
                                    {
                                        snake.snake[i].xdir = speed;
                                    }
                                    else if (snake.turns[snake.snake[i].pointsreached].x < snake.snake[i].x)
                                    {
                                        snake.snake[i].xdir = -speed;
                                    }
                                }
                                if (snake.turns[snake.snake[i].pointsreached].y != snake.snake[i].y)
                                {
                                    if (snake.turns[snake.snake[i].pointsreached].y > snake.snake[i].y)
                                    {
                                        snake.snake[i].ydir = speed;
                                    }
                                    else if (snake.turns[snake.snake[i].pointsreached].y < snake.snake[i].y)
                                    {
                                        snake.snake[i].ydir = -speed;
                                    }
                                }
                                if (snake.turns[snake.snake[i].pointsreached].y == snake.snake[i].y && snake.turns[snake.snake[i].pointsreached].x == snake.snake[i].x)
                                {
                                    try
                                    {
                                        if (snake.turns[snake.snake[i].pointsreached + 1].x > snake.snake[i].x)
                                        {
                                            snake.snake[i].xdir = speed;
                                        }
                                        else if (snake.turns[snake.snake[i].pointsreached + 1].x < snake.snake[i].x)
                                        {
                                            snake.snake[i].xdir = -speed;
                                        }
                                        else
                                        {
                                            snake.snake[i].xdir = 0;
                                        }
                                        if (snake.turns[snake.snake[i].pointsreached + 1].y > snake.snake[i].y)
                                        {
                                            snake.snake[i].ydir = speed;
                                        }
                                        else if (snake.turns[snake.snake[i].pointsreached + 1].y < snake.snake[i].y)
                                        {
                                            snake.snake[i].ydir = -speed;
                                        }
                                        else
                                        {
                                            snake.snake[i].ydir = 0;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        snake.snake[i].xdir = snake.snake[i - 1].xdir;
                                        snake.snake[i].ydir = snake.snake[i - 1].ydir;
                                    }
                                    snake.snake[i].pointsreached++;
                                }
                            }
                        }
                        snake.snake[i].x += snake.snake[i].xdir;
                        snake.snake[i].y += snake.snake[i].ydir;
                    }
                }
                else
                {
                    if (playcrashsound)
                    {
                        crashsound.Play();
                        playcrashsound = false;
                    }
                }
                if (!board.istInnerhalb(snake.snake[0].x, snake.snake[0].y) && !end)
                {
                    end = true;
                    if (snake == player1.snake)
                    {
                        player2.score += 50;
                    }
                    else
                    {
                        player1.score += 50;
                    }
                    if (player1.score < player2.score)
                    {
                        player1.snake.lost = true;
                    }
                    else if (player1.score > player2.score)
                    {
                        player2.snake.lost = true;
                    }
                    else
                    {
                        unentschieden = true;
                    }
                    if (playcrashsound)
                    {
                        crashsound.Play();
                        playcrashsound = false;
                    }
                }
            }
        }

        private void t1Work()
        {
            while (RenderScreen.Peek() == "GAME")
            {
                try
                {
                    Thread.Sleep(10000);
                }
                catch (ThreadInterruptedException)
                {
                }
                if (!eaten && !end && !wait)
                {
                    food = new Food(board.width, board.height, player1.snake, player2.snake, multiplayer);
                    foodnum = new Random().Next(3);
                }
                else
                {
                    eaten = false;
                }
            }
        }
        private void t2Work()
        {
            while (RenderScreen.Peek() != "GAME")
            {
                try
                {
                    Thread.Sleep(250);
                }
                catch (ThreadInterruptedException)
                {
                }
                if (choosencolor == Color.Blue)
                {
                    choosencolor = Color.LightBlue;
                }
                else if (choosencolor != Color.Blue)
                {
                    choosencolor = Color.Blue;
                }
            }
        }
        private void Beer()
        {
            while (RenderScreen.Peek() == "GAME")
            {
                if (!end && !wait)
                {
                    try
                    {
                        Thread.Sleep(new Random().Next(10000, 50000));
                        if (!end)
                        {
                            beerdraw = true;
                            specialfood = new Food(board.width, board.height, player1.snake, player2.snake, multiplayer);
                            Thread.Sleep(5000);
                            beerdraw = false;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                }
                else
                {
                    try
                    {
                        Thread.Sleep(10);
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                }
            }
        }
        private void Ladethread()
        {
            try
            {
                ladetext = "3";
                Thread.Sleep(1000);
                ladetext = "2";
                if (RenderScreen.Peek() == "GAME")
                {
                    clicksound.Play();
                }
                Thread.Sleep(1000);
                ladetext = "1";
                if (RenderScreen.Peek() == "GAME")
                {
                    clicksound.Play();
                }
                Thread.Sleep(1000);
                if (RenderScreen.Peek() == "GAME")
                {
                    clicksound.Play();
                }
                wait = false;
                ladethread = null;
                if (beerthread == null && t1 == null)
                {
                    beerthread = new Thread(Beer);
                    beerthread.Start();
                    t1 = new Thread(t1Work);
                    t1.Start();
                }
                else
                {
                    beerthread.Interrupt();
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SaveHighScores(HighScoreData data, string filename)
        {
            string fullpath = filename;

            FileStream stream = File.Open(fullpath, FileMode.OpenOrCreate);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                serializer.Serialize(stream, data);
            }
            catch (Exception)
            {
            }
            finally
            {
                stream.Close();
            }
        }
        public static HighScoreData LoadHighScores(string filename)
        {
            HighScoreData data = new HighScoreData();

            string fullpath = filename;

            FileStream stream = File.Open(fullpath, FileMode.OpenOrCreate,
            FileAccess.Read);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                data = (HighScoreData)serializer.Deserialize(stream);
            }
            catch (Exception)
            {
            }
            finally
            {
                stream.Close();
            }

            return (data);
        }
        private void SaveHighScore(Player player)
        {
            HighScoreData data = LoadHighScores(HighScoresFilename);

            int scoreIndex = -1;
            for (int i = 0; i < data.count; i++)
            {
                if (player.score > data.Score[i])
                {
                    scoreIndex = i;
                    break;
                }
            }

            if (scoreIndex > -1)
            {
                for (int i = data.count - 1; i > scoreIndex; i--)
                {
                    data.Playername[i] = data.Playername[i - 1];
                    data.Score[i] = data.Score[i - 1];
                }

                data.Playername[scoreIndex] = player.name;
                data.Score[scoreIndex] = player.score;

                SaveHighScores(data, HighScoresFilename);
            }
        }

        private int[] EintragsPosition()
        {
            int x = 0, y = AufloesungHeight / 10;
            int[] koordinaten = new int[2];
            if (this.eintrnum == 1)
            {
                x += (int)(AufloesungWidth / 42.7);
            }
            if (this.eintrnum != 0)
            {
                y += (int)(AufloesungHeight / 21.6);
            }
            for (int i = 0; i < this.eintrnum; i++)
            {
                x -= (int)(AufloesungWidth / 38.4);
                y += (int)(AufloesungHeight / 10.8);
            }
            this.eintrnum++;
            koordinaten[0] = (int)((AufloesungWidth / 3) * 2) + x;
            koordinaten[1] = (int)(AufloesungHeight / 6) + y;
            return koordinaten;
        }
        private void MenueEintraege()
        {
            if (menue.Count == 0)
            {
                int[] position = new int[2];
                position = EintragsPosition();
                eintrag = new MenueEintrag("Snake", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Spiel starten", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Einstellungen", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Rangliste", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Hilfe", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Beenden", position[0], position[1], Color.Maroon);
                menue.Add(eintrag);
            }
            else
            {
                menue.Clear();
                this.eintrnum = 1;
                MenueEintraege();
            }
        }
        private void OptionsEintraege()
        {
            int[] position = new int[2];
            this.eintrnum = 0;
            if (options.Count == 0)
            {
                position = EintragsPosition();
                eintrag = new MenueEintrag("Einstellungen", position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Musik <" + this.option[0] + ">", position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Mehrspieler <" + this.option[1] + ">", position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Langsam <" + this.option[2] + ">", position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Spieler 1: " + stroption[0], position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Spieler 2: " + stroption[1], position[0], position[1], Color.Maroon);
                options.Add(eintrag);
                position = EintragsPosition();
                eintrag = new MenueEintrag("Auflösung: " + AufloesungWidthChanged + " x " + AufloesungHeightChanged + (fullscreen == true ? " Vollbild" : ""), position[0], position[1], Color.Maroon);
                options.Add(eintrag);
            }
            else
            {
                options.Clear();
                OptionsEintraege();
            }
        }
        private void Optionaendern()
        {
            if (Choosen == 1)
            {
                if (this.option[0] == false)
                {
                    this.option[0] = true;
                    this.musik = true;
                    MediaPlayer.Play(menuesong);
                }
                else
                {
                    this.option[0] = false;
                    this.musik = false;
                    MediaPlayer.Stop();
                }
            }
            if (Choosen == 2)
            {
                if (this.option[1] == false)
                {
                    this.option[1] = true;
                    this.multiplayer = true;
                }
                else
                {
                    this.option[1] = false;
                    this.multiplayer = false;
                }
            }
            if (Choosen == 3)
            {
                if (this.option[2] == false)
                {
                    this.option[2] = true;
                }
                else
                {
                    this.option[2] = false;
                }
            }
            if (Choosen == 4)
            {
                if (!player1isselected)
                {
                    player1isselected = true;
                    name = stroption[0];
                    stroption[0] = "";
                }
                else
                {
                    player1isselected = false;
                    if (stroption[0] == "")
                    {
                        stroption[0] = name;
                    }
                }
            }
            if (Choosen == 5)
            {
                if (!player2isselected)
                {
                    player2isselected = true;
                    name = stroption[1];
                    stroption[1] = "";
                }
                else
                {
                    player2isselected = false;
                    if (stroption[1] == "")
                    {
                        stroption[1] = name;
                    }
                }
            }
            if (Choosen == 6)
            {
                if (AufloesungWidthChanged == 1024 && AufloesungHeightChanged == 768 && !fullscreen)
                {
                    this.AufloesungWidthChanged = 1024;
                    this.AufloesungHeightChanged = 768;
                    fullscreen = true;
                }
                else if (AufloesungWidthChanged == 1024 && AufloesungHeightChanged == 768 && fullscreen && 1280 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 720 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1280;
                    this.AufloesungHeightChanged = 720;
                    fullscreen = false;
                }
                else if (AufloesungWidthChanged == 1280 && AufloesungHeightChanged == 720 && !fullscreen && 1280 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 720 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1280;
                    this.AufloesungHeightChanged = 720;
                    fullscreen = true;
                }
                else if (AufloesungWidthChanged == 1280 && AufloesungHeightChanged == 720 && fullscreen && 1280 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 1024 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1280;
                    this.AufloesungHeightChanged = 1024;
                    fullscreen = false;
                }
                else if (AufloesungWidthChanged == 1280 && AufloesungHeightChanged == 1024 && !fullscreen && 1280 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 1024 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1280;
                    this.AufloesungHeightChanged = 1024;
                    fullscreen = true;
                }
                else if (AufloesungWidthChanged == 1280 && AufloesungHeightChanged == 1024 && fullscreen && 1366 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 768 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1366;
                    this.AufloesungHeightChanged = 768;
                    fullscreen = false;
                }
                else if (AufloesungWidthChanged == 1366 && AufloesungHeightChanged == 768 && !fullscreen && 1366 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 768 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1366;
                    this.AufloesungHeightChanged = 768;
                    fullscreen = true;
                }
                else if (AufloesungWidthChanged == 1366 && AufloesungHeightChanged == 768 && fullscreen && 1920 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 1080 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1920;
                    this.AufloesungHeightChanged = 1080;
                    fullscreen = false;
                }
                else if (AufloesungWidthChanged == 1920 && AufloesungHeightChanged == 1080 && !fullscreen && 1920 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && 1080 <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    this.AufloesungWidthChanged = 1920;
                    this.AufloesungHeightChanged = 1080;
                    fullscreen = true;
                }
                else
                {
                    this.AufloesungWidthChanged = 1024;
                    this.AufloesungHeightChanged = 768;
                    fullscreen = false;
                }
            }
            OptionsEintraege();
        }

        private void EatFood(Player player)
        {
            if (food.istInnerhalb(player.snake.snake[0].x + randstonegroesse / 2, player.snake.snake[0].y + randstonegroesse / 2))
            {
                if (!option[2])
                {
                    player.score += 2;
                }
                else
                {
                    player.score += 1;
                }
                player.snake.Add();
                food = new Food(board.width, board.height, player1.snake, player2.snake, multiplayer);
                foodnum = new Random().Next(3);
                eaten = true;
                eatsound.Play();
            }
            if (beerdraw)
            {
                if (specialfood.istInnerhalb(player.snake.snake[0].x + randstonegroesse / 2, player.snake.snake[0].y + randstonegroesse / 2))
                {
                    if (!option[2])
                    {
                        player.score += 4;
                    }
                    else
                    {
                        player.score += 2;
                    }
                    player.snake.Add();
                    beerdraw = false;
                    eatsound.Play();
                }
            }
        }

        private void Reset()
        {
            bildnum = 0;
            rownum = 0;
            end = false;
            if (option[2])
            {
                speed = 5;
            }
            else
            {
                speed = 10;
            }
            player1 = new Player(0, 0, -speed, AufloesungWidth / 4, AufloesungHeight / 2, 15, speed, stroption[0]);
            player1.snake.turns.Clear();
            player2 = new Player(0, 0, -speed, AufloesungWidth / 2, AufloesungHeight / 2, 15, speed, stroption[1]);
            player2.snake.turns.Clear();
            food = new Food(board.width, board.height, player1.snake, player2.snake, multiplayer);
            foodnum = new Random().Next(3);
            highscoreischanged = false;
            wait = true;
            playcrashsound = true;
        }
        private void SelfCrash(Player p)
        {
            for (int i = 3; i < p.snake.len; i++)
            {
                if (p.snake.snake[i].istInnerhalb(p.snake.snake[0].x, p.snake.snake[0].y, p.snake.snake[0].xdir, p.snake.snake[0].ydir))
                {
                    end = true;
                    if (p.snake == player1.snake)
                    {
                        player2.score += 50;
                    }
                    else
                    {
                        player1.score += 50;
                    }
                    if (player1.score < player2.score)
                    {
                        player1.snake.lost = true;
                    }
                    else if (player1.score > player2.score)
                    {
                        player2.snake.lost = true;
                    }
                    else
                    {
                        unentschieden = true;
                    }
                }
            }
        }

        private void DrawTextRand(SpriteFont font, String text, float x, float y, Color randcolor, Color fontcolor)
        {
            for (int i = -2; i <= 2; i += 2)
            {
                for (int j = -2; j <= 2; j += 2)
                {
                    this.sprite.DrawString(font, text, new Vector2(x + i, y + j), randcolor);
                }
            }
            this.sprite.DrawString(font, text, new Vector2(x, y), fontcolor);
        }
        private void DrawTextWithShadow(SpriteFont font, String text, float x, float y, Color randcolor, Color fontcolor)
        {
            DrawTextRand(font, text, x - 3, y + 3, randcolor, fontcolor);
            DrawTextRand(font, text, x - 2, y + 2, randcolor, fontcolor);
            DrawTextRand(font, text, x - 1, y + 1, randcolor, fontcolor);
            DrawTextRand(font, text, x, y, randcolor, fontcolor);
        }
        private void DrawTextureWithRand(Texture2D texture, int x, int y, Color rand, Color color, int dicke, int width, int height)
        {
            this.sprite.Draw(texture, new Rectangle(menueposx + x - dicke, y, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x - dicke, y - dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x - dicke, y + dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x, y - dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x, y + dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x + dicke, y + dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x + dicke, y - dicke, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x + dicke, y, width, height), rand);
            this.sprite.Draw(texture, new Rectangle(menueposx + x, y, width, height), color);
        }
        private void DrawHeadWithRand(int x, int y, Color color, float winkel)
        {
            sprite.Draw(head, new Vector2(x + 1, y - 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x + 1, y + 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x + 1, y), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x, y - 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x, y + 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x - 1, y), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x - 1, y - 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x - 1, y + 1), null, Color.Black, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
            sprite.Draw(head, new Vector2(x, y), null, color, winkel, new Vector2(menueposx + head.Width / 2, head.Height / 2), 1.0f, SpriteEffects.None, 0.0f);
        }

        private bool MouseOverEintr(MouseState mouse, List<MenueEintrag> eintrlist)
        {
            foreach (MenueEintrag m in eintrlist)
            {
                if (mouse.X > m.X_position && mouse.X < m.X_position + m.Name.Length * 15 && mouse.Y > m.Y_position && mouse.Y < m.Y_position + 50 && this.IsActive)
                {
                    akteintr = m;
                    return true;
                }
            }
            return false;
        }

        protected override void Initialize()
        {
            string fullpath = HighScoresFilename;

            MediaPlayer.IsRepeating = true;

            board = new Board(randstonegroesse - 12, randstonegroesse - 7, AufloesungWidth / 4 * 3, AufloesungHeight - ((randstonegroesse) - 5) - stonegroesse);
            hilfetext = "\n Auf ... Pfeil auf\t   Auf ... W\n Ab ... Pfeil ab\t     Ab ... S\n Links ... Pfeil links\tLinks ... A\n Rechts ... Pfeil rechts       Rechts ... D\n\nLinke Maustaste ... Auswählen/Ändern\nRechte Maustaste ... Zurück\nEnter ... Auswählen\nEsc ... Zurück";

            MenueEintraege();
            OptionsEintraege();

            this.musik = option[0];
            this.multiplayer = option[1];

            this.RenderScreen.Push("MENU");

            t2 = new Thread(t2Work);
            t2.Start();

            if (!File.Exists(fullpath))
            {
                HighScoreData data = new HighScoreData(5);
                data.Playername[0] = "ANDI";
                data.Score[0] = 100;

                data.Playername[1] = "SHAWN";
                data.Score[1] = 85;

                data.Playername[2] = "MARK";
                data.Score[2] = 75;

                data.Playername[3] = "CINDY";
                data.Score[3] = 65;

                data.Playername[4] = "SAM";
                data.Score[4] = 10;

                SaveHighScores(data, HighScoresFilename);
            }
            else
            {
                HighScoreData data = LoadHighScores(HighScoresFilename);
            }
            Reset();
            if (musik)
            {
                MediaPlayer.Play(menuesong);
            }
            base.Initialize();
        }
        protected override void LoadContent()
        {
            Content = new ContentManager(Services, "Content\\");
            this.sprite = new SpriteBatch(GraphicsDevice);
            this.device = graphics.GraphicsDevice;

            this.explosion = Content.Load<Texture2D>("Pics/explosprite");
            this.star = Content.Load<Texture2D>("Pics/star");
            this.pointer = Content.Load<Texture2D>("Pics/pointer");
            this.bg = Content.Load<Texture2D>("Pics/bg");
            this.logo = Content.Load<Texture2D>("Pics/logo");
            this.gamebg = Content.Load<Texture2D>("Pics/Snake_Wiese");
            this.randstone = Content.Load<Texture2D>("Pics/randstone");
            this.stone = Content.Load<Texture2D>("Pics/Snake/stone");
            this.head = Content.Load<Texture2D>("Pics/Snake/head");
            this.foodpic[0] = Content.Load<Texture2D>("Pics/Food/banana");
            this.foodpic[1] = Content.Load<Texture2D>("Pics/Food/cherry");
            this.foodpic[2] = Content.Load<Texture2D>("Pics/Food/orange");
            this.foodpic[3] = Content.Load<Texture2D>("Pics/Food/Bier");

            this.gamesong = Content.Load<Song>("Audio/gamemusic");
            this.menuesong = Content.Load<Song>("Audio/menuemusic");
            this.eatsound = Content.Load<SoundEffect>("Audio/eat");
            this.clicksound = Content.Load<SoundEffect>("Audio/click");
            this.crashsound = Content.Load<SoundEffect>("Audio/crash");

            this.font = Content.Load<SpriteFont>("Fonts/font");
            this.big = Content.Load<SpriteFont>("Fonts/big");
            this.copyright = Content.Load<SpriteFont>("Fonts/copyright");
            this.selected = Content.Load<SpriteFont>("Fonts/selected");
            this.countdown = Content.Load<SpriteFont>("Fonts/Countdown");

            base.LoadContent();
        }
        protected override void UnloadContent()
        {
            try
            {
                t1.Interrupt();
                t1.Abort();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                beerthread.Interrupt();
                beerthread.Abort();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                t2.Interrupt();
                t2.Abort();
            }
            catch (NullReferenceException)
            {
            }
            MediaPlayer.Stop();
            base.UnloadContent();
        }
        protected override void Update(GameTime gameTime)
        {
            KeyboardState key = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            int x = rand.Next(-10, 10);
            int y = rand.Next(-10, 10);

            if (stars.Count <= 40)
            {
                this.stars.Add(new Rectangle(mouse.X + 5 + x, mouse.Y + 10 + y, 10, 10));
                x = rand.Next(-10, 10);
                y = rand.Next(-10, 10);
                this.stars.Add(new Rectangle(mouse.X + 5 + x, mouse.Y + 10 + y, 10, 10));
            }
            else
            {
                if (starnum > 40)
                {
                    starnum = 0;
                }
                this.stars[starnum] = new Rectangle(mouse.X + 5 + x, mouse.Y + 10 + y, 10, 10);
                x = rand.Next(-10, 10);
                y = rand.Next(-10, 10);
                starnum++;
                this.stars[starnum] = new Rectangle(mouse.X + 5 + x, mouse.Y + 10 + y, 10, 10);
                starnum++;
            }

            #region Menuekeys
            if (RenderScreen.Peek() == "MENU")
            {
                int choosenbefor = choosen;
                if (MouseOverEintr(mouse, menue))
                {
                    if (menue.IndexOf(akteintr) != 0)
                    {
                        Choosen = menue.IndexOf(akteintr);
                    }
                    if (choosen != choosenbefor)
                    {
                        clicksound.Play();
                    }
                }
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (MouseOverEintr(mouse, menue))
                    {
                        menueposx = AufloesungWidth;
                        if (Choosen == 1)
                        {
                            RenderScreen.Push("GAME");
                            MediaPlayer.Stop();
                            if (musik)
                            {
                                MediaPlayer.Play(gamesong);
                            }
                            Reset();
                            try
                            {
                                beerthread.Abort();
                            }
                            catch (NullReferenceException)
                            {
                            }
                            try
                            {
                                t1.Abort();
                            }
                            catch (NullReferenceException)
                            {
                            }
                        }
                        if (Choosen == 2)
                        {
                            RenderScreen.Push("SETTINGS");
                            Choosen = 1;
                        }
                        if (Choosen == 3)
                        {
                            RenderScreen.Push("HIGHSCORE");
                            Choosen = 1;
                        }
                        if (Choosen == 4)
                        {
                            RenderScreen.Push("HELP");
                            Choosen = 1;
                        }
                        if (Choosen == 5)
                        {
                            this.Exit();
                        }
                    }
                }

                if (key.IsKeyDown(Keys.Escape) && !oldkey.IsKeyDown(Keys.Escape))
                {
                    clicksound.Play();
                    UnloadContent();
                    this.Exit();
                }
                if (key.IsKeyDown(Keys.Down) && !oldkey.IsKeyDown(Keys.Down))
                {
                    Choosen += 1;
                    clicksound.Play();
                }
                if (key.IsKeyDown(Keys.Up) && !oldkey.IsKeyDown(Keys.Up))
                {
                    Choosen -= 1;
                    clicksound.Play();
                }
                if (key.IsKeyDown(Keys.Enter) && !oldkey.IsKeyDown(Keys.Enter))
                {
                    menueposx = AufloesungWidth;
                    clicksound.Play();
                    if (Choosen == 1)
                    {
                        RenderScreen.Push("GAME");
                        MediaPlayer.Stop();
                        if (musik)
                        {
                            MediaPlayer.Play(gamesong);
                        }
                        Reset();
                        try
                        {
                            beerthread.Abort();
                        }
                        catch (NullReferenceException)
                        {
                        }
                        try
                        {
                            t1.Abort();
                        }
                        catch (NullReferenceException)
                        {
                        }
                    }
                    if (Choosen == 2)
                    {
                        RenderScreen.Push("SETTINGS");
                        Choosen = 1;
                    }
                    if (Choosen == 3)
                    {
                        RenderScreen.Push("HIGHSCORE");
                        Choosen = 1;
                    }
                    if (Choosen == 4)
                    {
                        RenderScreen.Push("HELP");
                        Choosen = 1;
                    }
                    if (Choosen == 5)
                    {
                        this.Exit();
                    }
                }
            }
            #endregion

            #region Settingkeys
            else if (RenderScreen.Peek() == "SETTINGS")
            {
                int choosenbefor = choosen;
                if (MouseOverEintr(mouse, options))
                {
                    if (akteintr.Name != "Einstellungen")
                    {
                        Choosen = options.IndexOf(akteintr);
                    }
                    if (choosen != choosenbefor)
                    {
                        clicksound.Play();
                    }
                }
                if (mouse.LeftButton == ButtonState.Pressed && oldmousestate.LeftButton != ButtonState.Pressed)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        if (mouse.X > options[i].X_position && mouse.X < options[i].X_position + options[i].Name.Length * 15 && mouse.Y > options[i].Y_position && mouse.Y < options[i].Y_position + 50)
                        {
                            clicksound.Play();
                            Optionaendern();
                        }
                    }
                }

                if (!player2isselected && !player1isselected)
                {
                    if (key.IsKeyDown(Keys.Escape) && !oldkey.IsKeyDown(Keys.Escape) || mouse.RightButton == ButtonState.Pressed && oldmousestate.RightButton != ButtonState.Pressed)
                    {
                        this.graphics.PreferredBackBufferHeight = AufloesungHeightChanged;
                        this.graphics.PreferredBackBufferWidth = AufloesungWidthChanged;
                        AufloesungWidth = this.graphics.PreferredBackBufferWidth;
                        AufloesungHeight = this.graphics.PreferredBackBufferHeight;
                        board = new Board(randstonegroesse - 12, randstonegroesse - 7, AufloesungWidth / 4 * 3, AufloesungHeight - ((randstonegroesse) - 5) - stonegroesse);
                        if (AufloesungWidth == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && AufloesungHeight == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height || fullscreen)
                        {
                            this.graphics.IsFullScreen = true;
                        }
                        else
                        {
                            this.graphics.IsFullScreen = false;
                        }
                        this.graphics.ApplyChanges();
                        MenueEintraege();
                        OptionsEintraege();
                        stroption[2] = AufloesungWidth + " x " + AufloesungHeight + " " + fullscreen;

                        menueposx = -AufloesungWidth;
                        RenderScreen.Pop();
                        Choosen = 1;
                    }
                    if (key.IsKeyDown(Keys.Back) && !oldkey.IsKeyDown(Keys.Back))
                    {
                        clicksound.Play();
                        RenderScreen.Pop();
                        Choosen = 1;
                    }
                    if (key.IsKeyDown(Keys.Down) && !oldkey.IsKeyDown(Keys.Down))
                    {
                        clicksound.Play();
                        Choosen += 1;
                    }
                    if (key.IsKeyDown(Keys.Up) && !oldkey.IsKeyDown(Keys.Up))
                    {
                        clicksound.Play();
                        Choosen -= 1;
                    }
                }
                else
                {
                    if (key.IsKeyDown(Keys.Back) && !oldkey.IsKeyDown(Keys.Back))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            if (stroption[0].Length > 0)
                            {
                                stroption[0] = stroption[0].Substring(0, stroption[0].Length - 1);
                            }
                        }
                        else
                        {
                            if (stroption[1].Length > 0)
                            {
                                stroption[1] = stroption[1].Substring(0, stroption[1].Length - 1);
                            }
                        }
                    }
                    if (key.IsKeyDown(Keys.A) && !oldkey.IsKeyDown(Keys.A))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'A';
                        }
                        else
                        {
                            stroption[1] += 'A';
                        }
                    }
                    if (key.IsKeyDown(Keys.B) && !oldkey.IsKeyDown(Keys.B))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'B';
                        }
                        else
                        {
                            stroption[1] += 'B';
                        }
                    }
                    if (key.IsKeyDown(Keys.C) && !oldkey.IsKeyDown(Keys.C))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'C';
                        }
                        else
                        {
                            stroption[1] += 'C';
                        }
                    }
                    if (key.IsKeyDown(Keys.D) && !oldkey.IsKeyDown(Keys.D))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'D';
                        }
                        else
                        {
                            stroption[1] += 'D';
                        }
                    }
                    if (key.IsKeyDown(Keys.E) && !oldkey.IsKeyDown(Keys.E))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'E';
                        }
                        else
                        {
                            stroption[1] += 'E';
                        }
                    }
                    if (key.IsKeyDown(Keys.F) && !oldkey.IsKeyDown(Keys.F))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'F';
                        }
                        else
                        {
                            stroption[1] += 'F';
                        }
                    }
                    if (key.IsKeyDown(Keys.G) && !oldkey.IsKeyDown(Keys.G))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'G';
                        }
                        else
                        {
                            stroption[1] += 'G';
                        }
                    }
                    if (key.IsKeyDown(Keys.H) && !oldkey.IsKeyDown(Keys.H))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'H';
                        }
                        else
                        {
                            stroption[1] += 'H';
                        }
                    }
                    if (key.IsKeyDown(Keys.I) && !oldkey.IsKeyDown(Keys.I))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'I';
                        }
                        else
                        {
                            stroption[1] += 'I';
                        }
                    }
                    if (key.IsKeyDown(Keys.J) && !oldkey.IsKeyDown(Keys.J))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'J';
                        }
                        else
                        {
                            stroption[1] += 'J';
                        }
                    }
                    if (key.IsKeyDown(Keys.K) && !oldkey.IsKeyDown(Keys.K))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'K';
                        }
                        else
                        {
                            stroption[1] += 'K';
                        }
                    }
                    if (key.IsKeyDown(Keys.L) && !oldkey.IsKeyDown(Keys.L))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'L';
                        }
                        else
                        {
                            stroption[1] += 'L';
                        }
                    }
                    if (key.IsKeyDown(Keys.M) && !oldkey.IsKeyDown(Keys.M))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'M';
                        }
                        else
                        {
                            stroption[1] += 'M';
                        }
                    }
                    if (key.IsKeyDown(Keys.N) && !oldkey.IsKeyDown(Keys.N))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'N';
                        }
                        else
                        {
                            stroption[1] += 'N';
                        }
                    }
                    if (key.IsKeyDown(Keys.O) && !oldkey.IsKeyDown(Keys.O))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'O';
                        }
                        else
                        {
                            stroption[1] += 'O';
                        }
                    }
                    if (key.IsKeyDown(Keys.P) && !oldkey.IsKeyDown(Keys.P))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'P';
                        }
                        else
                        {
                            stroption[1] += 'P';
                        }
                    }
                    if (key.IsKeyDown(Keys.Q) && !oldkey.IsKeyDown(Keys.Q))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'Q';
                        }
                        else
                        {
                            stroption[1] += 'Q';
                        }
                    }
                    if (key.IsKeyDown(Keys.R) && !oldkey.IsKeyDown(Keys.R))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'R';
                        }
                        else
                        {
                            stroption[1] += 'R';
                        }
                    }
                    if (key.IsKeyDown(Keys.S) && !oldkey.IsKeyDown(Keys.S))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'S';
                        }
                        else
                        {
                            stroption[1] += 'S';
                        }
                    }
                    if (key.IsKeyDown(Keys.T) && !oldkey.IsKeyDown(Keys.T))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'T';
                        }
                        else
                        {
                            stroption[1] += 'T';
                        }
                    }
                    if (key.IsKeyDown(Keys.U) && !oldkey.IsKeyDown(Keys.U))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'U';
                        }
                        else
                        {
                            stroption[1] += 'U';
                        }
                    }
                    if (key.IsKeyDown(Keys.V) && !oldkey.IsKeyDown(Keys.V))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'V';
                        }
                        else
                        {
                            stroption[1] += 'V';
                        }
                    }
                    if (key.IsKeyDown(Keys.W) && !oldkey.IsKeyDown(Keys.W))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'W';
                        }
                        else
                        {
                            stroption[1] += 'W';
                        }
                    }
                    if (key.IsKeyDown(Keys.X) && !oldkey.IsKeyDown(Keys.X))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'X';
                        }
                        else
                        {
                            stroption[1] += 'X';
                        }
                    }
                    if (key.IsKeyDown(Keys.Y) && !oldkey.IsKeyDown(Keys.Y))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'Y';
                        }
                        else
                        {
                            stroption[1] += 'Y';
                        }
                    }
                    if (key.IsKeyDown(Keys.Z) && !oldkey.IsKeyDown(Keys.Z))
                    {
                        clicksound.Play();
                        if (player1isselected)
                        {
                            stroption[0] += 'Z';
                        }
                        else
                        {
                            stroption[1] += 'Z';
                        }
                    }
                    if (key.IsKeyDown(Keys.Back) && !oldkey.IsKeyDown(Keys.Back))
                    {
                        if (player1isselected)
                        {
                            string text = "";
                            for (int i = 0; i < stroption[0].Length; i++)
                            {
                                text += stroption[0][i];
                            }
                            stroption[0] = text;
                        }
                        else
                        {
                            string text = "";
                            for (int i = 0; i < stroption[1].Length; i++)
                            {
                                text += stroption[1][i];
                            }
                            stroption[1] = text;
                        }
                    }
                    if (player1isselected)
                    {
                        player1.name = stroption[0];
                    }
                    if (player2isselected)
                    {
                        player2.name = stroption[1];
                    }
                    OptionsEintraege();
                }
                if (key.IsKeyDown(Keys.Enter) && !oldkey.IsKeyDown(Keys.Enter))
                {
                    clicksound.Play();
                    Optionaendern();
                }
            }
            #endregion

            #region highscorekeys
            else if (RenderScreen.Peek() == "HIGHSCORE")
            {
                if (key.IsKeyDown(Keys.Escape) && !oldkey.IsKeyDown(Keys.Escape) || mouse.RightButton == ButtonState.Pressed && oldmousestate.RightButton != ButtonState.Pressed)
                {
                    menueposx = -AufloesungWidth;
                    RenderScreen.Pop();
                    Choosen = 1;
                }
            }
            #endregion

            #region helpkeys
            else if (RenderScreen.Peek() == "HELP")
            {
                if (key.IsKeyDown(Keys.Escape) && !oldkey.IsKeyDown(Keys.Escape) || mouse.RightButton == ButtonState.Pressed && oldmousestate.RightButton != ButtonState.Pressed)
                {
                    menueposx = -AufloesungWidth;
                    RenderScreen.Pop();
                }
            }
            #endregion

            #region gamekeys
            else if (RenderScreen.Peek() == "GAME")
            {
                if (key.IsKeyDown(Keys.Escape) && !oldkey.IsKeyDown(Keys.Escape) || mouse.RightButton == ButtonState.Pressed && oldmousestate.RightButton != ButtonState.Pressed)
                {
                    menueposx = -AufloesungWidth;
                    RenderScreen.Pop();
                    MediaPlayer.Stop();
                    if (musik)
                    {
                        MediaPlayer.Play(menuesong);
                    }
                    Reset();
                    try
                    {
                        beerthread.Interrupt();
                        t2.Abort();
                    }
                    catch (NullReferenceException)
                    {
                    }
                    t2 = new Thread(t2Work);
                    t2.Start();
                }
                if (!end && !wait)
                {
                    if (key.IsKeyDown(Keys.Left) && !oldkey.IsKeyDown(Keys.Left))
                    {
                        Left(player1.snake);
                    }
                    if (key.IsKeyDown(Keys.Right) && !oldkey.IsKeyDown(Keys.Right))
                    {
                        Right(player1.snake);
                    }
                    if (key.IsKeyDown(Keys.Down) && !oldkey.IsKeyDown(Keys.Down))
                    {
                        Down(player1.snake);
                    }
                    if (key.IsKeyDown(Keys.Up) && !oldkey.IsKeyDown(Keys.Up))
                    {
                        Up(player1.snake);
                    }
                    if (key.IsKeyDown(Keys.A) && !oldkey.IsKeyDown(Keys.A))
                    {
                        Left(player2.snake);
                    }
                    if (key.IsKeyDown(Keys.D) && !oldkey.IsKeyDown(Keys.D))
                    {
                        Right(player2.snake);
                    }
                    if (key.IsKeyDown(Keys.S) && !oldkey.IsKeyDown(Keys.S))
                    {
                        Down(player2.snake);
                    }
                    if (key.IsKeyDown(Keys.W) && !oldkey.IsKeyDown(Keys.W))
                    {
                        Up(player2.snake);
                    }
                }
                else if (end)
                {
                    if (key.IsKeyDown(Keys.Enter) && !oldkey.IsKeyDown(Keys.Enter))
                    {
                        Reset();
                        clicksound.Play();
                    }
                }
            }
            #endregion

            oldkey = key;
            oldmousestate = mouse;
            base.Draw(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            this.sprite.Begin();
            Render();
            this.sprite.End();

            base.Update(gameTime);
        }
        #endregion
    }
}