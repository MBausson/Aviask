import { type responseModels } from ".";
import { AtplCategory, type AnswerRecordDetails, type MockExam } from "./DTO/responseModels";
import moment from "moment";

export type MockExamStatusType = "Ongoing" | "Finished";

export class MockExamStatus {
    public name: MockExamStatusType;
    public value: number;

    constructor(name: MockExamStatusType, value: number) {
        this.name = name;
        this.value = value;
    }

    static getMockExamStatus(value: number): MockExamStatus {
        return (
            Object.values(this.status)
                .filter(s => s.value === value)
                .at(0) || this.status["Finished"]
        );
    }

    static status: Record<MockExamStatusType, MockExamStatus> = {
        Ongoing: new MockExamStatus("Ongoing", 0),
        Finished: new MockExamStatus("Finished", 1),
    };
}

export class AviaskMockExam {
    public id: string;
    public status: MockExamStatus;
    public category: AtplCategory;
    public maxDuration: moment.Duration;
    public maxQuestions: number;
    public correctnessRatio: number;
    public userId: string;
    public questionId: string;
    public answerRecords: AnswerRecordDetails[];
    public startedAt: Date;
    public endedAt: Date;

    constructor(mockExam: MockExam) {
        this.id = mockExam.id;
        this.status = MockExamStatus.getMockExamStatus(mockExam.status);
        this.category = AtplCategory.getCategory(mockExam.category) || AtplCategory.getCategory(-1)!;
        this.maxDuration = moment.duration(mockExam.maxDuration);
        this.maxQuestions = mockExam.maxQuestions;
        this.correctnessRatio = mockExam.correctnessRatio;
        this.userId = mockExam.userId;
        this.questionId = mockExam.questionId;
        this.answerRecords = mockExam.answerRecords;
        this.startedAt = new Date(mockExam.startedAt);
        this.endedAt = new Date(mockExam.endedAt);
    }

    toInterface(): responseModels.MockExam {
        return {
            id: this.id,
            status: this.status.value,
            category: this.category.code,
            maxDuration: this.maxDuration.toJSON(),
            maxQuestions: this.maxQuestions,
            correctnessRatio: this.correctnessRatio,
            questionId: this.questionId,
            userId: this.userId,
            answerRecords: this.answerRecords,
            startedAt: this.startedAt.toString(),
            endedAt: this.endedAt.toString(),
        };
    }

    static default(): AviaskMockExam {
        return new AviaskMockExam({} as responseModels.MockExam);
    }
}
