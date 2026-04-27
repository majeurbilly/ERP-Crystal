import { Button } from "@mui/material";
import { useAuth } from "../../context/AuthContext";

export default function LogoutButton() {
    const { logout } = useAuth();
    return (
        <Button onClick={logout} id="logout" sx={{ color: "text.primary", fontFamily: "Arial, Helvetica, sans-serif", fontWeight: 500, textTransform: "none", "&:hover": { backgroundColor: "rgba(255,255,255,0.12)", }, }}>
            Déconnexion
        </Button>
    );
}