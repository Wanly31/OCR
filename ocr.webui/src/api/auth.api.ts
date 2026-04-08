import axios from 'axios'

import type {LoginRequest, RegisterRequest, AuthResult} from "../types/auth.types"

const api = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json'
    }

})

export const login = async (data: LoginRequest): Promise<AuthResult> => {
    const response = await api.post<AuthResult>('auth/login', data)
    return response.data
}

export const register = async (data: RegisterRequest): Promise<AuthResult> => {
    const response = await api.post<AuthResult>('auth/register', data)
    return response.data
}