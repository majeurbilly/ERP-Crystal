import { Box, Typography, Stack } from "@mui/material";
import type { ReactNode } from "react";

interface DetailsPageHeaderProps {
    title: string;
    subtitle?: string;
    canEdit: boolean;
    canDelete: boolean;
    editButton?: ReactNode;
    deleteButton?: ReactNode;
}

export default function GenericPageHeader({
    title,
    subtitle,
    canEdit,
    canDelete,
    editButton,
    deleteButton
}: DetailsPageHeaderProps) {
    return (
        <Box sx={{ mb: 4, pb: 2, borderBottom: '1px solid', borderColor: 'secondary.main', width: '100%' }}>
            <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="flex-start"
                sx={{ width: '100%' }}
            >
                <Box sx={{ textAlign: 'left', flexGrow: 1 }}>
                    <Typography
                        variant="h4"
                        component="h1"
                        gutterBottom
                        sx={{ textAlign: 'left' }}
                    >
                        {title}
                    </Typography>
                    {subtitle && (
                        <Typography
                            variant="subtitle1"
                            color="text.secondary"
                            sx={{ textAlign: 'left' }}
                        >
                            {subtitle}
                        </Typography>
                    )}
                </Box>

                <Stack direction="row" spacing={2} sx={{ flexShrink: 0 }}>
                    {canEdit && editButton}
                    {canDelete && deleteButton}
                </Stack>
            </Stack>
        </Box>
    );
}