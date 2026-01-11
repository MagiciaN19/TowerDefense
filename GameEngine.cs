using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class GameEngine
    {
        private GameMap map;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Tower> towers = new List<Tower>();
        private List<Bullet> bullets = new List<Bullet>();
        private List<Explosion> explosions = new List<Explosion>();
        public int Gold { get; private set; } = 100;
        public int Lives { get; private set; } = 5;
        private Font uiFont = new Font("Arial", 16, FontStyle.Bold);
        public int SelectedTowerType { get; set; } = 1; // 0 = Snajper, 1 = Karabin maszynowy
        private int sidebarWidth = 220;
        public int GetScreenWidth() => map.GetWidth() + sidebarWidth;
        private bool isGameOver = false;
        private Tower selectedTower = null;
        // --- SYSTEM FAL ---
        private List<Wave> waves = new List<Wave>();
        private int currentWaveIndex = 0;       // Która fala teraz leci?
        private int timeToNextWave = 150;       // Czas do startu pierwszej fali (ok. 5 sek)
        private int spawnTimer = 0;             // Licznik do wypuszczania pojedynczych wrogów
        private bool isWaveActive = false;      // Czy wrogowie aktualnie wychodzą?
        private bool isVictory = false;         // Czy gracz wygrał?
        // ------------------
        public int GetMapWidth() => map.GetWidth();
        public int GetMapHeight() => map.GetHeight();
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

            // Rysowanie obwódki wokół zaznaczonej wieży
            if (selectedTower != null)
            {
                // Biała ramka
                g.DrawRectangle(new Pen(Color.White, 3), selectedTower.X - 25, selectedTower.Y - 25, 50, 50);
                // Kółko zasięgu
                float r = selectedTower.Range;
                g.DrawEllipse(Pens.White, selectedTower.X - r, selectedTower.Y - r, r * 2, r * 2);
            }

            foreach (var bullet in bullets) bullet.Draw(g);
            foreach (var explosion in explosions) explosion.Draw(g);

            // --- WIZUALIZACJA ZASIĘGU (Tylko gdy budujemy, czyli NIE mamy zaznaczonej wieży) ---
            if (selectedTower == null && mousePosition.X < map.GetWidth() && mousePosition.Y < map.GetHeight())
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
            // 2. RYSOWANIE PANELU BOCZNEGO
            // =========================================================
            int uiX = map.GetWidth();
            int height = map.GetHeight();

            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), uiX, 0, sidebarWidth, height);
            g.DrawLine(Pens.Black, uiX, 0, uiX, height);

            // Czcionki
            Font fontStats = new Font("Arial", 15, FontStyle.Bold); // DUŻA czcionka dla statystyk
            Font fontSmall = new Font("Arial", 9);
            Font fontBold = new Font("Arial", 10, FontStyle.Bold);

            int margin = 10;
            int currentY = 15;

            // --- SEKCJA 1: STATYSTYKI (POWIĘKSZONE) ---
            g.DrawString("STATYSTYKI", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 20;

            // ZŁOTO - Duże i Złote
            g.DrawString($"Złoto: {Gold}", fontStats, Brushes.Gold, uiX + margin, currentY);
            currentY += 25;

            // ŻYCIE - Duże i Czerwone
            g.DrawString($"Życie: {Lives}", fontStats, Brushes.Crimson, uiX + margin, currentY);
            currentY += 30; // Większy odstęp po statystykach

            // FALA
            if (!isVictory && !isGameOver)
            {
                string waveInfo = $"Fala: {currentWaveIndex + 1}";
                if (!isWaveActive && currentWaveIndex < waves.Count)
                {
                    float seconds = timeToNextWave / 30f;
                    waveInfo += $" (Start: {seconds:0.0}s)";
                }
                g.DrawString(waveInfo, fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 15;
            }

            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 10;

            // --- SEKCJA 2: SKLEP (KOMPAKTOWA) ---
            g.DrawString("SKLEP:", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 15;

            int btnW = 95;
            int btnH = 45; // Niższe przyciski, żeby wszystko się zmieściło
            int gap = 5;

            DrawTowerButton(g, 1, "SNIPER", SniperTower.Cost, Brushes.Blue, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 2, "CKM", MachineGunTower.Cost, Brushes.DarkOrange, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + gap;

            DrawTowerButton(g, 3, "ROCKET", RocketTower.Cost, Brushes.Purple, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 4, "SLOW", SlowTower.Cost, Brushes.LightSkyBlue, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + 10;

            // --- SEKCJA 3: SZCZEGÓŁY ---
            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 5;

            if (selectedTower != null)
            {
                // ZAZNACZONA WIEŻA
                g.DrawString($"WIEŻA (Lvl {selectedTower.Level})", fontBold, Brushes.Cyan, uiX + margin, currentY);
                currentY += 20;
                g.DrawString($"Obrażenia: {selectedTower.Damage}", fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 15;
                g.DrawString($"Zasięg: {selectedTower.Range:0}", fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 20;

                // CENA ULEPSZENIA (Wyróżniona)
                string upgradeText = $"[U] ULEPSZ: ${selectedTower.UpgradeCost}";
                Brush costBrush = (Gold >= selectedTower.UpgradeCost) ? Brushes.Lime : Brushes.Red;
                g.DrawString(upgradeText, fontBold, costBrush, uiX + margin, currentY);

                if (Gold < selectedTower.UpgradeCost)
                {
                    currentY += 15;
                    g.DrawString("(Brak złota)", fontSmall, Brushes.Red, uiX + margin, currentY);
                }
            }
            else
            {
                // OPIS ZE SKLEPU
                string name = "", stats = "", desc = "";
                if (SelectedTowerType == 1) { name = "SNIPER"; stats = "Atak: 50 | Zasięg: Duży"; desc = "Wolny, silny strzał."; }
                else if (SelectedTowerType == 2) { name = "CKM"; stats = "Atak: 15 | Zasięg: Mały"; desc = "Szybki ostrzał."; }
                else if (SelectedTowerType == 3) { name = "ROCKET"; stats = "Atak: 40 | Wybuch"; desc = "Obrażenia obszarowe."; }
                else if (SelectedTowerType == 4) { name = "SLOW"; stats = "Atak: 5 | Lód"; desc = "Spowalnia wrogów."; }

                g.DrawString(name, fontBold, Brushes.White, uiX + margin, currentY);
                currentY += 20;
                g.DrawString(stats, fontSmall, Brushes.LightGray, uiX + margin, currentY);
                currentY += 20;
                g.DrawString(desc, new Font("Arial", 8, FontStyle.Italic), Brushes.Gray, uiX + margin, currentY);
            }

            // =========================================================
            // 3. EKRANY KOŃCOWE
            // =========================================================
            if (isGameOver || isVictory)
            {
                Color bg = isVictory ? Color.FromArgb(200, 0, 50, 0) : Color.FromArgb(200, 0, 0, 0);
                string txt = isVictory ? "ZWYCIĘSTWO!" : "GAME OVER";
                Brush color = isVictory ? Brushes.Lime : Brushes.Red;

                g.FillRectangle(new SolidBrush(bg), 0, 0, map.GetWidth(), map.GetHeight());

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                int cx = map.GetWidth() / 2;
                int cy = map.GetHeight() / 2;

                g.DrawString(txt, new Font("Arial", 48, FontStyle.Bold), color, cx, cy - 50, sf);
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
        public void HandleClick(int mouseX, int mouseY)
        {
            // 1. Sprawdź, czy kliknęliśmy w istniejącą wieżę (ZAZNACZANIE)
            foreach (var tower in towers)
            {
                // Prosta kolizja: czy myszka jest blisko środka wieży?
                if (Math.Abs(tower.X - mouseX) < 25 && Math.Abs(tower.Y - mouseY) < 25)
                {
                    selectedTower = tower; // Zaznaczamy tę wieżę
                    return; // Kończymy, nie stawiamy nowej
                }
            }

            // 2. Jeśli nie kliknęliśmy w wieżę, to może chcemy postawić nową? (BUDOWANIE)
            // Odznaczamy poprzednią wieżę, jeśli kliknęliśmy w trawę
            selectedTower = null;

            if (!map.IsGrass(mouseX, mouseY)) return;

            int cellSize = map.CellSize;
            int gridX = (mouseX / cellSize) * cellSize + cellSize / 2;
            int gridY = (mouseY / cellSize) * cellSize + cellSize / 2;

            // Sprawdzamy czy w tym miejscu już coś nie stoi (żeby nie stawiać wieży na wieży)
            foreach (var t in towers)
            {
                if (t.X == gridX && t.Y == gridY) return;
            }

            int cost = 0;
            Tower newTower = null;

            if (SelectedTowerType == 1) { cost = SniperTower.Cost; newTower = new SniperTower(gridX, gridY); }
            else if (SelectedTowerType == 2) { cost = MachineGunTower.Cost; newTower = new MachineGunTower(gridX, gridY); }
            else if (SelectedTowerType == 3) { cost = RocketTower.Cost; newTower = new RocketTower(gridX, gridY); }
            else if (SelectedTowerType == 4) { cost = SlowTower.Cost; newTower = new SlowTower(gridX, gridY); }

            if (Gold >= cost && newTower != null)
            {
                towers.Add(newTower);
                Gold -= cost;
            }
        }
        //--------------------------------------------------------------
        public void TryUpgradeTower()
        {
            if (selectedTower != null)
            {
                if (Gold >= selectedTower.UpgradeCost)
                {
                    Gold -= selectedTower.UpgradeCost;
                    selectedTower.Upgrade();
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
