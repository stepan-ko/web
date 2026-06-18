# Запустить сервер rtsp через терминал
./mediamtx

# Остановить сервер rtsp 
pkill mediamtx

# Запустить трансляцию видео cam.mp4
ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam1

# Запустить необходимое количество видеопотоков меняя конечный адрес: cam2, cam3 и тд.
ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam2
