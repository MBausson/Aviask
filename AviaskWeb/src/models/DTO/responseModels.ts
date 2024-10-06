export class AtplCategory {
    public name: string;
    public code: number;

    constructor(name: string, code: number) {
        this.name = name;
        this.code = code;
    }

    static getCategory(code: number): AtplCategory | undefined {
        return this.categories.filter(c => c.code == code).at(0);
    }

    static categoryCodeExist(code: number): boolean {
        return this.categories.map(c => c.code).includes(code);
    }

    static categories: AtplCategory[] = [
        { name: "Air law", code: 10 },
        { name: "Airframe Systems", code: 21 },
        { name: "Instrumentation", code: 22 },
        { name: "Mass and Balance", code: 31 },
        { name: "Performance", code: 32 },
        { name: "Flight Planning", code: 33 },
        { name: "Human Performance", code: 40 },
        { name: "Meteorology", code: 50 },
        { name: "General Navigation", code: 61 },
        { name: "Radio Navigation", code: 62 },
        { name: "Operational Procedures", code: 70 },
        { name: "Principles of Flight", code: 81 },
        { name: "VFR Communications", code: 91 },
        { name: "IFR Communications", code: 92 },
    ];

    static fullTitle(cat: AtplCategory): string {
        return `${cat.name} [${cat.code}]`;
    }
}

export class QuestionVisibility {
    public name: string;
    public value: number;

    constructor(name: string, value: number) {
        this.name = name;
        this.value = value;
    }

    static getQuestionVisibility(value: number): QuestionVisibility | undefined {
        return this.visibilities.filter(v => v.value == value).at(0);
    }

    static visibilities: QuestionVisibility[] = [
        { name: "Private", value: 0 },
        { name: "Public", value: 1 },
    ];

    static default: QuestionVisibility = this.getQuestionVisibility(0)!;
}

export class SubscriptionStatus {
    public name: string;
    public value: number;

    constructor(name: string, value: number) {
        this.name = name;
        this.value = value;
    }

    static getSubscriptionStatus(value: number): SubscriptionStatus | undefined {
        return this.status.filter(s => s.value == value).at(0);
    }

    static status: SubscriptionStatus[] = [
        { name: "Active", value: 0 },
        { name: "Cancelled", value: 1 },
    ];
}

export type QuestionStatusType = "Accepted" | "Declined" | "Pending";

export class QuestionStatus {
    public readonly name: QuestionStatusType;
    public readonly value: number;

    private constructor(name: QuestionStatusType, value: number) {
        this.name = name;
        this.value = value;
    }

    static status: Record<QuestionStatusType, QuestionStatus> = {
        Pending: new QuestionStatus("Pending", 2),
        Accepted: new QuestionStatus("Accepted", 0),
        Declined: new QuestionStatus("Declined", 1),
    };

    static getStatus(value: number): QuestionStatus {
        return (
            Object.values(this.status)
                .filter(s => s.value == value)
                .at(0) || this.status["Pending"]
        );
    }
}

export interface ApiResponse<T> {
    success: boolean;
    result: T;
    error: ApiError;
}

export interface ApiError {
    message: string;
}

export interface AuthenticationResult {
    token: string;
    createdAt: Date;
}

export interface QuestionDetails {
    id: string;
    title: string;
    description: string;
    category: number;
    source: string;
    answers: string[];
    publisherId: string;
    publishedAt: string;
    illustrationId: string;
    status: number;
}

export interface AnswerRecordDetails {
    id: string;

    questionId: string;
    question: QuestionDetails;

    userId: string;
    user: AviaskUserDetails;

    answered: string;
    isCorrect: boolean;

    answeredAt: number;
}

export interface QuestionAnswers {
    id: string;

    answer1: string;
    answer2: string;
    answer3: string | null;
    answer4: string | null;

    correctAnswer: string;
    explications: string | null;
}

export interface Question {
    id: string;
    title: string;
    description: string;
    category: number;
    source: string;
    visibility: number;
    status: number;

    questionAnswersId: string;
    questionAnswers: QuestionAnswers;

    publisherId: string;
    illustrationId: string | null;
}

export interface AviaskUserDetails {
    id: string;
    userName: string;
    role: string;
    createdAt: string;
    isPremium: boolean;
}

export interface CheckAnswerResult {
    id: string;
    isCorrect: boolean;
    correctAnswer: string;
    explications: string | null;
}

export interface CategoryStatistics {
    category: number;
    correctnessRatio: number;
    answerCount: number;
}

export interface DayActivity {
    day: string;
    answerCount: number;
}

export interface UserStatistics {
    readyForExamCategories: CategoryStatistics[];
    weakestCategories: CategoryStatistics[];
    totalCategories: CategoryStatistics[];
    last30DaysCorrectAnswers: DayActivity[];
    last30DaysWrongAnswers: DayActivity[];
}

export interface DataValidationError {
    errors: { [key: string]: string[] };
}

export interface UserExtended {
    id: string;
    userName: string;
    email: string;
    createdAt: string;
    role: string;
    isPremium: boolean;
}

export interface QuestionReport {
    id: string;
    message: string;
    category: number;
    state: number;
    issuerId: string;
    issuer: AviaskUserDetails;
    questionId: string;
    question: QuestionDetails;
}

export interface QuestionSuggestionDetails {
    status: number;
    question: QuestionDetails;
    publisher: AviaskUserDetails;
}

export interface UserProfile {
    userDetails: AviaskUserDetails;
    acceptedSuggestions: number;
    answersCount: number;
}

export interface SubscriptionInformations {
    status: number;
    startedAt: string;
    nextPayment: string;
}

export interface MockExam {
    id: string;
    status: number;
    category: number;
    answerRecords: AnswerRecordDetails[];
    maxDuration: string;
    maxQuestions: number;
    correctnessRatio: number;
    userId: string;
    questionId: string;
    startedAt: string;
    endedAt: string;
}

export interface FilteredResult<T> {
    elements: T[];
    totalCount: number;
}

export interface LeaderboardUser {
    user: AviaskUserDetails;
    questionsCount: number;
    rank: number;
}

export interface CurrentLeaderboard {
    users: LeaderboardUser[];
    currentUserCount: number;
}
