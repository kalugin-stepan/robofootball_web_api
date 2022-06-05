import requests

url = 'http://localhost:5213/api'

def login(username: str, password: str):
    res = requests.post(f'{url}/login', {'username': username, 'password': password})
    if res.status_code == 200:
        return res.json()

def logout(id: int, token: str) -> bool:
    return requests.post(f'{url}/logout', {'id': id, 'token': token}).status_code == 200


data = login('user1', '12345678')
print(data)
logout(data['id'], data['token'])