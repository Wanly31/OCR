import {useState, type FormEvent} from 'react'
import { useNavigate, Link } from 'react-router-dom'
import {useAuth} from '../../context/AuthContext'
import styles from './RegisterPage.module.css'

export default function RegisterPage(){
    const navigate = useNavigate()
    const {register} = useAuth()

    const [username, setUsername] = useState('')
    const[password, setPassword] = useState('')
    const[confirmPassword, setConfirmPassword] = useState('')
    const[error, setError] = useState('')
    const[loading, setLoading] = useState(false)


    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()
        setError('')
        setLoading(true)
    
    if (password !== confirmPassword) {
      setError('Паролі не співпадають')
      setLoading(false)
      return
    }
    if (password.length < 6) {
      setError('Пароль повинен бути не менше 6 символів')
      setLoading(false)
      return
    }
    try{
        await register({username, password})
        navigate('/')
    }
    catch(err: any){
        setError(err.response?.data?.message ?? 'Помилка реєстрації')
    }
    finally{
        setLoading(false)
    }
    }

    return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.header}>
          <h1 className={styles.title}>Реєстрація</h1>
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
              placeholder="Мінімум 6 символів"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
            />
          </div>
          <div className={styles.field}>
            <label htmlFor="confirmPassword">Підтвердити пароль</label>
            <input
              id="confirmPassword"
              type="password"
              placeholder="Повторіть пароль"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              required
            />
          </div>
          {error && <p className={styles.error}>{error}</p>}
          <button type="submit" className={styles.button} disabled={loading}>
            {loading ? 'Реєстрація...' : 'Зареєструватись'}
          </button>
        </form>
        <p className={styles.link}>
          Вже є акаунт? <Link to="/login">Увійти</Link>
        </p>
      </div>
    </div>
  )
}