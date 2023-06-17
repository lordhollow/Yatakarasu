# ヤタガラス

## 実行方法
karasu.exeを起動した状態で(3分以内に)karasu.exeを管理者権限で起動する。  
karasu.exeに"+yata"オプションをつけて起動すると上記動作を行うので、以下をショートカットで作成しておくとよい。

^^^
> karasu.exe +yata 
^^^


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

行番号、列番号は 表示位置。今のところ2行(0～1)16列(0～15)のみ利用可能(読み込み自体は行われる）  
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
^^^
     /lpc/nct6799d/control/0,100        
     /lpc/nct6799d/control/0,auto
^^^
ほかのファン制御アプリと干渉し「自動」が働かない場合があるので注意する。一般的なファン制御アプリのように温度/dutyのテーブルを保持せず、レジスタに「変更前の値」を書き込む仕様であるため(HardwareMonitorの仕様)。
ASUSマザーボード利用時にArmouryCrate(FanXPart)を使用せずUEFIで指定したQ-FanCtrlに従い、一時的に変化させるような用途を想定している。

+ internal::CommandWithFile  
パラメータに指定した実行ファイルを実行する。
その際、ファイル名を${infile}に渡す。
例えばffmpegを利用しNVEncで品質指定エンコードを行う場合、以下のように記述する。  
ファイル名に使用されるのは、「クリップボードにコピーされたファイル」のファイル名、または、「クリップボードにダブルクオートで囲ったファイル名」のいずれかであり、いずれも指定されていない場合はダイアログを開いて入力することができる。

^^^
    ffmpeg -i "${infile}" -c:v h264_nvenc -b:v 0 -cq 26 -c:a copy "${infile}.mp4"
^^^

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
^^^
InternalApplicationIconsImage::1,0
^^^
この場合、実行フォルダにある SystemApp.png の1行目0桁目のアイコン(アイコンのサイズは48x48)を参照する。


## モニター定義ファイル(sensor.txt)
モニターする項目を定義するファイル。  
Plotで開始する行とMeterで開始する行、Plotの行に続くことで項目を表す。
トークン間はタブ区切り。

### リスト/グラフ項目
^^^
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
^^^

+ Plot[\t]\(表示タイトル)[\t]\(単位)[\t]\(最小値)[\t]\(最大値)[\t]\(下境界値)[\t]\(上境界値)

Plot行に続くのはそのプロットに含める項目の定義。最小最大はプロットエリアの上下限値。下境界値・上境界値の位置に目安となる線を引くほか、下境界値以下はグレー、上境界値以上は赤でリスト表示する。  

+ [\t]\(表示名)[\t]\(センサー項目ID)[\t]\(色情報)

センサー項目IDはOpenHardwareMonitorのセンサーIDの文字列表現。（「HWモニター項目と特殊表記」も合わせて参照）
色情報はR,G,B[,A] の形式。各要素は0～255の値を指定する（Aは省略可能)。


現行verでは２個のグラフ(Plot行)まで有効。

### メーター項目
^^^
	Meter	CPUFan	 rpm	0	1850	/lpc/nct6799d/fan/1
^^^
メーターグラフに表示する項目の設定。  
+ Meter[\t]\(キャプション)[\t]\(単位)[\t]\(最小値)[\t]\(最大値)[\t]\(センサー項目ID)

現行verでは７個のメーター(Meter行)まで有効。


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
