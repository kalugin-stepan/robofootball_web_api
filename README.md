# Web-API для управления доступом к топикам MQTT Broker'а

## Установка

### Установка .NET Core

`https://docs.microsoft.com/ru-ru/dotnet/core/install`

### Клонирование

```bash
    git clone https://github.com/kalugin-stepan/robofootball_web_api
    cd robofootball_web_api
```

### Настройка

В файле `appsettings.json` изменяем полe `ConnectionStrings.default`,
вписываем в него сроку подключения к MySQL, и изменяем поле `secret`,
вписываем в него секрет, ограничивающий досту к командам сервера.

### сборка

```bash
    dotnet tool install --global dotnet-ef
    dotnet ef database update
    dotnet build -c Release
```

### Размещение

`https://docs.microsoft.com/ru-ru/aspnet/core/host-and-deploy/?view=aspnetcore-6.0`

Файл с собранным проектрм находится здесь -> `./bin/Release/net6.0/robofootball_web_api.dll`
