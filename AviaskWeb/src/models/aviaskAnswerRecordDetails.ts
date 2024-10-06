import type { AnswerRecordDetails } from "./DTO/responseModels";
import { AviaskQuestionDetails } from "./aviaskQuestion";
import { AviaskUser } from "./aviaskUser";

export class AviaskAnswerRecordDetails {
    public id: string;
    public question: AviaskQuestionDetails;
    public user: AviaskUser;
    public answered: string;
    public isCorrect: boolean;
    public answeredAt: Date;

    constructor(details: AnswerRecordDetails) {
        this.id = details.id;
        this.question = new AviaskQuestionDetails(details.question);
        this.user = new AviaskUser(details.user);
        this.answered = details.answered;
        this.isCorrect = details.isCorrect;
        this.answeredAt = new Date(details.answeredAt);
    }

    toInterface(): AnswerRecordDetails {
        return {
            id: this.id,
            userId: this.user.id,
            user: this.user.toInterface(),
            answered: this.answered,
            answeredAt: this.answeredAt.getTime(),
            isCorrect: this.isCorrect,
            question: this.question.toInterface(),
            questionId: this.question.id,
        };
    }
}
