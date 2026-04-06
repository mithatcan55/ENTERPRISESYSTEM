export interface ErpService {
  endpoint: string;
  name: string;
  description: string;
  category: string;
  parameterCount: number;
}

export interface ErpParam {
  name: string;
  type: string;
  isRequired: boolean;
  description: string;
  defaultValue?: string;
}

export interface ErpRunResponse {
  columns: string[];
  rows: Record<string, unknown>[];
  rowCount: number;
  duration: string;
}
