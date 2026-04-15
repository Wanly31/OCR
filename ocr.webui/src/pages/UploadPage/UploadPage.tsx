import { useState, type FormEvent, type ChangeEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { performOcr } from '../../api/ocr.api'
import styles from './UploadPage.module.css'

export default function UploadPage() {
    const navigate = useNavigate()

    const [file, setFile] = useState<File | null>(null)
    const [fileName, setFileName] = useState<string>('')
    const [fileDescription, setFileDescription] = useState<string>('')
    const [error, setError] = useState<string>('')
    const [loading, setLoading] = useState<boolean>(false)

    const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
        const selected = e.target.files?.[0] ?? null
        setFile(selected)
        if (selected) setFileName(selected.name)
    }

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()
        if (!file) {
            setError('Оберіть файл')
            return
        }
        setError('')
        setLoading(true)

        try {
            const result = await performOcr({
                File: file,
                Filename: fileName,
                FileDescription: fileDescription || undefined,
            })

            navigate('/review', { state: { result } })
        } catch (err: any) {
            setError(err.response?.data?.message ?? 'Помилка розпізнавання')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className={styles.page}>
            <div className={styles.card}>
                <h1 className={styles.title}>Завантажити документ</h1>
                <p className={styles.subtitle}>OCR розпізнає медичні дані автоматично</p>

                <form onSubmit={handleSubmit} className={styles.form}>
                    <div className={styles.field}>
                        <label htmlFor="file">Файл (PDF, JPG, PNG)</label>
                        <input
                            id="file"
                            type="file"
                            accept=".pdf,.jpg,.jpeg,.png"
                            onChange={handleFileChange}
                            required
                        />
                    </div>

                    <div className={styles.field}>
                        <label htmlFor="fileName">Назва документу</label>
                        <input
                            id="fileName"
                            type="text"
                            value={fileName}
                            onChange={e => setFileName(e.target.value)}
                            placeholder="Назва документу"
                            required
                        />
                    </div>

                    <div className={styles.field}>
                        <label htmlFor="fileDescription">Опис (необов'язково)</label>
                        <input
                            id="fileDescription"
                            type="text"
                            value={fileDescription}
                            onChange={e => setFileDescription(e.target.value)}
                            placeholder="Опис документу"
                        />
                    </div>

                    {error && <p className={styles.error}>{error}</p>}

                    <button type="submit" className={styles.button} disabled={loading}>
                        {loading ? 'Розпізнавання...' : 'Завантажити та розпізнати'}
                    </button>
                </form>
            </div>
        </div>
    )
}