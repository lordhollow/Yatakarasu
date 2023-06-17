chcp 65001
ffmpeg -i "C:\Users\rrr_r\AppData\LocalLow\Nolla_Games_Noita\save_rec\screenshots_animated\noita-20230611-221913-1346089802-00074316.gif" -c:v h264_nvenc -b:v 0 -cq 26 -movflags faststart -pix_fmt yuv420p -c:a copy -vf scale=960:-1 "C:\Users\rrr_r\AppData\LocalLow\Nolla_Games_Noita\save_rec\screenshots_animated\noita-20230611-221913-1346089802-00074316.gif.mp4"
@echo off
pause
exit
