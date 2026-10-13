import { isRouteErrorResponse, Link, useRouteError } from 'react-router-dom';
import { Card } from '../components/Card';
import { ROUTES } from '../layout/routes';

const HTTP_STATUS_NOT_FOUND = 404;

function describeError(error: unknown): { title: string; description: string } {
  if (isRouteErrorResponse(error) && error.status === HTTP_STATUS_NOT_FOUND) {
    return { title: 'Página no encontrada', description: 'La dirección que intentó abrir no existe o fue movida.' };
  }
  return {
    title: 'Algo salió mal',
    description: 'Ocurrió un error inesperado al mostrar esta pantalla. Recargue la página o vuelva al inicio.',
  };
}

export function RouteErrorPage({ standalone = false }: { standalone?: boolean }) {
  const { title, description } = describeError(useRouteError());

  const content = (
    <Card>
      <div className="state" role="alert">
        <span className="state__title">{title}</span>
        <span>{description}</span>
        <span className="inline">
          <button type="button" className="button button--secondary" onClick={() => window.location.reload()}>
            Recargar
          </button>
          <Link className="button button--primary" to={ROUTES.home}>
            Ir al inicio
          </Link>
        </span>
      </div>
    </Card>
  );

  return standalone ? <main className="error-page">{content}</main> : content;
}
