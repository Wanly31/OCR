import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { searchPatients } from '../../api/patient.api'
import type { SearchPatientResult, PaginatedResult } from '../../api/patient.api'
import styles from './PatientsPage.module.css'

export default function PatientsPage() {
    const navigate = useNavigate()

    const [firstName, setFirstName] = useState('')
    const [lastName, setLastName] = useState('')
    const [birthDate, setBirthDate] = useState('')
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')
    const [result, setResult] = useState<PaginatedResult<SearchPatientResult> | null>(null)
    const [page, setPage] = useState(1)
    const PAGE_SIZE = 10

    const doSearch = async (p: number = 1) => {
        if (!firstName.trim()) {
            setError("Введіть ім'я для пошуку")
            return
        }
        setLoading(true)
        setError('')
        try {
            const data = await searchPatients({
                FirstName: firstName,
                LastName: lastName || undefined,
                BirthDate: birthDate || undefined,
                Page: p,
                PageSize: PAGE_SIZE,
            })
            setResult(data)
            setPage(p)
        } catch {
            setError('Помилка запиту')
        } finally {
            setLoading(false)
        }
    }

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault()
        doSearch(1)
    }

    const totalPages = result ? Math.ceil(result.TotalCount / PAGE_SIZE) : 1

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>👥 Пацієнти</h1>
                <p className={styles.subtitle}>Пошук пацієнтів за ім'ям, прізвищем та датою народження</p>
            </div>

            {/* Форма пошуку */}
            <div className={styles.searchCard}>
                <form onSubmit={handleSubmit} className={styles.searchForm}>
                    <div className={styles.searchRow}>
                        <div className={styles.field}>
                            <label>Ім'я *</label>
                            <input
                                type="text"
                                value={firstName}
                                placeholder="Іван"
                                onChange={e => setFirstName(e.target.value)}
                                required
                            />
                        </div>
                        <div className={styles.field}>
                            <label>Прізвище</label>
                            <input
                                type="text"
                                value={lastName}
                                placeholder="Петренко"
                                onChange={e => setLastName(e.target.value)}
                            />
                        </div>
                        <div className={styles.field}>
                            <label>Дата народження</label>
                            <input
                                type="date"
                                value={birthDate}
                                onChange={e => setBirthDate(e.target.value)}
                            />
                        </div>
                        <button type="submit" className={styles.searchBtn} disabled={loading}>
                            {loading ? '⏳' : '🔍'} Шукати
                        </button>
                    </div>
                </form>
            </div>

            {error && <div className={styles.error}>{error}</div>}

            {/* Результати */}
            {result && (
                <>
                    <div className={styles.resultsHeader}>
                        <span className={styles.totalBadge}>
                            Знайдено: <strong>{result.TotalCount}</strong>
                        </span>
                    </div>

                    {result.Items.length === 0 ? (
                        <div className={styles.empty}>
                            <span>🔍</span>
                            <p>Пацієнтів не знайдено</p>
                        </div>
                    ) : (
                        <div className={styles.tableWrapper}>
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>Ім'я</th>
                                        <th>Прізвище</th>
                                        <th>Дата народження</th>
                                        <th>Записів</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {result.Items.map(p => (
                                        <tr key={p.Id} className={styles.row}>
                                            <td className={styles.nameCell}>
                                                <span className={styles.avatar}>
                                                    {p.FirstName[0]?.toUpperCase()}
                                                </span>
                                                {p.FirstName}
                                            </td>
                                            <td>{p.LastName || '—'}</td>
                                            <td>{p.BirthDate || '—'}</td>
                                            <td>
                                                <span className={styles.recordsBadge}>
                                                    {p.TotalRecords}
                                                </span>
                                            </td>
                                            <td>
                                                <button
                                                    className={styles.viewBtn}
                                                    onClick={() => navigate(`/patients/${p.Id}`)}
                                                >
                                                    Переглянути →
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}

                    {/* Пагінація */}
                    {totalPages > 1 && (
                        <div className={styles.pagination}>
                            <button
                                className={styles.pageBtn}
                                disabled={page <= 1}
                                onClick={() => doSearch(page - 1)}
                            >
                                ← Попередня
                            </button>
                            <span className={styles.pageInfo}>
                                Сторінка {page} з {totalPages}
                            </span>
                            <button
                                className={styles.pageBtn}
                                disabled={page >= totalPages}
                                onClick={() => doSearch(page + 1)}
                            >
                                Наступна →
                            </button>
                        </div>
                    )}
                </>
            )}
        </div>
    )
}
