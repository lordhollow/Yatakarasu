# 設定画面の列がDesigner開閉で消える問題

## 再現と原因

センサー設定をVisual StudioのDesignerで開閉すると、`InitializeComponent`から列の生成と`Columns.AddRange`が削除され、実行時の`Rows.Add`で「列を含んでいないDataGridViewコントロールに行を追加することはできません」となる。

前回はメモリ内のCodeDOMシリアライズと再生成を検証したが、VSのDesignerがソースから読み込む際の処理とは異なり、この条件を検出できていなかった。
共通Bodyパネルの継承修正後も、列だけが消える現象は残っていた。

ユーザーの管理者VSインスタンスは操作せず、`tests/bin/DesignerVs`へプロジェクトをコピーし、別の通常権限のVisual Studio 2022で再現した。
コピーのProgramは空のセンサー編集画面だけを起動するものに変更し、HWの初期化や制御を行わない。

1. 修正前の5列の生成コードをコピーへ戻し、ビルドする。
2. `SensorSettingsForm.cs`を開き、Shift+F7でDesignerを表示する。
3. 列はソースにあっても表示されず、保存すると列の生成・追加コードが消える。
4. VS上の「列の追加」で作った未バインド列は表示・保存される。
5. 同じ5列のコードから`DataPropertyName`の設定だけを除くと、5列が表示され、保存・閉じる・再表示後も保持される。

配列初期化の改行や列プロパティ設定の順番を変えるだけでは改善しなかった。
この環境では、設計時のDataSourceがない表に`DataPropertyName`を指定すると、VSがその列をバインド列として除去していた。

## 対応

- 列のインスタンス生成・追加・Name・HeaderText・幅は`InitializeComponent`に残す。
- SensorSettingsFormの5列、LauncherSettingsFormの6列、PlotEditorの3列から設計時の`DataPropertyName`を外す。
- 対応する`LoadDefinition`で、ファイル読み込み／データバインドの際にデータ項目を割り当てる。
- Launcherの`AutoGenerateColumns = false`もバインド直前に適用する。

列を実行時に作り直す回避策は使用していない。列のレイアウトは引き続きVSで編集できる。

## 確認

実際のVSで次を実施した。

- センサー設定: 5列の表示、保存、閉じる、再表示、再保存を確認。
- グラフ編集: サイズを一度変更して戻し、Designerのコード生成を発生させて保存。閉じる・再表示後に3列を確認。
- ランチャー編集: 保存・閉じる・再表示後に6列を確認。
- 保存後のソースに各列の生成と`Columns.AddRange`があり、Designerの`DataPropertyName`設定がないことを確認。

自動回帰テストには、引数なしコンストラクターで設計用に生成した全グリッドが未バインドであることと、設定を読み込むと実行用のデータ項目対応と行が復元されることを追加した。
既存の保存・再読み込み・表示・CodeDOMテストも継続する。

VSで保存したコピーそのものをビルドしてテストするには、次を実行する。

```powershell
./tests/run-configuration-tests.ps1 -Configuration Debug `
  -ProjectPath tests/bin/DesignerVs/Yata.csproj `
  -OutputDirectory tests/bin/Configuration/DesignerVsSaved
```

通常のプロジェクトは以下で検証する。

```powershell
./tests/run-configuration-tests.ps1 -Configuration Release
./tests/run-configuration-tests.ps1 -Configuration Debug
```

コピーのプロジェクトとVS生成物は`tests/bin`内にあり、リポジトリには含めない。

検証結果: 通常プロジェクトのDebug・Release、およびVSで保存したコピーのDebugビルドで、それぞれ24項目すべて成功した。
