import type { UserApiDTO, User } from "../../types/hr/user";

import { createDataMapper } from "../dataMapper";

import { DEFAULT_ASSIGNED_ROLE_ID } from "../../types/hr/userRoles";



export const userMapper = createDataMapper<UserApiDTO, User>({

    toDomain: (dto: UserApiDTO) => ({

        id: dto.id,

        userName: dto.userName,

        email: dto.email,

        dynamicRoleId: dto.dynamicRoleId ?? null,

        dynamicRoleName: dto.dynamicRoleName ?? null,

    }) as User,

    toApi: (domain: User & { password?: string }) => {

        const assignedRoleId: string = domain.dynamicRoleId ?? DEFAULT_ASSIGNED_ROLE_ID;



        return {

            id: domain.id,

            email: domain.email,

            userName: domain.userName,

            dynamicRoleId: assignedRoleId,

            ...(domain.password && { password: domain.password }),

        } as UserApiDTO;

    },

});

