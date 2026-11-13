import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/** Convierte los errores HTTP (ProblemDetails del backend) en un Error con mensaje legible. */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(catchError((err: unknown) => throwError(() => new Error(describeHttpError(err)))));

export function describeHttpError(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) return err instanceof Error ? err.message : 'Error inesperado.';
  if (err.status === 0) return 'No se pudo conectar con el servidor. ¿Está corriendo el backend en el puerto 5090?';
  if (err.status === 403) return 'No tienes permiso para esta acción.';

  const problem = err.error as { title?: string; detail?: string } | null;
  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;
  return `Error ${err.status}`;
}
