import requests

rez = requests.post('http://localhost:5213/user/add', {'username': 'bot3', 'password': '123'}, headers={'secret': '123'})

print(rez.status_code)