interface FloatingInputProps {
    name: string;
    label: string;
    type?: string;
    required?: boolean;
    className?: string;
}

export default function FloatingInput({
    name,
    label,
    type = "text",
    required = false,
    className = ""
}: FloatingInputProps) {
    return (
        <div className={`relative mb-6 ${className}`}>
            <input
                type={type}
                name={name}
                id={name}
                className="peer text-white w-full bg-transparent border-b-2 border-blue-400 focus:border-blue-600 outline-none py-2 px-1 placeholder-transparent transition-all"
                placeholder={label}
                required={required}
            />
            <label
                htmlFor={name}
                className="absolute left-1 top-2 text-white/70 text-sm transition-all peer-placeholder-shown:top-2 peer-placeholder-shown:text-base peer-focus:-top-4 peer-focus:text-blue-400 peer-focus:text-sm"
            >
                {label}
            </label>
        </div>
    );
}