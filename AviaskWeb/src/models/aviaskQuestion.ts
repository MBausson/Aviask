import { shuffleArray } from "@/helper";
import { responseModels } from ".";
import { AtplCategory, QuestionStatus, QuestionVisibility } from "./DTO/responseModels";

export class AviaskQuestionAnswers {
    public id: string;
    public answers: string[];
    public correctAnswer: string;
    public explications: string | null;

    constructor(qa: responseModels.QuestionAnswers) {
        this.id = qa.id;

        this.answers = [qa.answer1, qa.answer2];

        if (qa.answer3 && qa.answer3?.length > 0) this.answers.push(qa.answer3);
        if (qa.answer4 && qa.answer4?.length > 0) this.answers.push(qa.answer4);

        this.correctAnswer = qa.correctAnswer;
        this.explications = qa.explications;
    }

    toInterface(): responseModels.QuestionAnswers {
        return {
            id: this.id,
            explications: this.explications,
            answer1: this.answers[0],
            answer2: this.answers[1],
            answer3: this.answers[2] || null,
            answer4: this.answers[3] || null,
            correctAnswer: this.correctAnswer,
        };
    }

    public static default(): AviaskQuestionAnswers {
        return new AviaskQuestionAnswers({} as responseModels.QuestionAnswers);
    }
}

export class AviaskQuestion {
    public id: string;
    public title: string;
    public description: string;
    public category: AtplCategory;
    public source: string;
    public questionAnswersId: string;
    public questionAnswers: AviaskQuestionAnswers;
    public publisherId: string;
    public visibility: responseModels.QuestionVisibility;
    public status: responseModels.QuestionStatus;
    public illustrationId: string | null;

    constructor(q: responseModels.Question) {
        this.id = q.id;
        this.title = q.title;
        this.description = q.description;
        this.category = AtplCategory.getCategory(q.category)!;
        this.source = q.source;
        this.questionAnswersId = q.questionAnswersId;
        this.questionAnswers = new AviaskQuestionAnswers(q.questionAnswers);
        this.publisherId = q.publisherId;
        this.visibility = QuestionVisibility.getQuestionVisibility(q.visibility)!;
        this.status = QuestionStatus.getStatus(q.status);
        this.illustrationId = q.illustrationId;
    }

    toInterface(): responseModels.Question {
        return {
            id: this.id,
            title: this.title,
            description: this.description,
            category: this.category.code,
            source: this.source,
            questionAnswersId: this.publisherId,
            questionAnswers: this.questionAnswers.toInterface(),
            publisherId: this.publisherId,
            visibility: this.visibility.value,
            status: this.status.value,
            illustrationId: this.illustrationId,
        };
    }

    public static default(): AviaskQuestion {
        return new AviaskQuestion({
            questionAnswers: AviaskQuestionAnswers.default().toInterface(),
            visibility: 0,
            source: "EASA",
        } as responseModels.Question);
    }
}

export class AviaskQuestionDetails {
    public id: string;
    public title: string;
    public description: string;
    public category: AtplCategory;
    public source: string;
    public answers: string[];
    public publisherId: string;
    public publishedAt: Date;
    public illustrationId: string;
    public status: QuestionStatus;

    constructor(q: responseModels.QuestionDetails) {
        this.id = q.id;
        this.title = q.title;
        this.description = q.description;
        this.category = AtplCategory.getCategory(q.category)!;
        this.source = q.source;
        this.answers = shuffleArray(q.answers);
        this.publisherId = q.publisherId;
        this.publishedAt = new Date(q.publishedAt);
        this.illustrationId = q.illustrationId;
        this.status = QuestionStatus.getStatus(q.status);
    }

    toInterface(): responseModels.QuestionDetails {
        return {
            id: this.id,
            title: this.title,
            description: this.description,
            category: this.category.code,
            source: this.source,
            answers: this.answers,
            publisherId: this.publisherId,
            publishedAt: this.publishedAt.toLocaleDateString(),
            illustrationId: this.illustrationId,
            status: this.status.value,
        };
    }

    static default(): AviaskQuestionDetails {
        return new AviaskQuestionDetails({} as responseModels.QuestionDetails);
    }
}
