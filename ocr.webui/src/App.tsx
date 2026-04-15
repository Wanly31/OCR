// src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage/LoginPage'
import RegisterPage from './pages/RegisterPage/RegisterPage'
import UploadPage from './pages/UploadPage/UploadPage'
import ReviewPage from './pages/ReviewPage/ReviewPage'
import DashboardPage from './pages/DashboardPage/DashboardPage'
import PatientsPage from './pages/PatientsPage/PatientsPage'
import PatientDetailPage from './pages/PatientDetailPage/PatientDetailPage'
import DocumentsPage from './pages/DocumentsPage/DocumentsPage'
import PageLayout from './components/layout/PageLayout'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Публічні — доступні всім */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Захищені — тільки після логіну */}
          <Route element={<ProtectedRoute />}>
            <Route element={<PageLayout />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/upload" element={<UploadPage />} />
              <Route path="/review" element={<ReviewPage />} />
              <Route path="/patients" element={<PatientsPage />} />
              <Route path="/patients/:id" element={<PatientDetailPage />} />
              <Route path="/documents" element={<DocumentsPage />} />
            </Route>
          </Route>

          {/* Будь-який невідомий маршрут → логін */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}