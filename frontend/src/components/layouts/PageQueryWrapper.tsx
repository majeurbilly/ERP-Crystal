import type { ReactNode } from "react";
import LoadingSpinner from "../LoadingSpinner";
import ErrorMessage from "./ErrorMessage";

interface PageQueryWrapperProps {
    isLoading: boolean;
    error: any;
    refetch: () => void;
    children: ReactNode;
    errorReturnUrl: string;
    errorReturnLabel: string;
    customErrorMessage?: string;
}

export default function PageQueryWrapper({
    isLoading,
    error,
    refetch,
    children,
    errorReturnUrl,
    errorReturnLabel,
    customErrorMessage = "Une erreur est survenue."
}: PageQueryWrapperProps) {

    if (isLoading) return <LoadingSpinner />;

    if (error) {
        return (
            <ErrorMessage
                errorMessage={error?.message}
                customMessage={customErrorMessage}
                onRetry={refetch}
                returnToUrl={errorReturnUrl}
                returnLabel={errorReturnLabel}
            />
        );
    }

    return (
        <>
            {children}
        </>
    );
}