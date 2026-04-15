import api from './axios.config';

export interface SearchPatientRequest {
    FirstName: string;
    LastName?: string;
    BirthDate?: string;
    Page?: number;
    PageSize?: number;
}

export interface SearchPatientResult {
    Id: string;
    FirstName: string;
    LastName?: string;
    BirthDate?: string;
    TotalRecords: number;
}

export interface PaginatedResult<T> {
    Items: T[];
    TotalCount: number;
    Page: number;
    PageSize: number;
}

export interface PatientDetail {
    Id: string;
    FirstName: string;
    LastName?: string;
    BirthDate?: string;
    TotalRecords: number;
}

export interface MedicalRecord {
    Id: string;
    PatientId: string;
    FirstName: string;
    LastName?: string;
    Examination?: string;
    Medicine?: string;
    Treatment?: string;
    ContraindicatedMedicine?: string;
    ContraindicatedReason?: string;
    DateDocument?: string;
    CreatedAt?: string;
}



// POST /api/Patient/search
export const searchPatients = async (
    data: SearchPatientRequest
): Promise<PaginatedResult<SearchPatientResult>> => {
    const response = await api.post<PaginatedResult<SearchPatientResult>>('/Patient/search', data);
    return response.data;
};

// GET /api/Patient/{id}
export const getPatientById = async (id: string): Promise<PatientDetail> => {
    const response = await api.get<PatientDetail>(`/Patient/${id}`);
    return response.data;
};

// GET /api/Patient/{id}/history
export const getPatientHistory = async (id: string): Promise<MedicalRecord[]> => {
    const response = await api.get<MedicalRecord[]>(`/Patient/${id}/history`);
    return response.data;
};
