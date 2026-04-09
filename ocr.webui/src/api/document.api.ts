import axios from "axios";
import type { DocumentDto } from "../types/document.types";

const api = axios.create({
    baseURL: '/api/Document', // Базовий роут згідно DocumentController.cs
});

// GET /api/Document/{id}/file
// Бекенд повертає БІНАРНИЙ файл (FileResult), тому вказуємо responseType: 'blob'
export const getDocumentStream = async (documentId: string): Promise<Blob> => {
    const response = await api.get<Blob>(`/${documentId}/file`, {
        responseType: 'blob' 
    });
    return response.data;
};

// DELETE /api/Document/{id}
export const deleteDocument = async (documentId: string): Promise<DocumentDto> => {
    const response = await api.delete<DocumentDto>(`/${documentId}`);
    return response.data;
};