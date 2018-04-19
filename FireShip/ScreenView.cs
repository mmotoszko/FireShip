using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;
using System.ComponentModel;

namespace FireShip
{
    public partial class ScreenView : Form
    {
        private BackgroundWorker bw = new BackgroundWorker();
        public static Random rnd = new Random();
        public static Stopwatch stopWatch = new Stopwatch();
        public static Stopwatch gameTime = new Stopwatch();
        public static int framesPerSecondTop = 50;
        public static int frameTime = 1000 / framesPerSecondTop;
        public static int screenHeight = 80 * 12;
        public static int screenWidth = 80 * 8;
        public static Menu menu = null;
        public static List<Bitmap> skins = new List<Bitmap>();
        public static List<Bitmap> bulletSkins = new List<Bitmap>();
        public static List<Bitmap> scenerySkins = new List<Bitmap>();
        public static List<MovingObject> playerShipList = new List<MovingObject>();
        public static List<MovingObject> enemyShipList = new List<MovingObject>();
        public static List<MovingObject> sceneryObjectList = new List<MovingObject>();
        public static List<PictureBox> playerShipBoxList = new List<PictureBox>();
        public static List<PictureBox> enemyShipBoxList = new List<PictureBox>();
        public static List<PictureBox> sceneryObjectBoxList = new List<PictureBox>();
        public static PowerUp powerUp = null;
        public static PictureBox powerUpBox = null;
        public static PictureBox sceneryBackground = null;
        public static List<string> highscoresNames = new List<string>();
        public static List<int> highscoresScores = new List<int>();
        public static string newName = null;
        public static bool highscoresOn = false;
        public static bool playerDead = false;
        public static bool gameStart = false;
        public static bool gameRunning = true;
        public static bool continueGame = true;
        public static bool drawing = false;
        public static int difficultyTimeIncrement = 15;
        public static int score;
        public static int timeScore;
        Label frameCounterLabel = new Label();
        Label scoreLabel = new Label();
        public static int frameCounter = 0;
        public static int sleepTime = 0;
        public static int maxEnemiesCount = 0;
        public static int playerMoveX = 0;
        public static int playerMoveY = 0;
        public static bool playerFire = false;
        public object lockXMovement = new object();
        public object lockYMovement = new object();
        public PictureBox titleImage;
        public List<Label> menuIcons;
        public List<Label> highscoresIcons;


