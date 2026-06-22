import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import ItemForm from "./inventory/ItemForm";
import LocationForm from "./inventory/LocationForm";
import InventoryQuantityForm from "./inventory/InventoryQuantityForm";
import UserForm from "./hr/UserForm";
import CategoryForm from "./inventory/CategoryForm";
import JobPositionForm from "./hr/JobPositionForm";
import EmployeeProfileForm from "./hr/EmployeeProfileForm";
import LeaveRequestForm from "./hr/LeaveRequestForm";
import ScheduledShiftForm from "./hr/ScheduledShiftForm";
import TimeEntryForm from "./hr/TimeEntryForm";
import TimesheetForm from "./hr/TimesheetForm";
import PayrollGenerateForm from "./hr/PayrollGenerateForm";
import EmployeeOnboardingWizard from "./hr/EmployeeOnboardingWizard";
import ShiftPlanningWizard from "./hr/ShiftPlanningWizard";
import AuthorForm from "./inventory/AuthorForm";

export default function FormRoot() {
    const { activeForm, editData, closeForm } = useFormContainer();

    if (!activeForm) return null;

    const scheduledShiftDefaultLocationId: number | null =
        editData?.defaultLocationId ?? null;
    const scheduledShiftEditData =
        scheduledShiftDefaultLocationId !== null ? null : editData;

    return (
        <>
            {activeForm === FORM_TYPES.ITEM && (
                <ItemForm
                    showItemForm={true}
                    setShowItemForm={closeForm}
                    editItem={editData}
                    setEditItem={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.USER && (
                <UserForm
                    showUserForm={true}
                    setShowUserForm={closeForm}
                    editUser={editData}
                    setEditUser={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.LOCATION && (
                <LocationForm
                    showLocationForm={true}
                    setShowLocationForm={closeForm}
                    editLocation={editData}
                    setEditLocation={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.QUANTITY && (
                <InventoryQuantityForm
                    showForm={true}
                    setShowForm={closeForm}
                    editQuantity={editData}
                />
            )}

            {activeForm === FORM_TYPES.CATEGORY && (
                <CategoryForm
                    showCategoryForm={true}
                    setShowCategoryForm={closeForm}
                    editCategory={editData}
                    setEditCategory={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.JOB_POSITION && (
                <JobPositionForm
                    showJobPositionForm={true}
                    setShowJobPositionForm={closeForm}
                    editJobPosition={editData}
                    setEditJobPosition={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.EMPLOYEE_PROFILE && (
                <EmployeeProfileForm
                    showEmployeeProfileForm={true}
                    setShowEmployeeProfileForm={closeForm}
                    editEmployeeProfile={editData}
                    setEditEmployeeProfile={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.LEAVE_REQUEST && (
                <LeaveRequestForm
                    showLeaveRequestForm={true}
                    setShowLeaveRequestForm={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.SCHEDULED_SHIFT && (
                <ScheduledShiftForm
                    showScheduledShiftForm={true}
                    setShowScheduledShiftForm={closeForm}
                    editScheduledShift={scheduledShiftEditData}
                    setEditScheduledShift={closeForm}
                    defaultLocationId={scheduledShiftDefaultLocationId}
                />
            )}

            {activeForm === FORM_TYPES.TIME_ENTRY && (
                <TimeEntryForm
                    showTimeEntryForm={true}
                    setShowTimeEntryForm={closeForm}
                    editTimeEntry={editData}
                    setEditTimeEntry={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.TIMESHEET && (
                <TimesheetForm
                    showTimesheetForm={true}
                    setShowTimesheetForm={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.PAYROLL_GENERATE && (
                <PayrollGenerateForm
                    showPayrollGenerateForm={true}
                    setShowPayrollGenerateForm={closeForm}
                />
            )}

            {activeForm === FORM_TYPES.EMPLOYEE_ONBOARDING && (
                <EmployeeOnboardingWizard open={true} onClose={closeForm} />
            )}

            {activeForm === FORM_TYPES.SHIFT_PLANNING && (
                <ShiftPlanningWizard open={true} onClose={closeForm} />
            )}

            {activeForm === FORM_TYPES.AUTHOR && (
                <AuthorForm showAuthorForm={true} setShowAuthorForm={closeForm} editAuthor={editData} setEditAuthor={closeForm} />
            )}
        </>
    );
}
