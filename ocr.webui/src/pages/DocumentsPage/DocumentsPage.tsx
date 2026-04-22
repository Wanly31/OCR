import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import styles from './DocumentsPage.module.css'
import { getDocumentStream } from '../../api/document.api'

export default function DocumentsPage() {
    const navigate = useNavigate()

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>📄 Документи</h1>
                <p className={styles.subtitle}>
                    Документи прив'язані до пацієнтів. Щоб переглянути документ — знайдіть пацієнта та відкрийте медичну картку.
                </p>
            </div>

            <div className={styles.infoGrid}>
                <div className={styles.infoCard}>
                    <span className={styles.infoIcon}>📤</span>
                    <h3>Завантажте документ</h3>
                    <p>Завантажте PDF або зображення для OCR-розпізнавання</p>
                    <button className={styles.actionBtn} onClick={() => navigate('/upload')}>
                        Завантажити →
                    </button>
                </div>

                <div className={styles.infoCard}>
                    <span className={styles.infoIcon}>👥</span>
                    <h3>Знайдіть пацієнта</h3>
                    <p>Пошук пацієнтів і перегляд усіх їхніх медичних документів</p>
                    <button className={styles.actionBtn} onClick={() => navigate('/patients')}>
                        До пацієнтів →
                    </button>
                </div>

                <div className={styles.infoCard}>
                    <span className={styles.infoIcon}>🔍</span>
                    <h3>Перегляд за ID</h3>
                    <p>Відкрийте документ безпосередньо, знаючи його ідентифікатор</p>
                    <DocumentByIdForm />
                </div>
            </div>
        </div>
    )
}

function DocumentByIdForm() {
    const [docId, setDocId] = useState('')
    const handleOpen = async () => {
        if (!docId.trim()) return
        try {
            const blob = await getDocumentStream(docId.trim())
            const url = URL.createObjectURL(blob)
            window.open(url, '_blank')
        } catch (error) {
            console.error('Failed to load document stream', error)
        }
    }
    return (
        <div className={styles.docIdForm}>
            <input
                type="text"
                value={docId}
                onChange={e => setDocId(e.target.value)}
                placeholder="UUID документа"
            />
            <button className={styles.actionBtn} onClick={handleOpen} disabled={!docId.trim()}>
                Відкрити →
            </button>
        </div>
    )
}
