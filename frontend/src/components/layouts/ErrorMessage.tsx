import { useNavigate } from "react-router-dom"
import { Box, Button, Paper, Stack, Typography, Collapse, Link } from "@mui/material";
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import RefreshIcon from '@mui/icons-material/Refresh';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useState } from "react";

interface ErrorMessageProps {
    errorMessage?: string;
    customMessage?: string;
    onRetry?: () => void;
    returnToUrl?: string;
    returnLabel?: string;
}

export default function ErrorMessage({
    errorMessage,
    customMessage,
    onRetry,
    returnToUrl = "/",
    returnLabel = "Retour à l'accueil"
}: ErrorMessageProps) {

    const navigate = useNavigate();
    const [showTechnical, setShowTechnical] = useState<boolean>(false);

    return (
        <Box
            display="flex"
            justifyContent="center"
            alignItems="center"
            minHeight="40vh"
        >
            <Paper
                elevation={3}
                sx={{
                    padding: 4,
                    maxWidth: 500,
                    textAlign: "center",
                    borderRadius: 2,
                    borderTop: "4px solid",
                    borderColor: "error.main"
                }}>

                <ErrorOutlineOutlinedIcon color="error" sx={{ fontSize: 60, mb: 2 }} />

                <Typography variant="h5" gutterBottom fontWeight="600">
                    Oups!
                </Typography>

                <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
                    {customMessage}
                </Typography>

                {errorMessage && (
                    <Box sx={{ mb: 3 }}>
                        <Link
                            component="button"
                            variant="caption"
                            onClick={() => setShowTechnical(!showTechnical)}
                            sx={{
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                margin: '0 auto',
                                color: 'text.secondary',
                                textDecoration: 'none'
                            }}
                        >
                            {showTechnical ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
                            {showTechnical ? "Cacher les détails techniques" : "Voir les détails techniques"}
                        </Link>

                        <Collapse in={showTechnical}>
                            <Paper
                                variant="outlined"
                                sx={{
                                    mt: 1,
                                    p: 1.5,
                                    bgcolor: 'grey.50',
                                    textAlign: 'left',
                                    overflowX: 'auto'
                                }}
                            >
                                <Typography variant="caption" sx={{ fontFamily: 'monospace', color: 'error.main' }}>
                                    {errorMessage}
                                </Typography>
                            </Paper>
                        </Collapse>
                    </Box>
                )

                }

                <Stack direction="row" spacing={2} justifyContent="center">
                    {onRetry && (
                        <Button
                            variant="contained"
                            color="primary"
                            startIcon={<RefreshIcon />}
                            onClick={onRetry}
                            sx={{ textTransform: "none" }}
                        >
                            Réessayer
                        </Button>
                    )}
                    <Button
                        variant="contained"
                        color="primary"
                        startIcon={<ArrowBackIcon />}
                        onClick={() => navigate(returnToUrl)}
                        sx={{ textTransform: "none" }}
                    >
                        {returnLabel}
                    </Button>
                </Stack>
            </Paper>
        </Box>
    );
}