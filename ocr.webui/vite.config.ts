import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 62651,
        proxy: {
            '/api': {
                target: 'https://localhost:7241',
                changeOrigin: true,
                secure: false, // дозволяє self-signed HTTPS сертифікат від VS
            },
            '/Documents': {
                target: 'https://localhost:7241',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})
