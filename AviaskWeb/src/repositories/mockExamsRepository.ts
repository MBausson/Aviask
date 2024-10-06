import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import { AviaskMockExam } from "@/models/aviaskMockExam";
import type { requestModels, responseModels } from "@/models";
import { getQueryParams } from "@/helper";
import type { ApiResponse, FilteredResult } from "@/models/DTO/responseModels";

export class MockExamsRepository extends Repository {
    private routeMap = {
        index: (page?: number) => `/api/mockExams?${getQueryParams({ page: page })}`,
        show: () => `/api/mockExam`,
        new: () => `/api/mockExams`,
        stop: () => `/api/mockExam/stop`,
        next: () => `/api/mockExam/next`,
    };

    async getAll(page?: number): Promise<FilteredResult<AviaskMockExam>> {
        const res = await this.$request<FilteredResult<responseModels.MockExam>>("get", this.routeMap.index(page));

        if (!res.success) return { elements: [], totalCount: 0 };

        return { elements: res.result.elements.map(m => new AviaskMockExam(m)), totalCount: res.result.totalCount };
    }

    async current(): Promise<AviaskMockExam | null> {
        try {
            const res = await this.$request<responseModels.MockExam>("get", this.routeMap.show());

            return new AviaskMockExam(res.result);
        } catch (e) {
            return null;
        }
    }

    async create(model: requestModels.CreateMockExamModel): Promise<ApiResponse<AviaskMockExam>> {
        const res = await this.$request<responseModels.MockExam>("post", this.routeMap.new(), model);

        if (!res.success)
            return {
                success: res.success,
                error: res.error,
                result: null!,
            };

        return {
            success: true,
            error: null!,
            result: new AviaskMockExam(res.result),
        };
    }

    async stop(): Promise<boolean> {
        const res = await this.$request<boolean>("patch", this.routeMap.stop());

        return res.success;
    }

    async next(): Promise<ApiResponse<string>> {
        return await this.$request<string>("get", this.routeMap.next());
    }
}

export default function useMockExamsRepository() {
    const apiStore = useApiStore();

    return new MockExamsRepository(apiStore.$request, apiStore.$requestForm);
}
