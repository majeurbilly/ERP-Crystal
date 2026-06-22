import { useEffect, useState } from "react";
import { Box, Button, Card, CardContent, TextField, Typography } from "@mui/material";
import type { User } from "../../../data/types/hr/user";
import { useUserMutations } from "../../../api/mutations/hr/useUserMutations";
import { useFormValidation } from "../useFormValidation";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";

interface MyAccountSettingsFormProps {
    user: User;
}

const INITIAL_ERRORS = {
    email: "",
    password: "",
    confirmPassword: "",
};

export default function MyAccountSettingsForm({ user }: MyAccountSettingsFormProps) {
    const { updateMe, isUpdatingMe } = useUserMutations();
    const { errors, setErrors, clearErrors } = useFormValidation(INITIAL_ERRORS);

    const [email, setEmail] = useState(user.email);
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");

    useEffect(() => {
        setEmail(user.email);
        setPassword("");
        setConfirmPassword("");
        clearErrors();
    }, [user.email, user.id]);

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    const validate = (): boolean => {
        const newErrors = { ...INITIAL_ERRORS };
        let isValid = true;

        if (!email.trim()) {
            newErrors.email = "Courriel requis";
            isValid = false;
        } else if (!emailRegex.test(email)) {
            newErrors.email = "Courriel invalide";
            isValid = false;
        }

        if (password.trim() && password.trim().length < 8) {
            newErrors.password = "Le mot de passe doit contenir au moins 8 caractères.";
            isValid = false;
        }

        if (password.trim() && password !== confirmPassword) {
            newErrors.confirmPassword = "Les mots de passe ne correspondent pas.";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        const trimmedEmail = email.trim();
        const payload = {
            email: trimmedEmail,
            userName: trimmedEmail,
            ...(password.trim() ? { password: password.trim() } : {}),
        };

        try {
            await updateMe(payload);
            setPassword("");
            setConfirmPassword("");
            notifySuccessMessage("Compte mis à jour avec succès !");
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    return (
        <Card variant="outlined" sx={{ maxWidth: 520 }}>
            <CardContent>
                <Typography variant="h6" gutterBottom>
                    Courriel et mot de passe
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    Modifiez votre courriel de connexion et/ou votre mot de passe.
                </Typography>

                <Box component="form" onSubmit={handleSubmit} sx={{ display: "grid", gap: 2 }}>
                    <TextField
                        fullWidth
                        label="Courriel"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        error={!!errors.email}
                        helperText={errors.email}
                        autoComplete="email"
                    />
                    <TextField
                        fullWidth
                        label="Nouveau mot de passe"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        error={!!errors.password}
                        helperText={errors.password || "Laissez vide pour conserver le mot de passe actuel."}
                        autoComplete="new-password"
                    />
                    <TextField
                        fullWidth
                        label="Confirmer le nouveau mot de passe"
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        error={!!errors.confirmPassword}
                        helperText={errors.confirmPassword}
                        autoComplete="new-password"
                        disabled={!password.trim()}
                    />
                    <Box sx={{ display: "flex", justifyContent: "flex-start" }}>
                        <Button type="submit" variant="contained" disabled={isUpdatingMe}>
                            {isUpdatingMe ? "Enregistrement…" : "Enregistrer les modifications"}
                        </Button>
                    </Box>
                </Box>
            </CardContent>
        </Card>
    );
}
