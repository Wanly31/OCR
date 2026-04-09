// src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage/LoginPage'
import RegisterPage from './pages/RegisterPage/RegisterPage'
import UploadPage from './pages/UploadPage/UploadPage'
import PageLayout from './components/layout/PageLayout'


function Dashboard() {
  return <h1 style={{ color: 'white', padding: '2rem' }}>✅ Ти залогінений!</h1>
}
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
              <Route path="/" element={<Dashboard />} />
              <Route path="/upload" element={<UploadPage />} />
            </Route>
          </Route>
          {/* Будь-який невідомий маршрут → логін */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}