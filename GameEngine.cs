using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class GameEngine
    {
        // --- 1. REFERENCJA DO RENDERERA ---
        private GameRenderer renderer;

        // --- 2. MAPA I OBIEKTY GRY ---

        private GameMap map;
        public GameMap Map => map;

        private List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> Enemies => enemies;

        private List<Tower> towers = new List<Tower>();
        public List<Tower> Towers => towers;

        private List<Bullet> bullets = new List<Bullet>();
        public List<Bullet> Bullets => bullets;

        private List<Explosion> explosions = new List<Explosion>();
        public List<Explosion> Explosions => explosions;

        // --- 3. STATYSTYKI GRACZA ---
        public int Gold { get; set; } = 300;
        public int Lives { get; set; } = 10;

        // --- 4. ZMIENNE UI I KONFIGURACJI ---
        public int SelectedTowerType { get; set; } = 1;

        private Tower selectedTower = null;
        public Tower SelectedTower => selectedTower;

        private int sidebarWidth = 220;
        public int SidebarWidth => sidebarWidth;

        // Konfiguracja poziomu trudności
        public int SelectedDifficulty { get; set; } = 1;
        public int SelectedWaveCount { get; set; } = 5;

        // --- 5. STANY GRY (MENU, PAUZA, KONIEC) ---

        private bool isMenu = true;
        public bool IsMenu { get { return isMenu; } set { isMenu = value; } }

        private bool isPaused = false;
        public bool IsPaused => isPaused;

        private bool isGameOver = false;
        public bool IsGameOver => isGameOver;

        private bool isVictory = false;
        public bool IsVictory => isVictory;

        // --- 6. SYSTEM FAL ---
        private List<Wave> waves = new List<Wave>();
        public List<Wave> Waves => waves;

        private int currentWaveIndex = 0;
        public int CurrentWaveIndex => currentWaveIndex;

        private int timeToNextWave = 150;
        public int TimeToNextWave => timeToNextWave;

        private bool isWaveActive = false;
        public bool IsWaveActive => isWaveActive;

        private int spawnTimer = 0;

        // --- 7. MENEDŻER DŹWIĘKÓW ---
        private SoundManager soundManager;

        // Pomocnicze metody wymiarów
        public int GetScreenWidth() => map.GetWidth() + sidebarWidth;
        public int GetMapWidth() => map.GetWidth();
        public int GetMapHeight() => map.GetHeight();

        // ---------------------------------------------------------
        // KONIEC SEKCJI ZMIENNYCH
        // ---------------------------------------------------------
        public GameEngine()
        {
            map = new GameMap();
            renderer = new GameRenderer(this);
            soundManager = new SoundManager();
        }
        //--------------------------------------------------------------
        private void InitializeWaves()
        {
            waves.Clear();

            float countMultiplier = 1.0f; 
            int speedInterval = 40;       

            
            if (SelectedDifficulty == 0) 
            {
                Gold = 300;     
                Lives = 10;     
                countMultiplier = 0.8f; 
                speedInterval = 60;     
            }
            else if (SelectedDifficulty == 1) 
            {
                Gold = 200;
                Lives = 5;
                countMultiplier = 1.0f; 
                speedInterval = 40;
            }
            else 
            {
                Gold = 150;
                Lives = 1;      
                countMultiplier = 1.5f; 
                speedInterval = 25;     
            }

            var path = map.GetWaypoints();

            // --- GENEROWANIE FAL W PĘTLI ---
            for (int i = 1; i <= SelectedWaveCount; i++)
            {
                Wave newWave = new Wave(speedInterval);

                int baseCount = 4 + i;

                int finalCount = (int)(baseCount * countMultiplier);

                // -- LOGIKA SKŁADU FALI --
                for (int j = 0; j < finalCount; j++)
                {
                    if (i % 3 == 0 && j < i) 
                    {
                        newWave.AddEnemy(new TankEnemy(path));
                    }
                    else if (i > 1 && j % 2 == 0) 
                    {
                        newWave.AddEnemy(new FastEnemy(path));
                    }
                    else
                    {
                        newWave.AddEnemy(new NormalEnemy(path));
                    }
                }

                waves.Add(newWave);
            }
        }
        //--------------------------------------------------------------
        public void Update()
        {
            if (isMenu) return;

            if (isPaused) return;

            if (isGameOver || isVictory) return;

            if (currentWaveIndex < waves.Count)
            {
                Wave currentWave = waves[currentWaveIndex];

                // ETAP 1: Oczekiwanie na falę (Przerwa między falami)
                if (!isWaveActive)
                {
                    timeToNextWave--;
                    if (timeToNextWave <= 0)
                    {
                        isWaveActive = true;
                        spawnTimer = 0;
                    }
                }
                // ETAP 2: Wypuszczanie wrogów (Spawnowanie)
                else
                {
                    spawnTimer++;
                    if (spawnTimer >= currentWave.SpawnInterval)
                    {
                        spawnTimer = 0; 

                        if (currentWave.Enemies.Count > 0)
                        {
                            Enemy newEnemy = currentWave.Enemies.Dequeue();
                            enemies.Add(newEnemy);
                        }
                        else { }
                    }
                }
            }

            // ETAP 3: Sprawdzenie końca fali
            if (isWaveActive &&
                waves[currentWaveIndex].Enemies.Count == 0 &&
                enemies.Count == 0)
            {
                isWaveActive = false;    
                currentWaveIndex++;      
                timeToNextWave = 150;    

                if (currentWaveIndex >= waves.Count)
                {
                    isVictory = true;
                    soundManager.Play(GameSound.Win);
                }
            }
            // -----------------------------
            foreach (var tower in towers)
            {
                Bullet newBullet = tower.Update(enemies);
                if (newBullet != null)
                {
                    bullets.Add(newBullet);
                    if (tower is SniperTower) soundManager.Play(GameSound.Sniper);
                    else if (tower is MachineGunTower) soundManager.Play(GameSound.Shoot);
                    else if (tower is RocketTower) soundManager.Play(GameSound.Shoot);
                    else if (tower is SlowTower) soundManager.Play(GameSound.Freeze);
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
                        soundManager.Play(GameSound.Explosion);
                    }

                    bullets.RemoveAt(i);
                }
            }

            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update();

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
                        soundManager.Play(GameSound.Lose);
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
            renderer.Draw(g, mousePosition);
        }
        // --------------------------------------------------------------
        public void HandleClick(int mouseX, int mouseY)
        {
            // --- CZĘŚĆ 1: OBSŁUGA MENU ---
            if (isMenu)
            {
                int cx = GetScreenWidth() / 2;
                int cy = GetMapHeight() / 2;

                // 1. Kliknięcie w TRUDNOŚĆ
                if (mouseY >= cy - 40 && mouseY <= cy)
                {
                    if (Math.Abs(mouseX - (cx - 120)) < 50) SelectedDifficulty = 0;
                    if (Math.Abs(mouseX - cx) < 50) SelectedDifficulty = 1;
                    if (Math.Abs(mouseX - (cx + 120)) < 50) SelectedDifficulty = 2;
                }

                // 2. Kliknięcie w FALE
                if (mouseY >= cy + 50 && mouseY <= cy + 90)
                {
                    if (Math.Abs(mouseX - (cx - 120)) < 50) SelectedWaveCount = 5;
                    if (Math.Abs(mouseX - cx) < 50) SelectedWaveCount = 10;
                    if (Math.Abs(mouseX - (cx + 120)) < 50) SelectedWaveCount = 20;
                }

                // 3. Kliknięcie w START
                if (mouseX >= cx - 100 && mouseX <= cx + 100 &&
                    mouseY >= cy + 110 && mouseY <= cy + 170)
                {
                    InitializeWaves();
                    isMenu = false;
                }
                return;
            }

            // --- CZĘŚĆ 2: GRA WŁAŚCIWA ---

            if (isPaused) return;

            // A. Sprawdzamy, czy kliknięto w istniejącą wieżę (ZAZNACZANIE)
            foreach (var tower in towers)
            {
                if (Math.Abs(tower.X - mouseX) < 25 && Math.Abs(tower.Y - mouseY) < 25)
                {
                    selectedTower = tower;
                    return;
                }
            }

            // B. Jeśli nie kliknęliśmy w wieżę -> Odznaczamy i próbujemy BUDOWAĆ
            selectedTower = null;

            if (!map.IsGrass(mouseX, mouseY)) { soundManager.Play(GameSound.Error); return; }

            int cellSize = map.CellSize;
            int gridX = (mouseX / cellSize) * cellSize + cellSize / 2;
            int gridY = (mouseY / cellSize) * cellSize + cellSize / 2;

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

            if (Gold < cost && newTower != null)
            {
                soundManager.Play(GameSound.Error);
            }
            if (Gold >= cost && newTower != null)
            {
                towers.Add(newTower);
                Gold -= cost;
                soundManager.Play(GameSound.Build);
            }
        }
        //--------------------------------------------------------------
        public void TryUpgradeTower()
        {
            if (selectedTower != null)
            {
                if (selectedTower.Level < Tower.MaxLevel && Gold >= selectedTower.UpgradeCost)
                {
                    Gold -= selectedTower.UpgradeCost;
                    selectedTower.Upgrade();
                    soundManager.Play(GameSound.Build);
                }
                else 
                {
                    soundManager.Play(GameSound.Error);
                }
            }
        }
        //--------------------------------------------------------------
        public void ResetGame()
        {
            enemies.Clear();
            towers.Clear();
            bullets.Clear();
            explosions.Clear();
            waves.Clear();

            isGameOver = false;
            isVictory = false;
            isPaused = false;
            isWaveActive = false;

            currentWaveIndex = 0;
            timeToNextWave = 150;

            isMenu = true;
        }
        //--------------------------------------------------------------
        public void SellSelectedTower()
        {
            if (selectedTower != null)
            {
                int refund = selectedTower.TotalSpent / 2;

                Gold += refund;
                soundManager.Play(GameSound.Sell);

                towers.Remove(selectedTower);
                selectedTower = null;
            }
        }
        //--------------------------------------------------------------
        public void TogglePause()
        {
            if (!isGameOver && !isVictory)
            {
                isPaused = !isPaused;
            }
        }
        // --------------------------------------------------------------
    }
}
