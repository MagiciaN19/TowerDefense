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
        private int enemySpawnTimer = 0;
        //--------------------------------------------------------------
        public GameEngine()
        {
            map = new GameMap();
        }
        //--------------------------------------------------------------
        public void Update()
        {
            enemySpawnTimer++;
            if (enemySpawnTimer >= 60)
            {
                enemies.Add(new NormalEnemy(map.GetWaypoints()));
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
                bullets[i].Move();
                if (bullets[i].HasHit)
                {
                    bullets.RemoveAt(i);
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Move();

                if (enemies[i].Health <= 0)
                {
                    enemies.RemoveAt(i);
                }
            }
        }
        //--------------------------------------------------------------
        public void Draw(Graphics g)
        {
            map.Draw(g);
            foreach (var enemy in enemies) enemy.Draw(g);
            foreach (var tower in towers) tower.Draw(g);
            foreach (var bullet in bullets) bullet.Draw(g);
        }
        //--------------------------------------------------------------
        public int GetMapWidth() => map.GetWidth();
        public int GetMapHeight() => map.GetHeight();
        //--------------------------------------------------------------
        public void TryPlaceTower(int mouseX, int mouseY)
        {
            if (map.IsGrass(mouseX, mouseY))
            {
                int cellSize = map.CellSize;
                int gridX = (mouseX / cellSize) * cellSize + cellSize / 2;
                int gridY = (mouseY / cellSize) * cellSize + cellSize / 2;
                towers.Add(new SniperTower(gridX, gridY));
            }
        }
    }
}
