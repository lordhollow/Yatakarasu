# Yataの継続的なページフォルト調査

調査日: 2026-10-26（JST）。対象は1280×800、.NET Framework版Yata。

## 結論

主因として再現できたのは、`WidgetContainer.Draw()`でARGB形式の全画面ビットマップ3枚を繰り返し合成する処理。
3層の形式を`Format32bppPArgb`へ変更すると、単独再現試験のページフォルトはほぼ消えた。
`DrawImageUnscaled()`への変更や`Graphics`の生成頻度の変更だけでは、この現象を解消しない。

GDI+のアルファ合成時の一時変換領域に整合する結果であり、増え続ける累積値をそのままメモリリークやページファイル読み込みと解釈するのは適切ではない。
ただし、実行中Yataの呼び出しスタック別内訳は採取しておらず、HW取得やWinForms内部の寄与まで全量を説明したわけではない。

## 実プロセスの観測

18:44:36～18:45:04 JST、21標本、カウンター時刻差27.57秒。
サンドボックスではホストのプロセスを列挙できないため、ホスト側の読み取り権限でWMIの累積カウンターを取得した。
Yataの停止・再起動や設定変更は実施していない。

| 項目 | 観測値 |
| --- | ---: |
| Yataのページフォルト | 38,121回/秒 |
| CPU使用率（1コアを100%とする） | 10.37% |
| Private Bytesの変化 | +824 KiB |
| Working Setの変化 | +28 KiB |
| プロセスのRead/Write Bytes累積値の変化 | どちらも0 |
| システム全体のPage Reads/sec平均 | 1.52 |
| システム全体のPages Input/sec平均 | 5.52 |
| 利用可能物理メモリの最小値 | 32,753 MiB |

大量のページフォルトに対してディスク読み込みは少なく、主にソフトフォルトと考えられる。
システムのカウンターは全プロセスを含み、プロセスのI/Oカウンターもハードフォルトそのものを数えるものではないため、Yata単独のハードフォルト数とは断定しない。
この短時間の観測だけで、長時間のメモリリークまで否定するものでもない。

ページフォルトの区別は[Microsoftの解説](https://techcommunity.microsoft.com/blog/askperf/the-basics-of-page-faults/373120)を参照。

## 描画だけの再現試験

`tools/diagnostics/GraphicsPageFaultProbe.cs`で、HW監視を開かず、同サイズの3層を描画した。
各ケース30回のウォームアップ後に200回×3組を測定した。`Graphics`生成のみのケースは1000回×3組。
描画ケースの測定区間ではGen0/Gen1/Gen2のGCはいずれも0回だった。
数値はこのPC・この描画先での結果であり、フレームレートや時間の保証ではない。

| ケース | ページフォルト/回（平均） | 所要ms/回（平均） |
| --- | ---: | ---: |
| 現行ARGB 3層 + DrawImage | 3006.002 | 10.382 |
| ARGBの背景1枚 | 1002.003 | 2.566 |
| 完全透明ARGBのオーバーレイ1枚 | 1002.002 | 2.290 |
| ARGB 3層 + DrawImageUnscaled | 3006.000 | 10.631 |
| ARGB 3層 + ピクセル矩形を明示 | 3006.040 | 10.617 |
| PArgb 3層 + DrawImage | 0.0017 | 6.916 |
| PArgb 3層 + DrawImageUnscaled | 0.0017 | 7.259 |
| Graphicsの生成・破棄のみ | 0.016 | 0.00045 |

1280×800×4バイトは約1000個の4 KiBページに相当し、1枚あたり約1002フォルトという実測と対応する。
3層を毎秒10回合成すれば、それだけで約3万フォルト/秒となる。実Yataの約3.8万フォルト/秒の大きな部分を説明できる規模。
完全透明でも変換が発生しており、`DrawImageUnscaled`だけへの変更では改善しなかった。
PArgbでのアルファ合成の効率については[MicrosoftのWinForms最適化記事](https://learn.microsoft.com/en-us/archive/msdn-magazine/2006/march/practical-tips-for-boosting-the-performance-of-windows-forms-apps)も参照。

## 対応と検証

`WidgetContainer.Initialize()`の背景・明るさ・ウィジェットの3層を`Format32bppPArgb`で生成するよう変更した。
描画座標・更新間隔・HW監視設定は変更していない。
従来のARGB形式との半透明合成の比較を回帰テストに追加し、明るさ1/0.45/0、半透明の重なりと文字を検証する。
RGBの差は乗算済みアルファの量子化を考慮して各成分2まで、出力アルファは一致を要求する。

実行中のYataは旧形式のままなので、実アプリでの改善量は両プロセスを再起動してから比較する必要がある。
通常利用するReleaseを再ビルドして再起動後、同じ表示状態で以下を採取する。試験ビルドは`tests/bin/`に分離してある。

```powershell
./tools/diagnostics/measure-process-page-faults.ps1 -DurationSeconds 30
./tools/diagnostics/run-graphics-page-fault-probe.ps1
./tests/run-configuration-tests.ps1 -Configuration Release
```

採取スクリプトはCSVと`.summary.json`を`tests/bin/PageFaultInvestigation/`に出力する。
管理者で実行中のYataでもWMIから読み取れる範囲を採取し、プロセスの動作や設定は変更しない。
もし修正後も大きな残差があれば、描画回数・HW機器別更新を分けた計測、またはWPR/WPAでスタック付きのメモリトレースを追加する。
