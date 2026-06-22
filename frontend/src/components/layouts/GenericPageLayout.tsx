import { DeleteButton, EditButton } from "../buttons/AddEditDeleteButtons";
import GenericPageHeader from "./GenericPageHeader";

interface GenericDetailsLayoutProps {
    title: string;
    subtitle?: string;
    onEditClick?: () => void;
    onDeleteClick?: () => void;
    children: React.ReactNode;
}

export default function GenericPageLayout({
    title,
    subtitle,
    onEditClick = undefined,
    onDeleteClick = undefined,
    children
}: GenericDetailsLayoutProps) {

    return (
        <>
            <GenericPageHeader
                title={title}
                subtitle={subtitle}
                canEdit={!!onEditClick}
                canDelete={!!onDeleteClick}
                editButton={onEditClick ? <EditButton label="Modifier" onClick={onEditClick} /> : undefined}
                deleteButton={onDeleteClick ? <DeleteButton label="Supprimer" onClick={onDeleteClick} /> : undefined}
            />
            {children}
        </>
    );
}