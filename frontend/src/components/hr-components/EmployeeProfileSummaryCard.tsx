import type { ReactNode } from "react";
import {
    Avatar,
    Box,
    Card,
    CardContent,
    Chip,
    Divider,
    Typography,
} from "@mui/material";
import EmailIcon from "@mui/icons-material/Email";
import StoreIcon from "@mui/icons-material/Store";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";
import PaymentsIcon from "@mui/icons-material/Payments";
import type { EmployeeProfile } from "../../data/types/hr/employeeProfile";

interface EmployeeProfileSummaryCardProps {
    profile: EmployeeProfile;
    showSalary?: boolean;
    footer?: ReactNode;
}

const STATUS_LABELS: Record<string, string> = {
    Active: "Actif",
    Inactive: "Inactif",
    OnLeave: "En congé",
};

const STATUS_COLORS: Record<string, "success" | "default" | "warning"> = {
    Active: "success",
    Inactive: "default",
    OnLeave: "warning",
};

function formatHiringDate(p_date: string): string {
    return new Date(p_date).toLocaleDateString("fr-CA", {
        year: "numeric",
        month: "long",
        day: "numeric",
    });
}

function ProfileRow({ icon, label, value }: { icon: ReactNode; label: string; value: string }) {
    return (
        <Box sx={{ display: "flex", gap: 1.5, alignItems: "flex-start" }}>
            <Box sx={{ color: "primary.main", mt: 0.25, display: "flex" }}>{icon}</Box>
            <Box>
                <Typography variant="caption" color="text.secondary" display="block">
                    {label}
                </Typography>
                <Typography variant="body1">{value}</Typography>
            </Box>
        </Box>
    );
}

export default function EmployeeProfileSummaryCard({
    profile,
    showSalary = false,
    footer,
}: EmployeeProfileSummaryCardProps) {
    const initials = `${profile.firstName.charAt(0)}${profile.lastName.charAt(0)}`.toUpperCase();
    const statusLabel = STATUS_LABELS[profile.status] ?? profile.status;
    const statusColor = STATUS_COLORS[profile.status] ?? "default";

    return (
        <Card variant="outlined" sx={{ maxWidth: 560 }}>
            <CardContent sx={{ display: "grid", gap: 2, textAlign: "left" }}>
                <Box sx={{ display: "flex", gap: 2, alignItems: "center" }}>
                    <Avatar
                        sx={{
                            width: 72,
                            height: 72,
                            bgcolor: "primary.main",
                            color: "primary.contrastText",
                            fontSize: "1.5rem",
                            fontWeight: 700,
                        }}
                    >
                        {initials}
                    </Avatar>
                    <Box>
                        <Typography variant="h5" fontWeight={700}>
                            {profile.firstName} {profile.lastName}
                        </Typography>
                        <Chip
                            label={statusLabel}
                            color={statusColor}
                            size="small"
                            sx={{ mt: 1 }}
                        />
                    </Box>
                </Box>

                <Divider />

                <Box sx={{ display: "grid", gap: 1.5 }}>
                    <ProfileRow icon={<EmailIcon fontSize="small" />} label="Courriel" value={profile.email} />
                    <ProfileRow
                        icon={<StoreIcon fontSize="small" />}
                        label="Succursale"
                        value={profile.locationTitle ?? "Non assignée"}
                    />
                    <ProfileRow
                        icon={<CalendarTodayIcon fontSize="small" />}
                        label="Date d'embauche"
                        value={formatHiringDate(profile.hiringDate)}
                    />
                    {showSalary && (
                        <ProfileRow
                            icon={<PaymentsIcon fontSize="small" />}
                            label="Salaire annuel"
                            value={profile.salary.toLocaleString("fr-CA", { style: "currency", currency: "CAD" })}
                        />
                    )}
                </Box>

                {footer && (
                    <>
                        <Divider />
                        {footer}
                    </>
                )}
            </CardContent>
        </Card>
    );
}
