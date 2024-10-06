import { getQueryParams } from "@/helper";
import { AviaskQuestionDetails, type requestModels, type responseModels } from "@/models";
import type { ApiResponse, FilteredResult, Question } from "@/models/DTO/responseModels";
import { AviaskQuestion } from "@/models/aviaskQuestion";
import useApiStore from "@/stores/apiStore";
import { AxiosError } from "axios";
import { Repository } from "./repository";

export class QuestionsRepository extends Repository {
    private routeMap = {
        index: (filters: requestModels.QuestionsIndexParameters) =>
            `/api/questions?${getQueryParams(filters as unknown as Record<string, unknown>)}`,
        show: (id: string) => `/api/question/${id}`,
        showExtended: (id: string) => `/api/question/extended/${id}`,
        new: () => `/api/questions`,
        edit: (id: string) => `/api/question/${id}`,
        showIllustration: (id: string) => `/api/question/${id}/illustration`,
        editIllustration: (id: string) => `/api/question/${id}/illustration`,
        destroy: (id: string) => `/api/question/${id}`,
        check: (id: string, answer: string, isMockExam: boolean) =>
            `/api/question/${id}/check/${answer}?isMockExam=${isMockExam}`,
        count: () => `/api/questions/count`,
        random: (categories: number[]) => `/api/questions/random?${getQueryParams({ categories: categories })}`,
    };

    async getAll(filters: requestModels.QuestionsIndexParameters): Promise<FilteredResult<AviaskQuestionDetails>> {
        const res = await this.$request<FilteredResult<responseModels.QuestionDetails>>(
            "get",
            this.routeMap.index(filters),
        );

        if (!res.success) return { elements: [], totalCount: 0 };

        return {
            elements: res.result.elements.map(r => new AviaskQuestionDetails(r)),
            totalCount: res.result.totalCount,
        };
    }

    async get(id: string): Promise<ApiResponse<AviaskQuestionDetails>> {
        const res = await this.$request<responseModels.QuestionDetails>("get", this.routeMap.show(id));

        return {
            error: res.error,
            result: res.success ? new AviaskQuestionDetails(res.result) : null!,
            success: res.success,
        };
    }

    async getExtended(id: string): Promise<ApiResponse<AviaskQuestion>> {
        const res = await this.$request<responseModels.Question>("get", this.routeMap.showExtended(id));

        return {
            error: res.error,
            result: res.success ? new AviaskQuestion(res.result) : null!,
            success: res.success,
        };
    }

    async create(question: Question): Promise<ApiResponse<responseModels.QuestionDetails>> {
        return await this.$request<responseModels.QuestionDetails>("post", this.routeMap.new(), question);
    }

    async edit(question: Question): Promise<ApiResponse<responseModels.QuestionDetails>> {
        return await this.$request<responseModels.QuestionDetails>("put", this.routeMap.edit(question.id), question);
    }

    async editIllustration(id: string, illustration: Blob | null): Promise<string | null | Error> {
        try {
            return await this.$formRequest<string | null>("patch", this.routeMap.editIllustration(id), {
                file: illustration,
            });
        } catch (e) {
            if (e instanceof AxiosError) return e;

            throw e;
        }
    }

    async check(
        id: string,
        answer: string,
        isMockExam: boolean = false,
    ): Promise<ApiResponse<responseModels.CheckAnswerResult>> {
        return await this.$request<responseModels.CheckAnswerResult>(
            "get",
            this.routeMap.check(id, answer, isMockExam),
        );
    }

    async delete(id: string): Promise<boolean> {
        const res = await this.$request<boolean>("delete", this.routeMap.destroy(id));

        return res.success;
    }

    async randomId(categories: number[] = []): Promise<ApiResponse<string>> {
        return await this.$request<string>("get", this.routeMap.random(categories));
    }

    async count(): Promise<number> {
        const res = await this.$request<number>("get", this.routeMap.count());

        return res.success ? res.result : 0;
    }
}

export default function useQuestionsRepository() {
    const apiStore = useApiStore();

    return new QuestionsRepository(apiStore.$request, apiStore.$requestForm);
}
