# Браузерная консоль оператора

## Установка

* [Полноценная установка](#полноценная-установка)
* [Установка для ленивых](#установка-для-ленивых)

### Полноценная установка

#### Установка Node.js и yarn

`https://nodejs.org/en`

```bash
    npm i -g yarn
```

#### Клонирование проекта и настройка конфига

```bash
    git clone https://github.com/kalugin-stepan/robofootball_web_api
    cd robofootball_web_api/ClientApp
```

В файле `src/config.json`, в поле `api_url`, вписываем ссылку на API-сервер,
а в полe `mqtt_url`, ссылку на MQTT Broker.

#### Установка зависимостей и сбрка проекта

```bash
    yarn
    yarn build
```

#### Размещение

Содержимое файла `robofootball_web_api/ClientApp/dist` - самостоятельное, не требующее зависимостей одностроничное приложение.
Его, например, можно разместить на Apache сервере и оно будет работать.

### Установка для ленивых

В файле `robofootball_web_api/ClientApp/dist/assets/index.*.js`
заменяем строку `http://localhost:5213/api` на ссылку на API-сервер,
а строку `ws://broker.emqx.io:8083/mqtt` на ссылку на MQTT Broker.

После этого размещаем полученые файлы [размещение](#размещение).
