import { useMutation, useQueryClient } from "@tanstack/react-query"


interface CRUDService<T, CreateDTO> {
    add: (data: CreateDTO) => Promise<T>;
    delete: (id: string) => Promise<void>;
    update: (id: string, data: Partial<T>) => Promise<T>;
}

export const useGenericMutations = <T, CreateDTO = T>(
    service: CRUDService<T, CreateDTO>,
    queryKey: ReadonlyArray<unknown>,
    getExtraInvalidateKeys?: (variables: { id: string; data: Partial<T> }) => ReadonlyArray<unknown>[],
    additionalInvalidateKeys?: ReadonlyArray<unknown>[]
) => {
    const queryClient = useQueryClient();

    const invalidateAll = (): void => {
        queryClient.invalidateQueries({ queryKey });

        if (additionalInvalidateKeys) {
            for (const extraKey of additionalInvalidateKeys) {
                queryClient.invalidateQueries({ queryKey: extraKey });
            }
        }
    };

    const addMutation = useMutation({
        mutationFn: (newData: CreateDTO) => service.add(newData),
        onSuccess: () => invalidateAll(),
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) => service.delete(id),
        onSuccess: () => invalidateAll(),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string; data: Partial<T> }) =>
            service.update(id, data),
        onSuccess: (_data, variables) => {
            invalidateAll();

            if (getExtraInvalidateKeys) {
                const extraKeys = getExtraInvalidateKeys(variables);
                extraKeys.forEach((key) => {
                    queryClient.invalidateQueries({ queryKey: key })
                })
            }
        }
    });

    return {
        add: addMutation.mutateAsync,
        isAdding: addMutation.isPending,
        addError: addMutation.error,

        delete: deleteMutation.mutateAsync,
        isDeleting: deleteMutation.isPending,
        deleteError: deleteMutation.error,

        update: updateMutation.mutateAsync,
        isUpdating: updateMutation.isPending,
        updateError: updateMutation.error,
    };
};