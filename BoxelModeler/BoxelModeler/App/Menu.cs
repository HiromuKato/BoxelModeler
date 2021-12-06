using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace BoxelModeler.App
{
    class Menu
    {
        public Color SelectedColor { get { return selectedColor; } private set { selectedColor = value; } }
        public float SliderValue { get { return sliderValue; } private set { sliderValue = value; } }
        public bool IsErase { get { return isErase; } private set { isErase = value; } }

        private Color selectedColor = new Color(1, 1, 1, 1);
        private float sliderValue = 8;
        private bool isErase = false;

        private List<CubeData> cubeData;
        private bool[,,] painted;

        private Pose menuPose = new Pose(0, 0, -0.5f, Quat.LookDir(0, 0, 1));
        private Mesh palleteMesh;
        private Material[] colorMatList;

        Color red = new Color(1, 0, 0, 1);
        Color yellow = new Color(1, 1, 0, 1);
        Color green = new Color(0, 1, 0, 1);
        Color cyan = new Color(0, 1, 1, 1);
        Color blue = new Color(0, 0, 1, 1);
        Color magenda = new Color(1, 0, 1, 1);
        Color white = new Color(1, 1, 1, 1);
        Color black = new Color(0, 0, 0, 1);

        HandMenuRadial handMenu;

        public void Initialize(List<CubeData> cubeData, bool[,,] painted)
        {
            this.cubeData = cubeData;
            this.painted = painted;

            palleteMesh = Mesh.GenerateCube(Vec3.One * 3 * U.cm);
            colorMatList = new Material[7];
            for (int i = 0; i < colorMatList.Length; i++)
            {
                colorMatList[i] = Default.Material.Copy();
            }

            // ラジアルメニュー
            handMenu = SK.AddStepper(new HandMenuRadial(
                new HandRadialLayer("Root",
                    new HandMenuItem("Save", null, OnSaveData),
                    new HandMenuItem("Load", null, OnLoadData),
                    new HandMenuItem("About", null, OnAbout),
                    new HandMenuItem("Cancel", null, null))));
        }

        bool aboutVisible = false;
        Pose aboutPose;

        private void OnAbout()
        {
            if (!aboutVisible)
            {
                aboutPose.position = Input.Head.position + Input.Head.Forward * 0.3f;
                aboutPose.orientation = Quat.LookAt(aboutPose.position, Input.Head.position);
            }
            aboutVisible = true;
        }

        private void ShowAbout()
        {
            if (!aboutVisible)
            {
                return;
            }

            UI.WindowBegin("About", ref aboutPose, new Vec2(20, 0) * U.cm);

            UI.Label("BoxelModeler v0.0.1");
            UI.HSeparator();

            if (UI.Button("Close"))
            {
                aboutVisible = false;
            }
            UI.WindowEnd();
        }

        /// <summary>
        /// メニューを描画する
        /// </summary>
        public void Update()
        {
            // メニューウィンドウを開始する
            UI.WindowBegin("Menu", ref menuPose, UIWin.Normal);
            {
                SwatchColor("Red", 0, red, false);
                UI.SameLine();
                SwatchColor("Yellow", 1, yellow, false);
                UI.SameLine();
                SwatchColor("Green", 2, green, false);
                UI.SameLine();
                SwatchColor("Cyan", 3, cyan, false);
                UI.SameLine();
                SwatchColor("Blue", 4, blue, false);
                UI.SameLine();
                SwatchColor("Magenda", 5, magenda, false);
                UI.SameLine();
                SwatchColor("White", 6, white, false);


                UI.Space(UI.LineHeight * 0.5f);
                UI.HSeparator();

                // スライダーの追加
                UI.Label("Grid", V.XY(5 * U.cm, UI.LineHeight));
                UI.SameLine();
                if (UI.HSlider("Grid", ref sliderValue, 2, 8, 1, 16 * U.cm, UIConfirm.Pinch)) { }

                UI.HSeparator();

                //SwatchColor("Erase", black, true);
                if (UI.Button("Erase"))
                {
                    selectedColor = black;
                    Default.MaterialHand[MatParamName.ColorTint] = black;
                    isErase = true;
                }
                UI.SameLine();
                if (UI.Button("All Clear"))
                {
                    cubeData.Clear();

                    for (int z = 0; z < sliderValue; z++)
                    {
                        for (int y = 0; y < sliderValue; y++)
                        {
                            for (int x = 0; x < sliderValue; x++)
                            {
                                painted[x, y, z] = false;
                            }
                        }
                    }
                }

                UI.HSeparator();

                // ボタンの追加
                if (UI.Button("Save"))
                {
                    OnSaveData();
                }
                // 同じ行にボタンを追加
                UI.SameLine();
                if (UI.Button("Load") && !Platform.FilePickerVisible)
                {
                    OnLoadData();
                }
                UI.SameLine();
                if (UI.Button("Export"))
                {
                    OnExportObj();
                }
            }
            // メニューウィンドウを終了する
            UI.WindowEnd();

            ShowAbout();
        }

        /// <summary>
        /// 見本となるカラーボタンを配置する
        /// </summary>
        /// <param name="id"></param>
        void SwatchColor(string id, int index, Color color, bool isErase)
        {
            Bounds bounds = UI.LayoutReserve(palleteMesh.Bounds.dimensions.XY);
            bounds.dimensions.z = U.cm * 4;

            Matrix cubeTransform = Matrix.TR(bounds.center, Quat.Identity);
            colorMatList[index][MatParamName.ColorTint] = color;
            palleteMesh.Draw(colorMatList[index], cubeTransform);

            BtnState state = UI.VolumeAt(id, bounds, UIConfirm.Push);

            // ボタンを押したときの処理
            if (state.IsJustActive())
            {
                Sound.Click.Play(Hierarchy.ToWorld(bounds.center));
                Default.MaterialHand[MatParamName.ColorTint] = color;

                selectedColor = color;
                this.isErase = isErase;
            }

            // ボタンを離したときの処理
            if (state.IsJustInactive())
            {
                Sound.Unclick.Play(Hierarchy.ToWorld(bounds.center));
            }
        }

        /// <summary>
        /// データを読み込む
        /// </summary>
        private void OnLoadData()
        {
            Platform.FilePicker(PickerMode.Open, file =>
            {
                if (Platform.ReadFile(file, out string text))
                {
                    Log.Info(text);
                    LoadDataFromText(text);
                }
            }, null, ".bxm");
        }

        CubeData tmpCubeData = null;
        private void LoadDataFromText(string text)
        {
            for (int z = 0; z < sliderValue; z++)
            {
                for (int y = 0; y < sliderValue; y++)
                {
                    for (int x = 0; x < sliderValue; x++)
                    {
                        painted[x, y, z] = false;
                    }
                }
            }
            cubeData.Clear();

            var lines = text.Split("\n");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    if (lines[i] != "BoxelModeler")
                    {
                        // 不正なファイル
                        Log.Err("Invalid file!");
                        return;
                    }
                    continue;
                }

                var line = lines[i];
                var words = line.Split(' ');
                if (words[0] == "p")
                {
                    // データ生成
                    tmpCubeData = new CubeData();

                    float x = float.Parse(words[1]);
                    float y = float.Parse(words[2]);
                    float z = float.Parse(words[3]);
                    tmpCubeData.pos = new Vec3(x, y, z);
                }
                else if (words[0] == "c")
                {
                    if (tmpCubeData == null) continue;

                    float r = float.Parse(words[1]);
                    float g = float.Parse(words[2]);
                    float b = float.Parse(words[3]);
                    float a = float.Parse(words[4]);
                    tmpCubeData.color = new Color(r, g, b, a);

                    // Cubeメッシュ生成
                    var mesh = Mesh.GenerateCube(Vec3.One * 5 * U.cm);
                    var colorMat = Default.Material.Copy();
                    colorMat[MatParamName.ColorTint] = tmpCubeData.color;
                    tmpCubeData.material = colorMat;
                    tmpCubeData.mesh = mesh;
                    cubeData.Add(tmpCubeData);

                    float x = (tmpCubeData.pos.x - 2.5f * U.cm) / (5 * U.cm);
                    float y = (tmpCubeData.pos.y - 2.5f * U.cm) / (5 * U.cm);
                    float z = (tmpCubeData.pos.z - 2.5f * U.cm) / (5 * U.cm);
                    painted[(int)x, (int)y, (int)z] = true;

                    tmpCubeData = null;
                }
            }
        }

        /// <summary>
        /// データを保存する
        /// </summary>
        private void OnSaveData()
        {
#if WINDOWS_UWP
            // 公式の処理でセーブできない（OSの不具合？）ため以下暫定処理
            StringBuilder sb = new StringBuilder("BoxelModeler\n");
            foreach (var data in cubeData)
            {
                sb.Append("p " + data.pos.x + " " + data.pos.y + " " + data.pos.z + "\n");
                sb.Append("c " + data.color.r + " " + data.color.g + " " + data.color.b + " " + data.color.a + "\n");
            }

            SaveTextFileForUWP(".bxm", sb.ToString());
#else
            Platform.FilePicker(PickerMode.Save, file =>
            {
                StringBuilder sb = new StringBuilder("BoxelModeler\n");

                foreach (var data in cubeData)
                {
                    sb.Append("p " + data.pos.x + " " + data.pos.y + " " + data.pos.z + "\n");
                    sb.Append("c " + data.color.r + " " + data.color.g + " " + data.color.b + " " + data.color.a + "\n");
                }

                Platform.WriteFile(file + ".bxm", sb.ToString());
            }, null, ".bxm"); // 中身はテキストだがtxtと見分けるため拡張子は独自のもの
#endif
        }

        private void SaveTextFileForUWP(string ext, string value, string filename = null)
        {
#if WINDOWS_UWP
            try
            {
                Task.Run(async () =>
                {
                    // アプリ内に保存
                    if (filename == null)
                    {
                        DateTime dt = DateTime.Now;
                        filename = dt.ToString($"{dt:yyyyMMddHHmmss}");
                    }
                    var storageFolder = ApplicationData.Current.LocalFolder;
                    var file = await storageFolder.CreateFileAsync(filename + ext, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, value);

                    // アプリ内のデータをDocuments/BoxelModelerにコピー
                    var documentsFolder = KnownFolders.DocumentsLibrary;
                    var files = await storageFolder.GetFilesAsync();
                    var targetFolder = await ((StorageFolder)documentsFolder).CreateFolderAsync("BoxelModeler", CreationCollisionOption.ReplaceExisting);
                    foreach (var f in files)
                    {
                        await f.CopyAsync(targetFolder);
                    }
                });
            }
            catch (Exception ex)
            {
            }
#endif
        }

        /// <summary>
        /// objフォーマットのデータをエクスポートする
        /// </summary>
        private void OnExportObj()
        {
            // Cubeの形状しかないため簡易的なエクスポート処理
            // 一般的なobjのエクスポートで利用できるものではない
            Vec3[] baseVecs_v = new Vec3[]
            {
                // 以下の場合cubeの原点は(0, 0, 0)だが、
                // アプリ上で描画したモデル座標の原点に一番近いcubeの原点は(0.025, 0.025, 0.025)なので
                // エクスポートしたモデルは位置が異なっている（がこのままとしておく）
                new Vec3( -1.000000f, -1.000000f,  1.000000f ),
                new Vec3( -1.000000f,  1.000000f,  1.000000f ),
                new Vec3( -1.000000f, -1.000000f, -1.000000f ),
                new Vec3( -1.000000f,  1.000000f, -1.000000f ),
                new Vec3(  1.000000f, -1.000000f,  1.000000f ),
                new Vec3(  1.000000f,  1.000000f,  1.000000f ),
                new Vec3(  1.000000f, -1.000000f, -1.000000f ),
                new Vec3(  1.000000f,  1.000000f, -1.000000f ),
            };

            Vec3[] baseVecs_vn = new Vec3[]
            {
                new Vec3(-1.0000f, 0.0000f, 0.0000f),
                new Vec3(0.0000f, 0.0000f, -1.0000f),
                new Vec3(1.0000f, 0.0000f, 0.0000f),
                new Vec3(0.0000f, 0.0000f, 1.0000f),
                new Vec3(0.0000f, -1.0000f, 0.0000f),
                new Vec3(0.0000f, 1.0000f, 0.0000f)
            };

            Vec3[] baseVecs_fv = new Vec3[]
            {
                new Vec3(2, 3, 1),
                new Vec3(4, 7, 3),
                new Vec3(8, 5, 7),
                new Vec3(6, 1, 5),
                new Vec3(7, 1, 3),
                new Vec3(4, 6, 8),
                new Vec3(2, 4, 3),
                new Vec3(4, 8, 7),
                new Vec3(8, 6, 5),
                new Vec3(6, 2, 1),
                new Vec3(7, 5, 1),
                new Vec3(4, 2, 6),
            };
            Vec3[] baseVecs_fn = new Vec3[]
{
                new Vec3(1, 1, 1),
                new Vec3(2, 2, 2),
                new Vec3(3, 3, 3),
                new Vec3(4, 4, 4),
                new Vec3(5, 5, 5),
                new Vec3(6, 6, 6),
                new Vec3(1, 1, 1),
                new Vec3(2, 2, 2),
                new Vec3(3, 3, 3),
                new Vec3(4, 4, 4),
                new Vec3(5, 5, 5),
                new Vec3(6, 6, 6),
            };

            string filename = "color";
            int index = 0;
            string matName = "WhiteMaterial";

            StringBuilder sb = new StringBuilder("# Exported from BoxelModeler\n");
            sb.Append("mtllib " + filename + ".mtl\n");
            sb.Append("o Boxels\n");

            // cubeData から obj ファイルを生成する
            foreach (var data in cubeData)
            {
                sb.Append(Calc_v(baseVecs_v[0], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[1], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[2], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[3], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[4], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[5], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[6], data.pos, index));
                sb.Append(Calc_v(baseVecs_v[7], data.pos, index));

                sb.Append("vn " + baseVecs_vn[0].x + " " + baseVecs_vn[0].y + " " + baseVecs_vn[0].z + "\n");
                sb.Append("vn " + baseVecs_vn[1].x + " " + baseVecs_vn[1].y + " " + baseVecs_vn[1].z + "\n");
                sb.Append("vn " + baseVecs_vn[2].x + " " + baseVecs_vn[2].y + " " + baseVecs_vn[2].z + "\n");
                sb.Append("vn " + baseVecs_vn[3].x + " " + baseVecs_vn[3].y + " " + baseVecs_vn[3].z + "\n");
                sb.Append("vn " + baseVecs_vn[4].x + " " + baseVecs_vn[4].y + " " + baseVecs_vn[4].z + "\n");
                sb.Append("vn " + baseVecs_vn[5].x + " " + baseVecs_vn[5].y + " " + baseVecs_vn[5].z + "\n");

                matName = GetMaterialName(data.color);
                sb.Append("usemtl " + matName + "\n");
                sb.Append("s off\n");

                sb.Append(Calc_f(baseVecs_fv[0], baseVecs_fn[0], index));
                sb.Append(Calc_f(baseVecs_fv[1], baseVecs_fn[1], index));
                sb.Append(Calc_f(baseVecs_fv[2], baseVecs_fn[2], index));
                sb.Append(Calc_f(baseVecs_fv[3], baseVecs_fn[3], index));
                sb.Append(Calc_f(baseVecs_fv[4], baseVecs_fn[4], index));
                sb.Append(Calc_f(baseVecs_fv[5], baseVecs_fn[5], index));
                sb.Append(Calc_f(baseVecs_fv[6], baseVecs_fn[6], index));
                sb.Append(Calc_f(baseVecs_fv[7], baseVecs_fn[7], index));
                sb.Append(Calc_f(baseVecs_fv[8], baseVecs_fn[8], index));
                sb.Append(Calc_f(baseVecs_fv[9], baseVecs_fn[9], index));
                sb.Append(Calc_f(baseVecs_fv[10], baseVecs_fn[10], index));
                sb.Append(Calc_f(baseVecs_fv[11], baseVecs_fn[11], index));
                index++;
            }

            Log.Info(sb.ToString());

#if WINDOWS_UWP
            // 公式の処理でセーブできない（OSの不具合？）ため以下暫定処理
            SaveTextFileForUWP(".obj", sb.ToString());

            // マテリアルファイルの出力
            StringBuilder matsb = new StringBuilder();
            matsb.Append("newmtl WhiteMaterial\n");
            matsb.Append("Kd 1.000000 1.000000 1.000000\n");
            matsb.Append("newmtl RedMaterial\n");
            matsb.Append("Kd 1.000000 0.000000 0.000000\n");
            matsb.Append("newmtl YellowMaterial\n");
            matsb.Append("Kd 1.000000 1.000000 0.000000\n");
            matsb.Append("newmtl GreenMaterial\n");
            matsb.Append("Kd 0.000000 1.000000 0.000000\n");
            matsb.Append("newmtl CyanMaterial\n");
            matsb.Append("Kd 0.000000 1.000000 1.000000\n");
            matsb.Append("newmtl BlueMaterial\n");
            matsb.Append("Kd 0.000000 0.000000 1.000000\n");
            matsb.Append("newmtl MagendaMaterial\n");
            matsb.Append("Kd 1.000000 0.000000 1.000000\n");
            SaveTextFileForUWP(".mtl", matsb.ToString(), "color");
#else
            Platform.FilePicker(PickerMode.Save, file =>
            {
                Platform.WriteFile(file + ".obj", sb.ToString());
            }, null, ".obj");
#endif
        }

        private string Calc_v(Vec3 v, Vec3 pos, int index)
        {
            v = v * 2.5f * U.cm;
            float x = (pos.x - 2.5f * U.cm) / (5 * U.cm);
            float y = (pos.y - 2.5f * U.cm) / (5 * U.cm);
            float z = (pos.z - 2.5f * U.cm) / (5 * U.cm);

            var str = "v " + (v.x + x * 5 * U.cm) + " " + (v.y + y * 5 * U.cm) + " " + (v.z + z * 5 * U.cm) + "\n";
            return str;
        }

        private string Calc_f(Vec3 fv, Vec3 fn, int index)
        {
            var str = "f " +
                (int)(fv.x + index * 8) + "//" + (int)(fn.x + index * 6) + " " +
                (int)(fv.y + index * 8) + "//" + (int)(fn.y + index * 6) + " " +
                (int)(fv.z + index * 8) + "//" + (int)(fn.z + index * 6) + "\n";
            return str;
        }

        private string GetMaterialName(Color color)
        {
            if (color.r == 1 && color.g == 1 && color.b == 1)
            {
                return "WhiteMaterial";
            }
            else if (color.r == 1 && color.g == 0 && color.b == 0)
            {
                return "RedMaterial";
            }
            else if (color.r == 1 && color.g == 1 && color.b == 0)
            {
                return "YellowMaterial";
            }
            else if (color.r == 0 && color.g == 1 && color.b == 0)
            {
                return "GreenMaterial";
            }
            else if (color.r == 0 && color.g == 1 && color.b == 1)
            {
                return "CyanMaterial";
            }
            else if (color.r == 0 && color.g == 0 && color.b == 1)
            {
                return "BlueMaterial";
            }
            else if (color.r == 1 && color.g == 0 && color.b == 1)
            {
                return "MagendaMaterial";
            }
            return "WhiteMaterial";
        }
    }
}
