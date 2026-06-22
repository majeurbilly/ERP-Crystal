import { useAuth } from "../context/AuthContext"
import UserProfilePageLayout from "../components/layouts/UserProfilePageLayout";

export default function MyProfilePage() {
    const { user } = useAuth();

    if (!user?.id) return <p>no id</p>;

    return (
        <>
            <UserProfilePageLayout myProfile={true} />
        </>
    );
}