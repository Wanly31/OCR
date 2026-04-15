import { useNavigate } from 'react-router-dom'
import styles from './DashboardPage.module.css'

const cards = [
    {
        icon: '📤',
        title: 'Завантажити документ',
        description: 'Завантажте медичний документ для OCR-розпізнавання',
        to: '/upload',
        accent: '#6366f1',
    },
    {
        icon: '👥',
        title: 'Пацієнти',
        description: 'Пошук та перегляд пацієнтів і їх медичної історії',
        to: '/patients',
        accent: '#06b6d4',
    },
    {
        icon: '📄',
        title: 'Документи',
        description: 'Перегляд та управління завантаженими файлами',
        to: '/documents',
        accent: '#10b981',
    },
]

export default function DashboardPage() {
    const navigate = useNavigate()

    return (
        <div className={styles.page}>
            <div className={styles.hero}>
                <div className={styles.heroIcon}>🩺</div>
                <h1 className={styles.heroTitle}>
                    Ласкаво просимо до OCR Medical!
                </h1>
                <p className={styles.heroSubtitle}>
                    Система медичного OCR — розпізнавання, перевірка та зберігання медичних документів
                </p>
            </div>

            <div className={styles.cards}>
                {cards.map(card => (
                    <button
                        key={card.to}
                        className={styles.card}
                        style={{ '--accent': card.accent } as React.CSSProperties}
                        onClick={() => navigate(card.to)}
                    >
                        <span className={styles.cardIcon}>{card.icon}</span>
                        <h2 className={styles.cardTitle}>{card.title}</h2>
                        <p className={styles.cardDesc}>{card.description}</p>
                        <span className={styles.cardArrow}>→</span>
                    </button>
                ))}
            </div>

            <div className={styles.workflowSection}>
                <h2 className={styles.workflowTitle}>Процес роботи (Human-in-the-Loop)</h2>
                <div className={styles.workflow}>
                    {[
                        { step: '1', label: 'Завантаження', desc: 'PDF, JPG або PNG документ' },
                        { step: '2', label: 'OCR-розпізнавання', desc: 'Azure AI обробляє файл' },
                        { step: '3', label: 'Перевірка', desc: 'Лікар верифікує дані' },
                        { step: '4', label: 'Збереження', desc: 'Запис у базу пацієнта' },
                    ].map((s, i, arr) => (
                        <div key={s.step} className={styles.workflowItem}>
                            <div className={styles.workflowStep}>
                                <div className={styles.stepCircle}>{s.step}</div>
                                {i < arr.length - 1 && <div className={styles.stepLine} />}
                            </div>
                            <div className={styles.stepContent}>
                                <span className={styles.stepLabel}>{s.label}</span>
                                <span className={styles.stepDesc}>{s.desc}</span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    )
}
