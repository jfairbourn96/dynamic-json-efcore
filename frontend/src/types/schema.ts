export type FieldType = 'text' | 'number' | 'date' | 'boolean' | 'select';

export interface FieldOption {
  label: string;
  value: string;
}

export interface FieldDefinition {
  id: string;
  name: string;
  label: string;
  fieldType: FieldType;
  required: boolean;
  options?: FieldOption[];
  order: number;
}

export interface UserType {
  id: string;
  name: string;
  description?: string;
  parentTypeId?: string | null;
  fields: FieldDefinition[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserTypeRequest {
  name: string;
  description?: string;
  parentTypeId?: string | null;
  fields: Omit<FieldDefinition, 'id'>[];
}

export interface UpdateUserTypeRequest extends CreateUserTypeRequest {
  id: string;
}
