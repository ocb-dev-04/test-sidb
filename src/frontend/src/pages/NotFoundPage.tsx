import { Link } from 'react-router-dom';
import { Card } from '../components/Card';
import { EmptyState } from '../components/States';
import { ROUTES } from '../layout/routes';

export function NotFoundPage() {
  return (
    <Card>
      <EmptyState
        title="Página no encontrada"
        description="La dirección que intentó abrir no existe o fue movida."
        action={
          <Link className="button button--primary" to={ROUTES.home}>
            Ir al inicio
          </Link>
        }
      />
    </Card>
  );
}
