using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    static class Program
    {
        #region Variablen
        public static bool laden = true;
        #endregion

        #region Funktionen
        static void Main()
        {
            FileStream fs;
            StreamReader reader;
            StreamWriter writer;
            Thread thread;
            bool[] option = new bool[3];
            string[] stroption = new string[3];
            Game1 game = null;

            Ladebild ladebild = new Ladebild();
            thread = new Thread(load);
            thread.Start();
            ladebild.Show();
            while (laden)
            {
                Thread.Sleep(10);
            }
            game = new Game1();

            if (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width < 1024 && GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height < 768)
            {
                MessageBox.Show("Ihre Auflösung ist zu niedrig um dieses Spiel zu spielen!", "Grafik Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!File.Exists("Content/Fonts/big.xnb") || !File.Exists("Content/Fonts/copyright.xnb") || !File.Exists("Content/Fonts/Countdown.xnb") || !File.Exists("Content/Fonts/font.xnb") || !File.Exists("Content/Fonts/selected.xnb")
                    || !File.Exists("Content/Pics/bg.xnb") || !File.Exists("Content/Pics/Snake_Wiese.xnb") || !File.Exists("Content/Pics/randstone.xnb") || !File.Exists("Content/Pics/Snake/head.xnb") || !File.Exists("Content/Pics/Snake/stone.xnb") || !File.Exists("Content/Pics/Food/banana.xnb")
                    || !File.Exists("Content/Pics/Food/Bier.xnb") || !File.Exists("Content/Pics/Food/orange.xnb") || !File.Exists("Content/Pics/Food/cherry.xnb") || !File.Exists("Content/Audio/click.xnb") || !File.Exists("Content/Audio/crash.xnb")
                    || !File.Exists("Content/Audio/eat.xnb") || !File.Exists("Content/Audio/gamemusic.wma") || !File.Exists("Content/Audio/gamemusic.xnb") || !File.Exists("Content/Audio/menuemusic.xnb") || !File.Exists("Content/Audio/menuemusic.wma"))
                {
                    MessageBox.Show("Einige Daten wurden nicht gefunden!", "Daten nicht gefunden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try
                    {
                        if (File.Exists("options.o"))
                        {
                            fs = new FileStream("options.o", FileMode.Open);
                            reader = new StreamReader(fs);
                            try
                            {
                                option[0] = Convert.ToBoolean(reader.ReadLine());
                                option[1] = Convert.ToBoolean(reader.ReadLine());
                                option[2] = Convert.ToBoolean(reader.ReadLine());
                                stroption[0] = reader.ReadLine();
                                stroption[1] = reader.ReadLine();
                                stroption[2] = reader.ReadLine();
                            }
                            catch (Exception)
                            {
                                option[0] = true;
                                option[1] = false;
                                option[2] = false;
                                stroption[0] = "HERBERT";
                                stroption[1] = "KURT";
                                stroption[2] = "1024 x 768";
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        else
                        {
                            option[0] = true;
                            option[1] = false;
                            option[2] = false;
                            stroption[0] = "HERBERT";
                            stroption[1] = "KURT";
                            stroption[2] = "1024 x 768";
                        }
                        game.option = option;
                        game.Stroption = stroption;
                        ladebild.Close();
                        game.Run();
                        game.Dispose();
                        fs = new FileStream("options.o", FileMode.OpenOrCreate);
                        writer = new StreamWriter(fs);
                        writer.WriteLine(game.option[0]);
                        writer.WriteLine(game.option[1]);
                        writer.WriteLine(game.option[2]);
                        writer.WriteLine(game.stroption[0]);
                        writer.WriteLine(game.stroption[1]);
                        writer.WriteLine(game.stroption[2]);
                        writer.Flush();
                        writer.Close();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Das Spiel wurde unerwartet Beendet!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        try
                        {
                            FileStream exceptionstream = new FileStream("ERRORLOG", FileMode.CreateNew);
                            writer = new StreamWriter(exceptionstream);
                            writer.Write(e.Message + "\n\nObject:" + e.Source + "\n\nKey/Value:" + e.Data + "\n\nMethod:" + e.TargetSite + "\n" + e.HelpLink);
                            writer.Flush();
                            writer.Close();
                        }
                        catch (IOException)
                        {
                        }
                        finally
                        {
                            Application.Exit();
                        }
                    }
                    finally
                    {
                        Application.Exit();
                    }
                }
            }
        }
        public static void load()
        {
            Thread.Sleep(3000);
            laden = false;
        }
        #endregion
    }
}