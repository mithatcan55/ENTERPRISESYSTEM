export type CrudColumnSchema = {
  key: string;
  titleKey: string;
  sortable?: boolean;
  filterable?: boolean;
  mobilePriority?: "high" | "medium" | "low";
  tone?: "default" | "success" | "warning" | "danger";
};

export type CrudFieldSchema = {
  key: string;
  labelKey: string;
  type: "text" | "email" | "password" | "number" | "select" | "switch" | "date" | "textarea";
  required?: boolean;
  placeholderKey?: string;
};

export type CrudModuleContract = {
  moduleKey: string;
  baseRoute: string;
  permissionScope: string;
  tCodeScope?: string[];
  columns: CrudColumnSchema[];
  fields: CrudFieldSchema[];
};
