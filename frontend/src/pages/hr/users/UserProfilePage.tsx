import { useParams } from "react-router-dom";
import UserProfilePageLayout from "../../../components/layouts/UserProfilePageLayout";

export default function UserProfilePage() {
    const { id } = useParams();

    if (!id) return <p>no id</p>;

    return (
        <>
            <UserProfilePageLayout myProfile={false} userId={id} />
        </>
    );
}