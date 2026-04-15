import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getPatientById, getPatientHistory } from '../../api/patient.api'
import type { PatientDetail, MedicalRecord } from '../../api/patient.api'
import styles from './PatientDetailPage.module.css'

export default function PatientDetailPage() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()

    const [patient, setPatient] = useState<PatientDetail | null>(null)
    const [history, setHistory] = useState<MedicalRecord[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [activeRecord, setActiveRecord] = useState<MedicalRecord | null>(null)

    useEffect(() => {
        if (!id) return
        setLoading(true)
        Promise.all([
            getPatientById(id),
            getPatientHistory(id),
        ])
            .then(([p, h]) => {
                setPatient(p)
                setHistory(h)
            })
            .catch(() => setError('Не вдалося завантажити дані пацієнта'))
            .finally(() => setLoading(false))
    }, [id])

    if (loading) {
        return (
            <div className={styles.center}>
                <div className={styles.spinner} />
                <p>Завантаження...</p>
            </div>
        )
    }

    if (error || !patient) {
        return (
            <div className={styles.center}>
                <span style={{ fontSize: '2.5rem' }}>⚠️</span>
                <p className={styles.errorText}>{error || 'Пацієнта не знайдено'}</p>
                <button className={styles.backBtn} onClick={() => navigate('/patients')}>← Назад</button>
            </div>
        )
    }

    const formatDate = (d?: string | null) => {
        if (!d) return '—'
        try { return new Date(d).toLocaleDateString('uk-UA') } catch { return d }
    }

    return (
        <div className={styles.page}>
            <div className={styles.topBar}>
                <button className={styles.backBtn} onClick={() => navigate('/patients')}>
                    ← Назад
                </button>
            </div>

            {/* Картка пацієнта */}
            <div className={styles.profileCard}>
                <div className={styles.avatar}>
                    {patient.FirstName[0]?.toUpperCase()}{patient.LastName?.[0]?.toUpperCase()}
                </div>
                <div className={styles.profileInfo}>
                    <h1 className={styles.profileName}>
                        {patient.FirstName} {patient.LastName || ''}
                    </h1>
                    <div className={styles.profileMeta}>
                        {patient.BirthDate && (
                            <span className={styles.metaChip}>
                                📅 {formatDate(patient.BirthDate)}
                            </span>
                        )}
                        <span className={styles.metaChip}>
                            📋 {patient.TotalRecords} {recordsLabel(patient.TotalRecords)}
                        </span>
                    </div>
                </div>
            </div>

            {/* Медична історія */}
            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>⚕️ Медична історія</h2>

                {history.length === 0 ? (
                    <div className={styles.empty}>
                        <span>📭</span>
                        <p>Записів ще немає</p>
                    </div>
                ) : (
                    <div className={styles.recordsList}>
                        {history.map(rec => (
                            <div
                                key={rec.Id}
                                className={`${styles.recordCard} ${activeRecord?.Id === rec.Id ? styles.active : ''}`}
                                onClick={() => setActiveRecord(activeRecord?.Id === rec.Id ? null : rec)}
                            >
                                <div className={styles.recordHeader}>
                                    <div className={styles.recordDate}>
                                        {rec.DateDocument ? formatDate(rec.DateDocument) : '—'}
                                    </div>
                                    <div className={styles.recordExcerpt}>
                                        {rec.Examination
                                            ? rec.Examination.slice(0, 80) + (rec.Examination.length > 80 ? '...' : '')
                                            : 'Без опису'}
                                    </div>
                                    <span className={styles.chevron}>
                                        {activeRecord?.Id === rec.Id ? '▲' : '▼'}
                                    </span>
                                </div>

                                {activeRecord?.Id === rec.Id && (
                                    <div className={styles.recordBody}>
                                        <RecordField label="Обстеження / Анамнез" value={rec.Examination} />
                                        <RecordField label="Призначене лікування" value={rec.Treatment} />
                                        <RecordField label="Препарати" value={rec.Medicine} />
                                        <RecordField label="Протипоказані ліки" value={rec.ContraindicatedMedicine} />
                                        <RecordField label="Причина протипоказання" value={rec.ContraindicatedReason} />
                                        {rec.CreatedAt && (
                                            <RecordField
                                                label="Дата запису"
                                                value={formatDate(rec.CreatedAt)}
                                            />
                                        )}
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    )
}

function RecordField({ label, value }: { label: string; value?: string | null }) {
    if (!value) return null
    return (
        <div className={styles.recordField}>
            <span className={styles.fieldLabel}>{label}</span>
            <span className={styles.fieldValue}>{value}</span>
        </div>
    )
}

function recordsLabel(count: number) {
    if (count === 1) return 'запис'
    if (count >= 2 && count <= 4) return 'записи'
    return 'записів'
}
