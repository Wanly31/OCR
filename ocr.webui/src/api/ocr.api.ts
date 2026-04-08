import axios from 'axios';

import type { OcrRequest, OcrResult, SaveMedicalRecordRequest, SaveMedicalRecordResult } from '../types/ocr.types';

const api = axios.create({
    baseURL: '/api',
})

export const performOcr = async (data: OcrRequest): Promise<OcrResult> => {
    const formData = new FormData();
    formData.append('File', data.File);
    formData.append('FileName', data.Filename);
    if (data.FileDescription) {
        formData.append('FileDescription', data.FileDescription);
    }

    const response = await api.post<OcrResult>('/ocr/UploadAndRecognize', formData);
    return response.data;
};

export const saveMedicalRecord = async (data: SaveMedicalRecordRequest): Promise<SaveMedicalRecordResult> => {
    const response = await api.post<SaveMedicalRecordResult>('/ocr/SaveMedicalRecord', data, {
        headers: { 'Content-Type': 'application/json' }
    });
    return response.data;
}