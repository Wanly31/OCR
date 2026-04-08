import {createContext, useContext, useState, type ReactNode} from 'react'
import {login as apiLogin, register as apiRegister} from '../api/auth.api'
import type { LoginRequest, RegisterRequest } from '../types/auth.types'

interface AuthContextType{
    token: string | null
    isAuthenticated: boolean
    login:(data: LoginRequest) => Promise<void>
    register: (data: RegisterRequest) => Promise<void>
    logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(
        localStorage.getItem('jwt_token')
    )

const login = async (data: LoginRequest) => {
    const result = await apiLogin(data)
    setToken(result._jwtToken)
    localStorage.setItem('jwt_token', result._jwtToken)
}

const register = async (data: RegisterRequest) =>{
    const result = await apiRegister(data)
    setToken(result._jwtToken)
    localStorage.setItem("jwt_token", result._jwtToken)
}

const logout = () => {
    setToken(null)
    localStorage.removeItem('jwt_token')
}

return (
    <AuthContext.Provider value={{
      token,
      isAuthenticated: token !== null,
      login,
      register,
      logout
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(){
    const ctx = useContext(AuthContext)
    if (!ctx) throw new Error('useAuth must be inside AuthProvider ')
    return ctx
}