import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { canAddInternalComments } from '../auth/permissions';
import { useToast } from '../hooks/useToast';
import { COMMENT_SCHEMA } from '../schemas/serviceRequestSchemas';
import type { CommentFormValues } from '../schemas/serviceRequestSchemas';
import { COMMENT_TEXT_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { QUERY_KEYS } from '../services/queryKeys';
import { serviceRequestService } from '../services/serviceRequestService';
import type { CommentResponse, UserRole } from '../types/api';
import { COMMENT_VISIBILITIES } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { formatDateTime } from '../utils/formatting';
import { VISIBILITY_LABELS } from '../utils/labels';
import { VisibilityBadge } from './Badges';
import { FormField } from './FormField';
import { Alert, EmptyState } from './States';

interface CommentsSectionProps {
  serviceRequestId: string;
  comments: CommentResponse[];
  role: UserRole | undefined;
}

export function CommentsSection({ serviceRequestId, comments, role }: CommentsSectionProps) {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const allowInternal = canAddInternalComments(role);
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CommentFormValues>({
    resolver: zodResolver(COMMENT_SCHEMA),
    defaultValues: { text: '', visibility: 'Public' },
  });

  const addComment = useMutation({
    mutationFn: (values: CommentFormValues) =>
      serviceRequestService.addComment(serviceRequestId, {
        text: values.text,
        visibility: allowInternal ? values.visibility : 'Public',
      }),
    onSuccess: async () => {
      reset({ text: '', visibility: 'Public' });
      showToast('Comentario agregado.');
      await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequestDetail(serviceRequestId) });
    },
  });

  return (
    <div className="stack">
      {comments.length === 0 ? (
        <EmptyState title="Aún no hay comentarios" />
      ) : (
        <ul className="comment-list">
          {comments.map((comment) => (
            <li
              key={comment.id}
              className={`comment ${comment.visibility === 'Internal' ? 'comment--internal' : ''}`.trim()}
            >
              <div className="comment__header">
                <strong>{comment.authorName}</strong>
                <VisibilityBadge visibility={comment.visibility} />
                <span className="text-muted">{formatDateTime(comment.createdOnUtc)}</span>
              </div>
              <div className="prose">{comment.text}</div>
            </li>
          ))}
        </ul>
      )}

      <form className="stack" onSubmit={handleSubmit((values) => addComment.mutate(values))} noValidate>
        {addComment.isError && <Alert tone="error">{errorMessage(addComment.error)}</Alert>}
        <FormField label="Nuevo comentario" required error={errors.text?.message}>
          <textarea className="textarea" rows={3} maxLength={COMMENT_TEXT_MAXIMUM_LENGTH} {...register('text')} />
        </FormField>
        <div className="inline" style={{ justifyContent: 'space-between' }}>
          {allowInternal ? (
            <label className="inline">
              <span className="field__label">Visibilidad</span>
              <select className="select" style={{ width: 'auto' }} {...register('visibility')}>
                {COMMENT_VISIBILITIES.map((visibility) => (
                  <option key={visibility} value={visibility}>
                    {VISIBILITY_LABELS[visibility]}
                  </option>
                ))}
              </select>
            </label>
          ) : (
            <span className="field__hint">Su comentario será visible para el equipo que atiende la solicitud.</span>
          )}
          <button type="submit" className="button button--primary" disabled={addComment.isPending}>
            {addComment.isPending ? 'Enviando…' : 'Agregar comentario'}
          </button>
        </div>
      </form>
    </div>
  );
}
