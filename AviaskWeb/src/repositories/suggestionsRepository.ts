import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import { AviaskQuestionDetails, type responseModels } from "@/models";
import { AviaskQuestion } from "@/models/aviaskQuestion";
import type { ApiResponse, FilteredResult, QuestionDetails } from "@/models/DTO/responseModels";

export class SuggestionRepository extends Repository {
    private routeMap = {
        index: (page: number) => `/api/suggestions?page=${page}`,
        show: (id: string) => `/api/suggestion/${id}`,
        new: () => `/api/suggestions/new`,
        edit: (id: string) => `/api/suggestion/${id}`,
        updateStatus: (id: string, status: number) => `api/suggestion/${id}/${status}`,
    };

    async getAll(page: number = 1): Promise<FilteredResult<AviaskQuestionDetails>> {
        const res = await this.$request<FilteredResult<QuestionDetails>>("get", this.routeMap.index(page));

        if (!res) return { elements: [], totalCount: 0 };

        return {
            elements: res.result.elements.map(s => new AviaskQuestionDetails(s)),
            totalCount: res.result.totalCount,
        };
    }

    async get(id: string): Promise<ApiResponse<AviaskQuestion>> {
        const res = await this.$request<responseModels.Question>("get", this.routeMap.show(id));

        if (!res.success)
            return {
                success: false,
                result: null!,
                error: res.error,
            };

        return {
            success: true,
            result: new AviaskQuestion(res.result),
            error: null!,
        };
    }

    async create(question: responseModels.Question): Promise<ApiResponse<responseModels.QuestionDetails>> {
        return await this.$request<responseModels.QuestionDetails>("post", this.routeMap.new(), question);
    }

    async edit(id: string, question: responseModels.Question): Promise<ApiResponse<responseModels.QuestionDetails>> {
        return await this.$request<responseModels.QuestionDetails>("put", this.routeMap.edit(id), question);
    }

    async updateStatus(id: string, status: number): Promise<boolean> {
        const res = await this.$request<boolean>("patch", this.routeMap.updateStatus(id, status));

        return res.success;
    }
}

export default function useSuggestionsRepository() {
    const apiStore = useApiStore();

    return new SuggestionRepository(apiStore.$request, apiStore.$requestForm);
}
