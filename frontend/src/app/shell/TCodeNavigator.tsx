import { Hash, Loader2 } from "lucide-react";
import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import type { ApiError } from "../../core/api/httpClient";
import { navigateByTCode } from "../../modules/authorization/tcode/tcode.api";

export function TCodeNavigator() {
  const { t } = useTranslation("common");
  const navigate = useNavigate();
  const [value, setValue] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const errorTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  function showError(key: string) {
    setErrorKey(key);
    if (errorTimerRef.current) clearTimeout(errorTimerRef.current);
    errorTimerRef.current = setTimeout(() => setErrorKey(null), 3500);
  }

  async function handleNavigate() {
    const trimmed = value.trim().toUpperCase();
    if (!trimmed || isLoading) return;

    setErrorKey(null);
    setIsLoading(true);

    try {
      const result = await navigateByTCode(trimmed);
      setValue("");
      if (result.routeLink) {
        navigate(result.routeLink);
      } else {
        showError("tcodeNoRoute");
      }
    } catch (err) {
      const apiErr = err as ApiError;
      if (apiErr.status === 403) {
        showError("tcodeAccessDenied");
      } else if (apiErr.status === 400) {
        showError("tcodeInvalidInput");
      } else {
        showError("tcodeError");
      }
    } finally {
      setIsLoading(false);
    }
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") void handleNavigate();
    if (event.key === "Escape") {
      setValue("");
      setErrorKey(null);
    }
  }

  return (
    <div className="tcode-navigator">
      <div className={`topbar__search tcode-navigator__input-row${errorKey ? " tcode-navigator__input-row--error" : ""}`}>
        {isLoading ? (
          <Loader2 size={16} className="tcode-navigator__spinner" />
        ) : (
          <Hash size={16} className="tcode-navigator__icon" />
        )}
        <input
          type="text"
          value={value}
          onChange={(e) => setValue(e.target.value.toUpperCase())}
          onKeyDown={handleKeyDown}
          placeholder={t("tcodePlaceholder")}
          maxLength={20}
          autoComplete="off"
          spellCheck={false}
          aria-label={t("tcodePlaceholder")}
        />
      </div>
      {errorKey && (
        <div className="tcode-navigator__error" role="alert">
          {t(errorKey)}
        </div>
      )}
    </div>
  );
}
