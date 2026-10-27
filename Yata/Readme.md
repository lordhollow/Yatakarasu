# ヤタガラス

## ビルド

Visual Studio 2022の「.NET デスクトップ開発」と.NET Framework 4.7.2のターゲットパックを使用する。
Developer PowerShellでリポジトリのルートから以下を実行する。

```powershell
msbuild Yata.sln /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RestoreLockedMode=true
msbuild Yata.sln /t:Build /p:Configuration=Debug /p:Platform=x64
```

Releaseをビルドするときは、両方のコマンドの`Configuration`を`Release`に変更する。
NuGetパッケージは`PackageReference`で管理し、`Yata/packages.lock.json`で間接依存も固定する。
依存バージョンを更新するときだけ`RestoreLockedMode`を指定せずに復元し、ロックファイルの差分も確認する。

LibreHardwareMonitorLibはNuGetの安定版0.9.6を使用し、`win-x64`用の実装と依存DLLをビルド出力に配置する。
以前のWinRing0に代わりPawnIOを使用するため、CPU温度やマザーボードのセンサー取得には
[PawnIO](https://pawnio.eu/)の別途インストールが必要。NuGetの復元はドライバをインストールしない。
切り替えの経緯は[公式PR](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/pull/1857)を参照。

実行用の画像・設定は従来どおり各exeと同じフォルダの`files/`に配置する。
配布時にはビルド出力の依存DLLと`Yata.exe.config`も含める。

## センサー設定・ランチャー編集

画面を右クリックすると設定メニューを開ける。Yataにフォーカスがある状態ではF2でセンサー設定、F3でランチャー編集を開くこともできる。
メイン画面のウィジェットの座標・サイズは従来のまま、タコメーターは6個、グラフは2個に固定する。

各編集画面の「開く…」で`sensor.txt`／`karasu.exec.txt`を選択し、「保存先を選択…」で別名のファイルへ保存できる。
「保存して反映」で保存した内容をその場で反映する。既存ファイルの直前の内容は同じ場所の`.bak`ファイルに残す。
選択中のパスは`%LOCALAPPDATA%/Yatakarasu/configuration.xml`に保存し、次回起動時にも使用する。
選択したファイルがなくなった場合は、実行フォルダの`files/sensor.txt`／`files/karasu.exec.txt`に戻る。
設定ファイルを切り替えても、実際の外部アプリ起動は通常権限のKarasuが担当する。YataとKarasuは両方を更新して再起動すること。

### センサー設定

タコメーターは6行の表で、表示名・単位・最小値・最大値・センサーIDを設定する。
この表は各メーターの親項目を表示する。行を選んで「層を編集…」を開くと、一つのメーターへ複数の項目を割り当てられる。
親のセンサーIDが未割当（空欄・`-`）で子項目がある場合、親は全層表示の名前だけを提供し、描画・欠測判定・クリック巡回には含めない。親の最小／最大は描画に使わない。
親にIDを割り当てた場合は、親が最外層となる。割当済みのIDで値が取得できない場合は、親も欠測の赤い層として表示する。
編集画面の上から外側→内側の順で同心円に配置する（名前だけの親は除く）。「項目を追加／削除」「上へ（外側）／下へ（内側）」で構成を変更する。
表示名・単位・最小／最大・センサーIDは項目ごとに保存する。「決定」の後、親画面の「保存して反映」で確定する。キャンセル時は親の設定を変更しない。
「少ない方を赤」にチェックすると、各項目の最小／最大に対する色分けを反転できる。GPT残量などに使用する。親項目はメーター一覧、子項目は「層を編集…」で変更する。
値・数値表示・グラフの量は反転せず、色だけを変更する。従来の警告境界を反転した色分けで、範囲の約7.4%未満が赤、約33.3%未満が橙、それ以上は青となる。0では開始位置に短い赤の印を表示する。取得不能は設定にかかわらず赤100%と`----`になる。
未チェックの項目と旧形式のファイルは、従来の「多い方を赤」を維持する。設定は項目ごとに保存され、並べ替えや再起動後も保持する。
通常の単一項目なら名前・数値・単位を表示する。全層表示では親の表示名だけを表示し、通常の数値・単位は表示しない。名前だけの親に子が1項目の場合も、全層表示では親の名前、クリック後は子の名前・数値・単位を表示する。
多層メーターをクリックすると「全層 → 1項目目 → 2項目目 → … → 全層」と切り替わる。個別表示ではその項目を通常サイズで表示し、対応する名前・数値・単位を表示する。
表示対象は保存せず、再起動・設定の再反映後は全層表示から始まる。個別表示中の欠測表示は、その表示対象の取得状態で判定する。
取得できない項目は、その層だけを100%の赤で表示する。全層表示では一つでも取得不能なら`----`を表示する。個別表示では選択中の項目が取得不能なら`----`とし、取得が戻れば通常表示へ戻る。
名前だけの親を除き、未設定のID（`-`）も取得不能として扱う。メーター自体の配置数は6個のままで、グラフは2個のままとする。
メーター一覧の「上へ／下へ」で、親と全子項目をまとめて配置順を変更できる。各プロットの「上へ／下へ」は系列の名前・ID・色をまとめて移動する。どちらも保存・再起動後に順序を保持する。

設定ファイルは`Meter`行を親とし、直後に`MeterLayer`行を外側から内側へ向かう順で追加する（区切りはタブ）。親にIDがあれば親を最外層とする。旧形式もそのまま読み込める。

```text
Meter	GPT残量	%	0	100	-
MeterLayer	GPT 5h	%	0	100	/codex/remaining/5h	low-red
MeterLayer	GPT週残量	%	0	100	/codex/remaining/1w	low-red
```

「センサーを選択…」では検出済みのセンサーを名前・種類・IDで検索して選べる。未設定のスロットは`-`で保存する。
グラフは2つのタブで、タイトル・単位・範囲・境界値と、各系列のセンサー・表示色を指定する。現在の一覧表示に合わせて1グラフ12系列までとする。
ID欄には従来の範囲平均・合計の記法も入力できる。元ファイルに6メーター／2グラフを超える設定がある場合は画面に注意を表示し、保存時に固定数へ揃える。

### GPTの残り使用量（Codex 5h／1w）

センサー選択の一覧で「GPT残量」を検索し、「GPT残量 5h」「GPT残量 1w」を選ぶ。
タコメーター・グラフのいずれにも追加できる。単位は`%`、最小値`0`、最大値`100`を指定する。
既存の6メーター・2グラフの枠へ割り当てるため、画面レイアウトと既存の設定は自動変更しない。

| センサーID | 表示内容 |
| --- | --- |
| `/codex/remaining/5h` | Codexの5時間枠の残り（%） |
| `/codex/remaining/1w` | Codexの1週間枠の残り（%） |

これはChatGPTアカウントに付属する**Codexの使用枠**で、ChatGPTの全モデル共通の残量やAPIクレジットではない。
[公式のApp Server仕様](https://learn.chatgpt.com/docs/app-server#6-rate-limits-chatgpt)の`account/rateLimits/read`を使用し、対象バケット`codex`の`100 - usedPercent`を表示する。
5h／1wの対応は返された期間（300分／10080分）で判定する。異なる期間や取得不能な枠を推測で埋めない。

前提は、Codexがインストールされ、同じWindowsユーザーのCodexでChatGPTアカウントへログイン済みであること。
Yataは非表示の`codex app-server`を取得時だけ起動し、既存のログイン状態を使う。Yata自体は認証ファイルを読まず、トークンやサーバー応答を保存・ログ出力しない。
モデルの実行、チャットの作成、使用枠のリセットは行わない。
実行ファイルは`PATH`の`codex.exe`、次にデスクトップ版の`%LOCALAPPDATA%/OpenAI/Codex/bin/*/codex.exe`を検索する。
別の場所へインストールした場合は、環境変数`YATA_CODEX_PATH`へ`codex.exe`の絶対パスを設定してYataを再起動する。
APIキーでのログインだけでは対象の残量を取得できない。

どちらかのセンサーを選択している場合だけ、バックグラウンドで5分ごとに取得する。2項目で1回の取得を共有し、最大待ち時間は20秒。
未取得・ログイン切れ・取得失敗時は欠測とし、タコメーターは赤100%と`----`、グラフの数値欄は`N/A`で表示する。赤100%は残量100%を意味しない。成功した値も10分経過、または返されたリセット時刻に達すると無効にし、古い値から残量100%を推測しない。
削除すると取得を停止する。5分間隔のため、Codex画面に表示される現在値とは時間差がある。

オフライン回帰テスト（Codex・ネット接続・実機HW不要）と、ログイン済みCodexでの任意の接続確認:

```powershell
./tests/run-codex-usage-tests.ps1
./tests/run-codex-usage-tests.ps1 -Live
```

### ランチャー編集

「項目を追加…」から、次のいずれかを選ぶ。

- **Exeを選択…**: 実行ファイルを選び、Exe／DLL内の複数アイコンや画像ファイルからアイコンを選択し、起動オプションを入力する。
- **Steamから…**: Steamの登録先と`libraryfolders.vdf`、各ライブラリの`appmanifest_*.acf`からインストール済みソフトを選ぶ。Steamが見つからない場合はSteam本体のフォルダを指定する。起動は`steam.exe -applaunch AppID`を使い、その後にゲームの起動オプションを追加できる。
- **システムアプリ**: 一覧から選択する。明るさ・ディスプレイ構成・入力切替にはオプションの候補を表示する。

選択したアイコンはPNGへ保存して参照するため、Exe内のリソースを毎回読み込む必要はない。既存の画像パス・内部画像アトラス指定も利用可能。
配置は「ページ（1～）・行（0～1）・列（0～15）」で指定する。同じ位置の重複は保存前に検出する。項目の編集・複製・削除にも対応する。
「ページを追加」では新しいページに前／次のボタンを追加する。既存ページの配置は保持するため、既存ページには必要に応じてページ切替のシステムアプリを置く。
右クリックメニューからも前／次へ移動でき、先頭と末尾のページでは折り返す。

追加したシステムアプリのコマンド名は次のとおり。

- `internal::LauncherNextPage` / `internal::LauncherPreviousPage`: 次／前のページへ移動する。
- `internal::MonitorPowerOff`: クリック後約0.7秒で接続中のモニターを消灯する。マウス・キーボード操作で復帰する。
- `internal::EditSensors` / `internal::EditLauncher`: それぞれの編集画面を開く。

設定ファイルの保存・検証、複数Steamライブラリ、アイコン抽出、ページ切替、Karasuの実行先選択、画面の描画を実機HW操作なしで検証するには、以下を実行する。
画面確認用PNGは`tests/bin/Configuration/<Configuration>/previews/`に出力する。

```powershell
./tests/run-configuration-tests.ps1
./tests/run-hardware-monitor-tests.ps1
```

### Visual Studioで設定画面を調整する

ソリューションを一度ビルドし、`Settings`配下のフォームの`.cs`を右クリック →「デザイナーの表示」（Shift+F7）で開く。
コントロールの生成・配置・初期プロパティ・イベント接続は、各`.Designer.cs`の`InitializeComponent`で定義する。
引数なしコンストラクターは画面の初期化だけを行い、設定ファイル読み込み・画像読み込みは実行時の引数付きコンストラクター、Steam検索は実行時のShownで行う。

| 編集対象 | ファイル |
| --- | --- |
| センサー設定 | `Settings/SensorSettingsForm.cs` |
| タコメーターの層編集 | `Settings/MeterLayersForm.cs` |
| ランチャー一覧 | `Settings/LauncherSettingsForm.cs` |
| ランチャー項目 | `Settings/LauncherEntryForm.cs` |
| センサー選択 | `Settings/SensorPickerForm.cs` |
| アイコン選択 | `Settings/IconPickerForm.cs` |
| Steamソフト選択 | `Settings/SteamPickerForm.cs` |
| 設定ファイル選択・保存ボタンなどの共通枠 | `Settings/ConfigurationEditorForm.cs` |
| グラフの編集欄（2個に共通） | `Settings/PlotEditor.cs` |

共通枠の中央の`Body`パネルへ各設定フォームのコントロールを配置する。`Body`はフォーム直下のprotectedパネルとし、継承されるTableLayoutPanelなどの内部には置かない。グラフの欄は`PlotEditor`をユーザーコントロールとして編集する。
グリッドの列の`Name`はDesignerのフィールド名と揃える。表示見出しの変更には`HeaderText`を使う。
**Designerでは`DataPropertyName`を空のままにする。** 設計時のDataSourceがない状態でバインド列にすると、VSが開閉・保存時に列を除去する場合がある。データ項目との対応は各`LoadDefinition`で実行時に設定する。
構成回帰テストでは、全9画面・部品のデザイナーホストへの読み込みに加え、編集領域・グラフ部品をCodeDOMで保存・再生成して、タブとグリッド列の名前・数・データ項目との対応が残ることを検証する。実行時の保存・表示も検証する。
VSで実際に保存したコピーを検証する場合は、`run-configuration-tests.ps1 -ProjectPath <コピーのcsproj> -OutputDirectory <テスト出力先>`を使う。[列が消える問題の調査記録](../docs/investigations/settings-designer-columns-2026-10-26.md)も参照。

## センサー取得に失敗した場合

機器の更新に失敗しても、ほかの機器の監視は継続する。失敗した機器とその合計・平均センサーは欠測表示（タコメーターは赤100%と`----`、グラフの数値欄は`N/A`）になり、次の更新周期で再試行する。
エラーの日時・機器ID・例外はexeと同じフォルダの`hardware-monitor.errors.log`に記録する。同じ機器の連続失敗は初回だけ記録する。

HWにアクセスせずに更新の失敗・復旧・重複防止・欠測表示を検証するには、リポジトリのルートから以下を実行する。

```powershell
./tests/run-hardware-monitor-tests.ps1
```

## ページフォルトの調査

ARGB形式の全画面3層を毎回合成すると、GDI+側で多数のページフォルトが発生することを再現できたため、描画用ビットマップはPArgb形式で保持する。
さらにWinFormsのダブルバッファが毎回再確保されないよう、実クライアント領域に合わせて保持上限を設定する。ダブルバッファは引き続き有効。
時計・ランチャーなどの画像も読み込み時にPArgb形式へ変換し、クリックの緑枠・赤枠の減衰中にアイコン全体を描き直しても変換領域を毎回確保しない。
計測結果と再現方法は[調査記録](../docs/investigations/page-faults-2026-10-26.md)を参照。
実プロセスの採取は`tools/diagnostics/measure-process-page-faults.ps1`、HWにアクセスしない描画単独試験は`tools/diagnostics/run-graphics-page-fault-probe.ps1`で実施できる。
画面転送バッファの比較には`run-graphics-page-fault-probe.ps1 -WindowBuffers`を指定する。
実ウィジェット・設定済みのアイコン・枠の表示から消去までを調べる場合は`tools/diagnostics/run-widget-page-fault-probe.ps1`を使う。起動コマンドは無害な内部処理に差し替え、外部アプリは起動しない。
待機時の単独試験は同スクリプトの`-IdleQueries`、実行中プロセスの発生周期の採取は`-ProcessId <PID>`を指定する。
機器別の計測は右クリック →「ページフォルトを30秒計測」。通常表示で約30秒待つと、`%LOCALAPPDATA%\Yatakarasu\diagnostics`へCSVを保存する。計測時だけ有効で、HW監視の更新頻度は変えない。重複区間の扱いとCSVの読み方は上記の調査記録を参照。
NVIDIAでは未使用の電力比率・電圧取得を省く。選択した項目や合計・平均に必要な場合は取得を戻す。消費電力Wは別経路で取得する。この互換処理はLibreHardwareMonitor 0.9.6.0の内部実装に依存するため同版に限定し、ライブラリ更新時に再検証する。
GPUの読み取りAPI単位の切り分けは`run-widget-page-fault-probe.ps1 -NvidiaQueries`で実施できる。

## 実行方法
karasu.exeを起動した状態で(3分以内に)karasu.exeを管理者権限で起動する。  
karasu.exeに"+yata"オプションをつけて起動すると上記動作を行うので、以下をショートカットで作成しておくとよい。

```
> karasu.exe +yata 
```


### プロセスの説明
+ yata.exe  
モニター/ランチャーの主要機能を担当する。  
一部のH/Wモニター機能実現のため管理者権限で実行する必要がある。  
ランチャ機能を備えるが、このプロセスから起動してしまうと起動されたプロセスも管理者権限で
動作することになりセキュリティ上の懸念があるため、実際の起動はkarasuに移譲する。  
yata-karasu間はTCPで接続(ポート54892)

+ karasu.exe  
ランチャ。  
"+yata" オプション指定にてyata.exeをRunas(管理者として実行)する。
yataが管理者権限で動作するため、ランチャとして常駐し、TCPを待ち受ける(port 54892)。  
TCPは1回のコネクションにつき1つのコマンドだけを処理し、3分間接続がないかEXITコマンドを受け付けた際にこのプロセスは終了する。  


## ファイル構成

root/　　　exeとライブラリのフォルダ  
　files/　　	データファイル置き場  
　　karasu.exec.txt　　	ランチャ定義ファイル  
　　sensor.txt　　	モニター定義ファイル  
　　background.jpg　　背景画像  
　　SystemApp.png　　システムアプリ用アイコン画像  
　　clockback.png　　アナログ時計の背景画像  
　　Volume.png　　ボリュームモニタ用アイコン  

## ランチャ定義ファイル(karasu.exec.txt)の書式

tsv形式、1行目はyata-karasu間で同一データを基にしていることを確認するためのキー(将来的にはコメント行とし、キーにはファイル全体のMD5等を使用したほうがいいかもしれない)。  
tsv各カラムは以下の通り。  
+ 0: 行番号
+ 1: 列番号
+ 2: キャプション
+ 3: コマンド
+ 4: 画像ファイルパス
+ 5: オプション

行番号、列番号は表示位置。1ページは2行16列で、行番号が2～3なら2ページ目、4～5なら3ページ目となる。列番号は0～15。
キャプションは画像下に表示する文字列。英語で10文字程度までしか入らない。  
コマンドは実行するコマンド。実行ファイルや開くファイルのパスを記載する。後述する「内部アプリ」も指定可能。  
画像ファイルパスは表示するアイコン。 アスペクト比を無視して48x48に縮小される。後述する「内部画像」も指定可能。  
オプションはコマンドに渡すオプション文字列。  

### 内部アプリ

コマンドとして以下に示すものを使用可能。

+ internal::ReloadLauncher  
ランチャ定義ファイル(karasu.exec.txt)を再読み込みする

+ internal::ScreenLock  
ロック画面に遷移する

+ internal::Suspender  
休止状態にする

+ internal::DashboardBrightnessChanger  
パネルの明るさを変更する。  
パラメータに0～1の値を設定する。256諧調設定可能。

+ internal::SetDisplayInput  
ディスプレイの入力信号を選択する。  
パラメータに"入力信号名"または"ディスプレイ番号=入力信号名"を指定する。  
ディスプレイ番号(0～)を指定しないとき、"0"が指定されているものとして扱う。  
入力信号名は"TOGGLE", "DP1"(display port 1)[default],"DP2"(display port 2), "HDMI1", "HDMI2"のいずれかを指定可能。  
TOGGLEを指定した場合、現在のソースが"DP1"であれば"HDMI1"を、そうでなければ"DP1"を設定する。

+  internal::SetDisplayTopology  
ディスプレイ構成のトポロジー(メイン画面のみ、サブ画面のみ、拡張、複製）を変更する。  
パラメータにはトポロジーを表す文字列(INTERNAL, EXTERNAL, EXTEND, CLONE)のいずれかを指定する。

+  internal::SendKeyStroke  
キーストロークを送る。パラメータに送るキーストロークを記述する。フォーマットはSystem.Windows.Forms.SendKeys.Send()に引き渡す書式に準拠する。

+ internal::FanControl
ファンの回転数を変更する。  
パラメータにHardWareMonitorの制御IDと回転数をコンマ区切りで指定する。  
回転数は0～100の値で、"auto"を指定することも可能。auto指定時はデバイスの規定回転数となる。  
以下に例を示す。control以前のIdは環境依存。  
```
     /lpc/nct6799d/control/0,100        
     /lpc/nct6799d/control/0,auto
```
ほかのファン制御アプリと干渉し「自動」が働かない場合があるので注意する。一般的なファン制御アプリのように温度/dutyのテーブルを保持せず、レジスタに「変更前の値」を書き込む仕様であるため(HardwareMonitorの仕様)。
ASUSマザーボード利用時にArmouryCrate(FanXPart)を使用せずUEFIで指定したQ-FanCtrlに従い、一時的に変化させるような用途を想定している。

+ internal::CommandWithFile  
パラメータに指定した実行ファイルを実行する。
その際、ファイル名を${infile}に渡す。
例えばffmpegを利用しNVEncで品質指定エンコードを行う場合、以下のように記述する。  
ファイル名に使用されるのは、「クリップボードにコピーされたファイル」のファイル名、または、「クリップボードにダブルクオートで囲ったファイル名」のいずれかであり、いずれも指定されていない場合はダイアログを開いて入力することができる。

```
    ffmpeg -i "${infile}" -c:v h264_nvenc -b:v 0 -cq 26 -c:a copy "${infile}.mp4"
```

バリエーションに "internal::CommandWithFile(A)"などがある。かっこの中身に使える文字と意味は以下の通り。  

　　(A) 管理者権限で実行(yata.exeプロセスから実行)する  
　　(K) KeepAlive。実行後のコマンドプロンプトを閉じない  
　　(R) Redirect。実行結果をファイル(karasu.console.txt)にリダイレクトする  
　　(P) コンソールを使用しない。Process.Start()で起動する（これ以外はcmd.exeで実行する)  
　　(AK) A+K  
　　(AR) A+R  
　　(AP) A+P  



### 内部画像

画像ファイルに以下のような文字列を指定できる。
```
InternalApplicationIconsImage::1,0
```
この場合、実行フォルダにある SystemApp.png の1行目0桁目のアイコン(アイコンのサイズは48x48)を参照する。


## モニター定義ファイル(sensor.txt)
モニターする項目を定義するファイル。  
Plotで開始する行とMeterで開始する行、Plotの行に続くことで項目を表す。
トークン間はタブ区切り。

### リスト/グラフ項目
```
    Plot	Load	%	0	100	3	95
		CPU	/amdcpu/0/load/0	255,0,0
		 Core #1	/amdcpu/0/load/1-2	255,128,128
		 Core #2	/amdcpu/0/load/3-4	255,192,255
		 Core #3	/amdcpu/0/load/5-6	255,128,255
		 Core #4	/amdcpu/0/load/7-8	255,255,192
		 Core #5	/amdcpu/0/load/9-10	255,128,128
		 Core #6	/amdcpu/0/load/11-12	255,192,255
		 Core #7	/amdcpu/0/load/13-14	255,128,255
		 Core #8	/amdcpu/0/load/15-16	255,255,192
		GPU	/gpu-nvidia/0/load/0	0,192,0
```

+ Plot[\t]\(表示タイトル)[\t]\(単位)[\t]\(最小値)[\t]\(最大値)[\t]\(下境界値)[\t]\(上境界値)

Plot行に続くのはそのプロットに含める項目の定義。最小最大はプロットエリアの上下限値。下境界値・上境界値の位置に目安となる線を引くほか、下境界値以下はグレー、上境界値以上は赤でリスト表示する。  

+ [\t]\(表示名)[\t]\(センサー項目ID)[\t]\(色情報)

センサー項目IDはOpenHardwareMonitorのセンサーIDの文字列表現。（「HWモニター項目と特殊表記」も合わせて参照）
色情報はR,G,B[,A] の形式。各要素は0～255の値を指定する（Aは省略可能)。


現行verでは２個のグラフ(Plot行)まで有効。

### メーター項目
```
	Meter	CPUFan	 rpm	0	1850	/lpc/nct6799d/fan/1
```
メーターグラフに表示する項目の設定。  
+ Meter[\t]\(キャプション)[\t]\(単位)[\t]\(最小値)[\t]\(最大値)[\t]\(センサー項目ID)

末尾に任意のタブ区切りフィールド`low-red`を追加すると「少ない方を赤」になる。省略・空欄・`high-red`は従来の「多い方を赤」。`MeterLayer`にも同じ形式を使用する。

現行verでは6個のメーター(Meter行)で固定。


###  HWモニター項目と特殊表記
PlotやMeterに設定するセンサー項目はOpenHardwareMonitorのセンサーID文字列表現であり、実行する環境のHW構成ごとに異なる。  
利用可能な項目はyata.exe実行時に生成されるkarasu.hw.txtを参照。  
表記として末尾の数字部分を"1-2"のようにすることで、複数項目の平均値をサンプルデータとして使用する。  
また、複数のIDを"+"で連結することで、その合計値をサンプルデータとして使用する。  
ただし。この二つの機能（平均化と合計化）は併用できない。  

　　/amdcpu/0/load/1　　AMDCPUのコア1の使用率  
　　/amdcpu/0/load/1-2　　  AMDCPUのコア1とコア2の使用率の平均
　　/amdcpu/0/load/1+/amdcpu/0/load/2　　AMDCPUのコア1とコア2の使用率の合計  



## 背景画像
files/background.jpgを背景画像として使用する。起動時読み込み。


## Licence

このプログラムは一部に NAudio(https://github.com/naudio/NAudio)のコード(NAudio.wasapi)を含みます。  
H/WモニタにLibreHardwareMonitorLibを使用します。  

このプログラム(yata.exeおよびkarasu.exe)のライセンス形態は MIT Licenceとします。
