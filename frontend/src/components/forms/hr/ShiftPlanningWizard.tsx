import { useEffect, useMemo, useState } from "react";
import {
    Alert,
    Box,
    FormControl,
    FormControlLabel,
    InputLabel,
    MenuItem,
    Radio,
    RadioGroup,
    Select,
    TextField,
    Typography,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import WizardModal from "./WizardModal";
import { TimeSelectField } from "../TimeSelectField";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import jobPositionService from "../../../api/services/hr/jobPositionService";
import locationService from "../../../api/services/inventory/locationService";
import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";
import {
    employeeProfilesCacheKey,
    jobPositionsCacheKey,
    locationsCacheKey,
    scheduledShiftsCacheKey,
} from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import type { JobPosition } from "../../../data/types/hr/jobPosition";
import type { Location } from "../../../data/types/inventory/location";
import type { ScheduledShift } from "../../../data/types/hr/scheduledShift";
import { normalizeTimeToHHmm } from "../../../data/data-mapper/hr/scheduledShiftMapper";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { useScheduledShiftMutations } from "../../../api/mutations/hr/useScheduledShiftMutations";

const WIZARD_STEPS: string[] = ["Lieu et horaire", "Assignation", "Confirmation"];

type AssignmentMode = "employee" | "position";

interface ShiftPlanningWizardProps {
    open: boolean;
    onClose: () => void;
}

function hasTimeOverlap(
    p_leftStart: string,
    p_leftEnd: string,
    p_rightStart: string,
    p_rightEnd: string
): boolean {
    return p_leftStart < p_rightEnd && p_leftEnd > p_rightStart;
}

export default function ShiftPlanningWizard({ open, onClose }: ShiftPlanningWizardProps) {
    const { addScheduledShift, isAddingScheduledShift } = useScheduledShiftMutations();

    const [activeStep, setActiveStep] = useState<number>(0);
    const [stepError, setStepError] = useState<string>("");
    const [locationId, setLocationId] = useState<string>("");
    const [date, setDate] = useState<string>("");
    const [startTime, setStartTime] = useState<string>("");
    const [endTime, setEndTime] = useState<string>("");
    const [assignmentMode, setAssignmentMode] = useState<AssignmentMode>("employee");
    const [employeeProfileId, setEmployeeProfileId] = useState<string>("");
    const [jobPositionId, setJobPositionId] = useState<string>("");

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: open,
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: open,
    });

    const jobPositionsQuery = useQuery<JobPosition[], Error>({
        queryKey: jobPositionsCacheKey.list(),
        queryFn: () => jobPositionService.getAll(),
        enabled: open,
    });

    const scheduledShiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
        enabled: open,
    });

    const locations: Location[] = locationsQuery.data ?? [];
    const employees: EmployeeProfile[] = employeesQuery.data ?? [];
    const jobPositions: JobPosition[] = jobPositionsQuery.data ?? [];
    const scheduledShifts: ScheduledShift[] = scheduledShiftsQuery.data ?? [];

    useEffect(() => {
        if (!open) {
            return;
        }

        setActiveStep(0);
        setStepError("");
        setLocationId(locations[0] ? String(locations[0].id) : "");
        setDate("");
        setStartTime("09:00");
        setEndTime("17:00");
        setAssignmentMode("employee");
        setEmployeeProfileId("");
        setJobPositionId(jobPositions[0] ? String(jobPositions[0].id) : "");
    }, [open, locations, jobPositions]);

    const availableEmployees: EmployeeProfile[] = useMemo(() => {
        const selectedLocationId = Number(locationId);
        const normalizedStart = normalizeTimeToHHmm(startTime);
        const normalizedEnd = normalizeTimeToHHmm(endTime);
        const canCheckAvailability = !!date && !!normalizedStart && !!normalizedEnd;

        if (!selectedLocationId) {
            return [];
        }

        return employees.filter((p_employee: EmployeeProfile) => {
            if (p_employee.locationId !== selectedLocationId) {
                return false;
            }

            if (!canCheckAvailability) {
                return true;
            }

            return !scheduledShifts.some((p_shift: ScheduledShift) => {
                if (
                    p_shift.isDeleted
                    || p_shift.employeeProfileId !== p_employee.id
                    || p_shift.date !== date
                ) {
                    return false;
                }

                return hasTimeOverlap(
                    normalizedStart,
                    normalizedEnd,
                    normalizeTimeToHHmm(p_shift.startTime),
                    normalizeTimeToHHmm(p_shift.endTime)
                );
            });
        });
    }, [date, employees, endTime, locationId, scheduledShifts, startTime]);

    const validateStep = (p_step: number): boolean => {
        setStepError("");

        if (p_step === 0) {
            if (!locationId) {
                setStepError("La succursale est requise.");
                return false;
            }
            if (!date) {
                setStepError("La date est requise.");
                return false;
            }
            const normalizedStart = normalizeTimeToHHmm(startTime);
            const normalizedEnd = normalizeTimeToHHmm(endTime);
            if (!normalizedStart || !normalizedEnd || normalizedEnd <= normalizedStart) {
                setStepError("L'heure de fin doit être postérieure à l'heure de début.");
                return false;
            }
        }

        if (p_step === 1) {
            if (assignmentMode === "employee" && !employeeProfileId) {
                setStepError("Sélectionnez un employé disponible pour ce créneau.");
                return false;
            }
            if (assignmentMode === "position" && !jobPositionId) {
                setStepError("Sélectionnez un poste.");
                return false;
            }
            if (assignmentMode === "employee") {
                const selectedEmployee: EmployeeProfile | undefined = employees.find(
                    (p_employee) => String(p_employee.id) === employeeProfileId
                );
                if (!selectedEmployee || selectedEmployee.jobPositionId <= 0) {
                    setStepError(
                        "L'employé sélectionné n'a pas de poste associé. Mettez à jour son profil employé avant de planifier un quart."
                    );
                    return false;
                }
            }
        }

        return true;
    };

    const handleNext = async (): Promise<void> => {
        if (!validateStep(activeStep)) {
            return;
        }

        if (activeStep < WIZARD_STEPS.length - 1) {
            setActiveStep((p_prev) => p_prev + 1);
            return;
        }

        try {
            const selectedEmployee: EmployeeProfile | undefined =
                assignmentMode === "employee"
                    ? employees.find((p_employee) => String(p_employee.id) === employeeProfileId)
                    : undefined;
            const resolvedJobPositionId: number =
                assignmentMode === "employee"
                    ? selectedEmployee?.jobPositionId ?? 0
                    : Number(jobPositionId);

            await addScheduledShift({
                locationId: Number(locationId),
                employeeProfileId:
                    assignmentMode === "employee" ? Number(employeeProfileId) : null,
                jobPositionId: resolvedJobPositionId,
                date,
                startTime: normalizeTimeToHHmm(startTime),
                endTime: normalizeTimeToHHmm(endTime),
            });
            notifySuccessMessage("Le quart planifié a été ajouté avec succès.");
            onClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const locationLabel: string =
        locations.find((p) => String(p.id) === locationId)?.title ?? "—";

    const assigneeLabel: string =
        assignmentMode === "employee"
            ? (() => {
                const employee = employees.find((p) => String(p.id) === employeeProfileId);
                return employee ? `${employee.firstName} ${employee.lastName}` : "—";
            })()
            : jobPositions.find((p) => String(p.id) === jobPositionId)?.name ?? "—";

    const renderStepContent = (): React.ReactNode => {
        switch (activeStep) {
            case 0:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Indiquez où et quand le quart aura lieu.
                        </Typography>
                        <FormControl fullWidth sx={{ mb: 2 }} required>
                            <InputLabel id="shift-location-label">Succursale</InputLabel>
                            <Select
                                labelId="shift-location-label"
                                label="Succursale"
                                value={locationId}
                                onChange={(p_event: SelectChangeEvent<string>) =>
                                    setLocationId(p_event.target.value)
                                }
                                disabled={locationsQuery.isLoading}
                            >
                                {locations.map((p_location: Location) => (
                                    <MenuItem key={p_location.id} value={String(p_location.id)}>
                                        {p_location.title}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                        <TextField
                            fullWidth
                            label="Date"
                            type="date"
                            value={date}
                            onChange={(p_event) => setDate(p_event.target.value)}
                            InputLabelProps={{ shrink: true }}
                            sx={{ mb: 2 }}
                            required
                        />
                        <Box sx={{ mb: 2 }}>
                            <TimeSelectField
                                label="Heure de début"
                                value={startTime}
                                onChange={setStartTime}
                            />
                        </Box>
                        <Box sx={{ mb: 2 }}>
                            <TimeSelectField
                                label="Heure de fin"
                                value={endTime}
                                onChange={setEndTime}
                            />
                        </Box>
                    </>
                );
            case 1:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Assignez le quart à un employé précis ou laissez-le ouvert par poste.
                        </Typography>
                        <FormControl component="fieldset" sx={{ mb: 2 }}>
                            <RadioGroup
                                value={assignmentMode}
                                onChange={(p_event) =>
                                    setAssignmentMode(p_event.target.value as AssignmentMode)
                                }
                            >
                                <FormControlLabel
                                    value="employee"
                                    control={<Radio />}
                                    label="Employé précis"
                                />
                                <FormControlLabel
                                    value="position"
                                    control={<Radio />}
                                    label="Quart ouvert (par poste)"
                                />
                            </RadioGroup>
                        </FormControl>
                        {assignmentMode === "employee" ? (
                            <>
                                <FormControl fullWidth sx={{ mb: 2 }} required>
                                    <InputLabel id="shift-employee-label">Employé</InputLabel>
                                    <Select
                                        labelId="shift-employee-label"
                                        label="Employé"
                                        value={employeeProfileId}
                                        onChange={(p_event: SelectChangeEvent<string>) =>
                                            setEmployeeProfileId(p_event.target.value)
                                        }
                                        disabled={employeesQuery.isLoading || scheduledShiftsQuery.isLoading}
                                    >
                                        {availableEmployees.map((p_employee: EmployeeProfile) => (
                                            <MenuItem key={p_employee.id} value={String(p_employee.id)}>
                                                {`${p_employee.firstName} ${p_employee.lastName}`}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                                {availableEmployees.length === 0 && (
                                    <Alert severity="info">
                                        Aucun employé disponible à cette succursale pour ce créneau.
                                        Essayez un autre horaire ou assignez par poste.
                                    </Alert>
                                )}
                            </>
                        ) : (
                            <FormControl fullWidth sx={{ mb: 2 }} required>
                                <InputLabel id="shift-position-label">Poste</InputLabel>
                                <Select
                                    labelId="shift-position-label"
                                    label="Poste"
                                    value={jobPositionId}
                                    onChange={(p_event: SelectChangeEvent<string>) =>
                                        setJobPositionId(p_event.target.value)
                                    }
                                    disabled={jobPositionsQuery.isLoading}
                                >
                                    {jobPositions.map((p_position: JobPosition) => (
                                        <MenuItem key={p_position.id} value={String(p_position.id)}>
                                            {p_position.name}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        )}
                    </>
                );
            default:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Vérifiez le quart avant de le créer.
                        </Typography>
                        <Typography variant="body2">
                            <strong>Succursale :</strong> {locationLabel}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Date :</strong> {date}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Horaire :</strong> {normalizeTimeToHHmm(startTime)} –{" "}
                            {normalizeTimeToHHmm(endTime)}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Assignation :</strong>{" "}
                            {assignmentMode === "employee" ? assigneeLabel : `Poste — ${assigneeLabel}`}
                        </Typography>
                    </>
                );
        }
    };

    return (
        <WizardModal
            open={open}
            onClose={onClose}
            title="Assistant — Planifier un quart"
            steps={WIZARD_STEPS}
            activeStep={activeStep}
            onBack={() => {
                setStepError("");
                setActiveStep((p_prev) => Math.max(0, p_prev - 1));
            }}
            onNext={() => void handleNext()}
            isSubmitting={isAddingScheduledShift}
        >
            {stepError && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {stepError}
                </Alert>
            )}
            {renderStepContent()}
        </WizardModal>
    );
}
