
## Описание функционала системы

Система предназначена для:

* Запуска сервера и анализа видеопотока с целью распознавания автомобильных номеров.
* Сохранения найденных данных в базу данных PostgreSQL.
* Редактирования параметров распознавания через web-страницу сервера.
* Просмотра видеопотока, который прошёл обработку распознаванием.

---

## Визуализация интерфейса сервиса

Ниже представлены ключевые экраны системы управления видеопотоками. Нажмите на ссылку, чтобы открыть изображение.

[Экран 1: Список активных видеопотоков. Общий дашборд со статусом каждого источника](Result/StreamList.PNG)

***

[Экран 2: Настройки параметров распознавания. Конфигурация OCR‑порогов, выбор регионов и типа номерных знаков](Result/StreamConfig.PNG)

***

[Экран 3: Результат обработки видео. Просмотр потока в реальном времени с наложенными рамками и распознанным текстом](Result/StreamResult.PNG)

---

## Процесс запуска

### 1. Запуск тестового видеопотока

Для организации тестового видеопотока необходимо выполнить следующие шаги в отдельных терминалах:

1. **Запустить сервер RTSP** из папки `test/rtsp`:
   ```bash
   ./mediamtx
   ```

2. **Запустить трансляцию видео `cam.mp4`** из папки `test/rtsp`:
   * Вариант для локального адреса:
     ```bash
     ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam1
     ```
   * Вариант для сетевого адреса:
     ```bash
     ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://10.6.173.233:8554/cam1
     ```

3. **Запустить необходимое количество видеопотоков** (с разными идентификаторами), меняя конечный адрес на `cam2`, `cam3` и т. д.:
   ```bash
   ffmpeg -re -stream_loop -1 -i cam.mp4 -c:v libx264 -preset ultrafast -tune zerolatency -f rtsp -rtsp_transport tcp rtsp://localhost:8554/cam2
   ```

---

### 2. Запуск проекта

#### Подготовка окружения и установка зависимостей

1. Для новой системы обновить репозитории и установить **.NET SDK 10.0**:
   ```bash
   sudo apt update
   sudo apt install dotnet-sdk-10.0
   ```

2. Установить основной пакет **EF Core**:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.Design
   ```

3. Установить провайдер для **PostgreSQL**:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

4. Установить инструменты **EF Core** (для миграций):
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet tool update --global dotnet-ef
   ```

5. Если пути нет, добавить его в файл конфигурации оболочки:
   ```bash
   export PATH="$PATH:$HOME/.dotnet/tools"
   ```

6. Повторно обновить пакеты:
   ```bash
   sudo apt update
   ```

7. Установить **OpenCv**:
   ```bash
   dotnet add package OpenCvSharp4
   dotnet add package OpenCvSharp4.official.runtime.linux-x64
   ```

8. Установить библиотеку **libopencv**:
   ```bash
   sudo apt install libopencv-dev libopencv-videoio-dev
   ```

9. Дополнительно установить системные зависимости, которые могут отсутствовать:
   ```bash
   sudo apt install libatomic1 libgtk-3-0 libgl1 libglib2.0-0
   sudo apt install ffmpeg
   ```

10. Установить **PostgreSQL** и пакет расширений **postgresql-contrib**:
    ```bash
    sudo apt install postgresql postgresql-contrib -y
    ```

#### Настройка и запуск PostgreSQL

1. Запустить сервис:
   ```bash
   sudo systemctl start postgresql
   ```

2. Сделать автозапуск сервиса:
   ```bash
   sudo systemctl enable postgresql
   ```

3. Подключиться к **PostgreSQL**:
   ```bash
   sudo -u postgres psql
   ```

4. Задать новый пароль для пользователя `postgres` (значение: `12345`):
   ```sql
   ALTER USER postgres WITH PASSWORD '12345';
   ```

5. Выйти в терминал:
   ```bash
   exit
   ```

#### Миграции, сборка и запуск проекта

1. Создать миграцию (если менялась модель данных для БД):
   ```bash
   dotnet ef migrations add NewMigration
   ```

2. Создать базу данных (предварительно задав пароль для `postgres`):
   ```bash
   dotnet ef database update
   ```

3. Пересобрать проект:
   ```bash
   dotnet clean
   dotnet build
   ```

4. Добавить пути к библиотекам:
   ```bash
   export LD_LIBRARY_PATH=/home/step/PROJECT/web/av/lib/:$LD_LIBRARY_PATH
   ```

5. Перед стартом задать путь к нативным библиотекам в текущей директории:
   ```bash
   export LD_LIBRARY_PATH=$PWD/bin/Debug/net10.0/runtimes/linux-x64/native:$LD_LIBRARY_PATH
   ```

6. Запустить проект:
   ```bash
   dotnet run
   ```
