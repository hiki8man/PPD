using PPDFramework;
using SharpDX;
using System;

namespace PPDEditor
{
    class GridComponent : GameComponent
    {
        SquareGrid savedGrid;
        SquareGrid grid;
        PPDFramework.Resource.ResourceManager resourceManager;

        public GridComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, SquareGrid grid) : base(device)
        {
            this.grid = grid;
            this.resourceManager = resourceManager;
            savedGrid = new SquareGrid();
            savedGrid.CopyFrom(grid);
            UpdateResource();
        }

        protected override void UpdateImpl()
        {
            if (savedGrid.Width != grid.Width || savedGrid.Height != grid.Height || savedGrid.OffsetX != grid.OffsetX || savedGrid.OffsetY != grid.OffsetY || savedGrid.GridColor != grid.GridColor)
            {
                savedGrid.CopyFrom(grid);
                UpdateResource();
            }
        }

        private void UpdateResource()
        {
            this.ClearChildren();
            int offsetx = grid.NormalizedOffsetX;
            int offsety = grid.NormalizedOffsetY;
            int maxx = 800 / grid.Width, maxy = 450 / grid.Height;
            //var color = new Color4(grid.GridColor.R / 255f, grid.GridColor.G / 255f, grid.GridColor.B / 255f, 1);
            var color = new Color4(0, 1, 0, 1);
            Action<float, float, float, float> drawRect = (x, y, width, height) =>
            {
                this.AddChild(new RectangleComponent(device, resourceManager, color)
                {
                    Position = new Vector2(x, y),
                    RectangleHeight = height,
                    RectangleWidth = width,
                    Color = color
                });
            };
            //添加多压十字架测试
            for (int i = 0; i < 4; i++)
            {
                float multi_x = ((i * 2) + 1) * 100;
                drawRect(multi_x - 20 + 5, 215 - 1.25f + 5, 30, 2.5f);
                drawRect(multi_x - 1.25f, 195 + 5 + 5, 2.5f, 30);
            }
            //添加红边框线
            color = new Color4(1, 0, 0, 1);
            drawRect(60, 55 - 1 + 5, 680, 2);
            drawRect(60, 375 - 1 + 5, 680, 2);
            drawRect(60 - 1, 55 + 5, 2, 320);
            drawRect(740 - 1, 55 + 5, 2, 320);
            //添加黄边框线
            color = new Color4(1, 1, 0.5f, 1);
            drawRect(80 - 1, 55 + 5, 2, 320);
            drawRect(720 - 1, 55 + 5, 2, 320);

            color = new Color4(grid.GridColor.R / 255f, grid.GridColor.G / 255f, grid.GridColor.B / 255f, 1);
            for (int i = 0; i < maxx; i++)
            {
                if (i % 2 == 0)
                {
                    drawRect(i * grid.Width + offsetx - 1, 0, 2, 450);
                }
                else
                {
                    drawRect(i * grid.Width + offsetx - 0.5f, 0, 1, 450);
                }
            }
            for (int i = 0; i < maxy; i++)
            {
                if (i % 2 == 0)
                {
                    drawRect(0, i * grid.Height + offsety - 1, 800, 2);
                }
                else
                {
                    drawRect(0, i * grid.Height + offsety - 0.5f, 800, 1);
                }
            }
        }
    }
}
