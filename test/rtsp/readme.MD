# Запустить сервер rtsp через отдельный терминал
./mediamtx

# Остановить сервер rtsp 
pkill mediamtx

# Запустить трансляцию видео cam.mp4 на отдельном терминале
ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam1

ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://10.6.173.233:8554/cam1

# Запустить необходимое количество видеопотоков в отдельных терминалах, меняя конечный адрес: cam2, cam3 и тд.
ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam2
