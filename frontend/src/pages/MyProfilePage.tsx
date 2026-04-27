import { getUserById } from "../api/userService"
import { useAuth } from "../context/AuthContext"
import { useQuery } from "@tanstack/react-query"
import UserProfilePageLayout from "../components/UserProfilePageLayout";

export default function MyProfilePage() {
    const { id } = useAuth();

    const { data: user, isLoading, error } = useQuery({
        queryKey: ["user", id],
        queryFn: () => getUserById(id!),
        enabled: !!id,
    });

    if (isLoading) return <p>loading...</p>;
    if (error) return <p>error</p>;

    return (
        <>
            <UserProfilePageLayout user={user!} />
        </>
    )
}