import ErrorMessage from "../components/layouts/ErrorMessage";
import { ROUTE_DASHBOARD } from "../data/routeNames";

export default function NotFoundPage() {
    return (
        <ErrorMessage
            customMessage="Désolé, la page que vous recherchez n'existe pas ou a été déplacée."
            errorMessage="Error 404: Page Not Found"
            returnToUrl={ROUTE_DASHBOARD}
            returnLabel="Retour au tableau de bord"
        />
    )
}