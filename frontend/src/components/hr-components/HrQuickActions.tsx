import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import EventAvailableOutlinedIcon from "@mui/icons-material/EventAvailableOutlined";
import PaymentsOutlinedIcon from "@mui/icons-material/PaymentsOutlined";
import PeopleOutlinedIcon from "@mui/icons-material/PeopleOutlined";
import PersonAddOutlinedIcon from "@mui/icons-material/PersonAddOutlined";
import ScheduleOutlinedIcon from "@mui/icons-material/ScheduleOutlined";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import { FORM_TYPES, type FormType, useFormContainer } from "../../context/FormContext";
import { ROUTE_LIST_USERS } from "../../data/routeNames";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES, type Entity } from "../../permissions/permissions";

interface QuickActionConfig {
    id: string;
    title: string;
    description: string;
    icon: React.ReactNode;
    entity: Entity;
    formType?: FormType;
    linkTo?: string;
    buttonLabel?: string;
    requiresCreate?: boolean;
}

const QUICK_ACTIONS: QuickActionConfig[] = [
    {
        id: "onboarding",
        title: "Nouvel employé",
        description: "Poste, profil, accès système et contrat en un seul parcours.",
        icon: <PersonAddOutlinedIcon fontSize="large" color="primary" />,
        formType: FORM_TYPES.EMPLOYEE_ONBOARDING,
        entity: ENTITY_TYPES.EMPLOYEE_PROFILE,
        requiresCreate: true,
    },
    {
        id: "shift",
        title: "Planifier un quart",
        description: "Succursale, horaire et assignation employé ou poste.",
        icon: <ScheduleOutlinedIcon fontSize="large" color="primary" />,
        formType: FORM_TYPES.SHIFT_PLANNING,
        entity: ENTITY_TYPES.SCHEDULED_SHIFT,
        requiresCreate: true,
    },
    {
        id: "payroll",
        title: "Générer une paie",
        description: "Créer un bulletin pour un employé et une période.",
        icon: <PaymentsOutlinedIcon fontSize="large" color="primary" />,
        formType: FORM_TYPES.PAYROLL_GENERATE,
        entity: ENTITY_TYPES.PAYROLL,
        requiresCreate: true,
    },
    {
        id: "timesheet",
        title: "Feuille de temps",
        description: "Ouvrir une période de temps pour un employé.",
        icon: <CalendarMonthOutlinedIcon fontSize="large" color="primary" />,
        formType: FORM_TYPES.TIMESHEET,
        entity: ENTITY_TYPES.TIMESHEET,
        requiresCreate: true,
    },
    {
        id: "leave",
        title: "Demande de congé",
        description: "Enregistrer une absence ou des vacances.",
        icon: <EventAvailableOutlinedIcon fontSize="large" color="primary" />,
        formType: FORM_TYPES.LEAVE_REQUEST,
        entity: ENTITY_TYPES.LEAVE_REQUEST,
        requiresCreate: true,
    },
    {
        id: "users",
        title: "Liste des utilisateurs",
        description: "Consulter et gérer les comptes, rôles et accès système.",
        icon: <PeopleOutlinedIcon fontSize="large" color="primary" />,
        linkTo: ROUTE_LIST_USERS,
        buttonLabel: "Consulter",
        entity: ENTITY_TYPES.USER,
        requiresCreate: false,
    },
];

export default function HrQuickActions() {
    const { openForm } = useFormContainer();

    const employeePermissions = usePermissions(ENTITY_TYPES.EMPLOYEE_PROFILE);
    const shiftPermissions = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    const payrollPermissions = usePermissions(ENTITY_TYPES.PAYROLL);
    const timesheetPermissions = usePermissions(ENTITY_TYPES.TIMESHEET);
    const leavePermissions = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const userPermissions = usePermissions(ENTITY_TYPES.USER);

    const permissionsByEntity: Partial<Record<Entity, { canCreate: boolean; canRead: boolean }>> = {
        [ENTITY_TYPES.EMPLOYEE_PROFILE]: employeePermissions,
        [ENTITY_TYPES.SCHEDULED_SHIFT]: shiftPermissions,
        [ENTITY_TYPES.PAYROLL]: payrollPermissions,
        [ENTITY_TYPES.TIMESHEET]: timesheetPermissions,
        [ENTITY_TYPES.LEAVE_REQUEST]: leavePermissions,
        [ENTITY_TYPES.USER]: userPermissions,
    };

    const visibleActions: QuickActionConfig[] = QUICK_ACTIONS.filter((p_action: QuickActionConfig) => {
        const permissions = permissionsByEntity[p_action.entity];
        if (!permissions) {
            return false;
        }
        return p_action.requiresCreate === false ? permissions.canRead : permissions.canCreate;
    });

    if (visibleActions.length === 0) {
        return null;
    }

    return (
        <Box sx={{ mt: 3 }}>
            <Typography variant="h6" gutterBottom>
                Actions rapides
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Assistants et formulaires guidés pour les tâches RH les plus fréquentes.
            </Typography>
            <Grid container spacing={2}>
                {visibleActions.map((p_action: QuickActionConfig) => (
                    <Grid key={p_action.id} size={{ xs: 12, md: 6 }}>
                        <Paper
                            elevation={0}
                            sx={{
                                p: 2.5,
                                height: "100%",
                                border: "2px solid",
                                borderColor: "divider",
                                borderRadius: 2,
                                display: "flex",
                                flexDirection: "column",
                                gap: 1.5,
                            }}
                        >
                            <Box sx={{ display: "flex", gap: 1.5, alignItems: "flex-start" }}>
                                {p_action.icon}
                                <Box>
                                    <Typography variant="subtitle1" fontWeight={600}>
                                        {p_action.title}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        {p_action.description}
                                    </Typography>
                                </Box>
                            </Box>
                            {p_action.linkTo ? (
                                <Button
                                    component={RouterLink}
                                    to={p_action.linkTo}
                                    variant="contained"
                                    sx={{ mt: "auto", alignSelf: "flex-start", textTransform: "none" }}
                                >
                                    {p_action.buttonLabel ?? "Consulter"}
                                </Button>
                            ) : (
                                <Button
                                    variant="contained"
                                    onClick={() => {
                                        if (p_action.formType) {
                                            openForm(p_action.formType);
                                        }
                                    }}
                                    sx={{ mt: "auto", alignSelf: "flex-start", textTransform: "none" }}
                                >
                                    {p_action.buttonLabel ?? "Démarrer"}
                                </Button>
                            )}
                        </Paper>
                    </Grid>
                ))}
            </Grid>
        </Box>
    );
}
