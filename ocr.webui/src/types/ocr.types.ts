export const RecordStatus = {
    Pending: 'Pending',
    Confirmed: 'Confirmed',
    Rejected: 'Rejected',
} as const

export type RecordStatus = typeof RecordStatus[keyof typeof RecordStatus]

export interface OcrRequest {
    File: File
    Filename: string
    FileDescription?: string

}

export interface RecognizedTextResultDto {
    FirstName?: string
    LastName?: string
    BirthDate?: string
    Examination?: string
    Medicine?: string
    Treatment?: string
    ContraindicatedMedicine?: string
    ContraindicatedReason?: string
    DateDocument?: string
}

export interface SimilarPatientResultDto {
    Id: string
    FirstName: string
    LastName?: string
    BirthDate?: string
    RecordCount: number
}

export interface OcrResult {
    RequiresConfirmation: boolean
    RecognizedId: string
    RecognizeData: RecognizedTextResultDto
    RecordStatus: RecordStatus
    FilePath: string
    SimilarPatients: SimilarPatientResultDto[]
    DocumentId: string
}

//SaveMedicalRecord
export interface SaveMedicalRecordRequest {
    ExistingPatientId?: string
    FirstName: string
    LastName?: string
    BirthDate?: string
    RecognizedId: string
    RecognizedData: RecognizedDataDto
}

export interface SaveMedicalRecordResult {
    Id: string
    FirstName: string
    LastName?: string
    BirthDate?: string
}

export interface RecognizedDataDto {
    Examination?: string
    Medicine?: string
    Treatment?: string
    ContraindicatedMedicine?: string
    ContraindicatedReason?: string
    DateDocument?: string
}