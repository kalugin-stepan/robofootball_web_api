import { useState } from 'react'
import { Navigate, Link } from 'react-router-dom'
import { Register } from '../../Auth'

export default function() {
    const [username, setUsername] = useState<string>('')
    const [password, setPassword] = useState<string>('')
    const [isRegistered, setIsRegistred] = useState(false)

    async function doRegister() {
        const rez = await Register(username, password)
        if (!rez) return
        setIsRegistred(true)
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
                <button onClick={doRegister}>Register</button>
            </div>
            <Link replace to='/login'>Login</Link>
            {isRegistered ? <Navigate replace to='/'/> : ''}
        </div>
    )
}