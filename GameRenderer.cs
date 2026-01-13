using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace TowerDefense
{
    public class GameRenderer
    {
        private GameEngine engine;

        // Czcionki
        private Font fontHeader = new Font("Arial", 12, FontStyle.Bold);
        private Font fontSmall = new Font("Arial", 9);
        private Font fontBold = new Font("Arial", 10, FontStyle.Bold);


        public GameRenderer(GameEngine engine)
        {
            this.engine = engine;
        }

        public void Draw(Graphics g, Point mousePosition)
        {
            // 1. Rysowanie Świata Gry
            engine.Map.Draw(g);
            foreach (var enemy in engine.Enemies) enemy.Draw(g);
            foreach (var tower in engine.Towers) tower.Draw(g);

            // Zaznaczenie wieży
            if (engine.SelectedTower != null)
            {
                g.DrawRectangle(new Pen(Color.White, 3), engine.SelectedTower.X - 25, engine.SelectedTower.Y - 25, 50, 50);
                float r = engine.SelectedTower.Range;
                g.DrawEllipse(Pens.White, engine.SelectedTower.X - r, engine.SelectedTower.Y - r, r * 2, r * 2);
            }

            foreach (var bullet in engine.Bullets) bullet.Draw(g);
            foreach (var explosion in engine.Explosions) explosion.Draw(g);

            // Wizualizacja zasięgu przy budowaniu
            DrawBuildRange(g, mousePosition);

            // 2. Rysowanie UI
            if (engine.IsMenu)
            {
                DrawMenu(g); // Teraz to zadziała poprawnie
            }
            else
            {
                DrawSidebar(g);
                DrawOverlays(g);
            }
        }

        private void DrawBuildRange(Graphics g, Point mousePosition)
        {
            if (engine.SelectedTower == null && !engine.IsMenu && !engine.IsPaused &&
                mousePosition.X < engine.GetMapWidth() && mousePosition.Y < engine.GetMapHeight())
            {
                float range = 0;
                if (engine.SelectedTowerType == 1) range = 200f;
                else if (engine.SelectedTowerType == 2) range = 120f;
                else if (engine.SelectedTowerType == 3) range = 150f;
                else if (engine.SelectedTowerType == 4) range = 130f;

                int cellSize = engine.Map.CellSize;
                int gridX = (mousePosition.X / cellSize) * cellSize + cellSize / 2;
                int gridY = (mousePosition.Y / cellSize) * cellSize + cellSize / 2;

                using (Brush rangeBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                using (Pen rangePen = new Pen(Color.White, 1))
                {
                    g.FillEllipse(rangeBrush, gridX - range, gridY - range, range * 2, range * 2);
                    g.DrawEllipse(rangePen, gridX - range, gridY - range, range * 2, range * 2);
                }
            }
        }

        private void DrawSidebar(Graphics g)
        {
            int uiX = engine.GetMapWidth();
            int height = engine.GetMapHeight();
            int sidebarWidth = engine.SidebarWidth; 

            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), uiX, 0, sidebarWidth, height);
            g.DrawLine(Pens.Black, uiX, 0, uiX, height);

            int margin = 10;
            int currentY = 15;

            // --- STATYSTYKI ---
            g.DrawString("STATYSTYKI", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 20;
            g.DrawString($"Złoto: {engine.Gold}", new Font("Arial", 15, FontStyle.Bold), Brushes.Gold, uiX + margin, currentY);
            currentY += 25;
            g.DrawString($"Życie: {engine.Lives}", new Font("Arial", 15, FontStyle.Bold), Brushes.Crimson, uiX + margin, currentY);
            currentY += 30;

            // FALA
            if (!engine.IsVictory && !engine.IsGameOver)
            {
                string waveInfo = $"Fala: {engine.CurrentWaveIndex + 1}";
                if (!engine.IsWaveActive && engine.CurrentWaveIndex < engine.Waves.Count)
                {
                    float seconds = engine.TimeToNextWave / 30f;
                    waveInfo += $" (Start: {seconds:0.0}s)";
                }
                g.DrawString(waveInfo, fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 15;
            }

            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 10;

            // --- SKLEP ---
            g.DrawString("SKLEP:", fontSmall, Brushes.Gray, uiX + margin, currentY);
            currentY += 15;

            int btnW = 95;
            int btnH = 45;
            int gap = 5;

            DrawTowerButton(g, 1, "SNIPER", SniperTower.Cost, Brushes.Blue, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 2, "CKM", MachineGunTower.Cost, Brushes.DarkOrange, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + gap;
            DrawTowerButton(g, 3, "ROCKET", RocketTower.Cost, Brushes.Purple, uiX + margin, currentY, btnW, btnH);
            DrawTowerButton(g, 4, "SLOW", SlowTower.Cost, Brushes.LightSkyBlue, uiX + margin + btnW + gap, currentY, btnW, btnH);
            currentY += btnH + 10;

            // --- SZCZEGÓŁY ---
            g.DrawLine(Pens.Gray, uiX + margin, currentY, uiX + sidebarWidth - margin, currentY);
            currentY += 5;

            if (engine.SelectedTower != null)
            {
                // ZAZNACZONA WIEŻA
                g.DrawString($"WIEŻA (Lvl {engine.SelectedTower.Level})", fontBold, Brushes.Cyan, uiX + margin, currentY);
                currentY += 20;
                g.DrawString($"Obrażenia: {engine.SelectedTower.Damage}", fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 15;
                g.DrawString($"Zasięg: {engine.SelectedTower.Range:0}", fontSmall, Brushes.White, uiX + margin, currentY);
                currentY += 20;

                // CENA ULEPSZENIA (Wyróżniona)
                if (engine.SelectedTower.Level >= Tower.MaxLevel)
                {
                    g.DrawString("MAKSYMALNY POZIOM", fontBold, Brushes.Gray, uiX + margin, currentY);
                }
                else
                {
                    string upgradeText = $"[U] ULEPSZ: ${engine.SelectedTower.UpgradeCost}";
                    Brush costBrush = (engine.Gold >= engine.SelectedTower.UpgradeCost) ? Brushes.Lime : Brushes.Red;
                    g.DrawString(upgradeText, fontBold, costBrush, uiX + margin, currentY);

                    if (engine.Gold < engine.SelectedTower.UpgradeCost)
                    {
                        currentY += 15;
                        g.DrawString("(Brak złota)", fontSmall, Brushes.Red, uiX + margin, currentY);
                    }
                }

                currentY += 25;
                g.DrawString($"[S] SPRZEDAJ: +${engine.SelectedTower.UpgradeCost / 2}", fontBold, Brushes.Orange, uiX + margin, currentY);
            }
            else
            {
                // OPIS ZE SKLEPU
                string name = "", stats = "", desc = "";
                if (engine.SelectedTowerType == 1) { name = "SNIPER"; stats = "Atak: 50 | Zasięg: Duży"; desc = "Wolny, silny strzał."; }
                else if (engine.SelectedTowerType == 2) { name = "CKM"; stats = "Atak: 15 | Zasięg: Mały"; desc = "Szybki ostrzał."; }
                else if (engine.SelectedTowerType == 3) { name = "ROCKET"; stats = "Atak: 40 | Wybuch"; desc = "Obrażenia obszarowe."; }
                else if (engine.SelectedTowerType == 4) { name = "SLOW"; stats = "Atak: 5 | Lód"; desc = "Spowalnia wrogów."; }

                g.DrawString(name, fontBold, Brushes.White, uiX + margin, currentY);
                currentY += 20;
                g.DrawString(stats, fontSmall, Brushes.LightGray, uiX + margin, currentY);
                currentY += 20;
                g.DrawString(desc, new Font("Arial", 8, FontStyle.Italic), Brushes.Gray, uiX + margin, currentY);
            }
        }

        private void DrawTowerButton(Graphics g, int type, string name, int cost, Brush color, int x, int y, int w, int h)
        {
            bool isSelected = (engine.SelectedTowerType == type);
            Brush bg = isSelected ? new SolidBrush(Color.FromArgb(80, 80, 80)) : new SolidBrush(Color.FromArgb(40, 40, 40));
            g.FillRectangle(bg, x, y, w, h);

            Pen border = isSelected ? new Pen(Color.Lime, 2) : Pens.Black;
            g.DrawRectangle(border, x, y, w, h);
            g.FillRectangle(color, x + 5, y + 5, 10, 10);

            Font fontName = new Font("Arial", 9, FontStyle.Bold);
            Font fontCost = new Font("Arial", 9);

            g.DrawString(name, fontName, isSelected ? Brushes.Lime : Brushes.White, x + 20, y + 3);
            g.DrawString($"${cost}", fontCost, (engine.Gold >= cost) ? Brushes.Gold : Brushes.Red, x + 5, y + 25);
            g.DrawString($"[{type}]", new Font("Arial", 8), Brushes.Gray, x + w - 20, y + h - 15);
        }

        // --- SEKCJA MENU ---

        // Metoda główna rysująca całe menu
        private void DrawMenu(Graphics g)
        {
            // Tło
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 30, 30, 30)), 0, 0, engine.GetScreenWidth(), engine.GetMapHeight());

            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            int cx = engine.GetScreenWidth() / 2;
            int cy = engine.GetMapHeight() / 2;

            // Tytuł
            g.DrawString("TOWER DEFENSE", new Font("Arial", 48, FontStyle.Bold), Brushes.Gold, cx, cy - 140, sf);

            // WYBÓR TRUDNOŚCI
            g.DrawString("POZIOM TRUDNOŚCI:", new Font("Arial", 12), Brushes.LightGray, cx, cy - 70, sf);
            DrawMenuButton(g, "ŁATWY", cx - 120, cy - 40, engine.SelectedDifficulty == 0);
            DrawMenuButton(g, "ŚREDNI", cx, cy - 40, engine.SelectedDifficulty == 1);
            DrawMenuButton(g, "TRUDNY", cx + 120, cy - 40, engine.SelectedDifficulty == 2);

            // WYBÓR LICZBY FAL
            g.DrawString("LICZBA FAL:", new Font("Arial", 12), Brushes.LightGray, cx, cy + 20, sf);
            DrawMenuButton(g, "5 FAL", cx - 120, cy + 50, engine.SelectedWaveCount == 5);
            DrawMenuButton(g, "10 FAL", cx, cy + 50, engine.SelectedWaveCount == 10);
            DrawMenuButton(g, "20 FAL", cx + 120, cy + 50, engine.SelectedWaveCount == 20);

            // PRZYCISK START
            g.FillRectangle(Brushes.Crimson, cx - 100, cy + 110, 200, 60);
            g.DrawRectangle(Pens.White, cx - 100, cy + 110, 200, 60);
            g.DrawString("START GRY", new Font("Arial", 20, FontStyle.Bold), Brushes.White, cx, cy + 140, sf);

            // Instrukcja
            g.DrawString("[1-4] Wieże | [U] Ulepsz | [S] Sprzedaj | [P] Pauza",
                new Font("Arial", 10), Brushes.Gray, cx, cy + 190, sf);
        }

        // Metoda pomocnicza do rysowania przycisków w menu
        private void DrawMenuButton(Graphics g, string text, int x, int y, bool isSelected)
        {
            Brush bg = isSelected ? Brushes.LimeGreen : Brushes.DimGray;
            g.FillRectangle(bg, x - 50, y, 100, 40);
            g.DrawRectangle(Pens.Black, x - 50, y, 100, 40);

            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, new Font("Arial", 10, FontStyle.Bold), Brushes.White, x, y + 20, sf);
        }

        // --- KONIEC SEKCJI MENU ---

        private void DrawOverlays(Graphics g)
        {
            if (engine.IsPaused)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, engine.Map.GetWidth(), engine.Map.GetHeight());

                int cx = engine.Map.GetWidth() / 2;
                int cy = engine.Map.GetHeight() / 2;
                int barWidth = 25;
                int barHeight = 80;
                int pauseGap = 15;

                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, cx - pauseGap - barWidth, cy - barHeight / 2, barWidth, barHeight);
                    g.FillRectangle(brush, cx + pauseGap, cy - barHeight / 2, barWidth, barHeight);
                }

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("PAUZA", new Font("Arial", 20, FontStyle.Bold), Brushes.White, cx, cy + 60, sf);
            }
            else if (engine.IsGameOver || engine.IsVictory)
            {
                Color bg = engine.IsVictory ? Color.FromArgb(200, 0, 50, 0) : Color.FromArgb(200, 0, 0, 0);
                string txt = engine.IsVictory ? "ZWYCIĘSTWO!" : "GAME OVER";
                Brush color = engine.IsVictory ? Brushes.Lime : Brushes.Red;

                g.FillRectangle(new SolidBrush(bg), 0, 0, engine.Map.GetWidth(), engine.Map.GetHeight());

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                int cx = engine.Map.GetWidth() / 2;
                int cy = engine.Map.GetHeight() / 2;

                g.DrawString(txt, new Font("Arial", 48, FontStyle.Bold), color, cx, cy - 50, sf);
                g.DrawString("Wciśnij 'R' aby zagrać ponownie", new Font("Arial", 24), Brushes.White, cx, cy + 20, sf);
            }
        }
    }
}