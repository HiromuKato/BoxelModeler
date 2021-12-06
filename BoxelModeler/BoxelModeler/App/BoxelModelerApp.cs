using System.Collections.Generic;
using StereoKit;

namespace BoxelModeler.App
{
    class BoxelModelerApp : IApp
    {
        // 全体
        Pose _pose = new Pose(0, 0, -0.6f, Quat.Identity);

        private Menu menu;
        private Grid grid;

        // 描画内容を保持したリスト
        private List<CubeData> cubeData = new List<CubeData>();
        private static int gridMaxNum = 8;
        // キューブを描画済みかどうかを格納する
        private bool[,,] painted = new bool[gridMaxNum, gridMaxNum, gridMaxNum];

        public void Initialize()
        {
            grid = new Grid();
            menu = new Menu();
            menu.Initialize(cubeData, painted);
        }

        public void Update()
        {
            /*
            // ワールド座標原点
            Lines.AddAxis(new Pose(0, 0, 0, Quat.Identity), 5 * U.cm);
            Text.Add("ワールド座標原点", Matrix.Identity);
            */

            // メニュー描画
            menu.Update();

            // 全体をハンドルの子にして動かせるようにする
            UI.HandleBegin("PaintingRoot", ref _pose, new Bounds(Vec3.One * 5 * U.cm), true);
            {
                // グリッドの描画
                grid.DrawGrid((int)menu.SliderValue);

                // ピンチ操作のチェック
                PaintCube();

                // キューブの描画
                foreach (var data in cubeData)
                {
                    data.mesh.Draw(data.material, Matrix.T(data.pos));
                }
            }
            UI.HandleEnd();
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// キューブの描画処理
        /// </summary>
        private void PaintCube()
        {
            // ピンチ操作をしていなければ以降の処理は行わない
            Hand hand = Input.Hand(Handed.Right);
            if (!hand.IsPinched)
            {
                return;
            }

            // 人差し指の位置を取得
            Vec3 fingertip = hand[FingerId.Index, JointId.Tip].position;
            fingertip = Hierarchy.ToLocal(fingertip);

            // Erase ボタン選択時
            if (menu.IsErase)
            {
                foreach (var data in cubeData)
                {
                    Bounds genVolume = new Bounds(data.pos, new Vec3(5 * U.cm, 5 * U.cm, 5 * U.cm));
                    bool contains = genVolume.Contains(fingertip);
                    if (contains)
                    {
                        float x = (data.pos.x - 2.5f * U.cm) / (5 * U.cm);
                        float y = (data.pos.y - 2.5f * U.cm) / (5 * U.cm);
                        float z = (data.pos.z - 2.5f * U.cm) / (5 * U.cm);
                        painted[(int)x, (int)y, (int)z] = false;
                        cubeData.Remove(data);
                        break;
                    }
                }
            }
            // カラー選択時
            else
            {
                // ピンチ操作時にグリッド内に人差し指がある場合の処理
                for (int z = 0; z < menu.SliderValue; z++)
                {
                    for (int y = 0; y < menu.SliderValue; y++)
                    {
                        for (int x = 0; x < menu.SliderValue; x++)
                        {
                            // 既に描画済みの場合はスキップ
                            if (painted[x, y, z] == true)
                            {
                                continue;
                            }

                            // 各グリッド(5cm幅)の中心
                            Vec3 center = new Vec3(x * 5 * U.cm + 2.5f * U.cm, y * 5 * U.cm + 2.5f * U.cm, z * 5 * U.cm + 2.5f * U.cm);
                            Bounds genVolume = new Bounds(center, new Vec3(5 * U.cm, 5 * U.cm, 5 * U.cm));
                            bool contains = genVolume.Contains(fingertip);
                            if (contains)
                            {
                                Log.Info(x + ", " + y + ", " + z);
                                // 描画されたフラグを立てる
                                painted[x, y, z] = true;

                                // Cubeメッシュ生成
                                var mesh = Mesh.GenerateCube(Vec3.One * 5 * U.cm);
                                var data = new CubeData();
                                data.pos = center;
                                data.color = menu.SelectedColor;
                                var colorMat = Default.Material.Copy();
                                colorMat[MatParamName.ColorTint] = menu.SelectedColor;
                                data.material = colorMat;
                                data.mesh = mesh;
                                cubeData.Add(data);
                            }
                        }
                    }
                }
            }
        }

    } // class BoxelModelerApp
} // namespace BoxelModeler.App
