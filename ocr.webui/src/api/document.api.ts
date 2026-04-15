import api from './axios.config'
import type { DocumentDto } from "../types/document.types";

// GET /api/Document/{id}/file
export const getDocumentStream = async (documentId: string): Promise<Blob> => {
    const response = await api.get<Blob>(`/Document/${documentId}/file`, {
        responseType: 'blob' 
    });
    return response.data;
};

// DELETE /api/Document/{id}
export const deleteDocument = async (documentId: string): Promise<DocumentDto> => {
    const response = await api.delete<DocumentDto>(`/Document/${documentId}`);
    return response.data;
};