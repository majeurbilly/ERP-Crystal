import { Box, Card, CardActionArea, CardContent, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import type { ReactNode } from "react";

interface DashboardWidgetProps {
    title: string;
    value: string | number;
    subtitle?: string;
    icon?: ReactNode;
    to: string;
}

export default function DashboardWidget({ title, value, subtitle, icon, to }: DashboardWidgetProps) {
    const navigate = useNavigate();

    return (
        <Card variant="outlined" sx={{ height: "100%" }}>
            <CardActionArea onClick={() => navigate(to)} sx={{ height: "100%" }}>
                <CardContent>
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 1 }}>
                        <Box>
                            <Typography variant="body2" color="text.secondary" gutterBottom>
                                {title}
                            </Typography>
                            <Typography variant="h5" fontWeight={600}>
                                {value}
                            </Typography>
                            {subtitle && (
                                <Typography variant="caption" color="text.secondary">
                                    {subtitle}
                                </Typography>
                            )}
                        </Box>
                        {icon}
                    </Box>
                </CardContent>
            </CardActionArea>
        </Card>
    );
}
