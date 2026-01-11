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
        private int enemySpawnTimer = 0;
        public int Gold { get; private set; } = 200;
        public int Lives { get; private set; } = 5;
        private Font uiFont = new Font("Arial", 16, FontStyle.Bold);
        public int SelectedTowerType { get; set; } = 1; // 0 = Snajper, 1 = Karabin maszynowy
        private int sidebarWidth = 220;
        public int GetScreenWidth() => map.GetWidth() + sidebarWidth;
        private bool isGameOver = false;
        // --- SYSTEM FAL ---
        private List<Wave> waves = new List<Wave>();
        private int currentWaveIndex = 0;       // Która fala teraz leci?
        private int timeToNextWave = 150;       // Czas do startu pierwszej fali (ok. 5 sek)
        private int spawnTimer = 0;             // Licznik do wypuszczania pojedynczych wrogów
        private bool isWaveActive = false;      // Czy wrogowie aktualnie wychodzą?
        private bool isVictory = false;         // Czy gracz wygrał?
        // ------------------
        //--------------------------------------------------------------
        public GameEngine()
        {
            map = new GameMap();
            InitializeWaves();
        }
        //--------------------------------------------------------------
        private void InitializeWaves()
        {
            var path = map.GetWaypoints();

            // --- FALA 1: Rozgrzewka (5 normalnych wrogów, wolno) ---
            Wave wave1 = new Wave(60); // Odstęp 60 klatek (2 sekundy)
            for (int i = 0; i < 5; i++) wave1.AddEnemy(new NormalEnemy(path));
            waves.Add(wave1);

            // --- FALA 2: Szybki atak (8 szybkich wrogów, gęsto) ---
            Wave wave2 = new Wave(30); // Odstęp 30 klatek (1 sekunda)
            for (int i = 0; i < 8; i++) wave2.AddEnemy(new FastEnemy(path));
            waves.Add(wave2);

            // --- FALA 3: Czołgi i wsparcie (3 Czołgi i 5 Szybkich) ---
            Wave wave3 = new Wave(50);
            wave3.AddEnemy(new TankEnemy(path)); // Czołg na start
            for (int i = 0; i < 5; i++) wave3.AddEnemy(new FastEnemy(path));
            wave3.AddEnemy(new TankEnemy(path)); // Czołg na koniec
            wave3.AddEnemy(new TankEnemy(path));
            waves.Add(wave3);
        }
        //--------------------------------------------------------------
        public void Update()
        {
            if (isGameOver) return;

            if (isVictory) return;
            // Sprawdzamy, czy wciąż są fale do rozegrania
            if (currentWaveIndex < waves.Count)
            {
                Wave currentWave = waves[currentWaveIndex];

                // ETAP 1: Oczekiwanie na falę (Przerwa między falami)
                if (!isWaveActive)
                {
                    timeToNextWave--;
                    if (timeToNextWave <= 0)
                    {
                        isWaveActive = true; // Startujemy falę!
                        spawnTimer = 0;      // Reset licznika spawnu
                    }
                }
                // ETAP 2: Wypuszczanie wrogów (Spawnowanie)
                else
                {
                    spawnTimer++;
                    if (spawnTimer >= currentWave.SpawnInterval)
                    {
                        spawnTimer = 0; // Reset

                        // Czy są jeszcze wrogowie w kolejce tej fali?
                        if (currentWave.Enemies.Count > 0)
                        {
                            // Wyjmij wroga z kolejki i dodaj na mapę
                            Enemy newEnemy = currentWave.Enemies.Dequeue();
                            enemies.Add(newEnemy);
                        }
                        else
                        {
                            // Kolejka pusta - fala "wyszła" w całości
                        }
                    }
                }
            }

            // ETAP 3: Sprawdzenie końca fali
            // Jeśli fala jest aktywna, ale kolejka pusta I na mapie nie ma wrogów...
            if (isWaveActive &&
                waves[currentWaveIndex].Enemies.Count == 0 &&
                enemies.Count == 0)
            {
                isWaveActive = false;    // Koniec walki
                currentWaveIndex++;      // Przełącz na następną falę
                timeToNextWave = 150;    // Ustaw czas przerwy (5 sek)

                // Sprawdź czy to była ostatnia fala
                if (currentWaveIndex >= waves.Count)
                {
                    isVictory = true;
                }
            }
            // -----------------------------
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
            // =========================================================
            // 1. RYSOWANIE GRY (WARSTWA DOLNA)
            // =========================================================
            map.Draw(g);
            foreach (var enemy in enemies) enemy.Draw(g);
            foreach (var tower in towers) tower.Draw(g);
            foreach (var bullet in bullets) bullet.Draw(g);
            foreach (var explosion in explosions) explosion.Draw(g);

            // --- WIZUALIZACJA ZASIĘGU ---
            // Rysujemy kółko zasięgu tylko nad mapą
            if (mousePosition.X < map.GetWidth() && mousePosition.Y < map.GetHeight())
            {
                float range = 0;
                if (SelectedTowerType == 1) range = 200f;      // Sniper
                else if (SelectedTowerType == 2) range = 120f; // CKM
                else if (SelectedTowerType == 3) range = 150f; // Rocket
                else if (SelectedTowerType == 4) range = 130f; // Slow

                int cellSize = map.CellSize;
                int gridX = (mousePosition.X / cellSize) * cellSize + cellSize / 2;
                int gridY = (mousePosition.Y / cellSize) * cellSize + cellSize / 2;

                using (Brush rangeBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                using (Pen rangePen = new Pen(Color.White, 1))
                {
                    g.FillEllipse(rangeBrush, gridX - range, gridY - range, range * 2, range * 2);
                    g.DrawEllipse(rangePen, gridX - range, gridY - range, range * 2, range * 2);
                }
            }

            // =========================================================
            // 2. RYSOWANIE PANELU BOCZNEGO (WARSTWA GÓRNA)
            // =========================================================
            int uiX = map.GetWidth();
            int uiY = 0;
            int height = map.GetHeight();

            // Tło panelu
            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), uiX, uiY, sidebarWidth, height);
            g.DrawLine(Pens.Black, uiX, 0, uiX, height);

            // Czcionki
            Font fontHeader = new Font("Arial", 14, FontStyle.Bold);
            Font fontSmall = new Font("Arial", 10);
            Font fontBold = new Font("Arial", 11, FontStyle.Bold);

            int margin = 10;
            int currentY = 15;

            // --- SEKCJA 1: STATYSTYKI ---
            g.DrawString("STATYSTYKI", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 20;
            g.DrawString($"Złoto: {Gold}", fontHeader, Brushes.Gold, uiX + margin, currentY);
            currentY += 25;
            g.DrawString($"Życie: {Lives}", fontHeader, Brushes.Crimson, uiX + margin, currentY);
            currentY += 25;

            // Informacja o Fali (jeśli masz system fal)
            if (!isVictory && !isGameOver)
            {
                // Zakładam, że dodałeś listę 'waves' z poprzedniego kroku.
                // Jeśli nie masz systemu fal, usuń te dwie linijki poniżej:
                // string waveInfo = $"Fala: {currentWaveIndex + 1}";
                // g.DrawString(waveInfo, fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 20;
            }

            currentY += 10;
            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 15;

            // --- SEKCJA 2: SKLEP (SIATKA PRZYCISKÓW) ---
            g.DrawString("WYBIERZ WIEŻĘ:", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 20;

            int btnW = 95;
            int btnH = 60;
            int gap = 5;

            // Rząd 1
            DrawTowerButton(g, 1, "SNIPER", SniperTower.Cost, Brushes.Blue, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 2, "CKM", MachineGunTower.Cost, Brushes.DarkOrange, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + gap;

            // Rząd 2
            DrawTowerButton(g, 3, "ROCKET", RocketTower.Cost, Brushes.Purple, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 4, "SLOW", SlowTower.Cost, Brushes.LightSkyBlue, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + 20;

            // --- SEKCJA 3: SZCZEGÓŁY ---
            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 10;

            string name = "";
            string stats = "";
            string desc = "";

            if (SelectedTowerType == 1)
            {
                name = "SNIPER TOWER [1]";
                stats = "Atak: 50 | Zasięg: Duży";
                desc = "Wolny, ale potężny strzał.\nDobry na silne cele.";
            }
            else if (SelectedTowerType == 2)
            {
                name = "MACHINE GUN [2]";
                stats = "Atak: 15 | Zasięg: Mały";
                desc = "Bardzo szybki ostrzał.\nNiszczy słabych wrogów.";
            }
            else if (SelectedTowerType == 3)
            {
                name = "ROCKET TOWER [3]";
                stats = "Atak: 40 | Wybuch!";
                desc = "Rani wszystkich wrogów\nw miejscu trafienia.";
            }
            else if (SelectedTowerType == 4)
            {
                name = "SLOW TOWER [4]";
                stats = "Atak: 5 | Lód";
                desc = "Spowalnia wrogów o 50%.\nNie zadaje obrażeń.";
            }

            g.DrawString(name, fontBold, Brushes.White, uiX + margin, currentY);
            currentY += 25;
            g.DrawString(stats, fontSmall, Brushes.LightGray, uiX + margin, currentY);
            currentY += 25;
            g.DrawString(desc, new Font("Arial", 9, FontStyle.Italic), Brushes.Gray, uiX + margin, currentY);

            // =========================================================
            // 3. EKRANY KOŃCOWE (NA WIERZCHU)
            // =========================================================

            // A) PRZEGRANA (GAME OVER)
            if (isGameOver)
            {
                // Ciemne tło na mapę
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 0, 0, map.GetWidth(), map.GetHeight());

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                int cx = map.GetWidth() / 2;
                int cy = map.GetHeight() / 2;

                g.DrawString("GAME OVER", new Font("Arial", 48, FontStyle.Bold), Brushes.Red, cx, cy - 50, sf);
                g.DrawString("Wciśnij 'R' aby zagrać ponownie", new Font("Arial", 24), Brushes.White, cx, cy + 20, sf);
            }
            // B) ZWYCIĘSTWO (VICTORY)
            else if (isVictory)
            {
                // Zielone tło na mapę
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 50, 0)), 0, 0, map.GetWidth(), map.GetHeight());

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                int cx = map.GetWidth() / 2;
                int cy = map.GetHeight() / 2;

                g.DrawString("ZWYCIĘSTWO!", new Font("Arial", 48, FontStyle.Bold), Brushes.Lime, cx, cy - 50, sf);
                g.DrawString("Wciśnij 'R' aby zagrać ponownie", new Font("Arial", 24), Brushes.White, cx, cy + 20, sf);
            }
        }

        // POMOCNICZA METODA DO RYSOWANIA PRZYCISKU (Dodaj ją też do klasy GameEngine)
        private void DrawTowerButton(Graphics g, int type, string name, int cost, Brush color, int x, int y, int w, int h)
        {
            // Czy ta wieża jest wybrana?
            bool isSelected = (SelectedTowerType == type);

            // Tło przycisku (jeśli wybrane, to jaśniejsze tło)
            Brush bg = isSelected ? new SolidBrush(Color.FromArgb(80, 80, 80)) : new SolidBrush(Color.FromArgb(40, 40, 40));
            g.FillRectangle(bg, x, y, w, h);

            // Ramka (Zielona jeśli wybrana, czarna jeśli nie)
            Pen border = isSelected ? new Pen(Color.Lime, 2) : Pens.Black;
            g.DrawRectangle(border, x, y, w, h);

            // Mały kolorowy kwadracik symbolizujący wieżę
            g.FillRectangle(color, x + 5, y + 5, 10, 10);

            // Napisy
            Font fontName = new Font("Arial", 9, FontStyle.Bold);
            Font fontCost = new Font("Arial", 9);

            g.DrawString(name, fontName, isSelected ? Brushes.Lime : Brushes.White, x + 20, y + 3);
            g.DrawString($"${cost}", fontCost, (Gold >= cost) ? Brushes.Gold : Brushes.Red, x + 5, y + 25);
            g.DrawString($"[{type}]", new Font("Arial", 8), Brushes.Gray, x + w - 20, y + h - 15);
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
            else if (SelectedTowerType == 4) // Slow Tower
            {
                if (Gold >= SlowTower.Cost)
                {
                    towers.Add(new SlowTower(gridX, gridY));
                    Gold -= SlowTower.Cost;
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

            isVictory = false;
            currentWaveIndex = 0;
            timeToNextWave = 150;
            isWaveActive = false;

            waves.Clear();
            InitializeWaves();
        }
    }
}
