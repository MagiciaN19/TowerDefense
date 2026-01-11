using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense
{
    public class GameEngine
    {
        private GameMap map;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Tower> towers = new List<Tower>();
        private List<Bullet> bullets = new List<Bullet>();
        private List<Explosion> explosions = new List<Explosion>();
        private Random random = new Random();
        private int enemySpawnTimer = 0;
        public int Gold { get; private set; } = 200;
        public int Lives { get; private set; } = 5;
        private Font uiFont = new Font("Arial", 16, FontStyle.Bold);
        public int SelectedTowerType { get; set; } = 1; // 0 = Snajper, 1 = Karabin maszynowy
        private int sidebarWidth = 220;
        public int GetScreenWidth() => map.GetWidth() + sidebarWidth;
        private bool isGameOver = false;
        //--------------------------------------------------------------
        public GameEngine()
        {
            map = new GameMap();
        }
        //--------------------------------------------------------------
        public void Update()
        {
            if (isGameOver) return;

            enemySpawnTimer++;
            if (enemySpawnTimer >= 60)
            {
                int dice = random.Next(3);

                var path = map.GetWaypoints();

                if (dice == 0)
                {
                    enemies.Add(new NormalEnemy(path));
                }
                else if (dice == 1)
                {
                    enemies.Add(new FastEnemy(path));
                }
                else
                {
                    enemies.Add(new TankEnemy(path));
                }

                enemySpawnTimer = 0;
            }

            foreach (var tower in towers)
            {
                Bullet newBullet = tower.Update(enemies);
                if (newBullet != null)
                {
                    bullets.Add(newBullet);
                }
            }

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Move(enemies);

                if (bullets[i].HasHit)
                {
                    if (bullets[i].ExplosionRadius > 0)
                    {
                        explosions.Add(new Explosion(bullets[i].X, bullets[i].Y, bullets[i].ExplosionRadius));
                    }

                    bullets.RemoveAt(i);
                }
            }

            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(); // <-- To zmniejsza alpha o 10

                if (explosions[i].IsFinished)
                {
                    explosions.RemoveAt(i);
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Move();

                if (enemies[i].HasReachedEnd)
                {
                    Lives--;
                    enemies.RemoveAt(i);

                    if (Lives <= 0)
                    {
                        isGameOver = true;
                    }

                    continue;
                }

                if (enemies[i].Health <= 0)
                {
                    Gold += enemies[i].Reward;
                    enemies.RemoveAt(i);
                }
            }
        }
        //--------------------------------------------------------------
        public void Draw(Graphics g, Point mousePosition)
        {
            map.Draw(g);
            foreach (var enemy in enemies) enemy.Draw(g);
            foreach (var tower in towers) tower.Draw(g);
            foreach (var bullet in bullets) bullet.Draw(g);
            foreach (var explosion in explosions) explosion.Draw(g);

            // 2. Rysowanie Panelu Bocznego (po prawej stronie)
            // Zaczynamy rysować od krawędzi mapy
            int uiX = map.GetWidth();
            int uiY = 0;
            int height = map.GetHeight(); // Wysokość panelu taka sama jak mapy

            // Tło panelu (Ciemnoszare)
            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), uiX, uiY, sidebarWidth, height);

            // Linia oddzielająca mapę od panelu
            g.DrawLine(Pens.Black, uiX, 0, uiX, height);

            // --- TEKSTY I STATYSTYKI ---
            Font fontHeader = new Font("Arial", 16, FontStyle.Bold);
            Font fontText = new Font("Arial", 11);
            int margin = 10;
            int currentY = 20; // Zmienna pomocnicza do układania elementów w pionie

            // Nagłówek STATYSTYKI
            g.DrawString("STATYSTYKI", fontText, Brushes.Gray, uiX + margin, currentY);
            currentY += 25;

            // Złoto i Życie
            g.DrawString($"Złoto: {Gold}", fontHeader, Brushes.Gold, uiX + margin, currentY);
            currentY += 30;
            g.DrawString($"Życie: {Lives}", fontHeader, Brushes.Crimson, uiX + margin, currentY);
            currentY += 40; // Większy odstęp

            // Linia pozioma
            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 20;

            // Nagłówek SKLEP
            g.DrawString("SKLEP (Klawisze 1-2)", fontText, Brushes.Gray, uiX + margin, currentY);
            currentY += 25;

            // Rysowanie zasięgu wybranej wieży pod myszką
            if (mousePosition.X < map.GetWidth() && mousePosition.Y < map.GetHeight())
            {
                // 1. Ustal jaki zasięg ma wybrana wieża
                float range = 0;
                if (SelectedTowerType == 1) range = 200f;       // Sniper
                else if (SelectedTowerType == 2) range = 120f;  // MachineGun
                else if (SelectedTowerType == 3) range = 150f;  // Rocket

                // 2. Przyciągnij do siatki (żeby kółko było tam, gdzie stanie wieża)
                int cellSize = map.CellSize;
                int gridX = (mousePosition.X / cellSize) * cellSize + cellSize / 2;
                int gridY = (mousePosition.Y / cellSize) * cellSize + cellSize / 2;

                // 3. Narysuj półprzezroczyste kółko zasięgu
                using (Brush rangeBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                using (Pen rangePen = new Pen(Color.White, 1))
                {
                    // Wzór na rysowanie okręgu znając środek i promień: (x-r, y-r, 2r, 2r)
                    g.FillEllipse(rangeBrush, gridX - range, gridY - range, range * 2, range * 2);
                    g.DrawEllipse(rangePen, gridX - range, gridY - range, range * 2, range * 2);
                }
            }

            // --- OPCJA 1: SNIPER ---
            // Sprawdzamy czy wybrany, żeby zmienić kolor
            Brush c1 = (SelectedTowerType == 1) ? Brushes.Lime : Brushes.White;
            g.DrawString($"[1] Sniper", new Font("Arial", 12, FontStyle.Bold), c1, uiX + margin, currentY);
            currentY += 20;
            g.DrawString($"Cena: {SniperTower.Cost}", fontText, Brushes.LightGray, uiX + margin, currentY);
            currentY += 20;
            g.DrawString("Zasięg: Duży, Wolny", new Font("Arial", 9), Brushes.Gray, uiX + margin, currentY);
            currentY += 30; // Odstęp

            // --- OPCJA 2: CKM ---
            Brush c2 = (SelectedTowerType == 2) ? Brushes.Lime : Brushes.White;
            g.DrawString($"[2] CKM", new Font("Arial", 12, FontStyle.Bold), c2, uiX + margin, currentY);
            currentY += 20;
            g.DrawString($"Cena: {MachineGunTower.Cost}", fontText, Brushes.LightGray, uiX + margin, currentY);
            currentY += 20;
            g.DrawString("Zasięg: Mały, Szybki", new Font("Arial", 9), Brushes.Gray, uiX + margin, currentY);
            currentY += 30; // Odstęp

            // --- OPCJA 3: ROCKET ---
            Brush c3 = (SelectedTowerType == 3) ? Brushes.Lime : Brushes.White;
            g.DrawString($"[3] Rocket", new Font("Arial", 12, FontStyle.Bold), c3, uiX + margin, currentY);
            currentY += 20;
            g.DrawString($"Cena: {RocketTower.Cost}", fontText, Brushes.LightGray, uiX + margin, currentY);
            currentY += 20;
            g.DrawString("Obszarowy wybuch!", new Font("Arial", 9), Brushes.Gray, uiX + margin, currentY);

            // --- GAME OVER ---
            if (isGameOver)
            {
                // Półprzezroczyste tło na całą mapę
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 0, 0, map.GetWidth(), map.GetHeight());

                // Napisy
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                Font fontBig = new Font("Arial", 48, FontStyle.Bold);
                Font fontSmall = new Font("Arial", 24);

                int centerX = map.GetWidth() / 2;
                int centerY = map.GetHeight() / 2;

                g.DrawString("GAME OVER", fontBig, Brushes.Red, centerX, centerY - 50, sf);
                g.DrawString("Wciśnij 'R' aby zagrać ponownie", fontSmall, Brushes.White, centerX, centerY + 20, sf);
            }
        }
        //--------------------------------------------------------------
        public int GetMapWidth() => map.GetWidth();
        public int GetMapHeight() => map.GetHeight();
        //--------------------------------------------------------------
        public void TryPlaceTower(int mouseX, int mouseY)
        {
            if (!map.IsGrass(mouseX, mouseY)) return;

            int cellSize = map.CellSize;
            int gridX = (mouseX / cellSize) * cellSize + cellSize / 2;
            int gridY = (mouseY / cellSize) * cellSize + cellSize / 2;

            // SPRAWDZAMY WYBÓR I KOSZT
            if (SelectedTowerType == 1) // Snajper
            {
                if (Gold >= SniperTower.Cost)
                {
                    towers.Add(new SniperTower(gridX, gridY));
                    Gold -= SniperTower.Cost;
                }
            }
            else if (SelectedTowerType == 2) // Machine Gun
            {
                if (Gold >= MachineGunTower.Cost)
                {
                    towers.Add(new MachineGunTower(gridX, gridY));
                    Gold -= MachineGunTower.Cost;
                }
            }
            else if (SelectedTowerType == 3) // Rocket Tower
            {
                if (Gold >= RocketTower.Cost)
                {
                    towers.Add(new RocketTower(gridX, gridY));
                    Gold -= RocketTower.Cost;
                }
            }
        }
        //--------------------------------------------------------------
        public void ResetGame()
        {
            enemies.Clear();
            towers.Clear();
            bullets.Clear();
            Gold = 100;
            Lives = 5;
            isGameOver = false;
            enemySpawnTimer = 0;
            explosions.Clear();
        }
    }
}
