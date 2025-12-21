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
        //--------------------------------------------------------------
        public GameEngine()
        {
            map = new GameMap();
            List<Point> path = map.GetWaypoints();
            enemies.Add(new NormalEnemy(path));
        }
        //--------------------------------------------------------------
        public void Update()
        {
            foreach (var enemy in enemies)
            {
                enemy.Move();
            }
        }
        //--------------------------------------------------------------
        public void Draw(Graphics g)
        {
            map.Draw(g);
            foreach (var enemy in enemies)
            {
                enemy.Draw(g);
            }
        }
        //--------------------------------------------------------------
        public int GetMapWidth() => map.GetWidth();
        public int GetMapHeight() => map.GetHeight();
    }
}
