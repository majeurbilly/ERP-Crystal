import type { ReactNode } from "react";
import { Box, Card, CardActionArea, CardContent, Typography } from "@mui/material";

export interface MySpaceNavCardConfig {
    id: string;
    label: string;
    preview: string;
    icon: ReactNode;
}

interface MySpaceNavCardsProps {
    cards: MySpaceNavCardConfig[];
    activeId: string;
    onSelect: (p_id: string) => void;
}

export default function MySpaceNavCards({ cards, activeId, onSelect }: MySpaceNavCardsProps) {
    return (
        <Box
            sx={{
                display: "grid",
                gridTemplateColumns: {
                    xs: "repeat(2, 1fr)",
                    sm: "repeat(3, 1fr)",
                    lg: "repeat(5, 1fr)",
                },
                gap: 2,
                mb: 3,
            }}
        >
            {cards.map((card) => {
                const isActive = card.id === activeId;

                return (
                    <Card
                        key={card.id}
                        variant="outlined"
                        sx={{
                            height: "100%",
                            borderWidth: isActive ? 2 : 1,
                            borderColor: isActive ? "primary.main" : "divider",
                            bgcolor: isActive ? "action.selected" : "background.paper",
                            transition: "border-color 0.2s, box-shadow 0.2s",
                            boxShadow: isActive ? 2 : 0,
                        }}
                    >
                        <CardActionArea
                            onClick={() => onSelect(card.id)}
                            sx={{ height: "100%", minHeight: 120 }}
                        >
                            <CardContent sx={{ textAlign: "center", py: 2 }}>
                                <Box
                                    sx={{
                                        display: "flex",
                                        justifyContent: "center",
                                        mb: 1,
                                        color: isActive ? "primary.main" : "text.secondary",
                                    }}
                                >
                                    {card.icon}
                                </Box>
                                <Typography
                                    variant="subtitle1"
                                    fontWeight={700}
                                    gutterBottom
                                >
                                    {card.label}
                                </Typography>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                    sx={{ minHeight: 40 }}
                                >
                                    {card.preview}
                                </Typography>
                            </CardContent>
                        </CardActionArea>
                    </Card>
                );
            })}
        </Box>
    );
}
