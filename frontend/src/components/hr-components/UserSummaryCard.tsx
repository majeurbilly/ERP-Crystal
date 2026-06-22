import type { ReactNode } from "react";
import {
    Avatar,
    Box,
    Card,
    CardContent,
    Chip,
    Divider,
    Link as MuiLink,
    Typography,
} from "@mui/material";
import EmailIcon from "@mui/icons-material/Email";
import PersonIcon from "@mui/icons-material/Person";
import BadgeIcon from "@mui/icons-material/Badge";
import { Link as RouterLink } from "react-router-dom";
import type { User } from "../../data/types/hr/user";
import {
    getAssignedRoleDisplayName,
    getRoleChipColorForAssignedRole,
    userAccessFieldLabels
} from "../../data/types/hr/userRoles";
import { ROUTE_USER_ROLE_DETAILS } from "../../data/routeNames";

interface UserSummaryCardProps {
    user: User;
    displayName?: string;
    footer?: ReactNode;
}

function ProfileRow({
    icon,
    label,
    value,
}: {
    icon: ReactNode;
    label: string;
    value: ReactNode;
}) {
    return (
        <Box sx={{ display: "flex", gap: 1.5, alignItems: "flex-start" }}>
            <Box sx={{ color: "primary.main", mt: 0.25, display: "flex" }}>{icon}</Box>
            <Box sx={{ minWidth: 0 }}>
                <Typography variant="caption" color="text.secondary" display="block">
                    {label}
                </Typography>
                <Typography variant="body1" component="div">
                    {value}
                </Typography>
            </Box>
        </Box>
    );
}

export function getUserDisplayName(p_user: User): string {
    if (p_user.userName && p_user.userName !== p_user.email) {
        return p_user.userName;
    }

    const localPart: string = p_user.email.split("@")[0] ?? p_user.email;
    return localPart.charAt(0).toUpperCase() + localPart.slice(1);
}

export function getUserInitials(p_user: User, p_displayName?: string): string {
    const displayName: string = p_displayName ?? getUserDisplayName(p_user);
    const nameParts: string[] = displayName.trim().split(/\s+/).filter(Boolean);

    if (nameParts.length >= 2) {
        return `${nameParts[0].charAt(0)}${nameParts[1].charAt(0)}`.toUpperCase();
    }

    const source: string = p_user.userName || p_user.email;
    const parts: string[] = source.split(/[@._-]/).filter(Boolean);

    if (parts.length >= 2) {
        return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
    }

    return source.slice(0, 2).toUpperCase();
}

export default function UserSummaryCard({
    user,
    displayName,
    footer,
}: UserSummaryCardProps) {
    const resolvedDisplayName: string = displayName ?? getUserDisplayName(user);
    const initials: string = getUserInitials(user, resolvedDisplayName);
    const roleDisplayName: string = getAssignedRoleDisplayName(user);
    const roleChipColor = getRoleChipColorForAssignedRole(user.dynamicRoleId);
    const roleValue: ReactNode = user.dynamicRoleId ? (
        <MuiLink
            component={RouterLink}
            to={ROUTE_USER_ROLE_DETAILS.replace(":id", user.dynamicRoleId)}
            underline="hover"
        >
            {roleDisplayName}
        </MuiLink>
    ) : (
        roleDisplayName
    );

    return (
        <Card variant="outlined" sx={{ height: "100%" }}>
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
                            {resolvedDisplayName}
                        </Typography>
                        <Chip
                            label={roleDisplayName}
                            color={roleChipColor}
                            size="small"
                            sx={{ mt: 1 }}
                        />
                    </Box>
                </Box>

                <Divider />

                <Box sx={{ display: "grid", gap: 1.5 }}>
                    <ProfileRow
                        icon={<EmailIcon fontSize="small" />}
                        label="Courriel"
                        value={user.email}
                    />
                    <ProfileRow
                        icon={<PersonIcon fontSize="small" />}
                        label="Nom d'utilisateur"
                        value={user.userName}
                    />
                    <ProfileRow
                        icon={<BadgeIcon fontSize="small" />}
                        label={userAccessFieldLabels.assignedRole}
                        value={roleValue}
                    />
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
