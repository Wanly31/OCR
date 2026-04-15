import {useState, type FormEvent} from 'react'
import { useNavigate, Link } from 'react-router-dom'
import {useAuth} from '../../context/AuthContext'
import styles from './LoginPage.module.css'

export default function LoginPage(){
    const navigate = useNavigate()
    const {login} = useAuth()

    const [username, setUsername] = useState('')
    const[password, setPassword] = useState('')
    const[error, setError] = useState('')
    const[loading, setLoading] = useState(false)

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()
        setError('')
        setLoading(true)

        try{
            await login({username, password})
            navigate('/')
        }
        catch (err: any) {
            setError(err.response?.data?.message ?? 'Невірний логін або пароль')
        }
        finally{
            setLoading(false)
        }
    }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.header}>
          <h1 className={styles.title}>Вхід в систему</h1>
          <p className={styles.subtitle}>OCR Medical Document System</p>
        </div>
        <form onSubmit={handleSubmit} className={styles.form}>
          <div className={styles.field}>
            <label htmlFor="username">Логін</label>
            <input
              id="username"
              type="text"
              placeholder="Введіть логін"
              value={username}
              onChange={e => setUsername(e.target.value)}
              required
            />
          </div>
          <div className={styles.field}>
            <label htmlFor="password">Пароль</label>
            <input
              id="password"
              type="password"
              placeholder="Введіть пароль"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
            />
          </div>
          {error && <p className={styles.error}>{error}</p>}
          <button
            type="submit"
            className={styles.button}
            disabled={loading}
          >
            {loading ? 'Вхід...' : 'Увійти'}
          </button>
        </form>
        <p className={styles.link}>
          Немає акаунту? <Link to="/register">Зареєструватись</Link>
        </p>
      </div>
    </div>
  )
}