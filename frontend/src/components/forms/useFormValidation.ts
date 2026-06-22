import { useCallback, useRef, useState } from "react"

export function useFormValidation<T extends Record<string, string>>(initialErrors: T) {
    const initialErrorsRef = useRef(initialErrors);
    const [errors, setErrors] = useState<T>(initialErrors);

    const clearErrors = useCallback(() => {
        setErrors({ ...initialErrorsRef.current });
    }, []);

    const hasErrors = Object.values(errors as object).some(msg => msg !== "");

    return { errors, setErrors, clearErrors, hasErrors }
}