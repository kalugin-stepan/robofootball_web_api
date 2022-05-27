import { useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { Login } from '../../Auth'

export default function() {
    const [username, setUsername] = useState<string>('')
    const [password, setPassword] = useState<string>('')
    const [isLoggedIn, setIsLoggedIn] = useState(false)

    async function doLogin() {
        const rez = await Login(username, password)
        if (!rez) return
        setIsLoggedIn(true)
    }

    return (
        <div>
            <div>
                <input placeholder='Username' value={username} onChange={(e) => setUsername(e.target.value)}/>
            </div>
            <div>
                <input type='password' placeholder='Password' value={password} onChange={(e) => setPassword(e.target.value)}/>
            </div>
            <div>
                <button onClick={doLogin}>Login</button>
            </div>
            <Link replace to='/register'>Register</Link>
            {isLoggedIn ? <Navigate replace to='/'/> : ''}
        </div>
    )
}