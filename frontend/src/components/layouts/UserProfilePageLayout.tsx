import { getAssignedRoleDisplayName } from "../../data/types/hr/userRoles";
import { useQuery } from "@tanstack/react-query";
import userService from "../../api/services/hr/userService";
import employeeProfileService from "../../api/services/hr/employeeProfileService";
import { usersCacheKey, employeeProfilesCacheKey } from "../../data/cacheKeys";
import {
    ROUTE_EMPLOYEE_PROFILE_DETAILS,
    ROUTE_LIST_USERS,
    ROUTE_MON_ESPACE,
} from "../../data/routeNames";
import PageQueryWrapper from "./PageQueryWrapper";
import GenericPageLayout from "./GenericPageLayout";
import EmployeeProfileSummaryCard from "../hr-components/EmployeeProfileSummaryCard";
import MyAccountSettingsForm from "../forms/hr/MyAccountSettingsForm";
import UserSummaryCard, { getUserDisplayName } from "../hr-components/UserSummaryCard";
import { Box, Link as MuiLink, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import { useUserMutations } from "../../api/mutations/hr/useUserMutations";
import { useDeleteDialog } from "../../context/DeleteDialogContext";
import { ENTITY_TYPES } from "../../permissions/permissions";
import { usePermissions } from "../../permissions/usePermissions";
import { useAuth } from "../../context/AuthContext";
import type { EmployeeProfile } from "../../data/types/hr/employeeProfile";
import type { User } from "../../data/types/hr/user";

interface UserProfilePageLayoutProps {
    userId?: string;
    myProfile: boolean;
}

function resolveLinkedEmployeeProfile(
    p_profiles: EmployeeProfile[],
    p_applicationUserId: string,
): EmployeeProfile | null {
    return p_profiles.find((p_profile) => p_profile.applicationUserId === p_applicationUserId) ?? null;
}

function resolvePageTitle(
    p_user: User,
    p_isMyProfilePage: boolean,
    p_linkedEmployee?: EmployeeProfile | null,
): string {
    if (p_isMyProfilePage) {
        return "Mon compte utilisateur";
    }

    if (p_linkedEmployee) {
        return `${p_linkedEmployee.firstName} ${p_linkedEmployee.lastName}`;
    }

    return getUserDisplayName(p_user);
}

export default function UserProfilePageLayout({ userId, myProfile }: UserProfilePageLayoutProps) {
    const { openForm } = useFormContainer();
    const { user: me } = useAuth();

    const isMyProfilePage: boolean = myProfile || userId === me?.id;

    const { data: user, isLoading, error, refetch } = useQuery({
        queryKey: isMyProfilePage ? usersCacheKey.me() : usersCacheKey.details(userId!),
        queryFn: () => (isMyProfilePage ? userService.getMe() : userService.getUserById(userId!)),
        enabled: isMyProfilePage || !!userId,
    });

    const { data: myEmployeeProfile } = useQuery({
        queryKey: employeeProfilesCacheKey.me(),
        queryFn: () => employeeProfileService.getMe(),
        enabled: isMyProfilePage,
        retry: false,
    });

    const { data: linkedEmployeeProfile } = useQuery({
        queryKey: employeeProfilesCacheKey.byApplicationUserId(userId ?? ""),
        queryFn: async () => {
            const profiles: EmployeeProfile[] = await employeeProfileService.getAll();
            return resolveLinkedEmployeeProfile(profiles, userId!);
        },
        enabled: !isMyProfilePage && !!userId,
        retry: false,
    });

    const { canUpdate: canUpdateUser, canDelete: canDeleteUser } = usePermissions(ENTITY_TYPES.USER);
    const { canDelete: canDeleteMe } = usePermissions(ENTITY_TYPES.ME);
    const { canRead: canReadEmployeeSalary } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);

    const canUpdate: boolean = isMyProfilePage ? true : canUpdateUser;
    const canDelete: boolean = isMyProfilePage ? canDeleteMe : canDeleteUser;

    const { deleteUser: deleteUserMutation } = useUserMutations();
    const { openConfirmDeleteWindow } = useDeleteDialog();

    const employeeProfile: EmployeeProfile | null | undefined = isMyProfilePage
        ? myEmployeeProfile
        : linkedEmployeeProfile;

    return (
        <PageQueryWrapper
            isLoading={isLoading}
            error={error || (!user && !isLoading ? { message: "Utilisateur introuvable" } : null)}
            refetch={refetch}
            errorReturnUrl={isMyProfilePage ? ROUTE_MON_ESPACE : ROUTE_LIST_USERS}
            errorReturnLabel={isMyProfilePage ? "Retour à Mon espace" : "Retour à la liste des utilisateurs"}
        >
            {user && (
                <GenericPageLayout
                    title={resolvePageTitle(user, isMyProfilePage, employeeProfile)}
                    subtitle={
                        isMyProfilePage
                            ? "Gérez vos informations de connexion et consultez vos accès."
                            : `${getAssignedRoleDisplayName(user)} · ${user.email}`
                    }
                    onEditClick={
                        !isMyProfilePage && canUpdate
                            ? () => openForm(FORM_TYPES.USER, user)
                            : undefined
                    }
                    onDeleteClick={
                        canDelete
                            ? () =>
                                openConfirmDeleteWindow({
                                    id: user.id,
                                    displayLabel: user.userName,
                                    onDelete: deleteUserMutation,
                                    redirectUrl: ROUTE_LIST_USERS,
                                })
                            : undefined
                    }
                >
                    <Box
                        sx={{
                            display: "grid",
                            gap: 3,
                            textAlign: "left",
                            gridTemplateColumns: {
                                xs: "1fr",
                                lg: employeeProfile ? "minmax(0, 1fr) minmax(0, 1fr)" : "minmax(0, 560px)",
                            },
                            alignItems: "start",
                        }}
                    >
                        <UserSummaryCard
                            user={user}
                            displayName={
                                employeeProfile
                                    ? `${employeeProfile.firstName} ${employeeProfile.lastName}`
                                    : undefined
                            }
                        />

                        {employeeProfile && (
                            <Box sx={{ display: "grid", gap: 1.5 }}>
                                <Typography variant="h6">
                                    {isMyProfilePage ? "Ma fiche employé" : "Fiche employé liée"}
                                </Typography>
                                <EmployeeProfileSummaryCard
                                    profile={employeeProfile}
                                    showSalary={!isMyProfilePage && canReadEmployeeSalary}
                                    footer={
                                        <MuiLink
                                            component={RouterLink}
                                            to={
                                                isMyProfilePage
                                                    ? `${ROUTE_MON_ESPACE}?tab=fiche`
                                                    : ROUTE_EMPLOYEE_PROFILE_DETAILS.replace(
                                                        ":id",
                                                        String(employeeProfile.id),
                                                    )
                                            }
                                        >
                                            {isMyProfilePage
                                                ? "Voir ma fiche employé complète dans Mon espace"
                                                : "Voir la fiche employé complète"}
                                        </MuiLink>
                                    }
                                />
                            </Box>
                        )}
                    </Box>

                    {isMyProfilePage && (
                        <Box sx={{ mt: 3, maxWidth: 560 }}>
                            <Typography variant="h6" sx={{ mb: 2 }}>
                                Paramètres du compte
                            </Typography>
                            <MyAccountSettingsForm user={user} />
                        </Box>
                    )}
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
