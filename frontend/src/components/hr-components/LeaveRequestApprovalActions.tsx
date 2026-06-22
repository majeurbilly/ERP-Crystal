import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import HighlightOffIcon from "@mui/icons-material/HighlightOff";
import { Button, IconButton, Stack, Tooltip } from "@mui/material";

interface LeaveRequestApprovalActionsProps {
    disabled?: boolean;
    onApprove: () => void;
    onReject: () => void;
    size?: "small" | "medium";
    variant?: "default" | "compact";
}

export default function LeaveRequestApprovalActions({
    disabled = false,
    onApprove,
    onReject,
    size = "medium",
    variant = "default",
}: LeaveRequestApprovalActionsProps) {
    if (variant === "compact") {
        return (
            <Stack direction="row" spacing={0.5} alignItems="center">
                <Tooltip title="Refuser">
                    <span>
                        <IconButton
                            size="small"
                            color="error"
                            disabled={disabled}
                            onClick={onReject}
                            aria-label="Refuser"
                        >
                            <HighlightOffIcon fontSize="small" />
                        </IconButton>
                    </span>
                </Tooltip>
                <Tooltip title="Approuver">
                    <span>
                        <IconButton
                            size="small"
                            color="success"
                            disabled={disabled}
                            onClick={onApprove}
                            aria-label="Approuver"
                        >
                            <CheckCircleOutlineIcon fontSize="small" />
                        </IconButton>
                    </span>
                </Tooltip>
            </Stack>
        );
    }

    return (
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Button
                size={size}
                variant="outlined"
                color="error"
                disabled={disabled}
                startIcon={<HighlightOffIcon />}
                onClick={onReject}
            >
                Refuser
            </Button>

            <Button
                size={size}
                variant="contained"
                color="success"
                disabled={disabled}
                startIcon={<CheckCircleOutlineIcon />}
                onClick={onApprove}
            >
                Approuver
            </Button>
        </Stack>
    );
}