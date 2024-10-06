import { responseModels } from ".";

export class QuestionReportCategory {
    public name: string;
    public value: number;

    constructor(name: string, value: number) {
        this.name = name;
        this.value = value;
    }

    static categories: QuestionReportCategory[] = [
        { name: "Incorrect informations", value: 0 },
        { name: "Typos", value: 1 },
        { name: "Irrelevant question", value: 2 },
        { name: "Inappropriate content", value: 3 },
        { name: "Incorrect explications", value: 4 },
        { name: "Feedback", value: 5 },
    ];

    static getCategory(value: number): QuestionReportCategory | undefined {
        return this.categories.filter(c => c.value == value).at(0);
    }
}

export enum QuestionReportState {
    Pending = 0,
    Treated = 1,
    Declined = 2,
}

export class AviaskQuestionReport {
    id: string;
    message: string;
    category: QuestionReportCategory;
    state: QuestionReportState;
    issuerId: string;
    issuer: responseModels.AviaskUserDetails;
    questionId: string;
    question: responseModels.QuestionDetails;

    constructor(report: responseModels.QuestionReport) {
        this.id = report.id;
        this.message = report.message;
        this.category = QuestionReportCategory.getCategory(report.category)!;
        this.state = report.state;
        this.issuerId = report.issuerId;
        this.issuer = report.issuer;
        this.questionId = report.questionId;
        this.question = report.question;
    }

    static default(): AviaskQuestionReport {
        return new AviaskQuestionReport({
            id: "",
            message: "",
            category: 0,
            state: 0,
            issuerId: "",
            issuer: {} as responseModels.AviaskUserDetails,
            questionId: "",
            question: {} as responseModels.QuestionDetails,
        });
    }
}
