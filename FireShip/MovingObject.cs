using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace FireShip
{
    public class MovingObject
    {
        protected Position position;
        protected Position previousPosition;
        protected Velocity velocity;
        protected double maxVelocity;
        protected double size;

        public Position getPosition()
        {
            return position;
        }

        public void setPosition(double x, double y)
        {
            position.X = x;
            position.Y = y;
        }

        public Position getPreviousPosition()
        {
            return previousPosition;
        }

        public void setPreviousPosition(double x, double y)
        {
            previousPosition.X = x;
            previousPosition.Y = y;
        }

        public Velocity getVelocity()
        {
            return velocity;
        }

        public void setVelocity(double x, double y)
        {
            velocity.X = x;
            velocity.Y = y;
        }

        public double getSize()
        {
            return size;
        }

        public void setSize(double newSize)
        {
            size = newSize;
        }

        public double getMaxVelocity()
        {
            return maxVelocity;
        }

        public void moveUp(double moveVelocity)
        {
            velocity.Y -= moveVelocity;
            if (velocity.Y > maxVelocity)
                velocity.Y = maxVelocity;
        }

        public void moveDown(double moveVelocity)
        {
            velocity.Y += moveVelocity;
            if (velocity.Y < 0 - maxVelocity)
                velocity.Y = 0 - maxVelocity;
        }

        public void moveLeft(double moveVelocity)
        {
            velocity.X -= moveVelocity;
            if (velocity.X > maxVelocity)
                velocity.X = maxVelocity;
        }

        public void moveRight(double moveVelocity)
        {
            velocity.X += moveVelocity;
            if (velocity.X < 0 - maxVelocity)
                velocity.X = 0 - maxVelocity;
        }
    }

    public class EnemyShip : MovingObject
    {
        int hitPoints;

        public EnemyShip(int enemyLDifficultyifeIncrementInSeconds, int enemySize)
        {
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = -0.05;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            velocity.X = 0;
            velocity.Y = 0.001 + 0.0006 * (int)ScreenView.gameTime.ElapsedMilliseconds / (enemyLDifficultyifeIncrementInSeconds * 1000);
            maxVelocity = 0.01;

            hitPoints = 1 + (int)ScreenView.gameTime.ElapsedMilliseconds / (enemyLDifficultyifeIncrementInSeconds * 2000);
            size = 0.0125 * enemySize;
        }

        public List<Bullet> fire() { return Bullet.Fire(this); }
        public int getHitpoints() { return hitPoints; }

        public int takeDamage(int damage)
        {
            hitPoints -= damage;
            return hitPoints;
        }
    }

    public class PlayerShip : MovingObject
    {
        int power;
        double fireRate;
        Stopwatch shotCooldown;
        double speed;
        int multipleBullets;
        double moveVelocity;

        public PlayerShip(int playerSize)
        {
            position.X = 0.5 - 0.0125;
            position.Y = 0.8;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            velocity.X = 0;
            velocity.Y = 0;
            maxVelocity = 0.005;
            moveVelocity = 0.002;
            size = 0.0125 * playerSize;

            power = 1;
            fireRate = 1;
            shotCooldown = new Stopwatch();
            shotCooldown.Start();
            speed = 1;
            multipleBullets = 1;
        }

        public int getPower() { return power; }
        public double getFirerate() { return fireRate; }
        public double getSpeed() { return speed; }
        public int getMultipleBullets() { return multipleBullets; }

        public void addPower() { power++; }
        public void addFirerate() { fireRate++; }
        public void addSpeed() { speed++; }
        public void addMultipleBullets() { multipleBullets++; }

        public void moveUp()
        {
            velocity.Y -= moveVelocity * speed / 2;
            if (velocity.Y < 0 - maxVelocity * speed)
                velocity.Y = 0 - maxVelocity * speed;
        }

        public void moveDown()
        {
            velocity.Y += moveVelocity * speed / 2;
            if (velocity.Y > maxVelocity * speed)
                velocity.Y = maxVelocity * speed;
        }

        public void moveLeft()
        {
            velocity.X -= moveVelocity * speed / 2;
            if (velocity.X < 0 - maxVelocity * speed)
                velocity.X = 0 - maxVelocity * speed;
        }

        public void moveRight()
        {
            velocity.X += moveVelocity * speed / 2;
            if (velocity.X > maxVelocity * speed)
                velocity.X = maxVelocity * speed;
        }

        public List<Bullet> fire()
        {
            if (shotCooldown.ElapsedMilliseconds >= 1000 / fireRate)
            {
                shotCooldown.Restart();
                return Bullet.Fire(this);
            }
            else
                return new List<Bullet>();
        }

        public void upgrade(char c)
        {
            switch (c)
            {
                case 'P':
                    power++;
                    break;
                case 'F':
                    fireRate += 0.3;
                    break;
                case 'S':
                    speed += 0.2;
                    break;
                case 'M':
                    multipleBullets++;
                    break;
                default: break;
            }
        }
    }

    public class Bullet : MovingObject
    {
        protected int damage;
        protected char type;

        private Bullet(Position position, Velocity velocity, int damage, char type)
        {
            this.position.X = position.X;
            this.position.Y = position.Y;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            this.velocity.X = velocity.X;
            this.velocity.Y = velocity.Y;
            maxVelocity = 0.04;
            size = 0.0125;
            this.damage = damage;
            this.type = type;
        }
        
        public int getDamage() { return damage; }
        public char getShape() { return type; }

        static public List<Bullet> Fire(MovingObject ship)
        {
            List<Bullet> backFire = new List<Bullet>();

            if (ship.GetType() == typeof(PlayerShip))
            {
                PlayerShip pShip = (PlayerShip)ship;
                Position bulletPosition = new Position(ship.getPosition(), 0, 0 - (ship.getSize() + 0.0125) / 2);
                int bulletCount = pShip.getMultipleBullets();

                if (bulletCount >= 1)
                    backFire.Add(new Bullet(bulletPosition, new Velocity(0, -0.01), pShip.getPower(), '|'));
                if (bulletCount >= 2)
                    backFire.Add(new Bullet(bulletPosition, new Velocity(-0.007, -0.007), pShip.getPower(), '\\'));
                if (bulletCount >= 3)
                    backFire.Add(new Bullet(bulletPosition, new Velocity(0.007, -0.007), pShip.getPower(), '/'));
                if (bulletCount >= 4)
                    backFire.Add(new Bullet(bulletPosition, new Velocity(0.01, 0), pShip.getPower(), '¯'));
                if (bulletCount >= 5)
                    backFire.Add(new Bullet(bulletPosition, new Velocity(-0.01, 0), pShip.getPower(), '¯'));
            }
            else
            {
                EnemyShip pShip = (EnemyShip)ship;
                Position bulletPosition = new Position(ship.getPosition(), 0, (ship.getSize() + 0.0125) / 2);
                backFire.Add(new Bullet(bulletPosition, new Velocity(0, 0.01), 1, '|'));
            }

            return backFire;
        }


    }

    public class SceneryObject : MovingObject
    {
        static List<Bitmap> imgList = new List<Bitmap>();
        public Bitmap image;

        public SceneryObject()
        {
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            position.X = tempPosX;
            position.Y = -0.05;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            maxVelocity = 0.01;
            size = 0.0125;
            
            double randomShape = ScreenView.rnd.NextDouble();

            try { image = imgList[1]; }
            catch { };
            size = randomShape / 100;
            if (size < 0.0045)
            {
                size = 0.0045;
                velocity.Y = 0.0005;
            }
            else
                velocity.Y = randomShape / 1000;

        }

        public SceneryObject(Velocity v, Position p, Bitmap image)
        {
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            position.X = p.X;
            position.Y = p.Y;
            previousPosition.X = p.X - 1;
            previousPosition.Y = 0;
            velocity.X = v.X;
            velocity.Y = v.Y;
            maxVelocity = 0.01;
            size = 0.0125;
            this.image = image;
        }
        
        public static void setImageList(List<Bitmap> list) { imgList = list; }
        internal Image getImage() { return image; }
    }

    public class PowerUp : MovingObject
    {
        protected char shape;

        public PowerUp(PlayerShip ship)
        {
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = -0.1;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = -0.1;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;
            size = 0.0125 * 3;

            double value = ScreenView.rnd.NextDouble();

            if (value < 0.2)
            {
                shape = 'S';
            }
            else if (value < 0.5)
            {
                if (ship.getMultipleBullets() < 5)
                {
                    shape = 'M';
                }
                else
                {
                    shape = 'P';
                }
            }
            else if (value < 0.8)
            {
                shape = 'F';
            }
            else if (value <= 1)
            {
                shape = 'P';
            }
        }

        public PowerUp(char shape)
        {
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;
            size = 0.0125 * 3;
            this.shape = shape;
        }

        public char getShape() { return shape; }
    }

    public class Menu
    {
        private static Menu instance;
        protected List<string> menuList = new List<string>();
        protected int selection;
        protected int previousSelection;
        protected bool selectionChanged;
        protected bool highscoresOn;

        private Menu()
        {
            menuList.Add("S t a r t");
            menuList.Add("H i g h s c o r e s");
            menuList.Add("E x i t");

            selection = 0;
            selectionChanged = false;
            highscoresOn = false;
        }

        public static Menu Create()
        {
            if (instance == null)
                instance = new Menu();

            return instance;
        }

        public List<string> getOptions() { return menuList; }
        public void setHighscoresOn() { highscoresOn = true; }
        public void setHighscoresOff() { highscoresOn = false; }

        public int selectUp()
        {
            previousSelection = selection;

            selection--;
            if (selection < 0)
                selection = menuList.Count - 1;

            selectionChanged = true;
            return selection;
        }

        public int selectDown()
        {
            previousSelection = selection;

            selection++;
            selection = selection % menuList.Count;

            selectionChanged = true;
            return selection;
        }

        public int getSelection()
        {
            if (highscoresOn)
            {
                selection = 1;
                return 3;
            }

            return selection;
        }
    }
}
