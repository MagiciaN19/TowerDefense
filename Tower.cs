using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public abstract class Tower
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public float Range { get; protected set; }
        public int Damage { get; protected set; }
        public int ReloadTime { get; protected set; }
        protected int currentCoolDown;
        public int Level { get; protected set; } = 1;
        public int UpgradeCost { get; protected set; } = 50;
        public const int MaxLevel = 4;

        public Tower(int x, int y)
        {
            X = x;
            Y = y;
        }

        public virtual Bullet Update(List<Enemy> enemies)
        {
            if (currentCoolDown > 0)
            {
                currentCoolDown--;
                return null;
            }

            Enemy nearestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                float dx = enemy.X - X;
                float dy = enemy.Y - Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= Range && dist < minDistance)
                {
                    minDistance = dist;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                currentCoolDown = ReloadTime;
                return new Bullet(X, Y, nearestEnemy, Damage);
            }

            return null;

        }

        public void Upgrade()
        {
            if (Level >= MaxLevel) return; // Maksymalny poziom osiągnięty

            Level++;
            Damage += 10;       // Zwiększamy obrażenia
            Range += 20f;       // Zwiększamy zasięg
            UpgradeCost += 50;  // Kolejne ulepszenie jest droższe


        }

        public abstract void Draw(Graphics g);

        protected void DrawLevelStars(Graphics g)
        {
            // 1. Konfiguracja wyglądu
            int starSize = 12;
            int spacing = 3;
            int count = Level - 1; // Liczba gwiazdek (Lvl 1 = 0, Lvl 2 = 1...)

            if (count <= 0) return;

            // 2. Matematyka centrowania
            // Obliczamy ile miejsca zajmą wszystkie gwiazdki razem
            int totalWidth = (count * starSize) + ((count - 1) * spacing);

            // Wyliczamy start X, tak aby środek grupy gwiazdek pokrywał się ze środkiem wieży (X)
            int startX = X - (totalWidth / 2);

            // Wyliczamy start Y, tak aby były idealnie w pionowym środku wieży
            int startY = Y - (starSize / 2);

            // 3. Rysowanie
            for (int i = 0; i < count; i++)
            {
                int currentX = startX + i * (starSize + spacing);

                // Złote koło
                g.FillEllipse(Brushes.Gold, currentX, startY, starSize, starSize);
                // Grubsza czarna obwódka dla lepszej widoczności na kolorowym tle wieży
                g.DrawEllipse(new Pen(Color.Black, 2), currentX, startY, starSize, starSize);
            }
        }
    }
}
