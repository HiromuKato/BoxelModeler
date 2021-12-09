# BoxelModeler

StereoKitを利用した、簡易モデリングアプリです。

HoloLens 2 で利用することを想定していますが、PC でシミュレートすることも可能です。

## 動画

[![BoxelModeler](https://img.youtube.com/vi/Q905tehb8P4/0.jpg)](https://www.youtube.com/watch?v=Q905tehb8P4)

## 操作方法

- 好きなカラーを選択して、グリッド内でピンチ操作（人差し指と親指を合わせる）するとキューブが描画されます

- Erase を選択してピンチ操作を行うとキューブを削除します

- All Clear を選択すると全てのキューブを削除します

- Save ボタンを押すとアプリ内（※）、および Documtns/BoxelModeler フォルダ内に描いたキューブ情報を保存します

- Load ボタンを押すとファイルピッカーが表示されるため、保存済みほファイルを選択することで以前の状態を復元できます

- Export ボタンを押すとアプリ内（※）、および Documtns/BoxelModeler フォルダ内に obj ファイルと mtl ファイルを出力します

- Grid のスライダーを操作すると、グリッドの数を調整できます

- グリッドの原点にあるキューブを掴むことでグリッドを移動・回転できます

  （※） デバイスポータルからアクセスできる System > File explorer > Boxel Modeler > LocalState 内