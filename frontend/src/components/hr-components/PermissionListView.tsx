import {
    Box,
    Chip,
    Paper,
    Stack,
    Typography,
} from "@mui/material";
import AdminPanelSettingsIcon from "@mui/icons-material/AdminPanelSettings";
import type { PermissionRule } from "../../data/types/hr/dynamicUserRole";
import {
    formatInventoryScopeSuffix,
    formatPermissionSentence,
    getActionLabel,
    groupPermissionsByEntity,
    isFullAdminAccess,
    isInventoryPermission
} from "../../permissions/permissionLabels";

interface PermissionListViewProps {
    permissions: PermissionRule[];
    emptyMessage?: string;
    locationTitlesById?: Record<number, string>;
}

function getActionChipColor(p_action: string): "default" | "primary" | "secondary" | "success" | "warning" | "error" | "info" {
    if (p_action === "manage") {
        return "primary";
    }
    if (p_action === "delete") {
        return "error";
    }
    if (p_action === "create") {
        return "success";
    }
    if (p_action === "update") {
        return "warning";
    }
    return "info";
}

export default function PermissionListView({
    permissions,
    emptyMessage = "Aucun droit configuré pour ce rôle.",
    locationTitlesById,
}: PermissionListViewProps) {
    if (permissions.length === 0) {
        return (
            <Paper
                variant="outlined"
                sx={{
                    p: 4,
                    textAlign: "center",
                    borderRadius: 2,
                    bgcolor: "background.default",
                }}
            >
                <Typography color="text.secondary">{emptyMessage}</Typography>
            </Paper>
        );
    }

    if (isFullAdminAccess(permissions)) {
        return (
            <Paper
                variant="outlined"
                sx={{
                    p: 3,
                    borderRadius: 2,
                    bgcolor: "background.default",
                    borderColor: "primary.main",
                }}
            >
                <Stack direction="row" spacing={2} alignItems="center">
                    <AdminPanelSettingsIcon color="primary" sx={{ fontSize: 40 }} />
                    <Box>
                        <Typography variant="h6" fontWeight={700}>
                            Administrateur
                        </Typography>
                        <Typography color="text.secondary">
                            Ce rôle a un accès complet à toutes les sections de l&apos;application.
                        </Typography>
                    </Box>
                </Stack>
            </Paper>
        );
    }

    const grouped = groupPermissionsByEntity(permissions);

    return (
        <Stack spacing={2}>
            {grouped.map((p_group) => (
                <Paper
                    key={p_group.subject}
                    variant="outlined"
                    sx={{
                        p: 2.5,
                        borderRadius: 2,
                        bgcolor: "background.default",
                    }}
                >
                    <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1.5 }}>
                        {p_group.subjectLabel}
                    </Typography>
                    <Stack direction="row" flexWrap="wrap" gap={1}>
                        {p_group.rules.map((p_rule) => {
                            const chipLabel = isInventoryPermission(p_rule.subject)
                                ? `${getActionLabel(p_rule.action)}${formatInventoryScopeSuffix(p_rule, locationTitlesById)}`
                                : getActionLabel(p_rule.action);

                            return (
                                <Chip
                                    key={`${p_rule.subject}_${p_rule.action}_${p_rule.locationScope ?? "global"}`}
                                    label={chipLabel}
                                    color={getActionChipColor(p_rule.action)}
                                    size="small"
                                    sx={{ fontWeight: 600 }}
                                />
                            );
                        })}
                    </Stack>
                </Paper>
            ))}
        </Stack>
    );
}

export function PermissionSentenceList({
    permissions,
    locationTitlesById,
}: {
    permissions: PermissionRule[];
    locationTitlesById?: Record<number, string>;
}) {
    return (
        <Stack spacing={1}>
            {permissions.map((p_rule, p_index) => (
                <Typography key={`${p_rule.subject}_${p_rule.action}_${p_index}`} variant="body2">
                    {formatPermissionSentence(p_rule, locationTitlesById)}
                </Typography>
            ))}
        </Stack>
    );
}
