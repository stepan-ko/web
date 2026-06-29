# Для  новой системы:
sudo apt update
sudo apt install dotnet-sdk-10.0

# Установка основного пакета EF Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design

# Установка провайдера для SQLite
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Установка инструментов EF Core (для миграций)
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

# Если пути нет, добавьте его в файл конфигурации оболочки:
export PATH="$PATH:$HOME/.dotnet/tools"

# Обновить пакеты
sudo apt update

# Установка OpenCv
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.official.runtime.linux-x64

# Установка libopencv
sudo apt install libopencv-dev libopencv-videoio-dev

# Дополнительно установите системные зависимости, которые могут отсутствовать
sudo apt install libatomic1 libgtk-3-0 libgl1 libglib2.0-0
sudo apt install ffmpeg

# Установите PostgreSQL и пакет расширений postgresql-contrib
sudo apt install postgresql postgresql-contrib -y

# Запустить сервис
sudo systemctl start postgresql

# Сделать автозапуск сервиса
sudo systemctl enable postgresql

# Подключиться к PostgreSQL
sudo -u postgres psql

# Задать новый пароль 12345
ALTER USER postgres WITH PASSWORD '12345';

# Выйти в терминал
exit

# Создать миграцию (если менялась модель данных для БД)
dotnet ef migrations add NewMigration

# Создать базу данных (сначала задать пароль для postgres)
dotnet ef database update

# Пересобьрать проект
dotnet clean
dotnet build


# перед стартом
export LD_LIBRARY_PATH=$PWD/bin/Debug/net10.0/runtimes/linux-x64/native:$LD_LIBRARY_PATH

# Запустить проект
dotnet run

_________________________

Трансляция потока RTSP описана в папке rtsp
________________________