        void calculateEnemyMovement()
        {
            int iCount = enemyShipList.Count;

            bool objectsRemoved = false;

            for (int i = 0; i < iCount; i++)
            {
                double x = enemyShipList[i].getPosition().X;
                double y = enemyShipList[i].getPosition().Y;

                enemyShipList[i].setPreviousPosition(x, y);

                double velX = enemyShipList[i].getVelocity().X;
                double velY = enemyShipList[i].getVelocity().Y;

                double newX = x + velX;
                double newY = y + velY;

                if (newY > 1.2 || newY < -0.2)
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            Controls.Remove(enemyShipBoxList[i]);
                            enemyShipBoxList.RemoveAt(i);
                        }));
                    }
                    catch { };
                    enemyShipList.RemoveAt(i);
                    i--;
                    iCount--;

                    if (!objectsRemoved)
                        objectsRemoved = true;
                }
                else
                {
                    if (enemyShipList[i].GetType() == typeof(EnemyShip))
                        if (newX > 1 - enemyShipList[i].getSize() || newX < 0)
                            enemyShipList[i].setVelocity(0 - velX, velY);

                    enemyShipList[i].setPosition(newX, newY);
                }
            }

            if (objectsRemoved)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        void calculatePlayerMovement()
        {
            bool objectsRemoved = false;
            PlayerShip mObj = (PlayerShip)playerShipList[0];

            if (playerMoveX == -1)
                mObj.moveLeft();
            else if (playerMoveX == 1)
                mObj.moveRight();
            if (playerMoveY == -1)
                mObj.moveUp();
            else if (playerMoveY == 1)
                mObj.moveDown();

            double x = mObj.getPosition().X;
            double y = mObj.getPosition().Y;
            double xVel = mObj.getVelocity().X;
            double yVel = mObj.getVelocity().Y;
            double speed = mObj.getSpeed();

            mObj.setPreviousPosition(x, y);

            double newX = x + xVel;
            double newY = y + yVel;

            mObj.setVelocity(xVel * 0.9, yVel * 0.9);

            if (xVel < 0.002 && xVel > -0.002)
                xVel = 0;
            if (yVel < 0.002 && yVel > -0.002)
                yVel = 0;

            if (newX < 0)
                newX = 0;
            else if (newX > 1 - mObj.getSize())
                newX = 1 - mObj.getSize();
            if (newY < 0)
                newY = 0;
            else if (newY > 1 - mObj.getSize())
                newY = 1 - mObj.getSize();

            mObj.setPosition(newX, newY);

            int iCount = playerShipList.Count;

            for (int i = 1; i < iCount; i++)
            {
                x = playerShipList[i].getPosition().X;
                y = playerShipList[i].getPosition().Y;

                playerShipList[i].setPreviousPosition(x, y);

                double velX = playerShipList[i].getVelocity().X;
                double velY = playerShipList[i].getVelocity().Y;

                newX = x + velX;
                newY = y + velY;

                if ((newY > 1.2 || newY < -0.2) || (newX > 1.2 || newX < -0.2))
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            Controls.Remove(playerShipBoxList[i]);
                            playerShipBoxList.RemoveAt(i);
                        }));
                    }
                    catch { };
                    playerShipList.RemoveAt(i);
                    i--;
                    iCount--;

                    if (!objectsRemoved)
                        objectsRemoved = true;
                }
                else
                    playerShipList[i].setPosition(newX, newY);
            }

            if (powerUp != null)
            {
                x = powerUp.getPosition().X;
                y = powerUp.getPosition().Y;

                powerUp.setPreviousPosition(x, y);

                double velX = powerUp.getVelocity().X;
                double velY = powerUp.getVelocity().Y;

                newX = x + velX;
                newY = y + velY;

                if ((newY > 1.2 || newY < -0.2) || (newX > 1.2 || newX < -0.2))
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            Controls.Remove(powerUpBox);
                            powerUpBox = null;
                        }));
                    }
                    catch { };
                    powerUp = null;

                    if (!objectsRemoved)
                        objectsRemoved = true;
                }
                else
                    powerUp.setPosition(newX, newY);
            }

            if (objectsRemoved)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        void calculateSceneryMovement()
        {
            int iCount = sceneryObjectList.Count;

            for (int i = 0; i < iCount; i++)
            {
                double x = sceneryObjectList[i].getPosition().X;
                double y = sceneryObjectList[i].getPosition().Y;

                sceneryObjectList[i].setPreviousPosition(x, y);

                double velX = sceneryObjectList[i].getVelocity().X;
                double velY = sceneryObjectList[i].getVelocity().Y;

                double newX = x + velX;
                double newY = y + velY;


                if (newY > 1.1)
                {
                    sceneryObjectList.RemoveAt(i);
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            Controls.Remove(sceneryObjectBoxList[i]);
                        }));
                    }
                    catch { };
                    sceneryObjectBoxList.RemoveAt(i);
                    i--;
                    iCount--;
                }
                else
                {
                    sceneryObjectList[i].setPosition(newX, newY);
                }
            }
        }

        void singlePositionUpdate(MovingObject mObj, Control box)
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    box.Left = (int)(mObj.getPosition().X * screenWidth - screenWidth * mObj.getSize() / 2);
                    box.Top = (int)(mObj.getPosition().Y * screenHeight - screenWidth * mObj.getSize() / 2);
                }));
            }
            catch { };
        }
        /*
        void updatePositions()
        {
            try
            {
                if (powerUp != null)
                {
                    Position pos = new Position(powerUp.getPosition().X, powerUp.getPosition().Y);
                    Position pPos = new Position(powerUp.getPreviousPosition().X, powerUp.getPreviousPosition().Y);
                    double size = powerUp.getSize();

                    if (pos.X != pPos.X)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            powerUpBox.Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        }));
                    if (pos.Y != pPos.Y)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            powerUpBox.Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);
                        }));

                    Invoke(new MethodInvoker(delegate ()
                    {
                        powerUpBox.Refresh();
                    }));
                }

                for (int i = 0; i < playerShipBoxList.Count; i++)
                {
                    Position pos = new Position(playerShipList[i].getPosition().X, playerShipList[i].getPosition().Y);
                    Position pPos = new Position(playerShipList[i].getPreviousPosition().X, playerShipList[i].getPreviousPosition().Y);
                    double size = playerShipList[i].getSize();

                    if (pos.X != pPos.X)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            playerShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        }));
                    if (pos.Y != pPos.Y)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            playerShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);
                        }));

                    Invoke(new MethodInvoker(delegate ()
                    {
                        playerShipBoxList[i].Refresh();
                    }));
                }

                for (int i = 0; i < enemyShipBoxList.Count; i++)
                {
                    Position pos = new Position(enemyShipList[i].getPosition().X, enemyShipList[i].getPosition().Y);
                    Position pPos = new Position(enemyShipList[i].getPreviousPosition().X, enemyShipList[i].getPreviousPosition().Y);
                    double size = enemyShipList[i].getSize();

                    if (pos.X != pPos.X)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            enemyShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        }));
                    if (pos.Y != pPos.Y)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            enemyShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);
                        }));

                    Invoke(new MethodInvoker(delegate ()
                    {
                        enemyShipBoxList[i].Refresh();
                    }));
                }
                for (int i = 0; i < sceneryObjectBoxList.Count; i++)
                {
                    Position pos = new Position(sceneryObjectList[i].getPosition().X, sceneryObjectList[i].getPosition().Y);
                    Position pPos = new Position(sceneryObjectList[i].getPreviousPosition().X, sceneryObjectList[i].getPreviousPosition().Y);
                    double size = sceneryObjectList[i].getSize();

                    if (pos.X != pPos.X)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            sceneryObjectBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        }));
                    if (pos.Y != pPos.Y)
                        Invoke(new MethodInvoker(delegate ()
                        {
                            sceneryObjectBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);
                        }));

                    Invoke(new MethodInvoker(delegate ()
                    {
                        sceneryObjectBoxList[i].Refresh();
                    }));
                }


            }
            catch { };
        }
        */
        /*
        void updatePositions()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    if (powerUp != null)
                    {
                        Position pos = new Position(powerUp.getPosition().X, powerUp.getPosition().Y);
                        Position pPos = new Position(powerUp.getPreviousPosition().X, powerUp.getPreviousPosition().Y);
                        double size = powerUp.getSize();

                        if (pos.X != pPos.X)
                            powerUpBox.Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        if (pos.Y != pPos.Y)
                            powerUpBox.Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                        powerUpBox.Refresh();
                    }
                    for (int i = 0; i < playerShipBoxList.Count; i++)
                    {
                        Position pos = new Position(playerShipList[i].getPosition().X, playerShipList[i].getPosition().Y);
                        Position pPos = new Position(playerShipList[i].getPreviousPosition().X, playerShipList[i].getPreviousPosition().Y);
                        double size = playerShipList[i].getSize();

                        if (pos.X != pPos.X)
                            playerShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        if (pos.Y != pPos.Y)
                            playerShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                        playerShipBoxList[i].Refresh();
                    }
                    for (int i = 0; i < enemyShipBoxList.Count; i++)
                    {
                        Position pos = new Position(enemyShipList[i].getPosition().X, enemyShipList[i].getPosition().Y);
                        Position pPos = new Position(enemyShipList[i].getPreviousPosition().X, enemyShipList[i].getPreviousPosition().Y);
                        double size = enemyShipList[i].getSize();

                        if (pos.X != pPos.X)
                            enemyShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        if (pos.Y != pPos.Y)
                            enemyShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                        enemyShipBoxList[i].Refresh();
                    }
                    for (int i = 0; i < sceneryObjectBoxList.Count; i++)
                    {
                        Position pos = new Position(sceneryObjectList[i].getPosition().X, sceneryObjectList[i].getPosition().Y);
                        Position pPos = new Position(sceneryObjectList[i].getPreviousPosition().X, sceneryObjectList[i].getPreviousPosition().Y);
                        double size = sceneryObjectList[i].getSize();

                        if (pos.X != pPos.X)
                            sceneryObjectBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                        if (pos.Y != pPos.Y)
                            sceneryObjectBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                        sceneryObjectBoxList[i].Refresh();
                    }
                }));
            }
            catch { };
        }*/

        void checkCollisions()
        {
            int iCount = playerShipList.Count;
            int jCount = enemyShipList.Count;

            if (powerUp != null)
                if (powerUp.getPosition().Y > 1)
                    powerUp = null;

            for (int i = 0; i < iCount; i++)
            {
                if (powerUp != null && i == 0)
                {
                    double size1 = playerShipList[i].getSize();
                    double size2 = powerUp.getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    double pos2X = powerUp.getPosition().X;
                    double pos2Y = powerUp.getPosition().Y;

                    if ((Math.Abs(pos1X - pos2X) < size1 - 0.0125 || Math.Abs(pos1X - pos2X) < size2 - 0.0125) &&
                           (Math.Abs(pos1Y - pos2Y) < size1 - 0.025 || Math.Abs(pos1Y - pos2Y) < size2 - 0.025))
                    {
                        ((PlayerShip)playerShipList[i]).upgrade(powerUp.getShape());
                        powerUp = null;
                        try
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                Controls.Remove(powerUpBox);
                            }));
                        }
                        catch { };
                        powerUpBox = null;
                    }
                }

                for (int j = 0; j < jCount; j++)
                {
                    double size1 = playerShipList[i].getSize();
                    double size2 = enemyShipList[j].getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    double pos2X = enemyShipList[j].getPosition().X;
                    double pos2Y = enemyShipList[j].getPosition().Y;

                    if ((Math.Abs(pos1X - pos2X) < size1 - 0.0125 || Math.Abs(pos1X - pos2X) < size2 - 0.0125) &&
                        (Math.Abs(pos1Y - pos2Y) < size1 - 0.025 || Math.Abs(pos1Y - pos2Y) < size2 - 0.025))
                    {
                        if (i == 0)
                        {
                            playerDead = true;
                        }
                        else
                        {
                            if (enemyShipList[j].GetType() == typeof(EnemyShip) && playerShipList[i].GetType() == typeof(Bullet))
                                if (((EnemyShip)enemyShipList[j]).takeDamage(((Bullet)playerShipList[i]).getDamage()) <= 0)
                                {
                                    score += 5;
                                    enemyShipList.RemoveAt(j);
                                    try
                                    {
                                        Invoke(new MethodInvoker(delegate ()
                                        {
                                            Controls.Remove(enemyShipBoxList[j]);
                                        }));
                                    }
                                    catch { };
                                    enemyShipBoxList.RemoveAt(j);
                                    j--;
                                    jCount--;
                                }
                                else
                                {
                                    Invoke(new MethodInvoker(delegate ()
                                    {
                                        PictureBox x = enemyShipBoxList[j];
                                        string hP = ((EnemyShip)enemyShipList[j]).getHitpoints().ToString();

                                        x.Paint += new PaintEventHandler((sender, e) =>
                                        {
                                            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                            Rectangle rect = new Rectangle(x.Width / 2 - hP.Length * 8 / 2, x.Height / 2 - 5, hP.Length * 8, 10);
                                            e.Graphics.FillRectangle(Brushes.Black, rect);
                                            e.Graphics.DrawString(hP, new Font("Arial", 10, FontStyle.Bold), Brushes.Red, x.Width / 2 - hP.Length * 8 / 2 - 2, x.Height / 2 - 6);
                                        });
                                    }));
                                }

                            playerShipList.RemoveAt(i);
                            try
                            {
                                Invoke(new MethodInvoker(delegate ()
                                {
                                    Controls.Remove(playerShipBoxList[i]);
                                }));
                            }
                            catch { };
                            playerShipBoxList.RemoveAt(i);
                            i--;
                            iCount--;
                        }
                    }
                }
            }
        }

        void initializeGame()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < enemyShipBoxList.Count; i++)
                        Controls.Remove(enemyShipBoxList[i]);

                    for (int i = 0; i < playerShipBoxList.Count; i++)
                        Controls.Remove(playerShipBoxList[i]);
                }));
            }
            catch { };

            enemyShipList.Clear();
            playerShipList.Clear();
            enemyShipBoxList.Clear();
            playerShipBoxList.Clear();

            gameStart = false;
            playerDead = false;
            newName = null;
            continueGame = true;
            score = 0;
            playerMoveY = 0;

            PlayerShip s = new PlayerShip(5);
            playerShipList.Add(s);
            PictureBox x = new PictureBox();
            x.SetBounds((int)(s.getPosition().X * screenWidth), (int)(s.getPosition().Y * screenHeight), (int)(s.getSize() * screenWidth), (int)(s.getSize() * screenHeight));
            x.Image = skins[0];
            playerShipBoxList.Add(x);
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(playerShipBoxList[0]);
                    Controls.Add(x);
                    x.BringToFront();
                }));
            }
            catch { };
            singlePositionUpdate(playerShipList[0], playerShipBoxList[0]);
        }

        static void readHighscores(List<string> listNames, List<int> listScores)
        {
            string[] highscores;

            if (File.Exists("highscores.txt"))
                highscores = File.ReadAllLines("highscores.txt");
            else
            {
                var fileHighscores = File.Create("highscores.txt");
                fileHighscores.Close();
                return;
            }

            for (int i = 0; i < highscores.Count(); i++)
            {
                int tempInt;
                string[] tempString = highscores[i].Split(null);
                int.TryParse(tempString[0], out tempInt);
                listScores.Add(tempInt);
                string fullName = "";
                for (int j = 1; j < tempString.Count(); j++)
                {
                    if (j > 1)
                        fullName += " ";
                    fullName += tempString[j];
                }
                listNames.Add(fullName);
            }
        }

        static void saveHighscores(List<string> listNames, List<int> listScores)
        {
            string[] highscores = new string[listNames.Count];

            for (int i = 0; i < listNames.Count(); i++)
            {
                string tempString = listScores[i] + " " + listNames[i];
                highscores[i] = tempString;
            }

            var fileHighscores = File.Create("highscores.txt");
            fileHighscores.Close();
            File.WriteAllLines("highscores.txt", highscores);
        }

        static bool checkHighscores(int value)
        {
            if (highscoresScores.Count < 9)
                return true;

            for (int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                    return true;
            }

            return false;
        }

        static void addHighscore(int value, string name)
        {
            for (int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                {
                    highscoresScores.Insert(i, value);
                    highscoresNames.Insert(i, name);

                    if (highscoresScores.Count > 9)
                    {
                        highscoresScores.RemoveAt(9);
                        highscoresNames.RemoveAt(9);
                    }

                    return;
                }
            }

            highscoresScores.Add(value);
            highscoresNames.Add(name);
        }

        void showHighscores()
        {
            highscoresIcons = new List<Label>();

            Label hLabel = new Label();
            hLabel.BackColor = Color.Black;
            hLabel.ForeColor = Color.LightGray;
            hLabel.TextAlign = ContentAlignment.MiddleCenter;
            hLabel.Text = "H i g h s c o r e s";
            hLabel.AutoSize = true;
            hLabel.Font = new Font("Arial", 10);
            hLabel.Top = screenHeight / 3 - 40;
            hLabel.Refresh();
            highscoresIcons.Add(hLabel);
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(hLabel);
                    hLabel.BringToFront();
                    hLabel.Left = screenWidth / 2 - hLabel.Width / 2;
                }));
            }
            catch { };

            int i;

            for (i = 0; i < highscoresNames.Count; i++)
            {
                hLabel = new Label();
                hLabel.BackColor = Color.Black;
                hLabel.ForeColor = Color.LightGray;
                hLabel.TextAlign = ContentAlignment.MiddleLeft;
                hLabel.Text = (i + 1).ToString() + ".  " + highscoresNames[i];
                hLabel.AutoSize = true;
                hLabel.Font = new Font("Arial", 10);
                hLabel.Left = screenWidth / 4;// - 100;
                hLabel.Top = screenHeight / 3 + 25 * i;
                hLabel.Refresh();
                highscoresIcons.Add(hLabel);
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Add(hLabel);
                        hLabel.BringToFront();
                    }));
                }
                catch { };
            }

            for (i = 0; i < highscoresScores.Count; i++)
            {
                hLabel = new Label();
                hLabel.BackColor = Color.Black;
                hLabel.ForeColor = Color.LightGray;
                hLabel.TextAlign = ContentAlignment.MiddleRight;
                hLabel.Text = highscoresScores[i].ToString();
                hLabel.AutoSize = true;
                hLabel.Font = new Font("Arial", 10);
                hLabel.Refresh();
                highscoresIcons.Add(hLabel);
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Add(hLabel);
                        hLabel.BringToFront();
                        hLabel.Left = screenWidth * 3 / 4 - hLabel.Width;// + 110// - hLabel.Width;
                        hLabel.Top = screenHeight / 3 + 25 * i;
                    }));
                }
                catch { };
            }

            hLabel = new Label();
            hLabel.BackColor = Color.Black;
            hLabel.ForeColor = Color.LightGray;
            hLabel.TextAlign = ContentAlignment.MiddleCenter;
            hLabel.Text = "B a c k";
            hLabel.AutoSize = true;
            hLabel.Font = new Font("Arial", 10);
            hLabel.Top = screenHeight / 3 + 25 * 9 + 50;
            hLabel.Refresh();
            highscoresIcons.Add(hLabel);
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(hLabel);
                    hLabel.BringToFront();
                    hLabel.Left = screenWidth / 2 - hLabel.Width / 2;
                }));
            }
            catch { };

        }

        void removeHighscores()
        {
            for (int i = 0; i < highscoresIcons.Count; i++)
            {
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Remove(highscoresIcons[i]);
                    }));
                }
                catch { };
            }

            highscoresIcons.Clear();
        }

        void showGameOver()
        {
            Label gOContinue = new Label();
            gOContinue.BackColor = Color.Black;
            gOContinue.ForeColor = Color.LightGray;
            gOContinue.AutoSize = true;
            gOContinue.Top = screenHeight / 3 + 25 * 9 + 50;
            gOContinue.Text = "C o n t i n u e";
            gOContinue.Font = new Font("Arial", 10);

            Label gOGameOver = new Label();
            gOGameOver.BackColor = Color.Black;
            gOGameOver.ForeColor = Color.LightGray;
            gOGameOver.AutoSize = true;
            gOGameOver.Top = screenHeight / 3;
            gOGameOver.Text = "G a m e   O v e r";
            gOGameOver.Font = new Font("Arial", 10);

            Label gOScore = new Label();
            gOScore.BackColor = Color.Black;
            gOScore.ForeColor = Color.LightGray;
            gOScore.AutoSize = true;
            gOScore.Top = screenHeight / 3 + 46;
            gOScore.Text = "Score:   " + (score + timeScore).ToString();
            gOScore.Font = new Font("Arial", 10);

            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(gOContinue);
                    gOContinue.BringToFront();
                    gOContinue.Left = screenWidth / 2 - gOContinue.Width / 2 + 5;

                    Controls.Add(gOGameOver);
                    gOGameOver.BringToFront();
                    gOGameOver.Left = screenWidth / 2 - gOGameOver.Width / 2 + 5;

                    Controls.Add(gOScore);
                    gOScore.BringToFront();
                    gOScore.Left = screenWidth / 2 - gOScore.Width / 2 + 5;

                    for (int i = menuIcons.Count - 2; i <= menuIcons.Count - 1; i++)
                    {
                        menuIcons[i].Top = screenHeight / 3 + 25 * 9 + 50;
                        menuIcons[i].Visible = true;
                    }
                }));
            }
            catch { };
        }

        void removeGameOver()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < 3; i++)
                        Controls.RemoveAt(0);
                }));
            }
            catch { };
        }

        void showNewHighscore()
        {
            Label gOContinue = new Label();
            gOContinue.BackColor = Color.Black;
            gOContinue.ForeColor = Color.LightGray;
            gOContinue.AutoSize = true;
            gOContinue.Top = screenHeight / 3 + 25 * 9 + 50;
            gOContinue.Text = "C o n t i n u e";
            gOContinue.Font = new Font("Arial", 10);

            Label gOGameOver = new Label();
            gOGameOver.BackColor = Color.Black;
            gOGameOver.ForeColor = Color.LightGray;
            gOGameOver.AutoSize = true;
            gOGameOver.Top = screenHeight / 3;
            gOGameOver.Text = "N e w   H i g h s c o r e !";
            gOGameOver.Font = new Font("Arial", 10);

            Label gOScore = new Label();
            gOScore.BackColor = Color.Black;
            gOScore.ForeColor = Color.LightGray;
            gOScore.AutoSize = true;
            gOScore.Top = screenHeight / 3 + 46;
            gOScore.Text = "Score:   " + (score + timeScore).ToString();
            gOScore.Font = new Font("Arial", 10);

            Label gOName = new Label();
            gOName.BackColor = Color.Black;
            gOName.ForeColor = Color.LightGray;
            gOName.AutoSize = true;
            gOName.Top = screenHeight / 3 + 80;
            gOName.Text = "Your name:";
            gOName.Font = new Font("Arial", 10);

            TextBox gOIn = new TextBox();
            gOIn.BackColor = Color.Black;
            gOIn.ForeColor = Color.LightGray;
            gOIn.BorderStyle = BorderStyle.None;
            gOIn.Height = 20;
            gOIn.Width = 300;
            gOIn.TextAlign = HorizontalAlignment.Center;
            gOIn.MaxLength = 20;
            gOIn.Top = screenHeight / 3 + 110;
            gOIn.Font = new Font("Arial", 10);
            gOIn.KeyDown += NameEntered;

            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(gOContinue);
                    gOContinue.BringToFront();
                    gOContinue.Left = screenWidth / 2 - gOContinue.Width / 2 + 5;

                    Controls.Add(gOGameOver);
                    gOGameOver.BringToFront();
                    gOGameOver.Left = screenWidth / 2 - gOGameOver.Width / 2 + 5;

                    Controls.Add(gOScore);
                    gOScore.BringToFront();
                    gOScore.Left = screenWidth / 2 - gOScore.Width / 2 + 5;

                    Controls.Add(gOName);
                    gOName.BringToFront();
                    gOName.Left = screenWidth / 2 - gOName.Width / 2 + 5;

                    Controls.Add(gOIn);
                    gOIn.BringToFront();
                    gOIn.Left = screenWidth / 2 - gOIn.Width / 2 + 5;
                    gOIn.Focus();

                    for (int i = menuIcons.Count - 2; i <= menuIcons.Count - 1; i++)
                    {
                        menuIcons[i].Top = screenHeight / 3 + 25 * 9 + 50;
                        menuIcons[i].Visible = true;
                    }
                }));
            }
            catch { };
        }

        void removeNewHighscore()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < 5; i++)
                        Controls.RemoveAt(0);
                }));
            }
            catch { };
        }


        public ScreenView()
        {
            InitializeComponent();
        }


        private void AnyKeyDown(object sender, KeyEventArgs e)
        {
            if (continueGame)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX > -1)
                            playerMoveX--;
                    }
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX < 1)
                            playerMoveX++;
                    }
                }
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    if (!gameStart)
                    {
                        if (menu.getSelection() != 3)
                        {
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.selectUp();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                        }
                    }
                    else
                    {
                        lock (lockYMovement)
                        {
                            if (playerMoveY > -1)
                                playerMoveY--;
                        }
                    }
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    if (!gameStart)
                    {
                        if (menu.getSelection() != 3)
                        {
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.selectDown();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                        }
                    }
                    else
                    {
                        lock (lockYMovement)
                        {
                            if (playerMoveY < 1)
                                playerMoveY++;
                        }
                    }
                }

            }
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (gameStart)
                    playerFire = true;
                else
                {
                    switch (menu.getSelection())
                    {
                        case 0:
                            gameStart = true;
                            titleImage.Visible = false;
                            for (int i = 0; i < menuIcons.Count; i++)
                                menuIcons[i].Visible = false;
                            gameTime.Restart();
                            break;
                        case 1:
                            for (int i = 0; i < menuIcons.Count - 2; i++)
                                menuIcons[i].Visible = false;
                            showHighscores();
                            menuIcons[menuIcons.Count - 2].Top = screenHeight / 3 + 25 * 9 + 50;
                            menuIcons[menuIcons.Count - 1].Top = screenHeight / 3 + 25 * 9 + 50;
                            menu.setHighscoresOn();
                            break;
                        case 2:
                            gameRunning = false;
                            Close();
                            break;
                        case 3:
                            highscoresOn = false;
                            menu.setHighscoresOff();
                            removeHighscores();
                            for (int i = 0; i < menuIcons.Count - 2; i++)
                                menuIcons[i].Visible = true;
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                            break;
                        default: break;
                    }
                }

                if (!continueGame)
                    continueGame = true;

            }
            if (e.KeyCode == Keys.Escape)
            {
                playerDead = false;

                if (gameStart)
                    gameStart = false;
                else
                    gameRunning = false;
            }
        }

        private void AnyKeyUp(object sender, KeyEventArgs e)
        {
            if (continueGame)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX < 1)
                            playerMoveX++;
                    }
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX > -1)
                            playerMoveX--;
                    }
                }
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    if (gameStart)
                        lock (lockYMovement)
                        {
                            if (playerMoveY < 1)
                                playerMoveY++;
                        }
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    if (gameStart)
                        lock (lockYMovement)
                        {
                            if (playerMoveY > -1)
                                playerMoveY--;
                        }
                }
            }
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                playerFire = false;
            }
        }

        private void NameEntered(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                newName = (sender as TextBox).Text.ToString();
            }
        }


        private void mainLoop()
        {
            while (gameRunning)
            {
                stopWatch.Start();
                frameCounter++;

                if (frameCounter == 10)
                {
                    SceneryObject s = new SceneryObject();
                    sceneryObjectList.Add(s);
                    PictureBox x = new PictureBox();
                    x.SetBounds((int)(s.getPosition().X * screenWidth), (int)(s.getPosition().Y * screenHeight), (int)(s.getSize() * screenWidth), (int)(s.getSize() * screenHeight));
                    x.Image = s.getImage();
                    x.SizeMode = PictureBoxSizeMode.StretchImage;
                    sceneryObjectBoxList.Add(x);
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            Controls.Add(x);
                            Controls.SetChildIndex(x, Controls.Count - 2);
                        }));
                    }
                    catch { };
                    singlePositionUpdate(s, x);
                }

                if (gameStart)
                {
                    if (!playerDead)
                    {
                        // in-game logic - random enemy movement etc. --------------------
                        if (playerFire)
                        {
                            List<Bullet> bulletList = new List<Bullet>();
                            bulletList = ((PlayerShip)playerShipList[0]).fire();
                            if (bulletList.Count > 0)
                            {
                                List<PictureBox> bullets = new List<PictureBox>();
                                for (int b = 0; b < bulletList.Count; b++)
                                {
                                    bullets.Add(new PictureBox());
                                    playerShipList.Add(bulletList[b]);
                                }

                                try
                                {
                                    Invoke(new MethodInvoker(delegate ()
                                    {
                                        for (int b = 0; b < bullets.Count; b++)
                                        {
                                            bullets[b].SetBounds(0, (int)(-0.1 * screenHeight), (int)(0.0125 * screenWidth), (int)(0.0125 * screenHeight));
                                            bullets[b].Image = bulletSkins[b + 1];
                                            playerShipBoxList.Add(bullets[b]);
                                            Controls.Add(bullets[b]);
                                            bullets[b].BringToFront();
                                        }
                                    }));
                                }
                                catch { };
                                for (int b = 0; b < bulletList.Count; b++)
                                {
                                    singlePositionUpdate(bulletList[b], bullets[b]);
                                }
                            }
                        }

                        if (frameCounter == 10)
                        {
                            if (maxEnemiesCount < 6)
                                maxEnemiesCount = 1 + (int)(gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 3000));

                            for (int i = 0; i < maxEnemiesCount; i++)
                            {
                                EnemyShip s = new EnemyShip(difficultyTimeIncrement, 3);
                                enemyShipList.Add(s);
                                PictureBox x = new PictureBox();
                                x.SetBounds((int)(s.getPosition().X * screenWidth), (int)(s.getPosition().Y * screenHeight), (int)(s.getSize() * screenWidth), (int)(s.getSize() * screenHeight));
                                x.Image = skins[1];
                                string hP = s.getHitpoints().ToString();
                                enemyShipBoxList.Add(x);
                                try
                                {
                                    Invoke(new MethodInvoker(delegate ()
                                    {
                                        Controls.Add(x);
                                        x.BringToFront();
                                        x.Paint += new PaintEventHandler((sender, e) =>
                                         {
                                             e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                             Rectangle rect = new Rectangle(x.Width / 2 - hP.Length * 8 / 2, x.Height / 2 - 5, hP.Length * 8, 10);
                                             e.Graphics.FillRectangle(Brushes.Black, rect);
                                             e.Graphics.DrawString(hP, new Font("Arial", 10, FontStyle.Bold), Brushes.Red, x.Width / 2 - hP.Length * 8 / 2 - 2, x.Height / 2 - 6);
                                         });
                                    }));
                                }
                                catch { };
                                singlePositionUpdate(enemyShipList[i], enemyShipBoxList[i]);
                            }
                        }

                        if (frameCounter == 20 && enemyShipList.Count != 0)
                        {
                            double maxMoveSpeed = 0.1;

                            if (enemyShipList[0] != null)
                                maxMoveSpeed = enemyShipList[0].getMaxVelocity();

                            for (int i = 0; i < enemyShipList.Count; i++)
                            {
                                if (enemyShipList[i].GetType() == typeof(PlayerShip) || enemyShipList[i].GetType() == typeof(Bullet))
                                    continue;

                                if (enemyShipList[i].getVelocity().X > 0)
                                    enemyShipList[i].moveLeft(rnd.NextDouble() % (maxMoveSpeed / 4));
                                else
                                    enemyShipList[i].moveRight(rnd.NextDouble() % (maxMoveSpeed / 4));

                                if (rnd.NextDouble() < (double)gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 1000) / 100)
                                {
                                    List<Bullet> bulletList = new List<Bullet>();
                                    bulletList = ((EnemyShip)enemyShipList[i]).fire();
                                    if (bulletList.Count > 0)
                                    {
                                        PictureBox bullet = new PictureBox();
                                        enemyShipList.Add(bulletList[0]);
                                        bullet.SetBounds((int)(bulletList[0].getPosition().X), (int)(bulletList[0].getPosition().Y), (int)(bulletList[0].getSize() * screenWidth), (int)(bulletList[0].getSize() * screenHeight));
                                        enemyShipBoxList.Add(bullet);
                                        try
                                        {
                                            Invoke(new MethodInvoker(delegate ()
                                            {
                                                bullet.Image = bulletSkins[0];
                                                Controls.Add(bullet);
                                                bullet.BringToFront();
                                            }));
                                        }
                                        catch { };
                                        singlePositionUpdate(bulletList[0], bullet);
                                    }
                                }
                            }
                        }

                        if (frameCounter == 25)
                        {
                            if (powerUp == null)
                            {
                                if ((double)gameTime.ElapsedMilliseconds % (difficultyTimeIncrement * 1000) > difficultyTimeIncrement * 800)

                                {
                                    powerUp = new PowerUp((PlayerShip)playerShipList[0]);
                                    powerUpBox = new PictureBox();
                                    powerUpBox.SetBounds(0, (int)(-0.1 * screenHeight), (int)(powerUp.getSize() * screenWidth), (int)(powerUp.getSize() * screenHeight));
                                    powerUpBox.Image = skins[2];
                                    try
                                    {
                                        Invoke(new MethodInvoker(delegate ()
                                        {
                                            Controls.Add(powerUpBox);
                                            Controls.SetChildIndex(powerUpBox, Controls.Count - 2);
                                            powerUpBox.Paint += new PaintEventHandler((sender, e) =>
                                            {
                                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                                e.Graphics.DrawString(powerUp.getShape().ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.LightGreen, powerUpBox.Width / 2 - 6, powerUpBox.Height / 2 - 7);
                                            });
                                        }));
                                    }
                                    catch { };
                                    singlePositionUpdate(powerUp, powerUpBox);
                                }
                            }
                        }



                        checkCollisions();
                        calculateEnemyMovement();

                        timeScore = (int)gameTime.ElapsedMilliseconds / 1000;
                        try
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                scoreLabel.Text = "Score: " + (score + timeScore);
                            }));
                        }
                        catch { };


                        // ------------------------------------------
                    }
                    else
                    {
                        if (checkHighscores(score + timeScore))
                        {
                            continueGame = false;
                            showNewHighscore();

                            while (newName == null)
                            {
                                if (!gameRunning)
                                    break;
                            }
                            removeNewHighscore();

                            addHighscore(score + timeScore, newName);
                            saveHighscores(highscoresNames, highscoresScores);

                            try
                            {
                                Invoke(new MethodInvoker(delegate ()
                                {
                                    titleImage.Visible = true;

                                    for (int i = 0; i < menuIcons.Count; i++)
                                        menuIcons[i].Visible = true;
                                    menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                    menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                }));
                            }
                            catch { };
                        }
                        else
                        {
                            continueGame = false;
                            showGameOver();
                            while (!continueGame)
                            {
                                if (!gameRunning)
                                    break;
                            }
                            removeGameOver();

                            try
                            {
                                Invoke(new MethodInvoker(delegate ()
                                {
                                    titleImage.Visible = true;

                                    for (int i = 0; i < menuIcons.Count; i++)
                                        menuIcons[i].Visible = true;
                                    menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                    menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                }));
                            }
                            catch { };
                        }

                        initializeGame();
                    }

                }

                if (!playerDead)
                    calculatePlayerMovement();
                calculateSceneryMovement();

                if (frameCounter % 3 == 0)
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            if (sceneryBackground.Top == -1)
                                sceneryBackground.Top = -screenHeight * 2;
                            else
                                sceneryBackground.Top += 1;
                        }));
                    }
                    catch { };
                }

                //if (!bw.IsBusy)
                //    bw.RunWorkerAsync();
                //updatePositions();
                drawing = true;
                Invalidate();
                while(drawing && gameRunning)
                {

                }

                sleepTime = frameTime - (int)stopWatch.ElapsedMilliseconds;
                if (sleepTime > 0)
                    Thread.Sleep(sleepTime);

                if (frameCounter == framesPerSecondTop)
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            frameCounter = 0;
                            frameCounterLabel.Text = "FPS: " + (1000 / stopWatch.ElapsedMilliseconds + 1) + " ";
                            frameCounterLabel.Refresh();
                        }));
                    }
                    catch { };
                }

                stopWatch.Reset();
            }
        }


        private void ScreenView_Shown(object sender, EventArgs e)
        {
            initializeGame();
            Refresh();

            Thread t = new Thread(new ThreadStart(mainLoop));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void ScreenView_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameRunning = false;
        }

        private void ScreenView_Load(object sender, EventArgs e)
        {
            skins.Add(new Bitmap("..\\..\\media\\playership.png"));
            skins.Add(new Bitmap("..\\..\\media\\enemyship.png"));
            skins.Add(new Bitmap("..\\..\\media\\powerup.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\enemybullet.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\playerbullet1.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\playerbullet2.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\playerbullet3.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\playerbullet4.png"));
            bulletSkins.Add(new Bitmap("..\\..\\media\\playerbullet5.png"));
            for (int i = 0; i < skins.Count; i++)
                skins[i].MakeTransparent();
            scenerySkins.Add(new Bitmap("..\\..\\media\\sceneryBackground.png"));
            scenerySkins.Add(new Bitmap("..\\..\\media\\scenery2.png"));
            for (int i = 0; i < scenerySkins.Count; i++)
                scenerySkins[i].MakeTransparent();
            SceneryObject.setImageList(scenerySkins);
            sceneryBackground = new PictureBox();
            sceneryBackground.Image = scenerySkins[0];
            sceneryBackground.SetBounds(0, -screenHeight * 2, screenWidth, screenHeight * 3);
            Controls.Add(sceneryBackground);
            frameCounterLabel.BackColor = Color.Black;
            frameCounterLabel.ForeColor = Color.LightGray;
            frameCounterLabel.Top = 20;
            frameCounterLabel.Left = 20;
            frameCounterLabel.AutoSize = true;
            frameCounterLabel.Font = new Font("Arial", 10);
            scoreLabel.BackColor = Color.Black;
            scoreLabel.ForeColor = Color.LightGray;
            scoreLabel.Top = screenHeight - 74;
            scoreLabel.Left = 20;
            scoreLabel.AutoSize = true;
            scoreLabel.Font = new Font("Arial", 10);
            Controls.Add(frameCounterLabel);
            Controls.Add(scoreLabel);
            frameCounterLabel.BringToFront();
            scoreLabel.BringToFront();
            menu = FireShip.Menu.Create();
            titleImage = new PictureBox();
            Bitmap title = new Bitmap("..\\..\\media\\title.png");
            title.MakeTransparent();
            titleImage.Image = title;
            titleImage.SetBounds(screenWidth / 2 - title.Width / 2, screenHeight / 10, title.Width, title.Height);
            Controls.Add(titleImage);
            titleImage.BringToFront();
            menuIcons = new List<Label>();
            List<string> menuOptions = menu.getOptions();
            for (int i = 0; i < menuOptions.Count + 2; i++)
            {
                Label menuOption = new Label();
                menuOption.BackColor = Color.Black;
                menuOption.ForeColor = Color.LightGray;
                menuOption.TextAlign = ContentAlignment.MiddleCenter;
                menuOption.AutoSize = true;
                menuOption.Font = new Font("Arial", 10);
                menuOption.Refresh();
                menuIcons.Add(menuOption);
                Controls.Add(menuIcons[i]);
                menuIcons[i].BringToFront();
                if (i < menuOptions.Count)
                {
                    menuOption.Text = menuOptions[i];
                    menuOption.Left = screenWidth / 2 - menuOption.Width / 2 + 5;
                    menuOption.Top = screenHeight * 2 / 5 + 30 * i;
                }
                else if (i == menuOptions.Count + 1)
                {
                    menuOption.Text = ">";
                    menuOption.Left = screenWidth / 3;
                    menuOption.Top = screenHeight * 2 / 5;
                }
                else
                {
                    menuOption.Text = "<";
                    menuOption.Left = screenWidth * 2 / 3;
                    menuOption.Top = screenHeight * 2 / 5;
                }
            }

            readHighscores(highscoresNames, highscoresScores);
        }

        private void ScreenView_Paint(object sender, PaintEventArgs e)
        {
            if (powerUp != null)
            {
                Position pos = new Position(powerUp.getPosition().X, powerUp.getPosition().Y);
                Position pPos = new Position(powerUp.getPreviousPosition().X, powerUp.getPreviousPosition().Y);
                double size = powerUp.getSize();

                if (pos.X != pPos.X)
                    powerUpBox.Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                if (pos.Y != pPos.Y)
                    powerUpBox.Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                powerUpBox.Refresh();
            }
            for (int i = 0; i < playerShipBoxList.Count; i++)
            {
                Position pos = new Position(playerShipList[i].getPosition().X, playerShipList[i].getPosition().Y);
                Position pPos = new Position(playerShipList[i].getPreviousPosition().X, playerShipList[i].getPreviousPosition().Y);
                double size = playerShipList[i].getSize();

                if (pos.X != pPos.X)
                    playerShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                if (pos.Y != pPos.Y)
                    playerShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                playerShipBoxList[i].Refresh();
            }
            for (int i = 0; i < enemyShipBoxList.Count; i++)
            {
                Position pos = new Position(enemyShipList[i].getPosition().X, enemyShipList[i].getPosition().Y);
                Position pPos = new Position(enemyShipList[i].getPreviousPosition().X, enemyShipList[i].getPreviousPosition().Y);
                double size = enemyShipList[i].getSize();

                if (pos.X != pPos.X)
                    enemyShipBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                if (pos.Y != pPos.Y)
                    enemyShipBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                enemyShipBoxList[i].Refresh();
            }
            for (int i = 0; i < sceneryObjectBoxList.Count; i++)
            {
                Position pos = new Position(sceneryObjectList[i].getPosition().X, sceneryObjectList[i].getPosition().Y);
                Position pPos = new Position(sceneryObjectList[i].getPreviousPosition().X, sceneryObjectList[i].getPreviousPosition().Y);
                double size = sceneryObjectList[i].getSize();

                if (pos.X != pPos.X)
                    sceneryObjectBoxList[i].Left = (int)(pos.X * screenWidth - screenWidth * size / 2);
                if (pos.Y != pPos.Y)
                    sceneryObjectBoxList[i].Top = (int)(pos.Y * screenHeight - screenWidth * size / 2);

                sceneryObjectBoxList[i].Refresh();
            }

            drawing = false;
        }
    }
}
