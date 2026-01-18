using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Wave
    {
        // Kolejka wrogów do zespawnowania w tej fali
        public Queue<Enemy> Enemies { get; private set; } = new Queue<Enemy>();

        public int SpawnInterval { get; private set; }

        public Wave(int spawnInterval)
        {
            SpawnInterval = spawnInterval;
        }

        public void AddEnemy(Enemy enemy)
        {
            Enemies.Enqueue(enemy);
        }
    }
}
