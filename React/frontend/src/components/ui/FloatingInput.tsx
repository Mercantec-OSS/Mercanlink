interface FloatingInputProps {
    name: string;
    label: string;
    label2: string;
    type?: string;
    required?: boolean;
    className?: string;
    multiline?: boolean;
}

export default function FloatingInput({
    name,
    label,
    label2,
    type = "text",
    required = false,
    className = "",
    multiline = false
}: FloatingInputProps) {
    return (
        <div className={`relative mb-6 ${className}`}>
            {multiline ? (
                <textarea
                    name={name}
                    id={name}
                    rows={4}
                    className="peer text-white w-full bg-transparent border-b-2 focus:border-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 placeholder-transparent transition-all resize-vertical mt-10 resize-none"
                    placeholder={label}
                    required={required}
                />

            ) : (
                <input
                    type={type}
                    name={name}
                    id={name}
                    className="peer text-white w-full bg-transparent border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 placeholder-transparent transition-all mt-2 }"
                    placeholder={label}
                    required={required}
                />
            )}
            <label
                htmlFor={name}
                className="absolute left-1 top-2 text-white/70 text-sm transition-all peer-placeholder-shown:top-2 peer-placeholder-shown:text-base peer-focus:-top-5 peer-focus:text-blue-400 peer-focus:text-sm peer-valid:-top-4 peer-valid:text-blue-400 peer-valid:text-sm mt-3"
            >
                {label}
            </label>
            <label
                htmlFor={name}
                className="absolute left-1 top-2 text-white/70 text-sm transition-all peer-placeholder-shown:top-2 peer-placeholder-shown:text-base peer-focus:-top-18 peer-focus:text-blue-400 peer-focus:text-sm peer-valid:-top-4 peer-valid:text-blue-400 peer-valid:text-sm mt-23"
            >
                {label2}
            </label>
        </div>
    );
}