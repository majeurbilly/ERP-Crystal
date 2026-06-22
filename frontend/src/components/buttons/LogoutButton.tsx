import { Button } from "@mui/material";
import { useAuth } from "../../context/AuthContext";

export default function LogoutButton() {
    const { logout } = useAuth();
    return (
        <Button fullWidth onClick={logout} id="logout" sx={{ justifyContent: "flex-start", color: "text.primary", fontFamily: "Arial, Helvetica, sans-serif", fontWeight: 500, textTransform: "none", }}>
            Déconnexion
        </Button>
    );
}