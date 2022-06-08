# Примеры клиентского приложения

```py
import requests
import paho.mqtt.client as mqtt_client

url = 'http://localhost:5213/api'

mqtt_host = 'localhost'

username = 'username'
password = 'password'

def register(username: str, password: str) -> bool:
    res = requests.post(f'{url}/register', {'username': username, 'password': password})
    return res.status_code == 200

def login(username: str, password: str) -> tuple[int, str] | None:
    res = requests.post(f'{url}/login', {'username': username, 'password': password})
    if res.status_code == 400:
        return None
    try:
        data: dict = res.json()
        if 'id' in data and 'token' in data:
            return data['id'], data['token']
        return None
    except requests.JSONDecodeError:
        return None

def logout(id: int, token: str) -> bool:
    res = requests.post(f'{url}/logout', {'id': id, 'token': token})
    return res.status_code == 200

global connected
connected = False

def on_connect(client, userdata, flags, rc):
    global connected
    if rc == 0:
        print('connected')
        connected = True
        return
    print('not connected')

if __name__ == '__main__':
    data = login(username, password)
    if data == None:
        print('username or password is not valid')
        exit()
    client = mqtt_client.Client()
    client.username_pw_set(str(data[0]), data[1])
    client.on_connect = on_connect
    client.connect(mqtt_host)
    while not connected:
        client.loop()
    logout(data[0], data[1])
```
