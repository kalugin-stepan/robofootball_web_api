import config from './config.json'

function get_id_and_token(): [number, string] {
    const id_str = localStorage.getItem('id')
    if (id_str === null) return [0, '']
    const token = localStorage.getItem('token')
    if (token === null) return [0, '']
    const id = parseInt(id_str)
    if (id === 0) return [0, '']
    return [id, token]
}

export async function Login(username: string, password: string): Promise<boolean> {
    const formData = new FormData()
    formData.append('username', username)
    formData.append('password', password)
    try {
        const res = await fetch(`${config.api_url}/login`, {method: 'POST', body: formData})
        if (!res.ok) return false
        const data = await res.json()
        localStorage.setItem('id', data.id)
        localStorage.setItem('token', data.token)
        return true
    } catch (err) {
        return false
    }
}

export async function Register(username: string, password: string) {
    const formData = new FormData()
    formData.append('username', username)
    formData.append('password', password)
    try {
        const res = await fetch(`${config.api_url}/register`, {method: 'POST', body: formData})
        return res.ok
    } catch {
        return false
    }
}

export async function IsLoggedIn(): Promise<boolean> {
    const [id, token] = get_id_and_token()
    if (id === 0 || token === '') return false
    const formData = new FormData()
    formData.append('id', id.toString())
    formData.append('token', token)
    try {
        const rez = await fetch(`${config.api_url}/is_token_valid`, {method: 'POST', body: formData})
        return rez.ok
    } catch (e) {
        console.log(e)
        return false
    }
}