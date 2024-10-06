export interface CreateUserModel {
    username: string;
    email: string;
    password: string;
}

export interface CreateQuestionReportModel {
    message: string;
    questionId: string;
    category: number;
}

export interface LoginModel {
    email: string;
    password: string;
}

export interface EditUserModel {
    username: string;
    email: string;
    role: string;
    isPremium: boolean;
}

export interface QuestionsIndexParameters {
    page: number;
    categoryFilter: number[];
    titleFilter: string;
    sourceFilter: string;
}

export interface PasswordResetModel {
    token: string;
    password: string;
    rePassword: string;
}

export interface CreateMockExamModel {
    maxQuestions: number;
    category: number;
    timeLimit: string;
}
