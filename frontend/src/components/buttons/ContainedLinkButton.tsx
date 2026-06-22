import { Button, type ButtonProps } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";

interface ContainedLinkButtonProps {
    linkTo: string;
    buttonText: string;
    sx?: ButtonProps["sx"];
}

export default function ContainedLinkButton({ linkTo, buttonText, sx }: ContainedLinkButtonProps) {
    return (
        <Button
            component={RouterLink}
            to={linkTo}
            variant="contained"
            sx={sx}
        >
            {buttonText}
        </Button>
    );
}
