import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import { AviaskAnswerRecordDetails, type responseModels } from "@/models";
import type { FilteredResult } from "@/models/DTO/responseModels";

export class AnswerRecordsRepository extends Repository {
    private routeMap = {
        index: (userId: string, page: number) => `/api/answerRecords/user/${userId}?page=${page}`,
        destroy: (id: string) => `/api/answerRecords/${id}`,
    };

    async getAll(userId: string, page: number = 1): Promise<FilteredResult<AviaskAnswerRecordDetails>> {
        const res = await this.$request<FilteredResult<responseModels.AnswerRecordDetails>>(
            "get",
            this.routeMap.index(userId, page),
        );

        if (!res.success) return { elements: [], totalCount: 0 };

        return {
            elements: res.result.elements.map(a => new AviaskAnswerRecordDetails(a)),
            totalCount: res.result.totalCount,
        };
    }

    async delete(id: string): Promise<boolean> {
        const res = await this.$request<boolean>("delete", this.routeMap.destroy(id));

        return res.success;
    }
}

export default function useAnswerRecordsRepository() {
    const apiStore = useApiStore();

    return new AnswerRecordsRepository(apiStore.$request, apiStore.$requestForm);
}
