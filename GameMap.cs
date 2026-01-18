using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class GameMap
    {
        public int CellSize { get; private set; } = 50;
        private int[,] levelLayout = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        public List<Point> GetWaypoints()
        {
            List<Point> points = new List<Point>();

            int offset = CellSize / 2;
            // Start
            points.Add(new Point(0 + offset, 1 * CellSize + offset));

            // 1. W prawo (kolumna 4, wiersz 1)
            points.Add(new Point(4 * CellSize + offset, 1 * CellSize + offset));

            // 3. W dół (kolumna 4, wiersz 3)
            points.Add(new Point(4 * CellSize + offset, 3 * CellSize + offset));

            // 4. W prawo (kolumna 8, wiersz 3)
            points.Add(new Point(8 * CellSize + offset, 3 * CellSize + offset));

            // 5. W dół (kolumna 8, wiersz 5)
            points.Add(new Point(8 * CellSize + offset, 5 * CellSize + offset));

            // 6. W lewo (kolumna 1, wiersz 5)
            points.Add(new Point(1 * CellSize + offset, 5 * CellSize + offset));

            // 7. W dół (kolumna 1, koniec mapy)
            points.Add(new Point(1 * CellSize + offset, 7 * CellSize + offset));

            // 8. W dół (kolumna 1, poza mapę)
            points.Add(new Point(1 * CellSize + offset, 8 * CellSize + offset));

            return points;
        }

        public bool IsGrass(int x, int y)
        {
            int col = x / CellSize;
            int row = y / CellSize;
            if (row < 0 || row >= levelLayout.GetLength(0) || col < 0 || col >= levelLayout.GetLength(1))
                return false;
            return levelLayout[row, col] == 0;
        }

        public void Draw(Graphics g)
        {
            int rows = levelLayout.GetLength(0);
            int cols = levelLayout.GetLength(1);

            Brush brush;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x<cols; x++)
                {
                    int posY = y* CellSize;
                    int posX = x* CellSize;

                    if (levelLayout[y,x] == 1)
                    {
                        brush = Brushes.SandyBrown; 
                    }
                    else
                    {
                        brush = Brushes.ForestGreen;
                    }
                    g.FillRectangle(brush, posX, posY, CellSize, CellSize);

                    g.DrawRectangle(Pens.Black, posX, posY, CellSize, CellSize);
                }
            }
        }
        public int GetWidth() => levelLayout.GetLength(1) * CellSize;
        public int GetHeight() => levelLayout.GetLength(0) * CellSize;
    }
}
