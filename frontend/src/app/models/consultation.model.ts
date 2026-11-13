/** Deben coincidir con los enums del dominio (.NET), que se serializan como texto. */
export type ConsultationStatus = 'Active' | 'Paused' | 'Finished';
export type CertaintyLevel = 'High' | 'Medium' | 'Low';
export type RecordStatus = 'Accepted' | 'Pending' | 'Confirmed' | 'NotConfirmed' | 'Discarded' | 'NotRecognized';

export interface TranslationRecord {
  id: string;
  signId: string | null;
  word: string | null;
  recognizedAt: string;
  similarityScore: number;
  certaintyLevel: CertaintyLevel;
  status: RecordStatus;
}

export interface Consultation {
  id: string;
  date: string;
  startedAt: string;
  endedAt: string | null;
  status: ConsultationStatus;
  records: TranslationRecord[];
}

export interface ConsultationSummary {
  id: string;
  date: string;
  startedAt: string;
  endedAt: string | null;
  status: ConsultationStatus;
  wordCount: number;
}

export interface CertaintyConfig {
  high: number;
  medium: number;
  confirmationSeconds: number;
}

/** Palabras que forman parte de la frase traducida. */
export const IN_PHRASE: ReadonlySet<RecordStatus> = new Set(['Accepted', 'Confirmed', 'NotConfirmed']);
