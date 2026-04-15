import { useState, useEffect, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { saveMedicalRecord } from '../../api/ocr.api'
import type { OcrResult, RecognizedDataDto } from '../../types/ocr.types'
import styles from './ReviewPage.module.css'

export default function ReviewPage() {
    const location = useLocation()
    const navigate = useNavigate()

    const result = location.state?.result as OcrResult | undefined

    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')

    const [patientInfo, setPatientInfo] = useState({
        FirstName: result?.RecognizeData?.FirstName || '',
        LastName: result?.RecognizeData?.LastName || '',
        BirthDate: result?.RecognizeData?.BirthDate || ''
    })

    const [medicalData, setMedicalData] = useState<RecognizedDataDto>({
        Examination: result?.RecognizeData?.Examination || '',
        Medicine: result?.RecognizeData?.Medicine || '',
        Treatment: result?.RecognizeData?.Treatment || '',
        ContraindicatedMedicine: result?.RecognizeData?.ContraindicatedMedicine || '',
        ContraindicatedReason: result?.RecognizeData?.ContraindicatedReason || '',
        DateDocument: result?.RecognizeData?.DateDocument || ''
    })

    const [selectedPatientId, setSelectedPatientId] = useState<string>('')

    useEffect(() => {
        if (!result) {
            navigate('/upload')
        }
    }, [result, navigate])

    if (!result) return null;

    const fileName = result.FilePath ? result.FilePath.split('\\').pop()?.split('/').pop() : ''
    const documentUrl = fileName ? `/Documents/${fileName}` : ''

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()
        setLoading(true)
        setError('')

        try {
            await saveMedicalRecord({
                ExistingPatientId: selectedPatientId || undefined,
                FirstName: patientInfo.FirstName,
                LastName: patientInfo.LastName || undefined,
                BirthDate: patientInfo.BirthDate || undefined,
                RecognizedId: result.RecognizedId,
                RecognizedData: {
                    Examination: medicalData.Examination || undefined,
                    Medicine: medicalData.Medicine || undefined,
                    Treatment: medicalData.Treatment || undefined,
                    ContraindicatedMedicine: medicalData.ContraindicatedMedicine || undefined,
                    ContraindicatedReason: medicalData.ContraindicatedReason || undefined,
                    DateDocument: medicalData.DateDocument || undefined
                }
            })
            navigate('/patients')
        } catch (err: any) {
            console.error("Save error:", err.response?.data);
            const errData = err.response?.data;
            let msg = 'Помилка при збереженні';
            if (errData?.errors) {
                msg = Object.values(errData.errors).flat().join(', ');
            } else if (errData?.message || errData?.title) {
                msg = errData.message || errData.title;
            }
            setError(msg);
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>Перевірка OCR Розпізнавання</h1>
                <p className={styles.subtitle}>Перевірте та відредагуйте витягнуті дані перед збереженням у систему (Human-in-the-loop)</p>
            </div>

            {error && <div className={styles.errorBanner}>{error}</div>}

            <div className={styles.twoColumns}>
                {/* ЛІВА КОЛОНКА: Оригінальний файл */}
                <div className={styles.documentPanel}>
                    <h2 className={styles.sectionTitle}>📄 Оригінальний документ</h2>
                    <div className={styles.iframeContainer}>
                        {documentUrl ? (
                            <iframe 
                                src={documentUrl} 
                                title="Original Document" 
                                className={styles.iframe}
                            />
                        ) : (
                            <div className={styles.noDocument}>Документ не знайдено</div>
                        )}
                    </div>
                </div>

                {/* ПРАВА КОЛОНКА: Форма редагування */}
                <div className={styles.formPanel}>
                    <form onSubmit={handleSubmit} className={styles.form}>
                        
                        {/* Блок 1: Пацієнт */}
                        <div className={styles.formSection}>
                            <h2 className={styles.sectionTitle}>👤 Дані пацієнта</h2>

                            {/* Схожі пацієнти, якщо є */}
                            {result.SimilarPatients && result.SimilarPatients.length > 0 && (
                                <div className={styles.similarPatients}>
                                    <label>Або виберіть існуючого пацієнта з бази:</label>
                                    <select 
                                        value={selectedPatientId} 
                                        onChange={e => setSelectedPatientId(e.target.value)}
                                        className={styles.select}
                                    >
                                        <option value="">-- Створити нового пацієнта --</option>
                                        {result.SimilarPatients.map(p => (
                                            <option key={p.Id} value={p.Id}>
                                                {p.FirstName} {p.LastName} ({p.BirthDate}) - {p.RecordCount} записів
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            )}

                            {/* Поля створюваного пацієнта (блокуються якщо вибрано існуючого) */}
                            <div className={styles.grid2}>
                                <div className={styles.field}>
                                    <label>Ім'я *</label>
                                    <input 
                                        required 
                                        type="text" 
                                        disabled={!!selectedPatientId}
                                        value={patientInfo.FirstName}
                                        onChange={e => setPatientInfo({...patientInfo, FirstName: e.target.value})}
                                    />
                                </div>
                                <div className={styles.field}>
                                    <label>Прізвище</label>
                                    <input 
                                        type="text" 
                                        disabled={!!selectedPatientId}
                                        value={patientInfo.LastName}
                                        onChange={e => setPatientInfo({...patientInfo, LastName: e.target.value})}
                                    />
                                </div>
                            </div>
                            <div className={styles.field}>
                                <label>Дата народження (YYYY-MM-DD)</label>
                                <input 
                                    type="date"
                                    disabled={!!selectedPatientId}
                                    value={patientInfo.BirthDate}
                                    onChange={e => setPatientInfo({...patientInfo, BirthDate: e.target.value})}
                                />
                            </div>
                        </div>

                        {/* Блок 2: Медичні дані */}
                        <div className={styles.formSection}>
                            <h2 className={styles.sectionTitle}>⚕️ Медичні дані</h2>

                            <div className={styles.field}>
                                <label>Дата документу</label>
                                <input 
                                    type="date"
                                    value={medicalData.DateDocument}
                                    onChange={e => setMedicalData({...medicalData, DateDocument: e.target.value})}
                                />
                            </div>

                            <div className={styles.field}>
                                <label>Обстеження/Анамнез</label>
                                <textarea 
                                    rows={3}
                                    value={medicalData.Examination}
                                    onChange={e => setMedicalData({...medicalData, Examination: e.target.value})}
                                />
                            </div>

                            <div className={styles.field}>
                                <label>Призначене лікування</label>
                                <textarea 
                                    rows={2}
                                    value={medicalData.Treatment}
                                    onChange={e => setMedicalData({...medicalData, Treatment: e.target.value})}
                                />
                            </div>

                            <div className={styles.field}>
                                <label>Препарати</label>
                                <input 
                                    type="text"
                                    value={medicalData.Medicine}
                                    onChange={e => setMedicalData({...medicalData, Medicine: e.target.value})}
                                />
                            </div>

                            <div className={styles.grid2}>
                                <div className={styles.field}>
                                    <label>Протипоказані ліки</label>
                                    <input 
                                        type="text"
                                        value={medicalData.ContraindicatedMedicine}
                                        onChange={e => setMedicalData({...medicalData, ContraindicatedMedicine: e.target.value})}
                                    />
                                </div>
                                <div className={styles.field}>
                                    <label>Причина протипоказання</label>
                                    <input 
                                        type="text"
                                        value={medicalData.ContraindicatedReason}
                                        onChange={e => setMedicalData({...medicalData, ContraindicatedReason: e.target.value})}
                                    />
                                </div>
                            </div>
                        </div>

                        <button type="submit" disabled={loading} className={styles.submitBtn}>
                            {loading ? "Збереження..." : "💾 Підтвердити та Зберегти"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    )
}