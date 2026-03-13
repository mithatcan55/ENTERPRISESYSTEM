type FormFieldOption = {
  value: string;
  label: string;
};

export type FormField = {
  key: string;
  label: string;
  type: "text" | "email" | "password" | "number" | "select" | "switch" | "textarea";
  value: string | number | boolean;
  placeholder?: string;
  helpText?: string;
  options?: FormFieldOption[];
};

type StandardFormProps = {
  fields: FormField[];
  onChange: (key: string, value: string | number | boolean) => void;
  onSubmit?: () => void;
  submitLabel?: string;
};

export function StandardForm({ fields, onChange, onSubmit, submitLabel }: StandardFormProps) {
  return (
    <div className="standard-form">
      <div className="standard-form__grid">
        {fields.map((field) => (
          <label key={field.key} className={`standard-form__field standard-form__field--${field.type}`}>
            <span>{field.label}</span>

            {field.type === "textarea" ? (
              <textarea
                value={String(field.value)}
                placeholder={field.placeholder}
                onChange={(event) => onChange(field.key, event.target.value)}
              />
            ) : field.type === "select" ? (
              <select value={String(field.value)} onChange={(event) => onChange(field.key, event.target.value)}>
                {field.options?.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            ) : field.type === "switch" ? (
              <button
                type="button"
                className={`standard-form__switch ${field.value ? "standard-form__switch--on" : ""}`}
                onClick={() => onChange(field.key, !field.value)}
              >
                <span />
                <strong>{field.value ? "Acik" : "Kapali"}</strong>
              </button>
            ) : (
              <input
                type={field.type}
                value={String(field.value)}
                placeholder={field.placeholder}
                onChange={(event) =>
                  onChange(field.key, field.type === "number" ? Number(event.target.value) : event.target.value)
                }
              />
            )}

            {field.helpText ? <small>{field.helpText}</small> : null}
          </label>
        ))}
      </div>

      {onSubmit ? (
        <div className="standard-form__actions">
          <button type="button" onClick={onSubmit}>
            {submitLabel ?? "Kaydet"}
          </button>
        </div>
      ) : null}
    </div>
  );
}
