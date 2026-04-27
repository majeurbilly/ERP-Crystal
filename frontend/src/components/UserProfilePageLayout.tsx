
import type { User } from "../data/types/user"

interface UserProfilePageLayoutProps {
    user: User;
}

export default function UserProfilePageLayout({ user }: UserProfilePageLayoutProps) {
    return (
        <>
            <p>
                {user.userName}
            </p>
        </>
    )
}