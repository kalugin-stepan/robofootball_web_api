import { useCallback, useEffect, useState } from 'react'
import { Joystick } from 'react-joystick-component'
import { Navigate } from 'react-router-dom'
import Robot from './Robot'
import Vector from './Vector'
import { IsLoggedIn } from '../../Auth'
import config from '../../config.json'
import { IJoystickUpdateEvent } from 'react-joystick-component/build/lib/Joystick'
import * as mqtt from 'mqtt/dist/mqtt.min'

function get_user_id(): number {
    const data = localStorage.getItem('id')
    if (data === null) return 0
    const id = parseInt(data)
    return id
}

function get_token() {
    return localStorage.getItem('token')
}

function get_joystick_size(): number {
    const w = window.innerWidth
    || document.documentElement.clientWidth
    || document.body.clientWidth
    const h = window.innerHeight
    || document.documentElement.clientHeight
    || document.body.clientHeight
    if (w > h) {
        return w * 0.3
    }
    return w * 0.6
}

function indexof_by_robot_name(robots: Robot[], robot_name: string) {
    for (let i = 0; i < robots.length; i++) {
        if (robots[i].name === robot_name) return i
    }
    return -1
}

let client: mqtt.MqttClient | null = null

let current_direction: Vector = {x: 0, y: 0}

function mqtt_clear(client: mqtt.MqttClient | null) {
    client?.removeAllListeners('message')
    client?.removeAllListeners('error')
    client?.removeAllListeners('disconnect')
    client?.removeAllListeners('end')
    client?.removeAllListeners('close')
}

export default function() {
    const [joystickSize, setJoystickSize] = useState(get_joystick_size())
    const [robots, setRobots] = useState<Robot[]>([])
    const [doRedirectToLogin, setDoRedirectToLogin] = useState(false)
    const [selectedRobotName, setSelectedRobotName] = useState('')

    useEffect(() => {

        function on_window_resize() {
            setJoystickSize(get_joystick_size())
        }
        document.body.onresize = on_window_resize
        return () => {
            document.body.removeEventListener('resize', on_window_resize)
        }
    }, [])

    useEffect(() => {
        const redirect = () => {
            setDoRedirectToLogin(true)
        }

        IsLoggedIn().then(rez => {
            if (!rez) redirect()
        })

        const id = get_user_id()
        const token = get_token()

        if (id === 0 || token === null) {
            redirect()
            return
        }

        try {
            client = mqtt.connect(config.mqtt_url,
                {username: id.toString(), password: token})
        } catch {
            redirect()
        }
        client?.on('error', redirect)
        client?.on('disconnect', redirect)
        client?.on('end', redirect)
        client?.on('close', redirect)
        client?.subscribe(id.toString())

        return () => {
            mqtt_clear(client)
            client = null
        }
    }, [])

    useEffect(() => {
        const id = get_user_id()
        const topic = `${id}/${selectedRobotName}`

        let last_direction = current_direction

        const send_pos_interval = setInterval(() => {
            if (current_direction === last_direction) return
            client?.publish(topic, JSON.stringify(current_direction))
            last_direction = current_direction
        }, 100)
        return () => {
            clearInterval(send_pos_interval)
        }
    }, [selectedRobotName])

    useEffect(() => {
        client?.on('message', (topic, data) => {
            const robot_name: string = data.toString('utf-8')
            const i = indexof_by_robot_name(robots, robot_name)
            if (i !== -1) {
                robots[i].pinged = true;
                return
            };
            robots.push({name: robot_name, pinged: true})
            if (selectedRobotName === '') setSelectedRobotName(robot_name)
            setRobots([...robots])
        })
        
        const ping_interval = setInterval(() => {
            let updated_robots = robots.filter(robot => robot.pinged)
            updated_robots.forEach(robot => robot.pinged = false)
            if (updated_robots.length === robots.length) return
            if (indexof_by_robot_name(updated_robots, selectedRobotName) === -1) {
                if (updated_robots.length === 0) setSelectedRobotName('')
                else {
                    setSelectedRobotName(updated_robots[0].name)
                }
            }
            setRobots(updated_robots)
        }, 5000)
        return () => {
            clearInterval(ping_interval)
            client?.removeAllListeners('message')
        }
    }, [robots, selectedRobotName])

    const on_joystick_move = useCallback((e: IJoystickUpdateEvent) => {
        current_direction = {x: e.x === null ? 0 : Math.round(e.x), y: e.y === null ? 0 : Math.round(e.y)}
    }, [])

    const on_joystick_stop = useCallback(() => {
        current_direction = {x: 0, y: 0}
    }, [])

    return (
        <div>
            <div style={{width: 'min-content', margin: 'auto'}}>
                <Joystick size={joystickSize} start={on_joystick_move} move={on_joystick_move} stop={on_joystick_stop}/>
            </div>
            <select value={selectedRobotName} onChange={(e) => setSelectedRobotName(e.target.value)}>
                {
                    robots.map(robot => {
                        return <option value={robot.name} key={robot.name}>{robot.name}</option>
                    })
                }
            </select>
            {doRedirectToLogin ? <Navigate replace to='/login'/> : ''}
        </div>
    )
}