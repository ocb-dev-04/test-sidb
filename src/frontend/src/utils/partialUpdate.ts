import type { ServiceRequestFormValues } from '../schemas/serviceRequestSchemas';
import type { UpdateServiceRequestRequest } from '../types/api';

// PATCH parcial: solo viajan los campos que cambiaron; los nulos no se modifican en el backend.
export function buildPartialUpdate(
  original: ServiceRequestFormValues,
  edited: ServiceRequestFormValues,
): UpdateServiceRequestRequest {
  const changes: UpdateServiceRequestRequest = {};
  if (edited.title !== original.title) {
    changes.title = edited.title;
  }
  if (edited.description !== original.description) {
    changes.description = edited.description;
  }
  if (edited.priority !== original.priority) {
    changes.priority = edited.priority;
  }
  if (edited.areaId !== original.areaId) {
    changes.areaId = edited.areaId;
  }
  if (edited.requestTypeId !== original.requestTypeId) {
    changes.requestTypeId = edited.requestTypeId;
  }
  if (edited.evidenceReference !== original.evidenceReference) {
    changes.evidenceReference = edited.evidenceReference;
  }
  return changes;
}
