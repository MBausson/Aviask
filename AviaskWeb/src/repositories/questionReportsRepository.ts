import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import { AviaskQuestionReport, type requestModels, type responseModels } from "@/models";
import type { ApiResponse, FilteredResult } from "@/models/DTO/responseModels";

export class QuestionReportsRepostory extends Repository {
    private routeMap = {
        index: (page: number) => `/api/questionReports?page=${page}`,
        show: (id: string) => `/api/questionReports/${id}`,
        new: () => `/api/questionReports`,
        edit: (id: string, state: number) => `/api/questionReport/${id}?state=${state}`,
        delete: (id: string) => `/api/questionReport/${id}`,
    };

    async getAll(page: number = 1): Promise<FilteredResult<AviaskQuestionReport>> {
        const result = await this.$request<FilteredResult<responseModels.QuestionReport>>(
            "get",
            this.routeMap.index(page),
        );

        if (!result.success) return { elements: [], totalCount: 0 };

        return {
            elements: result.result.elements.map(r => new AviaskQuestionReport(r)),
            totalCount: result.result.totalCount,
        };
    }

    async get(id: string): Promise<ApiResponse<AviaskQuestionReport>> {
        const result = await this.$request<responseModels.QuestionReport>("get", this.routeMap.show(id));

        if (!result)
            return {
                success: false,
                result: null!,
                error: null!,
            };

        return {
            success: true,
            result: new AviaskQuestionReport(result.result),
            error: null!,
        };
    }

    async create(report: requestModels.CreateQuestionReportModel): Promise<ApiResponse<unknown>> {
        return await this.$request<unknown>("post", this.routeMap.new(), report);
    }

    async edit(id: string, state: number): Promise<ApiResponse<AviaskQuestionReport>> {
        const res = await this.$request<responseModels.QuestionReport>("patch", this.routeMap.edit(id, state));

        if (!res.success)
            return {
                success: false,
                result: null!,
                error: null!,
            };

        return {
            success: true,
            result: new AviaskQuestionReport(res.result),
            error: null!,
        };
    }

    async delete(id: string): Promise<boolean> {
        const res = await this.$request<string>("delete", this.routeMap.delete(id));

        return res.success;
    }
}

export default function useQuestionReportsRepository() {
    const apiStore = useApiStore();

    return new QuestionReportsRepostory(apiStore.$request, apiStore.$requestForm);
}
