using StereoKit;
using System;
using BoxelModeler.App;

namespace BoxelModeler
{
    /*
     * PC での操作
     * 参考：https://stereokit.net/Pages/Guides/Using-The-Simulator.html
     *  - マウスの移動：手の上下左右移動
     *  - マウスホイール：手の奥行き移動
     *  - マウスの左クリック：つかむ
     *  - マウスの右クリック：つつく
     *  - マウスの左クリック + 右クリック：手を閉じる
     *  - Shift(またはCaps Lock) + マウスの右クリック + マウスの移動：頭の回転
     *  - Shift(またはCaps Lock) + W A S D Q Eキー：頭の移動
     *  - Alt + マウス操作：アイトラッキングのシミュレート
     */
    class Program
    {
        static void Main(string[] args)
        {
            // StereoKit を初期化する
            SKSettings settings = new SKSettings
            {
                // アプリ名
                appName = "BoxelModeler",
                // アセットフォルダの相対パス（StereoKit はこの配下のアセットを探しに行く）
                assetsFolder = "Assets",
            };

            // 初期化に失敗した場合はアプリ終了する
            if (!SK.Initialize(settings))
            {
                Environment.Exit(1);
            }

            // アプリ生成
            var app = new BoxelModelerApp();
            app.Initialize();
            Action step = app.Update;

            // アプリのメインループ
            // ここのコールバックが入力・システムイベントの後、描画イベントの前に毎フレーム呼ばれる
            while (SK.Step(step)) { }

            app.Shutdown();

            // アプリ終了処理（StereoKit と全てのリソースをクリーンアップする）
            SK.Shutdown();
        }
    }
}
