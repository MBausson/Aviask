import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import { getQueryParams } from "@/helper";
import { AviaskQuestionDetails, type requestModels, type responseModels } from "@/models";
import { AxiosError } from "axios";
import { type ApiResponse, type FilteredResult, type UserExtended } from "@/models/DTO/responseModels";

export class UsersRepository extends Repository {
    private routeMap = {
        index: (page: number) => `/api/users?page=${page}`,
        show: (id: string) => `/api/user/${id}`,
        showExtended: (id: string) => `/api/user/extended/${id}`,
        new: () => `/api/users`,
        update: (id: string) => `/api/user/${id}/update`,
        edit: (id: string) => `/api/user/${id}`,
        destroy: (id: string) => `/api/user/${id}`,
        statistics: (id: string | null) => `/api/user/statistics?${getQueryParams({ id: id })}`,
        publications: (page: number) => `/api/user/publications?page=${page}`,
        leaderboard: () => `/api/users/leaderboard`,
    };

    async getAll(page: number = 1): Promise<FilteredResult<UserExtended>> {
        const result = await this.$request<FilteredResult<UserExtended>>("get", this.routeMap.index(page));

        if (!result.success) return { elements: [], totalCount: 0 };

        return result.result;
    }

    async create(model: requestModels.CreateUserModel): Promise<ApiResponse<responseModels.AuthenticationResult>> {
        try {
            return await this.$request<responseModels.AuthenticationResult>("post", this.routeMap.new(), model);
        } catch (e) {
            if (e instanceof AxiosError) return e.response?.data;

            throw e;
        }
    }

    async update(userId: string): Promise<ApiResponse<requestModels.EditUserModel>> {
        return await this.$request<requestModels.EditUserModel>("get", this.routeMap.update(userId));
    }

    async edit(userId: string, editUserModel: requestModels.EditUserModel): Promise<ApiResponse<string>> {
        return await this.$request<string>("put", this.routeMap.edit(userId), editUserModel);
    }

    async delete(userId: string): Promise<boolean> {
        const result = await this.$request<string>("delete", this.routeMap.destroy(userId));

        return result.success;
    }

    async statistics(userId: string | null = null): Promise<ApiResponse<responseModels.UserStatistics>> {
        return await this.$request<responseModels.UserStatistics>("get", this.routeMap.statistics(userId));
    }

    async publications(page: number): Promise<FilteredResult<AviaskQuestionDetails>> {
        const result = await this.$request<FilteredResult<responseModels.QuestionDetails>>(
            "get",
            this.routeMap.publications(page),
        );

        if (result.success)
            return {
                elements: result.result.elements.map(r => new AviaskQuestionDetails(r)),
                totalCount: result.result.totalCount,
            };

        return { elements: [], totalCount: 0 };
    }

    async profile(userId: string): Promise<ApiResponse<responseModels.UserProfile>> {
        return await this.$request<responseModels.UserProfile>("get", this.routeMap.show(userId));
    }

    async leaderboard(): Promise<ApiResponse<responseModels.CurrentLeaderboard>> {
        return await this.$request<responseModels.CurrentLeaderboard>("get", this.routeMap.leaderboard());
    }
}

export default function useUsersRepository() {
    const apiStore = useApiStore();

    return new UsersRepository(apiStore.$request, apiStore.$requestForm);
}
