import { createContext, useContext, useState, type ReactNode } from "react";

export const FORM_TYPES = {
    USER: "user",
    ITEM: "item",
    LOCATION: "location",
    CATEGORY: "category",
    QUANTITY: "quantity",
    JOB_POSITION: "jobPosition",
    EMPLOYEE_PROFILE: "employeeProfile",
    LEAVE_REQUEST: "leaveRequest",
    SCHEDULED_SHIFT: "scheduledShift",
    TIME_ENTRY: "timeEntry",
    TIMESHEET: "timesheet",
    PAYROLL_GENERATE: "payrollGenerate",
    EMPLOYEE_ONBOARDING: "employeeOnboarding",
    SHIFT_PLANNING: "shiftPlanning",
    AUTHOR: "author"
} as const;

export type FormType = (typeof FORM_TYPES)[keyof typeof FORM_TYPES] | null;

interface FormContextType {
    activeForm: FormType;
    editData: any;
    openForm: (type: FormType, data?: any) => void;
    closeForm: () => void;
}

const FormContext = createContext<FormContextType | undefined>(undefined);

export function FormProvider({ children }: { children: ReactNode }) {
    const [activeForm, setActiveForm] = useState<FormType>(null);
    const [editData, setEditData] = useState<any>(null);

    const openForm = (type: FormType, data: any = null) => {
        setEditData(data);
        setActiveForm(type);
    };

    const closeForm = () => {
        setActiveForm(null);
        setEditData(null);
    };

    return (
        <FormContext.Provider value={{ activeForm, editData, openForm, closeForm }}>
            {children}
        </FormContext.Provider>
    );
}

export function useFormContainer() {
    const context = useContext(FormContext);
    if (!context) {
        throw new Error("useFormContainer must be used within a FormProvider");
    }
    return context;
}
