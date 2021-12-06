using StereoKit;

namespace BoxelModeler.App
{
    /// <summary>
    /// グリッドを描画するクラス
    /// </summary>
    class Grid
    {
        private Color gridColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        /// <summary>
        /// グリッドを描画する
        /// </summary>
        public void DrawGrid(int gridNum)
        {
            {
                var gridScale = 5 * U.cm;

                // グリッドの原点
                Lines.AddAxis(new Pose(0, 0, 0, Quat.Identity), 5 * U.cm);
                //Text.Add("グリッド原点", Matrix.R(0, 180, 0));

                for (int z = 0; z <= gridNum; z++)
                {
                    for (int y = 0; y <= gridNum; y++)
                    {
                        for (int x = 0; x <= gridNum; x++)
                        {
                            Vec3 pos1 = new Vec3(0, y, z) * gridScale;
                            Vec3 pos2 = new Vec3(gridNum, y, z) * gridScale;
                            var p1 = new LinePoint { pt = pos1, color = gridColor, thickness = U.mm };
                            var p2 = new LinePoint { pt = pos2, color = gridColor, thickness = U.mm };
                            Lines.Add(new LinePoint[] { p1, p2 });

                            pos1 = new Vec3(x, 0, z) * gridScale;
                            pos2 = new Vec3(x, gridNum, z) * gridScale;
                            p1 = new LinePoint { pt = pos1, color = gridColor, thickness = U.mm };
                            p2 = new LinePoint { pt = pos2, color = gridColor, thickness = U.mm };
                            Lines.Add(new LinePoint[] { p1, p2 });

                            pos1 = new Vec3(x, y, 0) * gridScale;
                            pos2 = new Vec3(x, y, gridNum) * gridScale;
                            p1 = new LinePoint { pt = pos1, color = gridColor, thickness = U.mm };
                            p2 = new LinePoint { pt = pos2, color = gridColor, thickness = U.mm };
                            Lines.Add(new LinePoint[] { p1, p2 });
                        }
                    }
                }
            }
        }
    }
}
