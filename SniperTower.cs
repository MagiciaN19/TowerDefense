using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class SniperTower : Tower
    {
        public SniperTower(int x, int y) : base(x, y)
        {
            Range = 200f;
            Damage = 50;
            ReloadTime = 60;
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X - 20, Y - 20, 40, 40);
        }
    }
}
