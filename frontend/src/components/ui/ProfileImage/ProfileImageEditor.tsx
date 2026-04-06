import { useState, useRef, useCallback } from "react";
import ReactCrop, { type Crop, type PixelCrop, centerCrop, makeAspectCrop } from "react-image-crop";
import "react-image-crop/dist/ReactCrop.css";
import { Camera, Upload, Link as LinkIcon, Trash2 } from "lucide-react";
import { toast } from "sonner";
import ProfileImageDisplay from "./ProfileImageDisplay";

export interface ProfileImageEditorProps {
  value: string | null;
  displayName?: string;
  onChange: (url: string | null) => void;
}

export default function ProfileImageEditor({ value, displayName, onChange }: ProfileImageEditorProps) {
  const [editorOpen, setEditorOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<"file" | "url">("file");
  const [imgSrc, setImgSrc] = useState("");
  const [crop, setCrop] = useState<Crop>();
  const [completedCrop, setCompletedCrop] = useState<PixelCrop>();
  const [urlInput, setUrlInput] = useState("");
  const [urlValid, setUrlValid] = useState(true);

  const fileRef = useRef<HTMLInputElement>(null);
  const imgRef = useRef<HTMLImageElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);

  function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) { toast.error("Dosya 5MB'dan büyük olamaz"); return; }
    if (!file.type.startsWith("image/")) { toast.error("Sadece resim dosyası yüklenebilir"); return; }
    const reader = new FileReader();
    reader.onload = () => { setImgSrc(reader.result as string); setCrop(undefined); };
    reader.readAsDataURL(file);
    e.target.value = "";
  }

  const cropAreaRef = useRef<HTMLDivElement>(null);

  function onImageLoad(e: React.SyntheticEvent<HTMLImageElement>) {
    const img = e.currentTarget;
    const ratio = img.naturalHeight / img.naturalWidth;
    const outerW = Math.min(cropAreaRef.current?.offsetWidth || 480, 480);

    let dispW = outerW;
    let dispH = Math.round(outerW * ratio);
    if (dispH > 220) { dispH = 220; dispW = Math.round(dispH / ratio); }
    if (dispW > 480) { dispW = 480; dispH = Math.round(dispW * ratio); }

    img.style.width = dispW + "px";
    img.style.height = dispH + "px";

    const c = centerCrop(makeAspectCrop({ unit: "%", width: 65 }, 1, dispW, dispH), dispW, dispH);
    setCrop(c);
  }

  const applyCrop = useCallback(() => {
    if (!completedCrop || !imgRef.current || !canvasRef.current) return;
    const canvas = canvasRef.current;
    const image = imgRef.current;
    const scaleX = image.naturalWidth / image.width;
    const scaleY = image.naturalHeight / image.height;
    canvas.width = completedCrop.width * scaleX;
    canvas.height = completedCrop.height * scaleY;
    const ctx = canvas.getContext("2d")!;
    ctx.drawImage(image, completedCrop.x * scaleX, completedCrop.y * scaleY, completedCrop.width * scaleX, completedCrop.height * scaleY, 0, 0, canvas.width, canvas.height);
    const base64 = canvas.toDataURL("image/jpeg", 0.85);
    onChange(base64);
    setImgSrc("");
    setEditorOpen(false);
    toast.success("Profil fotoğrafı güncellendi");
  }, [completedCrop, onChange]);

  function applyUrl() {
    if (!urlInput.trim()) return;
    onChange(urlInput.trim());
    setUrlInput("");
    setEditorOpen(false);
    toast.success("Profil fotoğrafı güncellendi");
  }

  const tabCls = (active: boolean) => ({
    padding: "8px 16px", fontSize: 12, fontWeight: 500, cursor: "pointer" as const, border: "none", background: "none",
    color: active ? "#2E6DA4" : "#7A96B0", borderBottom: active ? "2px solid #2E6DA4" : "2px solid transparent", transition: "all 0.15s",
  });

  return (
    <div>
      {/* Avatar with camera overlay */}
      <div className="relative inline-block">
        <ProfileImageDisplay src={value} displayName={displayName} size={96} />
        <button type="button" onClick={() => setEditorOpen(!editorOpen)}
          className="absolute bottom-0 right-0 flex items-center justify-center rounded-full transition-colors hover:bg-[#2E6DA4]"
          style={{ width: 28, height: 28, background: "#1B3A5C", color: "#fff" }}>
          <Camera size={13} />
        </button>
      </div>

      {/* Editor panel */}
      {editorOpen && (
        <div className="mt-3 rounded-[10px] p-4" style={{ background: "#F7FAFD", border: "1px solid #E2EBF3" }}>
          {/* Tabs */}
          <div className="flex mb-3" style={{ borderBottom: "1px solid #E2EBF3" }}>
            <button type="button" style={tabCls(activeTab === "file")} onClick={() => setActiveTab("file")}>Dosya Yükle</button>
            <button type="button" style={tabCls(activeTab === "url")} onClick={() => setActiveTab("url")}>URL ile Ekle</button>
          </div>

          {/* Tab 1: File upload */}
          {activeTab === "file" && !imgSrc && (
            <div className="rounded-lg p-5 text-center cursor-pointer transition-all hover:border-[#5B9BD5] hover:bg-[#EAF1FA]"
              style={{ border: "2px dashed #D6E4F0", background: "#FAFCFF" }}
              onClick={() => fileRef.current?.click()}
              onDragOver={(e) => { e.preventDefault(); e.stopPropagation(); }}
              onDrop={(e) => { e.preventDefault(); const file = e.dataTransfer.files[0]; if (file) { const input = fileRef.current!; const dt = new DataTransfer(); dt.items.add(file); input.files = dt.files; input.dispatchEvent(new Event("change", { bubbles: true })); } }}>
              <Upload size={24} style={{ color: "#A8C8E8", margin: "0 auto 8px" }} />
              <div className="text-[13px]" style={{ color: "#7A96B0" }}>Fotoğraf seçin veya buraya sürükleyin</div>
              <div className="text-[11px] mt-1" style={{ color: "#B0BEC5" }}>JPG, PNG, WEBP — maks. 5MB</div>
              <input ref={fileRef} type="file" accept="image/*" style={{ display: "none" }} onChange={handleFileSelect} />
            </div>
          )}

          {/* Crop editor */}
          {activeTab === "file" && imgSrc && (
            <div ref={cropAreaRef}>
              <div style={{ maxHeight: 220, maxWidth: 480, overflow: "hidden" }}>
                <ReactCrop crop={crop} onChange={setCrop} onComplete={setCompletedCrop} aspect={1} circularCrop>
                  <img ref={imgRef} src={imgSrc} onLoad={onImageLoad} alt="Crop" />
                </ReactCrop>
              </div>
              <canvas ref={canvasRef} style={{ display: "none" }} />
              <div className="flex gap-2 mt-3 justify-end">
                <button type="button" onClick={() => setImgSrc("")}
                  className="rounded-md px-3 py-1.5 text-[12px] font-medium" style={{ border: "1px solid #D6E4F0", color: "#7A96B0" }}>
                  İptal
                </button>
                <button type="button" onClick={applyCrop}
                  className="rounded-md px-3 py-1.5 text-[12px] font-medium" style={{ background: "#1B3A5C", color: "#fff" }}>
                  Uygula
                </button>
              </div>
            </div>
          )}

          {/* Tab 2: URL input */}
          {activeTab === "url" && (
            <div>
              <div className="flex gap-2">
                <div className="relative flex-1">
                  <LinkIcon size={14} className="absolute left-2.5 top-1/2 -translate-y-1/2" style={{ color: "#A8C8E8" }} />
                  <input value={urlInput} onChange={(e) => { setUrlInput(e.target.value); setUrlValid(true); }}
                    placeholder="https://example.com/photo.jpg"
                    className="w-full rounded-lg pl-8 pr-3 h-[40px] text-[13px] outline-none transition-all"
                    style={{ background: "#FAFCFF", border: "1.5px solid #E2EBF3", color: "#1B3A5C" }}
                    onFocus={(e) => (e.currentTarget.style.borderColor = "#5B9BD5")}
                    onBlur={(e) => (e.currentTarget.style.borderColor = "#E2EBF3")} />
                </div>
                <button type="button" onClick={applyUrl} disabled={!urlInput.trim()}
                  className="rounded-md px-3 h-[40px] text-[12px] font-medium disabled:opacity-40"
                  style={{ background: "#1B3A5C", color: "#fff" }}>
                  Uygula
                </button>
              </div>
              {urlInput && (
                <div className="mt-3 flex items-center gap-3">
                  <img src={urlInput} alt="Preview" className="rounded-full object-cover"
                    style={{ width: 48, height: 48 }}
                    onError={() => setUrlValid(false)} onLoad={() => setUrlValid(true)} />
                  {!urlValid && <span className="text-[11px]" style={{ color: "#E05252" }}>Geçersiz URL</span>}
                </div>
              )}
            </div>
          )}

          {/* Remove button */}
          {value && (
            <div className="mt-3 pt-3" style={{ borderTop: "1px solid #E2EBF3" }}>
              <button type="button" onClick={() => { onChange(null); setEditorOpen(false); toast.success("Fotoğraf kaldırıldı"); }}
                className="flex items-center gap-1.5 text-[12px] transition-colors hover:underline"
                style={{ color: "#E05252", background: "none", border: "none", cursor: "pointer" }}>
                <Trash2 size={12} /> Fotoğrafı kaldır
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
